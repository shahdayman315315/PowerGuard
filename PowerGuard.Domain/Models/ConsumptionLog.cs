using PowerGuard.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Domain.Models
{
    public class ConsumptionLog
    {
        public int Id { get; set; }
        public decimal ConsumptionValue { get; set; }
        public DateTime CapturedAt { get; set; }
        public ConsumptionStatus Status { get; set; }
        public int DepartmentId { get; set; }
        public Department Department { get; set; }
        public  ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    }
}
