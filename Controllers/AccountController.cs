using System.Security.Claims;
using EagleFlow.Data;
using EagleFlow.Models;
using EagleFlow.Models.ViewModels;
using EagleFlow.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EagleFlow.Controllers;

[AllowAnonymous]
public class AccountController(
    ApplicationDbContext dbContext,
    IEmailSender emailSender,
    ISmsSender smsSender) : Controller
{
    private static readonly PasswordHasher<AdminUser> PasswordHasher = new();

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Admin");
        }

        return View(new AdminLoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(AdminLoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var email = model.Email.Trim().ToLowerInvariant();
        var admin = await dbContext.AdminUsers.FirstOrDefaultAsync(a => a.Email == email && a.IsActive);
        if (admin is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        var verifyResult = PasswordHasher.VerifyHashedPassword(admin, admin.PasswordHash, model.Password);
        if (verifyResult == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, admin.Id.ToString()),
            new(ClaimTypes.Name, admin.Email),
            new(ClaimTypes.Email, admin.Email),
            new(ClaimTypes.Role, "Admin")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        return RedirectToAction("Index", "Admin");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var email = model.Email.Trim().ToLowerInvariant();
        var admin = await dbContext.AdminUsers.FirstOrDefaultAsync(a => a.Email == email && a.IsActive);

        TempData["Info"] = "If the account exists, OTP has been sent.";

        if (admin is null)
        {
            return RedirectToAction(nameof(ResetPassword));
        }

        var channel = model.Channel.Trim().ToLowerInvariant();
        if (channel is not ("email" or "mobile"))
        {
            ModelState.AddModelError(nameof(model.Channel), "Invalid OTP delivery method.");
            return View(model);
        }

        var destination = email;
        if (channel == "mobile")
        {
            destination = string.IsNullOrWhiteSpace(model.MobileNumber)
                ? admin.MobileNumber?.Trim() ?? string.Empty
                : model.MobileNumber.Trim();

            if (string.IsNullOrWhiteSpace(destination))
            {
                ModelState.AddModelError(nameof(model.MobileNumber), "Mobile number is required for mobile OTP.");
                return View(model);
            }
        }

        var otpCode = Random.Shared.Next(100000, 999999).ToString();

        var pendingOtps = await dbContext.PasswordResetOtps
            .Where(o => o.Email == email && !o.IsUsed && o.ExpiresAtUtc > DateTime.UtcNow)
            .ToListAsync();

        foreach (var pendingOtp in pendingOtps)
        {
            pendingOtp.IsUsed = true;
        }

        dbContext.PasswordResetOtps.Add(new PasswordResetOtp
        {
            Email = email,
            OtpCode = otpCode,
            Channel = channel,
            Destination = destination,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false
        });

        await dbContext.SaveChangesAsync();

        if (channel == "mobile")
        {
            await smsSender.SendAsync(destination, $"Your EagleFlow OTP is {otpCode}. Valid for 10 minutes.");
        }
        else
        {
            await emailSender.SendAsync(email, "EagleFlow password reset OTP", $"Your OTP is {otpCode}. It is valid for 10 minutes.");
        }

        return RedirectToAction(nameof(ResetPassword), new { email });
    }

    [HttpGet]
    public IActionResult ResetPassword(string? email)
    {
        return View(new ResetPasswordViewModel { Email = email?.Trim() ?? string.Empty });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var email = model.Email.Trim().ToLowerInvariant();
        var otpCode = model.OtpCode.Trim();

        var otp = await dbContext.PasswordResetOtps
            .Where(o => o.Email == email && o.OtpCode == otpCode && !o.IsUsed && o.ExpiresAtUtc > DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedAtUtc)
            .FirstOrDefaultAsync();

        if (otp is null)
        {
            ModelState.AddModelError(nameof(model.OtpCode), "Invalid or expired OTP.");
            return View(model);
        }

        var admin = await dbContext.AdminUsers.FirstOrDefaultAsync(a => a.Email == email && a.IsActive);
        if (admin is null)
        {
            ModelState.AddModelError(string.Empty, "Admin account not found.");
            return View(model);
        }

        admin.PasswordHash = PasswordHasher.HashPassword(admin, model.NewPassword);
        otp.IsUsed = true;
        await dbContext.SaveChangesAsync();

        TempData["Info"] = "Password reset successful. Please login.";
        return RedirectToAction(nameof(Login));
    }
}
