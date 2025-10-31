namespace MarketingMessages.Services;

public class UnsubscribeGroupModel
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public int Unsubscribes { get; set; }
}
