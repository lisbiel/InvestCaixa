namespace InvestCaixa.API.Middlewares;

using InvestCaixa.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Ocorreu uma exceção: {ExceptionType} | Path: {Path} | Metodo: {Method} | TraceId: {TraceId}",
            exception.GetType().Name,
            httpContext.Request.Path,
            httpContext.Request.Method,
            httpContext.TraceIdentifier);
        var problemDetails = new ProblemDetails
        {
            Status = exception switch
            {
                NotFoundException => StatusCodes.Status404NotFound,
                ValidationException => StatusCodes.Status400BadRequest,
                DomainException => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError
            },
            Title = exception switch
            {
                NotFoundException => "Recurso não encontrado",
                ValidationException => "Erro de validação",
                DomainException => "Erro de negócio",
                UnauthorizedAccessException => "Acesso negado",
                _ => "Erro interno do servidor"
            },
            Detail = exception switch
            {
                ValidationException => exception.Message,
                DomainException => exception.Message,
                NotFoundException => exception.Message,
                UnauthorizedAccessException => "Credenciais inválidas, insuficientes para acessar este recurso ou Token Expirado.",
                _ => "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde."
            },
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
        problemDetails.Extensions["timestamp"] = DateTime.UtcNow.ToString("O");

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        if(_environment != null && _environment.IsDevelopment())
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            problemDetails.Extensions["innerException"] = exception.InnerException?.Message;
        }

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
