using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MarketingMessages.Services;
using MarketingMessages.Shared.DTO;
using MarketingMessages.Shared.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using SendGrid;

using Serilog;

using MarketingMessages.Data;
using MarketingMessages.Shared.Extensions;

namespace MarketingMessages.Controllers;
[Route("api/[controller]")]
[ApiController]
public class UnsubscribeGroupsController : BaseController
{

    SuppressionGroupService _unsubscribeGroupService;

    public UnsubscribeGroupsController(SuppressionGroupService unsubscribeGroupServie, UserManager<ApplicationUser> userManager) : base(userManager)
    {
        _unsubscribeGroupService = unsubscribeGroupServie;
    }

    // GET: api/UnsubscribeGroups
    [HttpGet]
    public async Task<ActionResult<List<SuppressionGroupModel>>> GetUnsubscribeGroups()
    {
        try
        {
            var userId = GetUserId();
            var groups = await _unsubscribeGroupService.GetUserSuppressionGroupsAsync(userId);
            return Ok(groups);
        }
        catch (Exception ex)
        {
            Log.Error("An error occurred while getting Unsubscribe groups from SendGrid", ex);
            return StatusCode(500, ex);
        }

    }

    [HttpGet("by-audience/{id}")]
    public ActionResult<SuppressionGroupModel> GetUnsubscribeGroupByAudienceId(int audienceId)
    {
        var unsubscribeGroup = _unsubscribeGroupService.GetGroupByAudienceId(audienceId);

        if (unsubscribeGroup == null)
        {
            return NotFound();
        }

        SuppressionGroupModel model = unsubscribeGroup.ToDisplayModel();
        return Ok(model);
    }

    //// PUT: api/UnsubscribeGroups/5
    //// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    //[HttpPut("{id}")]
    //public async Task<IActionResult> PutUnsubscribeGroup(int id, UnsubscribeGroup unsubscribeGroup)
    //{
    //    if (id != unsubscribeGroup.Id)
    //    {
    //        return BadRequest();
    //    }

    //    _context.Entry(unsubscribeGroup).State = EntityState.Modified;

    //    try
    //    {
    //        await _context.SaveChangesAsync();
    //    }
    //    catch (DbUpdateConcurrencyException)
    //    {
    //        if (!UnsubscribeGroupExists(id))
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

    //// POST: api/UnsubscribeGroups
    //// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    //[HttpPost]
    //public async Task<ActionResult<UnsubscribeGroup>> PostUnsubscribeGroup(UnsubscribeGroup unsubscribeGroup)
    //{
    //    _context.UnsubscribeGroups.Add(unsubscribeGroup);
    //    await _context.SaveChangesAsync();

    //    return CreatedAtAction("GetUnsubscribeGroup", new { id = unsubscribeGroup.Id }, unsubscribeGroup);
    //}

    //// DELETE: api/UnsubscribeGroups/5
    //[HttpDelete("{id}")]
    //public async Task<IActionResult> DeleteUnsubscribeGroup(int id)
    //{
    //    var unsubscribeGroup = await _context.UnsubscribeGroups.FindAsync(id);
    //    if (unsubscribeGroup == null)
    //    {
    //        return NotFound();
    //    }

    //    _context.UnsubscribeGroups.Remove(unsubscribeGroup);
    //    await _context.SaveChangesAsync();

    //    return NoContent();
    //}

    //private bool UnsubscribeGroupExists(int id)
    //{
    //    return _context.UnsubscribeGroups.Any(e => e.Id == id);
    //}
}
