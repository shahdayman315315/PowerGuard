using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Helpers
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if(!_validators.Any())
            {
                return await next();
            }

            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

            if(failures.Count != 0)
            {
                var errorMessages = failures.Select(f => f.ErrorMessage).ToList();

                var errorMessage = string.Join(", ", errorMessages);
                var responseType = typeof(TResponse); // ده هيكون Result<AuthResultDto> مثلاً
            
            var failureMethod = responseType.GetMethod("Failure", new[] { typeof(string), typeof(int) });
            
            if (failureMethod != null)
            {
                return (TResponse)failureMethod.Invoke(null, new object[] { errorMessage, 400 })!;
            }

            }

            return await next();
        }
    }
}
