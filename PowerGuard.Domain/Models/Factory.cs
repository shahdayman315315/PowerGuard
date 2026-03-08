using PowerGuard.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Domain.Models
{
    public class Factory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        public FactoryStatus Status { get; set; } = FactoryStatus.Pending;
        public string? AdminRemarks { get; set; } 
        public decimal? CurrentConsumptionLimit { get; set; }
        public string ManagerId { get; set; }
        public ApplicationUser Manager { get; set; }
        public ICollection<Department> Departments { get; set; } = new List<Department>();
        public ICollection<LimitHistory> LimitHistories { get; set; } = new List<LimitHistory>();
        public ICollection<AISuggestion> AISuggestions { get; set; } = new List<AISuggestion>();
    }
}
