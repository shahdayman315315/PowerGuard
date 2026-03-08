using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Domain.Models
{
    public class UserOTP
    {
        public int Id { get; set; }
        public string Code { get; set; } 
        public DateTime ExpirationDate { get; set; } 
        public bool IsUsed { get; set; } = false; 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? ResetToken { get; set; } 
        public DateTime? ResetTokenExpiration { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
