using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Dtos
{
    public class AuthResultDto
    {
        public string UserName { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string Token { get; set; } = null!;
        public DateTime ExpirationDate { get; set; }
        public string RefreshToken { get; set; } = null!;
        public DateTime RefreshTokenExpiration { get; set; }
        public string Message { get; set; } = null!;
        public bool IsSuccess { get; set; }
        public int FactoryId { get; set; }  
        public int DepartmentId { get; set; }
    }
}
