using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Domain.Models
{
    public class LimitHistory
    {
        public int Id { get; set; }
        public int? FactoryId { get; set; }
        public Factory? Factory { get; set; }
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }
        public decimal LimitValue { get; set; }
        public string SetBy { get; set; }
        public ApplicationUser Setter { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ActiveFrom { get; set; }
    }
}
