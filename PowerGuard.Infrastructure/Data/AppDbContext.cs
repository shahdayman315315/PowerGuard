using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PowerGuard.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Factory> Factories { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<LimitHistory> LimitHistories { get; set; }
        public DbSet<AISuggestion> AISuggestions { get; set; }
        public DbSet<ConsumptionLog> ConsumptionLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<UserOTP> UserOTPs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Factory>(e =>
            {
                e.HasMany(f => f.Departments)
                 .WithOne(d => d.Factory)
                 .HasForeignKey(d => d.FactoryId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasMany(f => f.LimitHistories)
                 .WithOne(lh => lh.Factory)
                 .HasForeignKey(lh => lh.FactoryId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasMany(f => f.AISuggestions)
                  .WithOne(s => s.Factory)
                  .HasForeignKey(s => s.FactoryId)
                  .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(f => f.Manager)
                  .WithMany(m => m.ManagedFactories)
                  .HasForeignKey(f => f.ManagerId)
                  .OnDelete(DeleteBehavior.Restrict);
            });


            modelBuilder.Entity<Department>(e =>
            {
                e.HasOne(d => d.Manager)
                .WithMany(m => m.ManagedDepartments)
                .HasForeignKey(d => d.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

                e.HasMany(d => d.ConsumptionLogs)
                .WithOne(cl => cl.Department)
                .HasForeignKey(cl => cl.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);

                e.HasMany(d => d.LimitHistories)
                .WithOne(lh => lh.Department)
                .HasForeignKey(lh => lh.DepartmentId)
                .OnDelete(DeleteBehavior.NoAction);

                e.HasMany(d => d.AISuggestions)
                .WithOne(s => s.Department)
                .HasForeignKey(s => s.DepartmentId)
                .OnDelete(DeleteBehavior.NoAction);
            });


            modelBuilder.Entity<ApplicationUser>(e =>
            {
                e.HasMany(u => u.LimitHistories)
                .WithOne(lh => lh.Setter)
                .HasForeignKey(lh => lh.SetBy)
                .OnDelete(DeleteBehavior.NoAction);

                e.HasMany(u => u.Notifications)
                .WithOne(n => n.User)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

                e.HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

                e.HasMany(u => u.OTPs)
                .WithOne(otp => otp.User)
                .HasForeignKey(otp => otp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(u => u.Factory)
                .WithMany() // المصنع عنده موظفين كتير
                .HasForeignKey(u => u.FactoryId)
                .OnDelete(DeleteBehavior.Restrict); 
           
                e.HasOne(u=>u.Department)
                .WithMany()
                .HasForeignKey(u=>u.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
               
            });


            modelBuilder.Entity<Alert>(e =>
            {
               e.HasOne(a=>a.ConsumptionLog)
                .WithMany(cl => cl.Alerts)
                .HasForeignKey(a => a.ConsumptionLogId)
                .OnDelete(DeleteBehavior.NoAction);

                e.HasMany(a => a.Notifications)
                .WithOne(n => n.Alert)
                .HasForeignKey(n => n.AlertId)
                .OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
}