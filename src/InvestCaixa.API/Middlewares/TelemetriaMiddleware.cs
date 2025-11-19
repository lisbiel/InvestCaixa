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
        var method = context.Request.Method;

        var nomeDoEndpoint = ExtrairEndpointCompleto(method, path);
        var sucesso = true;
        string tipoErro = null;
        Dictionary<string, object>? contexto = null;
        try
        {
            await _next(context);

            sucesso = context.Response.StatusCode < 400;

            if (!sucesso)
                tipoErro = DeterminarTipoErro(context.Response.StatusCode);
        }
        catch (Exception ex)
        {
            sucesso = false;
            tipoErro = ex.GetType().Name;
            contexto = new Dictionary<string, object>
            {
                ["ExceptionType"] = ex.GetType().FullName,
                ["ExceptionMessage"] = ex.Message,
                ["UserAgent"] = context.Request.Headers["User-Agent"].ToString()
            };
            //Propaga a exceção para o próximo middleware
            throw;
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

    private string DeterminarTipoErro(int statusCode)
    {
        return statusCode switch
            {
            >= 500 => "ServerError",
            404 => "NotFound",
            400 => "BadRequest",
            401 => "Unauthorized",
            403 => "Forbidden",
            409 => "Conflict",
            422 => "ValidationError",
                _ => "OtherClientError",
        };
    }

    private static string ExtrairEndpointCompleto(string method, string path)
    {
        var normalizado = NormalizarPath(path);
        return $"{method} {normalizado}";
    }

    private static string NormalizarPath(string path)
    {
        if (string.IsNullOrEmpty(path) || path == "/")
            return "/";

        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var normalizados = new List<string>();

        foreach (var segment in segments)
        {
            if (Guid.TryParse(segment, out _) || int.TryParse(segment, out _))
                normalizados.Add("{guid}");
            else if (int.TryParse(segment, out _) || long.TryParse(segment, out _))
                normalizados.Add("{id}");
            //não deixa vazar documentos na telemtria
            else if (segment.All(char.IsDigit) && segment.Length >= 11)
                normalizados.Add("{documento}");
            else
                normalizados.Add(segment.ToLowerInvariant());
        }

        return "/" + string.Join('/', normalizados);
    }

    private static string ExtrairNomeServico(string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return segments.Length >= 2 ? segments[1] : "unknown";
    }
}
