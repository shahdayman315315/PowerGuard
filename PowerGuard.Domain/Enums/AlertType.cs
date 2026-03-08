using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Domain.Enums
{
    public enum AlertType
    {
        LimitExceeded,    // تجاوز الحد المسموح
        SpikeDetected,    // سحب مفاجئ وكبير للكهرباء
        DeviceFailure,    // عطل في جهاز القياس
        PowerOutage       // انقطاع التيار عن القسم
    }
}
