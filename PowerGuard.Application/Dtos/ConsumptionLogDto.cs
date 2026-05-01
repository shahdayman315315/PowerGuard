using PowerGuard.Domain.Enums;
using PowerGuard.Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Dtos
{
    public class ConsumptionLogDto
    {
        public decimal ConsumptionValue { get; set; }

        public DateTime CapturedAt { get; set; }= DateTime.UtcNow;

        public int DepartmentId { get; set; }
    }
}
