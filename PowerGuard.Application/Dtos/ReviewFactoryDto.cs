using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Dtos
{
    public class ReviewFactoryDto
    {
        [Required]
        public int FactoryId { get; set; }

        [Required]
        public bool IsApproved { get; set; }

        public string AdminRemarks { get; set; }
    }
}
