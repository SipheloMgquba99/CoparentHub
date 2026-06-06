using CoparentHub.Domain.Common;
using FluentValidation;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace CoparentHub.Application.Validations
{
    public interface ICommand<T> : IRequest<Result<T>> { }
    public interface IQuery<T> : IRequest<Result<T>> { }

    public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
        : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
        {
            var errors = validators
                .Select(v => v.Validate(request))
                .SelectMany(r => r.Errors)
                .Select(f => f.ErrorMessage)
                .ToList();

            if (errors.Count == 0) return await next();

            var type = typeof(TResponse);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var fail = typeof(Result<>).MakeGenericType(type.GetGenericArguments()[0])
                               .GetMethod(nameof(Result<object>.Fail))!;
                return (TResponse)fail.Invoke(null, [string.Join("; ", errors)])!;
            }

            throw new System.ComponentModel.DataAnnotations.ValidationException(string.Join("; ", errors));
        }
    }
}
