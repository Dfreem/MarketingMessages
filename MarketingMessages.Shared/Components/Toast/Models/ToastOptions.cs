namespace MarketingMessages.Shared.Components.Toast.Models;

public class ToastOptions
{
    /// <summary>
    /// Length of time that each toast message is visible for (in milliseconds).
    /// </summary>
    public int Duration { get; set; }

    /// <summary>
    /// The position that the toast notification is displayed. Multiple <see cref="ToastPosition"/> values can be combined to target corner.
    /// <br /> Example (Default): ToastPosition.Top | ToastPosition.Right  // This will display the notification in the top right corner of the screen.
    /// </summary>
    public ToastPosition Position { get; set; }
}
