using MarketingMessages.Data;
using MarketingMessages.Shared.DTO;
using MarketingMessages.Shared.Models;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Text.Json;

namespace MarketingMessages.Repository;

public class AudienceRepository
{
    private MarketingMessagesContext db;
    ILogger<AudienceRepository> _logger;

    public AudienceRepository(IDbContextFactory<MarketingMessagesContext> dbContextFactory, ILogger<AudienceRepository> logger)
    {
        db = dbContextFactory.CreateDbContext();
        _logger = logger;
    }

    public List<Audience> GetSegmentsForUser(string userId, bool includedDeleted = false)
    {
        var result = db.Audiences.Where(l => l.CreatedBy == userId);
        if (!includedDeleted)
            result = result.Where(l => !l.Deleted);
        return result.ToList();
    }

    public AudienceFormModel? GetAudienceForEditing(int audienceId)
    {
        var audience = db.Audiences.FirstOrDefault(a => a.Id == audienceId);
        if (audience is null)
            return null;
        return JsonSerializer.Deserialize<AudienceFormModel>(audience.JSONForm);
    }

    public List<Contact> ExecuteQueryForList(int sendListId)
    {
        var sendList = db.Audiences.FirstOrDefault(a => a.Id == sendListId);
        if (sendList is null)
        {
            _logger.LogWarning("Unable to find send list with id {sendListId}", sendListId);
            return [];
        }
        try
        {
            var contacts = db.Contacts.FromSqlRaw(sendList.Query).ToList();
            return contacts;
        }
        catch (Exception ex)
        {
            _logger.LogError("Query for Audience is invalid.\n{ex}", ex);
            return [];
        }

    }

    public async Task<Audience> SaveAudienceAsync(AudienceFormModel model, string userId)
    {

        Audience? audience = db.Audiences.FirstOrDefault(a => a.Id == model.SegmentId && a.CreatedBy == userId);
        if (audience is null)
        {

            audience = new()
            {
                CreatedBy = userId,
                CreatedOn = DateTime.Now,
                SuppressionGroupId = model.UnsubscribeGroup?.Id,
                Query = BuildAudienceQuery(model.Rows, model.AnyOrAll.ToString(), userId),
                Name = model.Title
            };
            await db.Audiences.AddAsync(audience);

            // first time save to get audience id, then serialize model after setting id
            db.SaveChanges();

            model.SegmentId = audience.Id;
            audience.JSONForm = JsonSerializer.Serialize(model);
        }
        else
        {
            audience.SuppressionGroupId = model.UnsubscribeGroup?.Id;
            audience.Query = BuildAudienceQuery(model.Rows, model.AnyOrAll.ToString(), userId);
            audience.Name = model.Title;
            audience.JSONForm = JsonSerializer.Serialize(model);
        }
        db.SaveChanges();
        return audience;
    }

    public Audience UpdateAudience(Audience audience)
    {
        db.Audiences.Update(audience);
        db.SaveChanges();
        return audience;
    }


    public int SaveSendList(SendListRequest request, string userId)
    {
        var contactQuery = db.Contacts.Where(
            c => c.Profession != null
            && request.Professions.Contains(c.Profession)
            || c.Profession2 != null
            && request.Professions.Contains(c.Profession2)
            ).ToQueryString();
        Audience list = new()
        {
            CreatedBy = userId,
            CreatedOn = DateTime.Now,
            Query = contactQuery,
            Name = request.ListName
        };
        db.Audiences.Add(list);
        db.SaveChanges();
        var contacts = db.Contacts.FromSqlRaw(contactQuery).ToList();
        return contacts.Count;
    }

    public int QueryCount(SendListRequest request, string userId)
    {
        if (request.CountAll)
            return db.Contacts.Count(c => c.CreatedBy == userId);
        var contactQuery = db.Contacts.Where(
            c => c.CreatedBy == userId
            && c.Profession != null
            && request.Professions.Contains(c.Profession)
            || c.Profession2 != null
            && request.Professions.Contains(c.Profession2)
            || !string.IsNullOrEmpty(c.Zip)
            && request.Zipcodes.Contains(c.Zip!)
            );
        //contactQuery = contactQuery.Where(
        //    c => c.Zip != null && request.Zipcodes.Contains(c.Zip));
        return contactQuery.Count();
    }

    /// <summary>
    /// UNFINISHED
    /// </summary>
    /// <param name="rows"></param>
    /// <param name="allOrAny"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public int QueryCount(List<SegmentFormRow> rows, string allOrAny, string userId)
    {

        if (rows.Count == 0) return db.Contacts.Count(c => c.CreatedBy == userId);
        var sql = BuildAudienceQuery(rows, allOrAny, userId);
        var userContacts = db.Contacts.FromSqlRaw(sql);
        return userContacts.ToList().Count;
    }

