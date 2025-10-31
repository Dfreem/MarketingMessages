namespace MarketingMessages.Shared.DTO.V1;

public class ContactList
{
    public int ListId { get; set; }
    public string ListName { get; set; } = "";
    public List<ContactModel> Contacts { get; set; } = [];

}

