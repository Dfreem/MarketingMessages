using MarketingMessages.Shared.Extensions;

namespace MarketingMessages.Shared.DTO.V1;

public class ContactListGroup
{
    /// <summary>
    /// The indexing key for this list group. (Default: <see cref="Guid.NewGuid"/> as string)
    /// </summary>
    public string GroupName { get; set; } = Guid.NewGuid().ToString();
    public List<ContactList> ContactLists { get; set; } = [];

    public ContactList this[string key]
    {
        get => ContactLists.First(list => list.ListName == key);
        set
        {
            var setting = ContactLists.First(c => c.ListName == key) ?? throw new KeyNotFoundException(key);
            if (setting is null)
            {
                ContactLists.Add(value);
                return;
            }
            int insertLocation = ContactLists.IndexOf(setting);
            ContactLists.Remove(setting);
            ContactLists.Insert(insertLocation, value);
        }
    }
    public bool TryAdd(string key, ContactList item)
    {
        if (ContactLists.Any(l => l.ContainsKey(key)))
            return false;

        this[key] = item;
        return true;
    }

}
