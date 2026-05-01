using MediatR;
using Microsoft.EntityFrameworkCore;
using PowerGuard.Application.Helpers;
using PowerGuard.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Features.Auth.RevokeRefreshToken
{
    public class RevokeRefreshTokenCommandHandler : IRequestHandler<RevokeRefreshTokenCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public RevokeRefreshTokenCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<Result<bool>> Handle(RevokeRefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var existRefreshToken = await _unitOfWork.RefreshTokens.Query
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken);

            if (existRefreshToken is null || !existRefreshToken.IsActive)
                return Result<bool>.Failure("Invalid or expired refresh token");

            existRefreshToken.RevokedOn = DateTime.UtcNow;

            _unitOfWork.RefreshTokens.Update(existRefreshToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
