using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using CsvHelper;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using NuGet.Packaging;

using Serilog;

namespace TrunkMonkey.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AudiencesController : ControllerBase
    {
        private readonly MarketingMessagesContext _context;
        private UserManager<ApplicationUser> _userManager;
        private ILogger<AudiencesController> _logger;
        private ContactsRepository _contactsRepo;
        private AudienceRepository _audienceRepo;
        private SuppressionGroupService _unsubscribeGroupService;

        public AudiencesController(MarketingMessagesContext context,
                                   ContactsRepository contactsRepo,
                                   UserManager<ApplicationUser> userManager,
                                   ILogger<AudiencesController> logger,
                                   SuppressionGroupService unsubscribeGroupService,
                                   AudienceRepository audienceRepo)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _contactsRepo = contactsRepo;
            _audienceRepo = audienceRepo;
            _unsubscribeGroupService = unsubscribeGroupService;
        }

        private string GetUserId()
        {
            string userId = _userManager.GetUserId(User) ?? "";
            if (String.IsNullOrEmpty(userId))
            {
                _logger.LogError("Unable to resolve user id in `{pathBase}/{path}", Request.PathBase, Request.Path);
                throw new Exception("Unable to resolve user id in `/api/Audiences");
            }
            return userId;
        }

        [HttpGet]
        public async Task<ActionResult<List<AudienceSegment>>> GetUserAudiencesAsync()
        {
            var userId = GetUserId();
            var audiences = _audienceRepo.GetSegmentsForUser(userId);
            var unsubscribeGroups = await _unsubscribeGroupService.GetUserSuppressionGroupsAsync(userId);
            var results = audiences.Select(a => new AudienceSegment()
            {
                Audience = a,
                UnsubscribeGroup = unsubscribeGroups.FirstOrDefault(u => u.SuppressionGroupId == a.SuppressionGroupId)?.ToDisplayModel(),
                ContactCount = _audienceRepo.QueryCount(a.Id)
            }).ToList();
            return Ok(results);
        }


        [HttpGet("count/{segmentId?}")]
        public ActionResult<int> GetContactCount(int? segmentId = null)
        {
            try
            {
                string userId = GetUserId();
                int result = 0;
                if (segmentId != null)
                    result = _audienceRepo.QueryCount(segmentId.Value);
                else
                    result = _audienceRepo.CountAllUserContacts(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("{ex}", ex);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("page-contacts")]
        public async Task<ActionResult<AudiencePageResponse>> GetContactsPageAsync(ContactsRequest request)
        {
            try
            {

                string userId = GetUserId();
                var result = await _audienceRepo.GetContactsPageAsync(userId, request.Index, request.PageSize, request.SortBy ?? "FirstName", request.Search);
                var totalItems = _audienceRepo.CountAllUserContacts(userId);
                result.Index = request.Index;
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("{ex}", ex);
                return StatusCode(500, ex);
            }
        }

        [HttpPost("count")]
        public ActionResult<int> GetContactCount([FromBody] AudienceFormModel model)
        {
            try
            {
                string userId = GetUserId();
                int result = _audienceRepo.QueryCount(model.Rows, model.AnyOrAll.ToString(), userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("{ex}", ex);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("segment-contacts/{segmentId}")]
        public ActionResult<Contact> GetSegmentContactsAsync(int segmentId)
        {
            try
            {
                var sendListResult = _audienceRepo.ExecuteQueryForList(segmentId);
                return Ok(sendListResult);
            }
            catch (Exception ex)
            {
                _logger.LogError("{ex}", ex);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("total-contacts")]
        public async Task<ActionResult<int>> GetUserTotalContactsAsync()
        {
            try
            {

                var result = await _contactsRepo.CountAllContacts(GetUserId());
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while counting all contacts for current user.");
                return StatusCode(500, ex);
            }
        }

        [HttpGet("segments")]
        public async Task<ActionResult<IEnumerable<AudienceFormModel>>> GetSegmentsAsync()
        {
            var userId = GetUserId();
            try
            {

                var unsubscribeGroups = await _unsubscribeGroupService.GetUserSuppressionGroupsAsync(userId);
                var audiences = await _context.Audiences.Where(a => a.CreatedBy == userId).ToListAsync();
                var result = audiences.Select(a => JsonSerializer.Deserialize<AudienceFormModel>(a.JSONForm)).ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("{ex}", ex);
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("segment-form/{segmentId}")]
        public ActionResult<AudienceFormModel> GetSegmentForEditing(int segmentId)
        {
            try
            {
                var result = _audienceRepo.GetAudienceForEditing(segmentId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while getting segment as editable form.\n{ex}", ex);
                return BadRequest(ex);
            }
        }

        [HttpGet("professions")]
        public async Task<ActionResult<List<string>>> GetFormOptionsAsync()
        {
            try
            {

                var userId = GetUserId();
                var professions = await _audienceRepo.GetProfessionsForUserAsync(userId);
                return Ok(professions);
            }
            catch (Exception ex)
            {
                _logger.LogError("{ex}", ex);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("save")]
        public async Task<ActionResult> SaveAudienceAsync([FromBody] AudienceFormModel audience)
        {
            try
            {
                var userId = GetUserId();
                await _audienceRepo.SaveAudienceAsync(audience, userId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while attempting to save new audience\n{ex}", ex);
                return StatusCode(500, ex);
            }
        }

        [HttpPut("update")]
        public ActionResult<Audience> UpdateAudience([FromBody] Audience audience)
        {
            try
            {
                var result = _audienceRepo.UpdateAudience(audience);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("{ex}", ex);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("delete-audience/{listId}")]
        public async Task<IActionResult> DeleteListAsync(int listId)
        {
            try
            {
                var list = await _context.Audiences.FindAsync(listId);
                if (list is not null)
                {
                    _context.Audiences.Remove(list);
                    _context.SaveChanges();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError("{ex}", ex);
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("remove-contacts")]
        public IActionResult DeleteContactsAsync([FromBody] List<int> contactIds)
        {
            var userId = GetUserId();
            var success = _contactsRepo.DeleteContacts(contactIds, userId);
            if (success.Success)
            {
                return Ok();
            }
            return StatusCode(500, success.Message);
        }

        [HttpGet("remove-contacts")]
        public IActionResult DeleteAllContactsAsync()
        {
            var userId = GetUserId();
            var success = _contactsRepo.DeleteContacts([], userId);
            if (success.Success)
            {
                return Ok();
            }
            return StatusCode(500, success.Message);
        }


        [HttpPost("upload")]
        public async Task<ActionResult<CsvContactsUpload>> Upload([FromForm] IFormFile csv)
        {
            try
            {
                CsvContactsUpload result = new();
                var userId = GetUserId();
                using StreamReader sr = new(csv.OpenReadStream());
                using CsvReader reader = new(sr, CultureInfo.InvariantCulture);
                await reader.ReadAsync();
                try
                {
                    reader.ReadHeader();
                    result.CsvHeader = reader.HeaderRecord ?? [];
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Csv does not have a header\n{ex}", ex);

                }
                CsvContactsUpload upload = new();
                while (await reader.ReadAsync())
                {
                    string[] row = [];
                    for (int i = 0; i < reader.ColumnCount; i++)
                    {
                        var fieldValue = reader.GetField<string>(i);
                        if (fieldValue is not null)
                            row = row.Append(fieldValue).ToArray();
                        else
                            row = row.Append("").ToArray();
                    }
                    result.CsvRows = result.CsvRows.Append(row).ToArray();
                }
                result.CsvRows = result.CsvRows.Where(t => t != null).ToArray();

                var csvFieldTitles = Enum.GetValues<CsvFields>();
                for (int i = 0; i < result.CsvHeader.Length; i++)
                {
                    var header = result.CsvHeader[i].Replace(" ", "").Replace(":", "").ToLower();
                    var preMatch = csvFieldTitles.FirstOrDefault(t => t.ToString().ToLower() == header);
                    if (preMatch != CsvFields.None)
                    {
                        if (!result.FieldMap.TryAdd(result.CsvHeader[i], preMatch))
                        {
                            _logger.LogWarning("Overwriting previously added value in FieldMap Key: {key}, Value: {value}", preMatch.ToString(), header);
                            result.FieldMap[header] = preMatch;
                        }

                    }
                    else
                    {
                        _logger.LogWarning("Unable to parse header for {header}", header);
                        if (!result.FieldMap.TryAdd(result.CsvHeader[i], CsvFields.None))
                            result.FieldMap[result.CsvHeader[i]] = CsvFields.None;
                    }
                }
                result.ListName = csv.FileName;
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while uploading contact list\n{ex}", ex);
                return StatusCode(500, ex);
            }
        }

        [HttpPost("save-upload")]
        public async Task<ActionResult<SaveContactsUploadResponse>> Save([FromBody] CsvContactsUpload contactsUpload, CancellationToken cancel)
        {
            SaveContactsUploadResponse response = new();
            Dictionary<string, Contact> contacts = [];
            List<Contact> duplicates = [];
            try
            {
                var userId = GetUserId();

                for (int i = 0; i < contactsUpload.CsvRows.Length; i++)
                {
                    if (cancel.IsCancellationRequested) return StatusCode(499, "Canceled");
                    var row = contactsUpload.CsvRows[i];
                    Contact contact = new()
                    {
                        CreatedBy = userId,
                        CreatedOn = DateTime.Now
                    };
                    for (int x = 0; x < contactsUpload.CsvHeader.Length; x++)
                    {

                        var key = contactsUpload.CsvHeader[x];
                        var headerFieldTitle = contactsUpload.FieldMap[key];
                        var fieldValue = row[x];
                        switch (headerFieldTitle)
                        {
                            case CsvFields.Title:
                                contact.Title = fieldValue;
                                break;
                            case CsvFields.FirstName:
                                contact.FirstName = fieldValue;
                                break;
                            case CsvFields.LastName:
                                contact.LastName = fieldValue;
                                break;
                            case CsvFields.ZipCode:
                                contact.Zip = fieldValue;
                                break;
                            case CsvFields.City:
                                contact.City = fieldValue;
                                break;
                            case CsvFields.State:
                                contact.State = fieldValue;
                                break;
                            case CsvFields.Country:
                                contact.Country = fieldValue;
                                break;
                            case CsvFields.Address:
                                contact.Address = fieldValue;
                                break;
                            case CsvFields.Email:
                                contact.ContactEmail = fieldValue;
                                break;
                            case CsvFields.Profession:
                                contact.Profession = fieldValue;
                                break;
                            case CsvFields.Profession2:
                                contact.Profession2 = fieldValue;
                                break;
                            case CsvFields.Custom1:
                                contact.Custom1 = fieldValue;
                                break;
                            case CsvFields.Custom2:
                                contact.Custom2 = fieldValue;
                                break;
                            default:
                                break;
                        }
                    }
                    if (!contacts.TryAdd(contact.ContactEmail, contact))
                    {
                        duplicates.Add(contact);
                    }
                }
                var contactValues = contacts.Values.ToList();
                var contactEmails = contacts.Keys.ToList();

                // find existing email address's for this user
                var existing = _context.Contacts.Where(c => c.CreatedBy == userId && contactEmails.Contains(c.ContactEmail));
                var existingEmails = existing.Select(c => c.ContactEmail).ToList();
                duplicates.AddRange(existing);

                // filter existing contact emails out of the list to be uploaded
                contactValues = contactValues.Where(v => !existingEmails.Contains(v.ContactEmail)).ToList();
                await _contactsRepo.SaveContacts(contactValues);
                //return StatusCode(417, "There were duplicate emails found, please resolve duplicate emails and resend.");
                response.DuplicateEmails = duplicates;
                response.TotalUploaded = contacts.Count;
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while uploading contact list\n{ex}", ex);
                return StatusCode(500, ex);
            }
        }

    }

}
