using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Dtos
{
    public class RegisterDto
    {
        public string UserName { get; set; } = null!;

        public string Email { get; set; }=null!;

        public string Password { get; set; } = null!;

        public string ConfirmPassword { get; set; } = null!;

        public string PhoneNumber { get; set; }= null!;

        public int? FactoryId { get; set; } // اختياري للأدمن، وإلزامي للباقي

    }
}
