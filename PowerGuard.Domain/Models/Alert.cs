using PowerGuard.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Domain.Models
{
    public class Alert
    {
        public int Id { get; set; }
        public AlertType Type { get; set; }
        public AlertSeverity Severity { get; set; }
        public int ConsumptionLogId { get; set; }
        public ConsumptionLog ConsumptionLog { get; set; }
        public DateTime CreatedAt { get; set; }= DateTime.UtcNow;
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
