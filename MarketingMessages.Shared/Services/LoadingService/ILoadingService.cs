namespace MarketingMessages.Shared.Services.LoadingService
{
    public interface ILoadingService
    {
        bool Loading { get; set; }

        event EventHandler<bool>? OnLoading;
    }
}