namespace MarketingMessages.Shared.Components.Toast.Models;

public class ToastEventArgs(string message = "", string cssClass = "", ToastOptions? options = null) : EventArgs
{
    public string Message { get; set; } = message;

    public string CssClass { get; set; } = cssClass;
    public int? Duration { get; set; } = options?.Duration;
    public ToastPosition Position { get; set; } = options?.Position ?? ToastPosition.Top;
}