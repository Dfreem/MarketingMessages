using AngleSharp.Dom;

using MarketingMessages.Shared.Enums;
using MarketingMessages.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketingMessages.Shared.DTO;

public class ContactModel
{
    public int ContactId { get; set; }
    public string ContactFirstName { get; set; } = "";
    public string ContactLastName { get; set; } = "";
    public string ContactEmail { get; set; } = "";
    public string? Title { get; set; }
    public string? Address { get; set; }
    public StateAbbreviation State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    public string? Phone { get; set; }
    public string? City { get; set; }
    public string? Profession { get; set; }
    public string? Custom1 { get; set; }
    public string? Custom2 { get; set; }
    public string? Profession2 { get; set; }
    public bool IsChecked { get; set; }

    public List<int> ListIds { get; set; } = [];

    public ContactModel(Contact contact)
    {
        ContactId = contact.ContactId;
        ContactFirstName = contact.FirstName;
        ContactEmail = contact.ContactEmail;
        ContactLastName = contact.LastName;
        Title = contact.Title;
        Address = contact.Address;
        if(!string.IsNullOrEmpty(contact.State))
            State = Enum.Parse<StateAbbreviation>(contact.State);
        City = contact.City;
        ZipCode = contact.Zip;
        Country = contact.Country;
        Phone = contact.MobilePhone;
        Profession = contact.Profession;
        Profession2 = contact.Profession2;
        Custom1 = contact.Custom1;
        Custom2 = contact.Custom2;
    }
    public ContactModel()
    {

    }

    public ContactModel Clone()
    {
        return (ContactModel)MemberwiseClone();
    }
}
