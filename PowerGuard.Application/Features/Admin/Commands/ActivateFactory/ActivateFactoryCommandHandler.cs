using MediatR;
using Microsoft.EntityFrameworkCore;
using PowerGuard.Application.Features.Admin.Commands.activateFactory;
using PowerGuard.Application.Helpers;
using PowerGuard.Application.Interfaces;
using PowerGuard.Domain.Enums;
using PowerGuard.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Admin.Commands.ActivateFactory
{
    public class ActivateFactoryCommandHandler : IRequestHandler<ActivateFactoryCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public ActivateFactoryCommandHandler(IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _emailService = emailService;   
            _unitOfWork = unitOfWork;
        }
        public async Task<Result<bool>> Handle(ActivateFactoryCommand request, CancellationToken cancellationToken)
        {
            var factory = await _unitOfWork.Factories.Query.Include(f => f.Manager).FirstOrDefaultAsync(f => f.Id == request.FactoryId,cancellationToken);

            if (factory is null)
            {
                return Result<bool>.Failure("Factory not found.", 404);
            }

            if (factory.Status != FactoryStatus.Deactivated)
            {
                return Result<bool>.Failure("Only inactive factories can reactivated.");
            }

            factory.Status = FactoryStatus.Approved;
            _unitOfWork.Factories.Update(factory);
            var result = await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (result > 0)
            {
                await _emailService.SendEmailAsync(factory.Manager.Email!, "Factory Reactivation", "Your factory has been reactivated");
                return Result<bool>.Success(true, "Factory has been activated");
            }

            return Result<bool>.Failure("Factory can't be activated");
        }
    }
}
