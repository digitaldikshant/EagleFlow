namespace EagleFlow.Services;

public class SmsSender(ILogger<SmsSender> logger) : ISmsSender
{
    public Task SendAsync(string mobileNumber, string message)
    {
        logger.LogInformation("Simulated SMS OTP to {Mobile}: {Message}", mobileNumber, message);
        return Task.CompletedTask;
    }
}
