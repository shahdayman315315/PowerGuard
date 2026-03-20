using PowerGuard.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Interfaces
{
    public interface IConsumptionEvaluationStrategy
    {
        ConsumptionStatus Evaluate(decimal currentReading, decimal limit);
    }
}
