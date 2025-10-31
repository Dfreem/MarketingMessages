
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MarketingMessages.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : BaseController
    {
        MarketingMessagesContext _db;
        public ImageController(UserManager<ApplicationUser> userManager, IDbContextFactory<MarketingMessagesContext> ctx) : base(userManager)
        {
            _db = ctx.CreateDbContext();
        }

        [HttpGet("{imageId}")]
        public async Task<IActionResult> GetImageByIdAsync(int imageId)
        {

            // TODO impliment a user check to ensure that a user cannot access other users images
            var image = await _db.Images.FirstOrDefaultAsync(i => i.Id == imageId);
            if (image is null)
                return NotFound();
            return File(image.ImageData, "image/png");
        }

    }
}
