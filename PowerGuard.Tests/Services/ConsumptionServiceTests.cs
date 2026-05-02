using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Moq;
using PowerGuard.Application.Dtos;
using PowerGuard.Application.Interfaces;
using PowerGuard.Application.Services;
using PowerGuard.Domain.Interfaces;
using PowerGuard.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using MockQueryable.Moq;
using System.Threading.Tasks;

namespace PowerGuard.Tests.Services
{
    public class ConsumptionServiceTests
    {
        // 1. بنعرف الروبوتات (المحاكيين)
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly List<IConsumptionEvaluationStrategy> _strategies;

        // 2. السيرفس الحقيقية اللي هنختبرها
        private readonly ConsumptionService _consumptionService;

        public ConsumptionServiceTests()
        {
            // تهيئة الروبوتات
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mediatorMock = new Mock<IMediator>();
            _mapperMock = new Mock<IMapper>();

            // الاستراتيجيات بنحطها كقائمة حقيقية لأننا اختبرناها خلاص وعارفين إنها صح
            _strategies = new List<IConsumptionEvaluationStrategy>
        {
            new CriticalEvaluationStrategy(),
            new WarningEvaluationSrategy()
        };

            // بنجمع كل دول ونحقنهم في السيرفس
            _consumptionService = new ConsumptionService(
                _httpContextAccessorMock.Object,
                _unitOfWorkMock.Object,
                _strategies,
                _mediatorMock.Object,
                _mapperMock.Object
            );
        }



        [Fact]
        public async Task EnterConsumptionAsync_ShouldReturnSuccess_WhenDataIsValid()
        {
            // --- 1. Arrange (التجهيز) ---

            // أ- تعريف الموكس الصغيرة (Repositories)
            var factoryRepoMock = new Mock<IBaseRepository<Factory>>();
            var departmentRepoMock = new Mock<IBaseRepository<Department>>();
            var consumptionLogRepoMock = new Mock<IBaseRepository<ConsumptionLog>>();

            // ب- ربطهم بالـ Unit of Work
            _unitOfWorkMock.Setup(u => u.Factories).Returns(factoryRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Departments).Returns(departmentRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.ConsumptionLogs).Returns(consumptionLogRepoMock.Object);

            // ج- تجهيز البيانات الوهمية (Data)
            var factoryId = 1;
            var departmentId = 1;
            var factory = new Factory { Id = factoryId, CurrentConsumptionLimit = 1000 };

            // د- سحر الـ Queryable (حل مشكلة الـ Query والـ FirstOrDefault)
            // بنجهز لستة وهمية فيها القسم بتاعنا
            var departmentsList = new List<Department>
    {
        new Department { Id = departmentId, FactoryId = factoryId, CurrentConsumptionLimit = 500 }
    };
            // بنستخدم MockQueryable عشان نخلي اللستة تتصرف كأنها قاعدة بيانات
            var mockDepartmentQuery = departmentsList.AsQueryable();
            departmentRepoMock.Setup(r => r.Query).Returns(departmentsList.AsQueryable());
            // هـ- حل مشكلة الـ Sum (حساب الاستهلاك القديم)
            // بنجهز لستة فاضية للوجات القديمة عشان الـ Sum يطلع صفر في البداية
            var logsList = new List<ConsumptionLog>();
            var mockLogQuery = logsList.AsQueryable();
            consumptionLogRepoMock.Setup(r => r.Query).Returns(logsList.AsQueryable());
            // و- برمجة باقي الموكس (Factory, Mapper, SaveChanges)
            factoryRepoMock.Setup(f => f.GetByIdAsync(factoryId,default)).ReturnsAsync(factory);

            _mapperMock.Setup(m => m.Map<ConsumptionLog>(It.IsAny<ConsumptionLogDto>()))
                       .Returns(new ConsumptionLog { DepartmentId = departmentId, ConsumptionValue = 100 });

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

            // ز- تجهيز اليوزر الوهمي (Claims)
            var claims = new List<Claim> { new Claim("FactoryId", factoryId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = claimsPrincipal };
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            // ح- بيانات الدخل (Input DTO)
            var dto = new ConsumptionLogDto
            {
                DepartmentId = departmentId,
                ConsumptionValue = 100,
                CapturedAt = DateTime.UtcNow
            };

            // --- 2. Act (التنفيذ) ---
            var result = await _consumptionService.EnterConsumptionAsync(dto);

            // --- 3. Assert (التأكد) ---
            // هنا بقى اللحظة الحاسمة
            Assert.True(result.IsSuccess);
            Assert.Equal("Consumption added and evaluated successfully.", result.Message);

            // نتأكد إن ميثود الإضافة اتنادت فعلاً
            consumptionLogRepoMock.Verify(r => r.AddAsync(It.IsAny<ConsumptionLog>(),default), Times.Once);
        }
    }
}
