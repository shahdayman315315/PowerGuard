using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Dtos
{
    public class ChartPointDto
    {
        public DateTime CapturedAt { get; set; } 
        public decimal ConsumptionValue { get; set; } 
    }
}
