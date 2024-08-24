using Microsoft.EntityFrameworkCore;
using FileSharing.Data;
using Microsoft.AspNetCore.Identity;
using FileSharing.Models;
using FileSharing.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity.UI.Services;
using FileSharing.Interfaces;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Azure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace FileSharing
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Key Vault
            var vaultUri = builder.Configuration["AzureKeyVaultUri"];
            if (!string.IsNullOrEmpty(vaultUri))
            {
                var azureCredential = new DefaultAzureCredential(includeInteractiveCredentials: true);
                builder.Configuration.AddAzureKeyVault(new Uri(vaultUri), azureCredential);
            }

            // Use Google OAuth
//             builder.Services.AddAuthentication(options =>
//             {
//                 options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//                 options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
//             })
//             .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
//                 {
//                 options.LoginPath = "/Account/Login";
//                 options.LogoutPath = "/Account/Logout";
//                 options.ExpireTimeSpan = TimeSpan.FromDays(14);
//                 options.SlidingExpiration = true;
//             })
//             .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
//             {
//                 options.ClientId = builder.Configuration["GoogleClientId"] ?? throw new InvalidOperationException("Google Client ID not valid");
//                 options.ClientSecret = builder.Configuration["GoogleClientSecret"] ?? throw new InvalidOperationException("Google Client Secret not valid");
//                 options.ClaimActions.MapJsonKey(System.Security.Claims.ClaimTypes.Name, "email");
//                 options.ClaimActions.MapJsonKey(System.Security.Claims.ClaimTypes.Email, "email");
//             });

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            string connectionString = builder.Configuration["DefaultConnection"] ?? "";
            builder.Services.AddDbContext<ApplicationDBContext>(
                options => options.UseSqlServer(connectionString)
            );

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDBContext>()
                .AddDefaultTokenProviders();

            // Data tokens like reset password expire after 1 hour. 
            builder.Services.Configure<DataProtectionTokenProviderOptions>(o =>
                o.TokenLifespan = TimeSpan.FromHours(1));

            // Dependency Injections
            builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.AddTransient<IRenderService, ViewRenderService>();
            builder.Services.AddTransient<IFileService, FileService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
