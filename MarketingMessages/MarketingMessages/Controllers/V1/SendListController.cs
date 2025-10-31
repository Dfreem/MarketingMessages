using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CsvHelper;
using System.Globalization;
using System.Text.RegularExpressions;
using MarketingMessages.Repository;
using MarketingMessages.Data;
using MarketingMessages.Shared.DTO.V1;
using MarketingMessages.Shared.Models;
using MarketingMessages.Shared.DTO;
using MarketingMessages.Shared.Extensions;

namespace MarketingMessages.Controllers.V1
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public partial class SendListController : ControllerBase
    {
        IDbContextFactory<MarketingMessagesContext> _dbFactory;
        AudienceRepository _listRepo;
        EmailRepository _emailRepo;
        ContactsRepository _contactsRepo;
        ILogger<SendListController> _logger;
        UserManager<ApplicationUser> _userManager;

        public SendListController(IDbContextFactory<MarketingMessagesContext> db,
                                  ILogger<SendListController> logger,
                                  UserManager<ApplicationUser> userManager,
                                  AudienceRepository repo,
                                  EmailRepository emailRepo,
                                  ContactsRepository contactsRepo)
        {
            // TODO remove db context from controller
            _dbFactory = db;
            _listRepo = repo;
            _logger = logger;
            _userManager = userManager;
            _emailRepo = emailRepo;
            _contactsRepo = contactsRepo;
        }

        private string GetUserId()
        {
            string userId = _userManager.GetUserId(User) ?? "";
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("Unable to resolve user id in `{pathBase}/{path}", Request.PathBase, Request.Path);
                throw new Exception("Unable to resolve user id in `/api/SendList/");
            }
            return userId;
        }

        //[HttpPost("contacts")]
        //public async Task<ActionResult<AudiencePageResponse>> GetUserContactsAsync([FromBody] ContactsRequest contactRequest)
        //{
        //    try
        //    {

        //        string userId = GetUserId();
        //        var contacts = await _contactsRepo.GetUserContactsAsync(userId, contactRequest.Index, contactRequest.PageSize, contactRequest.Search ?? "");

        //        int index = contactRequest.Index + contacts.Count;
        //        return Ok(new AudiencePageResponse() { Contacts = contacts, Index = index, TotalItems = contacts.Count });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("{ex}", ex);
        //        return StatusCode(500, ex);
        //    }

        //}

        [HttpGet("senders")]
        public ActionResult<List<EmailSender>> GetUserSenders()
        {
            try
            {
                var userId = GetUserId();
                var senders = _contactsRepo.GetSenders(userId);
                return Ok(senders);
            }
            catch (Exception ex)
            {
                _logger.LogError("{ex}", ex);
                return StatusCode(500, ex);
            }
        }

        [HttpGet("form-options")]
        public ActionResult<ListFormModel> GetListFormOptions()
        {
            ListFormModel result = new();
            string userId = _userManager.GetUserId(User) ?? "";
            using var db = _dbFactory.CreateDbContext();

            var userContacts = db.Contacts.Where(c => c.CreatedBy == userId);

            var professions = userContacts.Select(l => l.Profession ?? "").Where(p => !string.IsNullOrEmpty(p));
            Regex zipcodeMatch = new("^\\d{5}(?:-\\d{4})?$");
            var zipcodes = userContacts
                .Where(c => c.Zip != null && c.CreatedBy == userId)
                .Select(c => c.Zip ?? "")
                .Distinct()
                .Order()
                .ToList();
            zipcodes = zipcodes.Where(z => zipcodeMatch.IsMatch(z)).ToList();

            professions = professions.Concat(userContacts.Select(l => l.Profession2 ?? "").Where(p => !string.IsNullOrEmpty(p)));

            var sendList = _listRepo.GetSegmentsForUser(userId);
            result.SendLists = sendList.ToDictionary(l => l.Name, l => l.Id);
            result.ProfessionDropdown = professions.Distinct().ToList();
            result.Zipcodes = zipcodes.ToDictionary(z => z, z => false);
            return result;
        }

        [HttpGet("user-lists")]
        public ActionResult<Dictionary<string, int>> GetSendListsForUser()
        {
            Dictionary<string, int> result = [];
            try
            {
                string userId = GetUserId();
                var userLists = _listRepo.GetSegmentsForUser(userId);
                var contactLists = userLists.ToDictionary(l => l.Name, l => l.Id);
                return Ok(contactLists);
            }
            catch (Exception ex)
            {
                _logger.LogError("{ex}", ex);
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPost("query-contacts")]
        public ActionResult<int> QueryContactCount([FromBody] SendListRequest request)
        {
            try
            {
                string userId = GetUserId();
                var sendListResult = _listRepo.QueryCount(request, userId);
                return Ok(sendListResult);
            }
            catch (Exception ex)
            {
                _logger.LogError("{ex}", ex);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("count-by-id/{listId}")]
        public ActionResult<int> CountByListId(int listId)
        {
            try
            {
                string userId = GetUserId();
                var sendListResult = _listRepo.QueryCount(listId);
                return Ok(sendListResult);
            }
            catch (Exception ex)
            {
                _logger.LogError("{ex}", ex);
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("professions-by-id/{listId}")]
        public ActionResult<SendListResponse> GetProfessionsByListId(int listId)
        {
            try
            {
                string userId = GetUserId();
                var sendListResult = _listRepo.ExecuteQueryForList(listId)
                    .SelectMany(l => new List<string>() { l.Profession ?? "", l.Profession2 ?? "" })
                    .Distinct()
                    .ToList();
                sendListResult.RemoveAll(p => string.IsNullOrEmpty(p));
                SendListResponse response = new();
                response.Professions = sendListResult;
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError("{ex}", ex);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("user-lists")]
        public ActionResult<int> SaveUserList([FromBody] SendListRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ListName))
                {
                    _logger.LogWarning("List Name was null while attempting to save the send list\n{request}", request.AsString());
                    return StatusCode(500, $"List Name was null while attempting to save the send list");
                }
                string userId = GetUserId();
                var sendListResult = _listRepo.SaveSendList(request, userId);
                return Ok(sendListResult);
            }
            catch (Exception ex)
            {
                _logger.LogError("{ex}", ex);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("delete/{listId}")]
        public async Task<IActionResult> DeleteListAsync(int listId)
        {
            try
            {

                using var db = _dbFactory.CreateDbContext();
                var list = await db.Audiences.FindAsync(listId);
                if (list is not null)
                {
                    db.Audiences.Remove(list);
                    db.SaveChanges();
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
    }
}
