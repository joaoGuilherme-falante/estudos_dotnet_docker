namespace TaskFlow.Api.Services;

public sealed class PingService : IPingService
{
    public string GetMessage()
    {
        return "TaskFlow API está no ar.";
    }
}
