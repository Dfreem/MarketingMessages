using MarketingMessages.Shared.DTO.V1;

namespace MarketingMessages.Shared.Extensions;

public static class ContactListExtensions
{
    public static bool ContainsId(this ContactList contactList, int id) => contactList.Contacts.Any(c => c.ContactId == id);
    public static bool ContainsId(this ContactListGroup listGroup, int id) => listGroup.ContactLists.Any(c => c.ListId == id);
    public static bool ContainsKey(this ContactList contactList, string key) => contactList.Contacts.Any(c => c.ContactFirstName == key);
    public static bool ContainsKey(this ContactListGroup listGroup, string key) => listGroup.ContactLists.Any(l => l.ListName == key);
}
