namespace InvestCaixa.API.Middlewares;

using InvestCaixa.Application.Interfaces;
using System.Diagnostics;

public class TelemetriaMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TelemetriaMiddleware> _logger;

    public TelemetriaMiddleware(
        RequestDelegate next,
        ILogger<TelemetriaMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITelemetriaService telemetriaService)
    {
        var stopwatch = Stopwatch.StartNew();
        var path = context.Request.Path.Value ?? string.Empty;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            if (!path.Contains("/swagger") && !path.Contains("/health"))
            {
                var nomeServico = ExtrairNomeServico(path);
                telemetriaService.RegistrarChamada(nomeServico, stopwatch.ElapsedMilliseconds);

                _logger.LogInformation(
                    "Request {Method} {Path} completado em {Duration}ms com status {StatusCode}",
                    context.Request.Method,
                    path,
                    stopwatch.ElapsedMilliseconds,
                    context.Response.StatusCode);
            }
        }
    }

    private static string ExtrairNomeServico(string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return segments.Length >= 2 ? segments[1] : "unknown";
    }
}
