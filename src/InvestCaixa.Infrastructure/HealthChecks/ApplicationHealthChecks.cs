using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace InvestCaixa.Infrastructure.HealthChecks;

public class ApplicationHealthChecks : IHealthCheck
{
    private readonly ILogger<ApplicationHealthChecks> _logger;

    public ApplicationHealthChecks(ILogger<ApplicationHealthChecks> logger)
    {
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var memoryUsed = GC.GetTotalMemory(false);
            var maxMemory = 1000 * 1024 * 1024; // 1000 MB
            if (memoryUsed > maxMemory)
                _logger.LogError("Uso de memória elevado: {MemoryUsed} MB", memoryUsed / 1024 / 1024);

            return Task.FromResult(HealthCheckResult.Healthy("A aplicação está saudável."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check falhou.");
            return Task.FromResult(HealthCheckResult.Unhealthy("A aplicação não está saudável."));
        }
    }
}
