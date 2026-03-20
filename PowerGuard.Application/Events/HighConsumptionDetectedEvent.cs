using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Events
{
    public class HighConsumptionDetectedEvent:INotification
    {
        public int LogId { get;  }
        public decimal Value {  get; }
        public string DepartmentName { get; }
        public string Status { get; }
        public HighConsumptionDetectedEvent(int logId,decimal value,string departmentName,string status)
        {
            LogId = logId;
            Value = value;
            DepartmentName = departmentName;
            Status = status;
            
        }
    }
}
