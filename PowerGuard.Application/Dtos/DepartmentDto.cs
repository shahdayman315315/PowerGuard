using PowerGuard.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Dtos
{
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string OperatingHours { get; set; }
        public decimal CurrentConsumptionLimit { get; set; }
        public int FactoryId { get; set; }
        public string? ManagerName { get; set; }
       
    }
}
