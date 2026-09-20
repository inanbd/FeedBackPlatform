namespace FeedbackPlatform.Application.Common.Exceptions;

public sealed class ForbiddenAccessException(string message = "You do not have access to this resource.")
    : Exception(message);
