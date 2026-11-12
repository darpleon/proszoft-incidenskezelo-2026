namespace incidenskezelo_backend.Services.AI;

public sealed class AiProviderTimeoutException(
    Exception? innerException = null)
    : Exception("The AI provider timed out.", innerException);

public sealed class AiProviderUnavailableException(
    string message,
    Exception? innerException = null)
    : Exception(message, innerException);

public sealed class AiInvalidResponseException(
    string message,
    Exception? innerException = null)
    : Exception(message, innerException);