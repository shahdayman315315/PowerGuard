using PowerGuard.Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Dtos
{
    public class CreateDepartmentDto
    {
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }

        [Required]
        public string OperatingHours { get; set; }
        public decimal CurrentConsumptionLimit { get; set; }
        public string? ManagerId { get; set; }
      
    }
}
