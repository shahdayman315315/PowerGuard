using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Dtos
{
    public class AdminDashboardDto
    {
        public int TotalFactories { get; set; }
        public int PendingFactories { get; set; }
        public int ActiveFactories { get; set; }
    }
}
