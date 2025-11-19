using System;
using System.ComponentModel;
using System.Reflection;

namespace InvestCaixa.Application.Extensions;

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        if (value is null) return string.Empty;
        var fi = value.GetType().GetField(value.ToString());
        if (fi is null) return value.ToString();

        var attr = fi.GetCustomAttribute<DescriptionAttribute>();
        return attr?.Description ?? value.ToString();
    }
}