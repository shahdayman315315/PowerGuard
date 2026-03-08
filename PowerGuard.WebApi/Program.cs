using Microsoft.AspNetCore.Identity;
using PowerGuard.Domain.Models;
using PowerGuard.Infrastructure.Data;
using PowerGuard.Infrastructure.Extensions;
using System.Threading.Tasks;

namespace PowerGuard.WebApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddInfrastructureServices(builder.Configuration);

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var UserManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var RoleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                await DataSeeder.SeedRolesAsync(RoleManager);
                await DataSeeder.SeedAdminAsync(UserManager);

            }
            // Configure the HTTP request pipeline.
               app.UseSwagger();
                app.UseSwaggerUI();
            

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
