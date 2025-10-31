namespace MarketingMessages.Shared.Services.LoadingService;

public class LoadingService : ILoadingService
{
    public event EventHandler<bool>? OnLoading;

    private bool _loading;

    public bool Loading
    {
        get => _loading;
        set
        {
            _loading = value;
            OnLoading?.Invoke(this, value);
        }
    }
}
