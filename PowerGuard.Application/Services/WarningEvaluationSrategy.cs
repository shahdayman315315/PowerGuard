using PowerGuard.Application.Interfaces;
using PowerGuard.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Services
{
    public class WarningEvaluationSrategy : IConsumptionEvaluationStrategy
    {
        public ConsumptionStatus Evaluate(decimal currentReading, decimal limit)
        {
            return currentReading > (limit * 0.8m) && (currentReading < limit) ? ConsumptionStatus.Warning : ConsumptionStatus.Normal;
        }
    }
}
