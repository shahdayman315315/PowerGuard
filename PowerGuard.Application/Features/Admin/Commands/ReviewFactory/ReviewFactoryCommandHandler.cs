using MediatR;
using Microsoft.EntityFrameworkCore;
using PowerGuard.Application.Helpers;
using PowerGuard.Application.Interfaces;
using PowerGuard.Domain.Enums;
using PowerGuard.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Admin.Commands.ReviewFactory
{
    public class ReviewFactoryCommandHandler : IRequestHandler<ReviewFactoryCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public ReviewFactoryCommandHandler(IEmailService emailService, IUnitOfWork unitOfWork)
        {
            _emailService = emailService;
            _unitOfWork = unitOfWork;
        }
        public async Task<Result<bool>> Handle(ReviewFactoryCommand request, CancellationToken cancellationToken)
        {
            var factory = await _unitOfWork.Factories.Query.Include(f => f.Manager).FirstOrDefaultAsync(f => f.Id == request.FactoryId);

            if (factory is null)
            {
                return Result<bool>.Failure("Factory not found.", 404);
            }

            if (factory.Status != FactoryStatus.Pending)
            {
                return Result<bool>.Failure("Only pending factories can be reviewed.");
            }

            if (request.IsApproved)
            {
                factory.Status = FactoryStatus.Approved;
            }
            else
            {
                factory.Status = FactoryStatus.Rejected;
                factory.AdminRemarks = request.AdminRemarks ?? "No remarks provided.";
            }

            var result = await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (result > 0)
            {
                await _emailService.SendEmailAsync(factory.Manager.Email!, request.IsApproved ? "Factory Approve" : "Factory Rejection", request.IsApproved ? "Congrats! Ur factory has been approved by the admin." : $"Ur Factory has been rejected due to : {request.AdminRemarks}");
                return Result<bool>.Success(true);
            }

            return Result<bool>.Failure("Can't update factory status in the database.");
        }
    }
}
