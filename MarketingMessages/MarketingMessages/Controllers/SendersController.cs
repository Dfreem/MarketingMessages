using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MarketingMessages.Data;
using MarketingMessages.Shared.DTO;
using MarketingMessages.Shared.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Sender = MarketingMessages.Shared.Models.Sender;

namespace MarketingMessages.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SendersController : ControllerBase
{
    private readonly MarketingMessagesContext _context;
    private UserManager<ApplicationUser> _userManager;
    private ILogger<SendersController> _logger;

    public SendersController(MarketingMessagesContext context, UserManager<ApplicationUser> userManager, ILogger<SendersController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: api/Senders
    [HttpGet]
    public async Task<ActionResult<SenderModel>> GetSenders()
    {
        string userId = _userManager.GetUserId(User) ?? throw new ArgumentNullException("Unable to find user");
        var result = await _context.Senders.FirstOrDefaultAsync(s => s.CreatedBy == userId);
        if (result == null)
            return StatusCode(500, "Unable to find sender for current user.");

        return Ok(result);
    }

    // GET: api/Senders/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Sender>> GetSender(int id)
    {
        var sender = await _context.Senders.FindAsync(id);

        if (sender == null)
        {
            return NotFound();
        }

        return sender;
    }

    //// PUT: api/Senders/5
    //// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    //[HttpPut("{id}")]
    //public async Task<IActionResult> PutSender(int id, Sender sender)
    //{
    //    if (id != sender.SenderId)
    //    {
    //        return BadRequest();
    //    }

    //    _context.Entry(sender).State = EntityState.Modified;

    //    try
    //    {
    //        await _context.SaveChangesAsync();
    //    }
    //    catch (DbUpdateConcurrencyException)
    //    {
    //        if (!SenderExists(id))
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

    //// POST: api/Senders
    //// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    //[HttpPost]
    //public async Task<ActionResult<Sender>> PostSender(Sender sender)
    //{
    //    _context.Senders.Add(sender);
    //    await _context.SaveChangesAsync();

    //    return CreatedAtAction("GetSender", new { id = sender.SenderId }, sender);
    //}

    //// DELETE: api/Senders/5
    //[HttpDelete("{id}")]
    //public async Task<IActionResult> DeleteSender(int id)
    //{
    //    var sender = await _context.Senders.FindAsync(id);
    //    if (sender == null)
    //    {
    //        return NotFound();
    //    }

    //    _context.Senders.Remove(sender);
    //    await _context.SaveChangesAsync();

    //    return NoContent();
    //}

    private bool SenderExists(int id)
    {
        return _context.Senders.Any(e => e.SenderId == id);
    }
}
