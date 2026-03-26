using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Dtos
{
    public class DepartmentDailyConsumptionSummaryDto
    {
        public string DepartmentName {  get; set; }
        public decimal TotalConsumption {  get; set; }
        public decimal CurrentLimit { get; set; }
        public double ConsumptionPercentage {  get; set; }
        public decimal RemainingAmount {  get; set; }
        public string Status {  get; set; }
        public decimal LastReadingValue { get; set; }
        public DateTime? LastReadingAt { get; set; }

        // هل الاستهلاك زاد ولا قل عن نفس الوقت إمبارح؟
        public double? ComparisonWithYesterday { get; set; }
    }
}
