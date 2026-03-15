using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Dtos
{
    public class UpdateDepartmentDto
    {
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? OperatingHours { get; set; }
        public decimal CurrentConsumptionLimit { get; set; }

    }
}
