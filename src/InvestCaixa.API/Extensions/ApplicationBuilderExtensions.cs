using InvestCaixa.API.Middlewares;
using Serilog;

namespace InvestCaixa.API.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseApiServices(this IApplicationBuilder app)
    {
        app.UseExceptionHandler();
        app.UseHttpsRedirection();
        app.UseSerilogRequestLogging();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCors();

        return app;
    }

    public static IApplicationBuilder UseTelemetria(this IApplicationBuilder app)
    {
        app.UseMiddleware<TelemetriaMiddleware>();
        return app;
    }
}