    private string BuildAudienceQuery(List<SegmentFormRow> rows, string allOrAny, string userId)
    {

        if (rows.Count == 0 || !rows.Any(r => r.QueryParams.Count != 0)) return db.Contacts.Where(c => c.CreatedBy == userId).ToQueryString();
        //var items = rows.Where(r => !String.IsNullOrEmpty(r.Operator)).ToList();
        if (allOrAny == ConditionalInclusion.Any.ToString())
        {
            string sql = "SELECT * FROM Contacts ";
            if (rows.Any(r => r.QueryParams.Count != 0))
                sql += $"WHERE CreatedBy = '{userId}' AND ";
            foreach (var row in rows)
            {
                string paramList = String.Join("', '", row.QueryParams);
                sql += $"{row.SegmentCategory} in ('{paramList}') ";

                if (rows.IndexOf(row) < rows.Count - 1)
                    sql += "OR ";
            }
            Console.WriteLine(sql);
            return sql;
        }
        else
        {
            var userContacts = db.Contacts.Where(c => c.CreatedBy == userId);
            foreach (var item in rows)
            {
                switch (Enum.Parse<SegmentCategory>(item.SegmentCategory))
                {
                    case (SegmentCategory.State):
                        userContacts = userContacts.Where(u => u.State != null && item.QueryParams.Contains(u.State));
                        break;

                    case (SegmentCategory.Profession):
                        userContacts = userContacts.Where(
                            c => (c.Profession != null && item.QueryParams.Contains(c.Profession))
                            || (c.Profession2 != null && item.QueryParams.Contains(c.Profession2)));
                        break;
                    case (SegmentCategory.ZipCode):
                        userContacts = userContacts.Where(c => item.QueryParams.Any(q => q == c.Zip));
                        break;
                    default:
                        break;
                }
            }
            return userContacts.ToQueryString();
        }
    }

    //private static string BuildAudienceQuery(List<SegmentFormRow> rows, ConditionalInclusion allOrAny)
    //{
    //    var items = rows.Where(r => !String.IsNullOrEmpty(r.Operator)).ToList();
    //    var sql = "SELECT * FROM Contacts Where ";
    //    for (int i = 0; i < items.Count; i++)
    //    {
    //        var item = items[i];
    //        switch (item.SegmentCategory)
    //        {
    //            case (SegmentCategory.Address):
    //                var addressOperator = Enum.Parse<AddressOperator>(item.Operator!);
    //                sql += "Address " + item.Operator + " ('" + String.Join("', '", item.QueryParams) + "') ";
    //                break;
    //            case (SegmentCategory.EmailDateAdded):
    //                var dateOperator = Enum.Parse<DateFieldOperator>(item.Operator!);
    //                if (dateOperator == DateFieldOperator.MonthIs)
    //                {
    //                    sql += "MONTH(CreatedOn) in ('" + String.Join("', '", item.QueryParams) + "') ";
    //                }
    //                else if (dateOperator == DateFieldOperator.DayIs)
    //                {
    //                    sql += "CreatedOn ";
    //                }
    //                break;
    //            default:
    //                break;

    //        }
    //        if (rows.Count > 1 && i < items.Count - 1)
    //        {
    //            sql += allOrAny == ConditionalInclusion.All ? "AND " : "OR ";
    //        }
    //    }
    //    return sql;

    //}

    public int QueryCount(int listId)
    {
        var list = db.Audiences.Find(listId);
        if (list is null)
        {
            _logger.LogError("Unable to find list with id {listId}", listId);
            return 0;
        }
        var contactQuery = db.Contacts.FromSqlRaw(list.Query).ToList();

        //contactQuery = contactQuery.Where(
        //    c => c.Zip != null && request.Zipcodes.Contains(c.Zip));
        return contactQuery.Count;
    }

    public int CountAllUserContacts(string userId)
    {
        return db.Contacts.Count(c => c.CreatedBy == userId);
    }

    public async Task<List<string>> GetProfessionsForUserAsync(string userId)
    {

        var contacts = db.Contacts.Where(c => c.CreatedBy == userId);
        var professions = contacts.Where(c => c.Profession != null).Select(c => c.Profession ?? "");
        professions = professions.Concat(contacts.Where(c => c.Profession2 != null).Select(c => c.Profession2 ?? ""));
        professions = professions.Where(p => !String.IsNullOrEmpty(p)).Distinct();
        return await professions.ToListAsync() ?? [];
    }

    public async Task<AudiencePageResponse> GetContactsPageAsync(string userId, int index, int itemsPerPage, string orderBy, string? search = null)
    {
        AudiencePageResponse response = new();
        string sql = $"Select * FROM Contacts WHERE CreatedBy = '{userId}' ";
        if (!String.IsNullOrEmpty(search))
        {
            sql += $"AND (FirstName like '%{search}%' OR LastName like '%{search}%' OR Profession like '%{search}%' OR Profession2 like '%{search}%') ";
        }
        if (!String.IsNullOrEmpty(orderBy))
        {
            sql += $"Order By {orderBy} ";
        }
        if (!String.IsNullOrEmpty(search))
        {
            var searchResults = db.Contacts.FromSqlRaw(sql).ToList();
            response.TotalItems = searchResults.Count;
        }
        else
            response.TotalItems = db.Contacts.Count(c => c.CreatedBy == userId);

        sql += $"OFFSET {index * itemsPerPage} ROWS FETCH NEXT {itemsPerPage} ROWS ONLY;";
        var contactsQuery = db.Contacts.FromSqlRaw(sql);
        response.Contacts = await contactsQuery.ToListAsync();

        //var contacts = db.Contacts.Where(
        //    c => c.CreatedBy == userId &&
        //    (
        //        search == null ||
        //        c.FirstName.Contains(search) ||
        //        c.LastName.Contains(search) ||
        //        (c.Profession != null && c.Profession.Contains(search)) ||
        //        (c.Profession2 != null && c.Profession2.Contains(search)) ||
        //        c.ContactEmail.Contains(search)
        //    ))
        //    .OrderBy(c => c.GetType().GetProperty(orderBy)!.GetValue(c))
        //    .Skip(index * itemsPerPage)
        //    .Take(itemsPerPage);


        return response;
    }
}
