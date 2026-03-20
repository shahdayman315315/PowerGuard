using PowerGuard.Application.Interfaces;
using PowerGuard.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Services
{
    public class CriticalEvaluationStrategy : IConsumptionEvaluationStrategy
    {
        public ConsumptionStatus Evaluate(decimal currentReading, decimal limit)
        {
            return currentReading>limit? ConsumptionStatus.Exceeded: ConsumptionStatus.Normal;
        }
    }
}
