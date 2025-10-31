using MarketingMessages.Shared.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Serilog;

namespace MarketingMessages.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        UserManager<ApplicationUser> _userManager;
        public BaseController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        protected string GetUserId()
        {
            string userId = _userManager.GetUserId(User) ?? "";
            if (string.IsNullOrEmpty(userId))
            {
                Log.Error("Unable to resolve user id in `{pathBase}/{path}", Request.PathBase, Request.Path);
                throw new Exception("Unable to resolve user id in `/api/Campaigns/");
            }
            return userId;
        }
    }
}
