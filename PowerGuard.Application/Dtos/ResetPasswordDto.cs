using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Dtos
{
    public class ResetPasswordDto
    {
        public string Email { get; set; }= null!;

        public string ResetToken { get; set; }= null!;

        public string NewPassword { get; set; }= null!;
    }
}
