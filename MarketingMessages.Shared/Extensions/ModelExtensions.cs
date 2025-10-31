using MarketingMessages.Shared.DTO;
using MarketingMessages.Shared.DTO.V1;
using MarketingMessages.Shared.Enums;
using MarketingMessages.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketingMessages.Shared.Extensions;

public static class ModelExtensions
{
    public static string ReplaceSubstitutionTags(this Content content, Contact contact)
    {
        var replacementList = content.TemplatePropertyNames.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        string result = content.HtmlContent;
        foreach (var r in replacementList)
        {
            var propertyName = r.Replace("#", "");
            result = result.Replace(r, typeof(Contact).GetProperty(propertyName)?.GetValue(contact)?.ToString());
        }
        return result;
    }
    public static ContactList ToContactList(this IEnumerable<ContactModel> contacts) =>
        new ContactList
        {
            Contacts = [.. contacts]
        };
    public static Contact ToContact(this ContactModel contactItem, string userId)
    {
        return new()
        {
            CreatedBy = userId,
            CreatedOn = DateTime.Now,
            ContactId = contactItem.ContactId,
            State = contactItem.State.ToString(),
            Title = contactItem.Title,
            ContactEmail = contactItem.ContactEmail,
            FirstName = contactItem.ContactFirstName,
            LastName = contactItem.ContactLastName,
            Address = contactItem.Address,
            Zip = contactItem.ZipCode,
            City = contactItem.City,
            Country = contactItem.Country,
            MobilePhone = contactItem.Phone
        };
    }
    public static ContactModel ToDisplayModel(this Contact contact)
    {
        return new()
        {
            ContactId = contact.ContactId,
            Address = contact.Address,
            City = contact.City,
            Country = contact.Country,
            ContactEmail = contact.ContactEmail,
            Title = contact.Title,
            Phone = contact.MobilePhone,
            ContactFirstName = contact.FirstName,
            ContactLastName = contact.LastName,
            State = !string.IsNullOrEmpty(contact.State) ? Enum.Parse<StateAbbreviation>(contact.State!) : StateAbbreviation.None,
            ZipCode = contact.Zip,
            Profession = contact.Profession,
            Profession2 = contact.Profession2,
            Custom1 = contact.Custom1,
            Custom2 = contact.Custom2,
            //ListIds = contact.Lists.Select(l => l.EmailListId).ToList()
        };
    }
    public static Sender ToSender(this EmailSender emailSender, string userId)
    {
        Sender result = new()
        {
            CreatedBy = userId,
            Email = emailSender.FromEmail,
            ReplyTo = emailSender.ReplyEmail ?? emailSender.FromEmail,
            Name = emailSender.Name,
            SenderId = emailSender.SenderId
        };
        return result;
    }

    public static Contact AsContact(this Sender sender)
    {
        return new()
        {
            ContactEmail = sender.Email,
            FirstName = sender.Name,
            CreatedBy = sender.CreatedBy,
            CreatedOn = DateTime.Now,
        };
    }

    public static HtmlEditorContent AsEditorContent(this EmailContentResponse c)
    {
        return new()
        {
            ContentId = c.ContentId,
            DocumentTitle = c.Name,
            HtmlContent = c.HtmlContent,
            Images = c.Images,
            TextContent = c.TextContent
        };

    }

    public static SuppressionGroupModel ToDisplayModel(this SuppressionGroup group)
    {
        return new()
        {
            Description = group.Description,
            Id = group.SuppressionGroupId,
            Name = group.Name,
            Unsubscribes = group.Suppressions.Select(s => s.ContactEmail).ToList(),
        };
    }

    public static EmailContentResponse ToDisplayModel(this Content content)
    {
        return new()
        {
            ContentId = content.ContentId,
            HtmlContent = content.HtmlContent,
            Name = content.Name,
            Substitions = content.TemplatePropertyNames.Split(",").ToList(),
            TextContent = content.TextContent,
        };
    }

    public static ImageUploadResponse ToDisplayModel(this ContentImage image, string baseUrl)
    {
        return new()
        {
            ImageData = image.ImageData,
            Id = image.Id.ToString(),
            Name = image.Name,
            Url = $"{baseUrl}/api/image/{image.Id}",
        };
    }


    ///// <summary>
    ///// Set's the next execution date for email job.<br />
    ///// If job is not recurring, nothing will happen.<br />
    ///// If this job should not run any longer, IE it has reached it's EndDate,<br />
    ///// the job is set to complete and the NextExecution is set to null.
    ///// </summary>
    ///// <param name="job"></param>
    //public static void SetNextExecution(this Campaign job)
    //{
    //    if (job.JobType == null || !job.JobType.Equals(EmailJobType.Recurring.ToString(), StringComparison.CurrentCultureIgnoreCase) || job.IsComplete)
    //        return;

    //    var currentDate = job.NextExecution;
    //    if (currentDate is null || currentDate.HasValue)
    //        return;

    //    static int tallyDaysForMonths(int totalDays, int month, int year, int n)
    //    {
    //        totalDays += DateTime.DaysInMonth(year, month);
    //        if (n == 0)
    //            return totalDays;
    //        month += 1;
    //        month %= 12;
    //        if (month == 0)
    //            year += 1;
    //        return tallyDaysForMonths(totalDays, month, year, n - 1);
    //    }

    //job.NextExecution = Enum.Parse<IntervalUnit>(job.FrequencyUnit) switch
    //{
    //    IntervalUnit.Hour => currentDate.Value.AddHours(job.FrequencyInterval),
    //    IntervalUnit.Day => currentDate.Value.AddDays(job.FrequencyInterval),
    //    IntervalUnit.Week => currentDate.Value.AddDays(job.FrequencyInterval * 7),
    //    IntervalUnit.Month => currentDate.Value.AddDays(tallyDaysForMonths(0, currentDate.Value.Month, currentDate.Value.Month, job.FrequencyInterval)),
    //    _ => null
    //};
    //if (job.NextExecution > job.EndDate)
    //{
    //    job.NextExecution = null;
    //    job.IsComplete = true;
    //}
    //}

}