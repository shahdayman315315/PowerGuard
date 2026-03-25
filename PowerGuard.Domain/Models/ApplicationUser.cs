using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Domain.Models
{
    public class ApplicationUser: IdentityUser
    {
        public int? FactoryId { get; set; }
        public Factory? Factory { get; set; }
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }
        public  ICollection<Factory> ManagedFactories { get; set; } = new List<Factory>();
        public  ICollection<Department> ManagedDepartments { get; set; } = new List<Department>();
        public  ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public  ICollection<LimitHistory> LimitHistories { get; set; } = new List<LimitHistory>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<UserOTP> OTPs { get; set; } = new List<UserOTP>();
    }
}
