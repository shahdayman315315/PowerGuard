using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PowerGuard.Application.Helpers;
using PowerGuard.Application.Interfaces;
using PowerGuard.Application.Services;
using PowerGuard.Domain.Interfaces;
using PowerGuard.Domain.Models;
using PowerGuard.Infrastructure.Data;
using PowerGuard.Infrastructure.RealTimeService;
using PowerGuard.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerGuard.Infrastructure.Extensions
{
    public static class ServiceRegisteration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure();
                }));

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredUniqueChars = 1;
                options.Password.RequiredLength = 8;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();


            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings!.Issuer,
                    ValidAudience = jwtSettings!.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ClockSkew = TimeSpan.Zero

                };
            }
            );



            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            services.Configure<MailSettings>(configuration.GetSection("MailSettings"));

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddScoped<IFactoryService, FactoryService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IConsumptionEvaluationStrategy, CriticalEvaluationStrategy>();
            services.AddScoped<IConsumptionEvaluationStrategy, WarningEvaluationSrategy>();
            services.AddScoped<IConsumptionService, ConsumptionService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IDepartmentDashboardService, DepartmentDashboardService>();
            services.AddScoped<IFactoryDashboardService, FactoryDashboardService>();
            // في ملف Program.cs
            services.AddScoped<IRealTimeNotificationService, RealTimeNotificationService>();

            // في ملف Program.cs
            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(typeof(IFactoryDashboardService).Assembly);
            });
            services.AddMemoryCache();
            services.AddSignalR();

            return services;
        }
    }
}
