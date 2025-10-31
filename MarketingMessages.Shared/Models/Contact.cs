using System;
using System.Collections.Generic;
using MarketingMessages.Shared.DTO;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace MarketingMessages.Shared.Models;

public partial class Contact
{
    public int ContactId { get; set; }

    public string FirstName { get; set; } = "";

    public string LastName { get; set; } = "";

    public string? Title { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Zip { get; set; }

    public string? Country { get; set; }

    public string? MobilePhone { get; set; }

    public string ContactEmail { get; set; } = "";

    public string? Profession { get; set; }

    public string? Profession2 { get; set; }

    public string? Custom1 { get; set; }

    public string? Custom2 { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    [AllowNull]
    public string? ModifiedBy { get; set; }

    [AllowNull]
    public DateTime? ModifiedOn { get; set; }


}
