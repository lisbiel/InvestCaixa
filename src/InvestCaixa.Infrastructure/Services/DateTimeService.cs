using System.Diagnostics.CodeAnalysis;

namespace InvestCaixa.Infrastructure.Services;

public interface IDateTimeService
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
}
[ExcludeFromCodeCoverage]
public class DateTimeService : IDateTimeService
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
}
