using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Dtos
{
    public class UpdateFactoryDto
    {
        
        public string Name { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        

    }
    
}
