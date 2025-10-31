using MarketingMessages.Data;
using MarketingMessages.Shared.DTO;
using MarketingMessages.Shared.Models;

using Microsoft.AspNetCore.Identity;

using Serilog;

namespace MarketingMessages.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminLogsController : BaseController
    {
        private readonly MarketingMessagesContext _context;

        public AdminLogsController(MarketingMessagesContext context, UserManager<ApplicationUser> userManager) : base(userManager)
        {
            _context = context;
        }

        [HttpPost("application-logs")]
        public ActionResult<List<ApplicationLog>> GetApplicationLogs([FromBody] ApplicationLogRequest request)
        {
            try
            {

                // default to 1 week ago
                request.FromDate ??= DateTime.Today.AddDays(-7);
                request.ToDate ??= DateTime.Now;
                string[] logLevels = request.Level.ToString().Replace("None", "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var applicationLogs = _context.ApplicationLogs
                    .Where(
                        l => l.TimeStamp >= request.FromDate
                        && l.TimeStamp <= request.ToDate
                        && (logLevels.Length == 0 || logLevels.Contains(l.Level)))
                    .OrderBy(l => l.TimeStamp)
                    .Take(100)
                    .ToList();

                if (applicationLogs == null)
                {
                    return NotFound();
                }

                return Ok(applicationLogs);
            }
            catch (Exception ex)
            {
                Log.Error("{ex}", ex);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("sent-logs")]
        public ActionResult<List<SentLog>> GetSentLogs([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            try
            {

                // default to 1 week ago
                fromDate ??= DateTime.Today.AddDays(-7);
                toDate ??= DateTime.Now;
                var applicationLogs = _context.SentLogs.Where(l => l.DateSent >= fromDate && l.DateSent <= toDate);

                if (applicationLogs == null)
                {
                    return NotFound();
                }

                return Ok(applicationLogs.ToList());
            }
            catch (Exception ex)
            {
                Log.Error("{ex}", ex);
                return StatusCode(500, ex.Message);
            }
        }
    }
}
