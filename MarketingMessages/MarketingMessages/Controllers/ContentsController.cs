using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Ganss.Xss;

using MarketingMessages.Data;
using MarketingMessages.Shared.DTO;
using MarketingMessages.Shared.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketingMessages.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ContentsController : ControllerBase
    {
        private readonly MarketingMessagesContext _db;
        ILogger<ContentsController> _logger;
        IConfiguration _config;
        UserManager<ApplicationUser> _userManager;
        public ContentsController(MarketingMessagesContext context, UserManager<ApplicationUser> userManager, ILogger<ContentsController> logger, IConfiguration config)
        {
            _db = context;
            _userManager = userManager;
            _logger = logger;
            _config = config;
        }


        private string GetUserId()
        {
            string userId = _userManager.GetUserId(User) ?? "";
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("Unable to resolve user id in `{pathBase}/{path}", Request.PathBase, Request.Path);
                throw new Exception("Unable to resolve user id in `/api/Contents/");
            }
            return userId;
        }

        [HttpGet]
        public async Task<ActionResult<List<EmailContentResponse>>> GetUserContentsAsync()
        {
            try
            {
                var userId = GetUserId();
                if (userId is null)
                {
                    _logger.LogError("Unable resolve user while attempting to update contact in EmailController.SaveEmailContentAsync");
                    return StatusCode(500, "Unable resolve user while attempting to update contact in EmailController.SaveEmailContentAsync");
                }
                var results = await _db.Contents
                    .Include(c => c.Images)
                    .ThenInclude(i => i.Image)
                    .Where(c => c.CreatedBy == userId && !c.Deleted).ToListAsync();
                List<EmailContentResponse> response = results.Select(r => new EmailContentResponse()
                {
                    Name = r.Name,
                    HtmlContent = r.HtmlContent,
                    TextContent = r.TextContent,
                    Substitions = r.TemplatePropertyNames.Split(",").ToList(),
                    ContentId = r.ContentId,
                    Images = r.Images.ToDictionary(i => i.Image.Name, i => Convert.ToBase64String(i.Image.ImageData)),
                }).ToList();
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError("{ex}", ex);
                return StatusCode(500, ex);
            }

        }

        // GET: api/Contents/5
        [HttpGet("by-id/{id}")]
        public async Task<ActionResult<EmailContentResponse>> GetContent(int id)
        {
            var userId = GetUserId();
            var content = await _db.Contents
                .Include(c => c.Images)
                .ThenInclude(i => i.Image)
                .FirstOrDefaultAsync(c => c.ContentId == id && c.CreatedBy == userId && !c.Deleted);

            if (content == null)
            {
                return NotFound();
            }

            var result = content.ToDisplayModel();
            result.Subject = _db.Campaigns.FirstOrDefault(c => c.EmailContentId == content.ContentId)?.Subject ?? "";
            return result;
        }

        [AllowAnonymous]
        [HttpGet("image/{imageId}")]
        public async Task<IActionResult> GetImageByIdAsync(int imageId)
        {

            // TODO impliment a user check to ensure that a user cannot access other users images
            var image = await _db.Images.FirstOrDefaultAsync(i => i.Id == imageId);
            if (image is null)
                return NotFound();
            return File(image.ImageData, "image/png");
        }

        /// <summary>
        /// Upload image from base64.
        /// </summary>
        /// <param name="name">the filename of the image</param>
        /// <param name="data">the image data in base64 encoding</param>
        /// <returns></returns>
        [HttpPost("upload-image/{name}")]
        public async Task<ActionResult<ImageUploadResponse>> UploadImageAsync(string name, [FromBody] string data)
        {
            // TODO impliment some kind of security on this endpoint
            try
            {

                var userId = GetUserId();
                if (data.Contains(","))
                    data = data.Split(',')[^1];
                ContentImage image = new()
                {
                    CreatedBy = userId,
                    CreatedOn = DateTime.Now,
                    ImageData = Convert.FromBase64String(data),
                    Name = name,
                };
                await _db.Images.AddAsync(image);
                _db.SaveChanges();
                string baseUrl = _config["HttpBaseUrl"] ?? HttpContext.Request.Path;
                if (baseUrl.EndsWith('/')) baseUrl = baseUrl[..^1];
                ImageUploadResponse response = image.ToDisplayModel(baseUrl);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ex}", ex);
                return StatusCode(500, ex);
            }
        }

        // POST: api/Contents
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("update-content")]
        public async Task<ActionResult> UpdateContentAsync(EmailRequest email)
        {
            try
            {

                var userId = GetUserId();
                var contents = _db.Contents.Include(c => c.Images)
                    .ThenInclude(i => i.Image)
                    .FirstOrDefault(i => i.ContentId == email.EmailContentId);
                if (contents == null)
                {
                    return BadRequest($"Unable to resolve content with id {email.EmailContentId}");
                }
                contents.HtmlContent = email.HtmlContent;
                contents.TextContent = email.TextContent;
                contents.ModifiedBy = userId;
                contents.ModifiedOn = DateTime.Now;
                contents.Name = email.ContentName;
                contents.TemplatePropertyNames = string.Join(',', email.TemplateVariableNames);
                await _db.SaveChangesAsync();
                //EmailContentResponse response = new()
                //{
                //    HtmlContent = contents.HtmlContent,
                //    TextContent = contents.TextContent,
                //    ContentId = contents.ContentId,
                //    Name = contents.Name,
                //    Substitions = content.TemplateVariableNames,
                //    Images = contents.Images.ToDictionary(i => i.Image.Name, i => Convert.ToBase64String(i.Image.ImageData))
                //};
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while attempting to update content.\n{ex}", ex);
                return StatusCode(500, ex);
            }
        }

        [HttpPost("save-content")]
        public async Task<IActionResult> SaveEmailContentAsync([FromBody] EmailRequest emailRequest)
        {
            List<string> errors = [];
            try
            {
                var userId = _userManager.GetUserId(User);
                if (userId is null)
                {
                    _logger.LogError("Unable resolve user while attempting to update contact in EmailController.SaveEmailContentAsync");
                    return StatusCode(500, "Unable resolve user while attempting to update contact in EmailController.SaveEmailContentAsync");
                }

                // TODO configure this
                //HtmlSanitizer sanitizer = new();
                //sanitizer.AllowedSchemes.Add("data");
                //var sanitizedContent = sanitizer.Sanitize(emailRequest.HtmlContent);
                var userImages = _db.Images.Where(i => i.CreatedBy == userId);
                var needsNewName = emailRequest.Images.Where(i => userImages.Any(m => m.Name == i.Name)).ToList();
                if (needsNewName is not null && needsNewName.Count != 0)
                {
                    foreach (var image in needsNewName)
                    {
                        int suffix = 0;
                        var newName = "";
                        do
                        {
                            suffix += 1;
                            image.Name = image.Name.Insert(image.Name.IndexOf('.'), $"({suffix})");
                        } while (userImages.Any(i => i.Name == newName) && suffix < 10);
                        if (suffix >= 9)
                        {
                            emailRequest.Images.Remove(image);
                            emailRequest.Errors.Add($"Unable to save the following image due to too many duplicate names\n{image.Name}");
                        }
                    }
                }
                emailRequest.Images.Where(i => i.Id == 0).ToList()
                    .ForEach(i =>
                    {
                        i.CreatedOn = DateTime.Now;
                        i.CreatedBy = userId;
                    });
                _db.Images.UpdateRange(emailRequest.Images);
                Content? emailContent = await _db.Contents.FirstOrDefaultAsync(c => c.ContentId == emailRequest.EmailContentId);
                if (emailContent is not null)
                {
                    emailContent.HtmlContent = emailRequest.HtmlContent;
                    emailContent.TextContent = emailRequest.TextContent;
                    emailContent.Name = emailRequest.ContentName;
                    emailContent.ModifiedBy = userId;
                    emailContent.ModifiedOn = DateTime.Now;
                    emailContent.TemplatePropertyNames = string.Join(',', emailRequest.TemplateVariableNames);
                    emailContent.Images.AddRange(emailRequest.Images.Select(i => new EmailContentImages() { EmailContent = emailContent, Image = i }));
                    _db.SaveChanges();
                }
                else
                {
                    emailContent = new()
                    {
                        HtmlContent = emailRequest.HtmlContent,
                        TextContent = emailRequest.TextContent,
                        Name = emailRequest.ContentName,
                        CreatedBy = userId,
                        CreatedOn = DateTime.Now,
                        TemplatePropertyNames = string.Join(',', emailRequest.TemplateVariableNames),
                    };
                    _db.Contents.Add(emailContent);
                    _db.SaveChanges();
                    emailContent.Images = emailRequest.Images.Select(i => new EmailContentImages() { EmailContent = emailContent, Image = i }).ToList();
                    _db.SaveChanges();

                }
                return Ok(emailContent?.ContentId);
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while saving email content\n{ex}", ex);
                return StatusCode(500, ex.Message);
            }

        }

        // DELETE: api/Contents/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContent(int id)
        {
            var userId = GetUserId();
            var content = await _db.Contents.FirstOrDefaultAsync(c => c.ContentId == id);
            if (content == null || content.CreatedBy != userId)
            {
                return NotFound();
            }
            if (_db.Campaigns.Any(c => c.EmailContentId == content.ContentId && c.IsEnabled && !c.IsComplete && c.IsStarted))
            {
                string message = "Unable to delete content because it belong to a currently active campaign.";
                _logger.LogError(message);
                return BadRequest(message);
            }

            //_db.Contents.Remove(content);
            content.Deleted = true;
            await _db.SaveChangesAsync();

            return NoContent();
        }

        private bool ContentExists(int id)
        {
            return _db.Contents.Any(e => e.ContentId == id);
        }
    }
}
