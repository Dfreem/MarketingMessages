using MarketingMessages.Shared.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using Serilog;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sender = MarketingMessages.Shared.Models.Sender;

namespace MarketingMessages.Data;

public static class SeedData
{
    public static async Task Seed(MarketingMessagesContext ctx, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        //ctx.Database.Migrate();
        bool roleExists = await roleManager.RoleExistsAsync("Admin");
        if (!roleExists)
            await roleManager.CreateAsync(new() { Name = "Admin" });
        var admin = await SeedAdminUser(ctx, userManager, "dfreem987@gmail.com", "!BassCase987");
        if (admin != null)
        {
            if (!ctx.Senders.Any(s => s.CreatedBy == admin.Id))
            {
                Sender sender = new()
                {
                    Name = "Devin",
                    CreatedBy = admin!.Id,
                    Email = admin.Email ?? admin.UserName!,
                    ReplyTo = "devin@devinfreeman.site"
                };
                ctx.Senders.Add(sender);
                ctx.SaveChanges();
            }
        }

        ApplicationLog seedLog = new() { Level = LogLevel.Information.ToString(), Message = "Seeding database" };
        ctx.ApplicationLogs.Add(seedLog);
    }

    public static void SeedSettings(MarketingMessagesContext ctx)
    {

        if (!ctx.Settings.Any(s => s.SettingName == "Log Rollover Interval (days)"))
        {
            ctx.Settings.Add(new()
            {
                SettingName = "Log Rollover Interval (days)",
                SettingValue = "10"

            });
            ctx.SaveChanges();
        }
    }

    private static async Task<ApplicationUser?> SeedAdminUser(MarketingMessagesContext ctx, UserManager<ApplicationUser> userManager, string username, string password)
    {

        var admin = await userManager.FindByEmailAsync(username);
        if (admin is null)
        {
            admin = new()
            {
                UserName = username,
                Email = username,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(admin, password);
            if (!result.Succeeded)
            {
                Log.Error("Cannot create seed user");
                return null;
            }
            else
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }

        bool hasRole = await userManager.IsInRoleAsync(admin, "Admin");
        if (!hasRole)
            await userManager.AddToRoleAsync(admin, "Admin");

        ctx.SaveChanges();
        return admin;
    }
}
