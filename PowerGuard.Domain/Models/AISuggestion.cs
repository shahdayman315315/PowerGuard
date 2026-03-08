using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Domain.Models
{
    public class AISuggestion
    {
        public int Id { get; set; }
        public string SuggestionText { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? FactoryId { get; set; }
        public Factory? Factory { get; set; }
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }
        public string CurrentPeakTime { get; set; } // الوقت اللي فيه زحمة حالياً
        public string SuggestedShiftTime { get; set; } // الوقت المقترح لتغيير الشيفت
        public decimal ExpectedSavings { get; set; } // التوفير المتوقع من تغيير الشيفت
    }
}
