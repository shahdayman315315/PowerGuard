using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Domain.Enums
{
    public enum AlertSeverity
    {
        Low,      // تنبيه بسيط (للعلم فقط)
        Medium,   // يحتاج مراجعة خلال اليوم
        High,     // خطر (يطلب تدخل فوري)
        Critical  // كارثي (قد يؤدي لتوقف الإنتاج)
    }
}
