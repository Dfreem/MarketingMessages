using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MarketingMessages.Data;
using MarketingMessages.Shared.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketingMessages.Controllers
{
    [Authorize("Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationSettingsController : ControllerBase
    {
        private readonly MarketingMessagesContext _context;
        private ILogger<ApplicationSettingsController> _logger;

        public ApplicationSettingsController(IDbContextFactory<MarketingMessagesContext> factory, ILogger<ApplicationSettingsController> logger)
        {
            _context = factory.CreateDbContext();
            _logger = logger;
        }

        // GET: api/ApplicationSettings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApplicationSetting>>> GetSettings()
        {
            return await _context.Settings.ToListAsync();
        }

        // GET: api/ApplicationSettings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApplicationSetting>> GetApplicationSetting(int id)
        {
            var applicationSetting = await _context.Settings.FindAsync(id);

            if (applicationSetting == null)
            {
                return NotFound();
            }

            return applicationSetting;
        }

        [HttpPost("update")]
        public ActionResult<ApplicationSetting> Update([FromBody] List<ApplicationSetting> setting)
        {
            try
            {
                _context.Settings.UpdateRange(setting);
                _context.SaveChanges();
                return Ok(setting);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        //// PUT: api/ApplicationSettings/5
        //// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutApplicationSetting(int id, ApplicationSetting applicationSetting)
        //{
        //    if (id != applicationSetting.Id)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(applicationSetting).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!ApplicationSettingExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        // POST: api/ApplicationSettings
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ApplicationSetting>> PostApplicationSetting(ApplicationSetting applicationSetting)
        {
            _context.Settings.Add(applicationSetting);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetApplicationSetting", new { id = applicationSetting.Id }, applicationSetting);
        }

        // DELETE: api/ApplicationSettings/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApplicationSetting(int id)
        {
            var applicationSetting = await _context.Settings.FindAsync(id);
            if (applicationSetting == null)
            {
                return NotFound();
            }

            _context.Settings.Remove(applicationSetting);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ApplicationSettingExists(int id)
        {
            return _context.Settings.Any(e => e.Id == id);
        }
    }
}
