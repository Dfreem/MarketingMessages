using MarketingMessages.Data;
using MarketingMessages.Shared.DTO;
using MarketingMessages.Shared.Extensions;
using MarketingMessages.Shared.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sender = MarketingMessages.Shared.Models.Sender;

namespace MarketingMessages.Repository;

/// <summary>
/// DEPRECATED - Use AudienceRepository instead
/// </summary>
public class ContactsRepository
{
    IDbContextFactory<MarketingMessagesContext> _contextFactory;
    ILogger<ContactsRepository> _logger;

    public ContactsRepository(IDbContextFactory<MarketingMessagesContext> contactFactory, ILogger<ContactsRepository> logger)
    {
        _logger = logger;
        _contextFactory = contactFactory;
    }

    public async Task<List<ContactModel>> GetUserContactsAsync(string userId, int index, int pageSize, string search = "")
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        var results = db.Contacts
            .Where(c => c.CreatedBy == userId)
            .OrderBy(c => c.ContactId)
            .Skip(index)
            .Take(pageSize)
            .Select(c => c.ToDisplayModel())
            .ToList();
        if (!string.IsNullOrEmpty(search))
            results = results.Where(c => $"{c.ContactFirstName} {c.ContactLastName} {c.ContactEmail} {c.Profession} {c.Profession2} {c.State} {c.Country}".Contains(search)).ToList();
        return results;
    }
    public async Task<int> CountAllContacts(string userId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.Contacts.CountAsync(c => c.CreatedBy == userId);
    }

    public List<Sender> GetSenders(string userId)
    {
        using var db = _contextFactory.CreateDbContext();
        return db.Senders.Where(s => s.CreatedBy == userId).ToList();
    }

    public async Task SaveContacts(List<Contact> contacts)
    {
        using var db = _contextFactory.CreateDbContext();
        do
        {
            var batch = contacts.Take(1000);
            await db.Contacts.AddRangeAsync(contacts);
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while attempting to save contacts\n{ex}", ex);
            }
            contacts.RemoveRange(0, batch.Count());

        } while (contacts.Any());

    }

    public SuccessResponse DeleteContacts(List<int> contactIds, string userId)
    {
        try
        {

            using var db = _contextFactory.CreateDbContext();
            IEnumerable<Contact> contacts = [];
            if (contactIds.Count == 0)
                contacts = db.Contacts.Where(c => c.CreatedBy == userId );
            else
                contacts = db.Contacts.Where(c => c.CreatedBy == userId && contactIds.Contains(c.ContactId));
            db.Contacts.RemoveRange(contacts);
            db.SaveChanges();
            return new() { Success = true };
        }
        catch (Exception ex)
        {
            _logger.LogError("Error occurred while attempting to delete contacts\n{ex}", ex);
            return new() { Error = ex.Message };
        }

    }
}
