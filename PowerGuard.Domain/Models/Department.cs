using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Domain.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string OperatingHours { get; set; }
        public decimal? CurrentConsumptionLimit { get; set; }
        public int FactoryId { get; set; }
        public Factory Factory { get; set; }
        public string? ManagerId { get; set; }
        public ApplicationUser? Manager { get; set; }
        public ICollection<ConsumptionLog> ConsumptionLogs { get; set; } = new List<ConsumptionLog>();
        public ICollection<LimitHistory> LimitHistories { get; set; } = new List<LimitHistory>();
        public ICollection<AISuggestion> AISuggestions { get; set; } = new List<AISuggestion>();
    }
}
