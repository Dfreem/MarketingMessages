using System.Text.Json.Serialization;

namespace MarketingMessages.Shared.DTO;

public class SuppressionGroupModel
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public List<string> Unsubscribes { get; set; } = [];
}
