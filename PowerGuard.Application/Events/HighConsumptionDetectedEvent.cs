using MediatR;
using PowerGuard.Domain.Enums;
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
        public int FactoryId { get; }
        public int DepartmentId { get; }
        public string DepartmentName { get; }
        public ConsumptionStatus DepartmentStatus { get; }
        public ConsumptionStatus FactoryStatus { get; }
        public HighConsumptionDetectedEvent(decimal value, ConsumptionStatus factoryStatus,
            int factoryId, ConsumptionStatus? departmentStatus=null, string? departmentName = null, int? departmentId = null, int? logId = null)
        {
            LogId = logId??0;
            Value = value;
            DepartmentName = departmentName??"";
            DepartmentStatus = departmentStatus??ConsumptionStatus.Normal;
            FactoryId = factoryId;
            DepartmentId = departmentId??0;
            FactoryStatus = factoryStatus;
            
        }
    }
}
