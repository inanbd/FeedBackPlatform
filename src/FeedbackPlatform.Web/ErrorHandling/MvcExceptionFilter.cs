using FeedbackPlatform.Application.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FeedbackPlatform.Web.ErrorHandling;

/// <summary>
/// Maps Application-layer exceptions to sensible MVC results for portal pages. Registered globally,
/// so it also sees the API controller's exceptions — those are left alone (ExceptionHandled stays
/// false) so they bubble up to the outer UseExceptionHandler pipeline and get the RFC 7807
/// problem+json treatment from <see cref="ApiExceptionHandler"/> instead.
/// </summary>
public sealed class MvcExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.HttpContext.Request.Path.StartsWithSegments("/api"))
        {
            return;
        }

        switch (context.Exception)
        {
            case NotFoundException:
                context.Result = new NotFoundResult();
                context.ExceptionHandled = true;
                break;

            case ForbiddenAccessException:
                context.Result = new ForbidResult();
                context.ExceptionHandled = true;
                break;

            case ConflictException conflict:
                context.ModelState.AddModelError(string.Empty, conflict.Message);
                context.Result = new BadRequestObjectResult(context.ModelState);
                context.ExceptionHandled = true;
                break;

            case ValidationException validation:
                foreach (var error in validation.Errors)
                {
                    context.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }

                context.Result = new BadRequestObjectResult(context.ModelState);
                context.ExceptionHandled = true;
                break;
        }
    }
}
