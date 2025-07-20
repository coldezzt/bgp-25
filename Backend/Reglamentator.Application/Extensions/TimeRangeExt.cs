using Reglamentator.Domain.Entities;

namespace Reglamentator.Application.Extensions;

public static class TimeRangeExt
{
    public static string? ToCronExpression(this TimeRange timeRange, DateTime? time = null)
    {
        var effectiveTime = time ?? DateTime.UtcNow;

        return timeRange switch
        {
            TimeRange.None => null,
            TimeRange.Min15 => $"{effectiveTime.Minute % 15}/15 * * * *",
            TimeRange.Hour => $"{effectiveTime.Minute} * * * *",
            TimeRange.Day => $"{effectiveTime.Minute} {effectiveTime.Hour} * * *",
            TimeRange.Week => $"{effectiveTime.Minute} {effectiveTime.Hour} * * {(int)effectiveTime.DayOfWeek}",
            TimeRange.Month => $"{effectiveTime.Minute} {effectiveTime.Hour} {effectiveTime.Day} * *",
            _ => throw new ArgumentOutOfRangeException(nameof(timeRange), timeRange, null)
        };
    }

    public static TimeRange ToTimeRange(this string? cronExpression)
    {
        if (string.IsNullOrEmpty(cronExpression))
            return TimeRange.None;

        var parts = cronExpression.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 5)
            throw new ArgumentException("Неправильный формат cron", nameof(cronExpression));
        
        var isMin15 = parts[0].Contains("/15") && parts[1] == "*" && parts[2] == "*" && parts[3] == "*" && parts[4] == "*";
        var isHour = parts[1] == "*" && parts[2] == "*" && parts[3] == "*" && parts[4] == "*";
        var isDay = parts[2] == "*" && parts[3] == "*" && parts[4] == "*";
        var isWeek = parts[2] == "*" && parts[3] == "*";
        var isMonth = parts[3] == "*" && parts[4] == "*";

        return isMin15 ? TimeRange.Min15
             : isHour ? TimeRange.Hour
             : isDay ? TimeRange.Day
             : isWeek ? TimeRange.Week
             : isMonth ? TimeRange.Month
             : throw new ArgumentException($"Неподдерживаемый формат cron: {cronExpression}", nameof(cronExpression));
    }
    
    public static TimeSpan ToTimeSpan(this TimeRange timeRange) =>
        timeRange switch
        {
            TimeRange.None => TimeSpan.Zero,
            TimeRange.Min15 => TimeSpan.FromMinutes(15),
            TimeRange.Hour => TimeSpan.FromHours(1),
            TimeRange.Day => TimeSpan.FromDays(1),
            TimeRange.Week => TimeSpan.FromDays(7),
            TimeRange.Month => TimeSpan.FromDays(30),
            _ => throw new ArgumentOutOfRangeException(nameof(timeRange), timeRange, null)
        };

    public static TimeRange ToTimeRange(this TimeSpan timeSpan) =>
        (timeSpan.TotalMinutes, timeSpan.TotalHours, timeSpan.TotalDays) switch
        {
            (0, 0, 0) => TimeRange.None,
            (<= 15, 0, 0) => TimeRange.Min15,
            (0, <= 1, 0) => TimeRange.Hour,
            (0, 0, <= 1) => TimeRange.Day,
            (0, 0, <= 7) => TimeRange.Week,
            (0, 0, <= 30) => TimeRange.Month,
            _ => throw new ArgumentException($"Время {timeSpan} не соответствует TimeRange", nameof(timeSpan))
        };
}