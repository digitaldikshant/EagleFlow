namespace EagleFlow.Services;

public interface ISmsSender
{
    Task SendAsync(string mobileNumber, string message);
}
