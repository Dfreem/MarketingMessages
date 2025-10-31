using MarketingMessages.Shared.Models;

namespace MarketingMessages.Shared.DTO;

public class SuccessResponse
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? Message { get; set; }
}

public class SuccessResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
}

public class SendListResponse
{
    public List<string> Professions { get; set; } = [];
}

public class EmailContentResponse
{
    public string Name { get; set; } = default!;
    public string Subject { get; set; } = "";
    public string HtmlContent { get; set; } = default!;
    public string TextContent { get; set; } = default!;
    public List<string> Substitions { get; set; } = [];
    public Dictionary<string, string> Images { get; set; } = [];
    public int ContentId { get; set; }

}
public class AudiencePageResponse
{
    public int Index { get; set; }
    public List<Contact> Contacts { get; set; } = [];
    public int TotalItems { get; set; }
}

public class SaveContactsUploadResponse
{
    public List<Contact> DuplicateEmails { get; set; } = [];
    public int TotalUploaded { get; set; }

}
public class ImageUploadResponse
{
    public byte[] ImageData { get; set; } = [];
    public string Id { get; set; } = default!;
    public string Url { get; set; } = default!;
    public string Name { get; set; } = default!;

}

