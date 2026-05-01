using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Dtos
{
    public class UpdateConsumptionLogDto
    {
        public int LogId { get; set; }

        public decimal ConsumptionValue { get; set; }    
    }
}
