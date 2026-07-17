using PowerGuard.Application.Services;
using PowerGuard.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Tests.Strategies
{
    public class ConsumptionStrategyTests
    {
        [Theory]
        [InlineData(120, 100)] // حالة Exceeded
        [InlineData(100, 100)] // حالة Normal (لأنه مش أكبر من، هو بيساوي)
        [InlineData(80, 100)]  // حالة Normal
        public void CriticalStrategy_ComprehensiveTest(decimal reading, decimal limit)
        {
            var strategy = new CriticalEvaluationStrategy();
            var result = strategy.Evaluate(reading, limit);

            if (reading > limit)
                Assert.Equal(ConsumptionStatus.Exceeded, result);
            else
                Assert.Equal(ConsumptionStatus.Normal, result);
        }


        [Theory]
        [InlineData(85, 100)] // حالة الـ Warning (أكبر من 80 وأصغر من 100)
        [InlineData(99, 100)] // حالة الـ Warning (على الحافة)
        [InlineData(70, 100)] // حالة Normal (أصغر من 80%)
        public void WarningStrategy_ShouldReturnCorrectStatus(decimal reading, decimal limit)
        {
            // Arrange
            var strategy = new WarningEvaluationSrategy();

            // Act
            var result = strategy.Evaluate(reading, limit);

            // Assert
            if (reading > (limit * 0.8m) && reading < limit)
                Assert.Equal(ConsumptionStatus.Warning, result);
            else
                Assert.Equal(ConsumptionStatus.Normal, result);
        }
    }
}
