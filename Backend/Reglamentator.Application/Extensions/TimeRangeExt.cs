using Reglamentator.Domain.Entities;

namespace Reglamentator.Application.Extensions;

/// <summary>
/// Статический класс с методами расширения для работы с TimeRange.
/// </summary>
public static class TimeRangeExt
{
    /// <summary>
    /// Преобразует TimeRange в cron-выражение.
    /// </summary>
    /// <param name="timeRange">Временной диапазон для преобразования.</param>
    /// <param name="time">Опциональное время для расчета (по умолчанию текущее UTC время).</param>
    /// <returns>
    /// Cron-выражение или null, если timeRange равен TimeRange.None.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Выбрасывается, если передан неподдерживаемый TimeRange
    /// </exception>
    /// <remarks>
    /// Поддерживаемые преобразования:
    /// <list type="bullet">
    ///   <item><description>Min15 → "s m/15 * * * *" (каждые 15 минут)</description></item>
    ///   <item><description>Hour → "s m * * * *" (ежечасно)</description></item>
    ///   <item><description>Day → "s m H * * *" (ежедневно)</description></item>
    ///   <item><description>Week → "s m H * * D" (еженедельно, D - день недели)</description></item>
    ///   <item><description>Month → "s m H d * *" (ежемесячно)</description></item>
    /// </list>
    /// </remarks>
    public static string? ToCronExpression(this TimeRange timeRange, DateTime? time = null)
    {
        var effectiveTime = time ?? DateTime.UtcNow;

        return timeRange switch
        {
            TimeRange.None => null,
            TimeRange.Min15 => $"{effectiveTime.Second} {effectiveTime.Minute % 15}/15 * * * *",
            TimeRange.Hour => $"{effectiveTime.Second} {effectiveTime.Minute} * * * *",
            TimeRange.Day => $"{effectiveTime.Second} {effectiveTime.Minute} {effectiveTime.Hour} * * *",
            TimeRange.Week => $"{effectiveTime.Second} {effectiveTime.Minute} {effectiveTime.Hour} * * {(int)effectiveTime.DayOfWeek}",
            TimeRange.Month => $"{effectiveTime.Second} {effectiveTime.Minute} {effectiveTime.Hour} {effectiveTime.Day} * *",
            _ => throw new ArgumentOutOfRangeException(nameof(timeRange), timeRange, null)
        };
    }

    /// <summary>
    /// Преобразует cron-выражение в TimeRange.
    /// </summary>
    /// <param name="cronExpression">Cron-выражение для анализа.</param>
    /// <returns>Соответствующий TimeRange.</returns>
    /// <exception cref="ArgumentException">
    /// Выбрасывается если:
    /// <list type="bullet">
    ///   <item><description>cron-выражение имеет неправильный формат.</description></item>
    ///   <item><description>cron-выражение имеет неподдерживаемый шаблон.</description></item>
    /// </list>
    /// </exception>
    /// <remarks>
    /// Определяет TimeRange по структуре cron-выражения:
    /// <list type="bullet">
    ///   <item><description>"* * * * *" → None</description></item>
    ///   <item><description>"s m/15 * * * *" → Min15</description></item>
    ///   <item><description>"s m * * * *" → Hour</description></item>
    ///   <item><description>"s m H * * *" → Day</description></item>
    ///   <item><description>"s m H * * D" → Week</description></item>
    ///   <item><description>"s m H d * *" → Month</description></item>
    /// </list>
    /// </remarks>
    public static TimeRange ToTimeRange(this string? cronExpression)
    {
        if (string.IsNullOrEmpty(cronExpression))
            return TimeRange.None;

        var parts = cronExpression.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 6)
            throw new ArgumentException($"Неправильный формат cron: {cronExpression}", nameof(cronExpression));
        
        var isMin15 = parts[1].Contains("/15") && parts[2] == "*" && parts[3] == "*" && parts[4] == "*" && parts[5] == "*";
        var isHour = parts[2] == "*" && parts[3] == "*" && parts[4] == "*" && parts[5] == "*";
        var isDay = parts[3] == "*" && parts[4] == "*" && parts[5] == "*";
        var isWeek = parts[3] == "*" && parts[4] == "*";
        var isMonth = parts[4] == "*" && parts[5] == "*";

        return isMin15 ? TimeRange.Min15
             : isHour ? TimeRange.Hour
             : isDay ? TimeRange.Day
             : isWeek ? TimeRange.Week
             : isMonth ? TimeRange.Month
             : throw new ArgumentException($"Неподдерживаемый формат cron: {cronExpression}", nameof(cronExpression));
    }
    
    /// <summary>
    /// Преобразует TimeRange в TimeSpan.
    /// </summary>
    /// <param name="timeRange">Временной диапазон для преобразования.</param>
    /// <returns>Эквивалентный TimeSpan.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Выбрасывается, если передан неподдерживаемый TimeRange.
    /// </exception>
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

    /// <summary>
    /// Преобразует TimeSpan в TimeRange.
    /// </summary>
    /// <param name="timeSpan">Временной промежуток для преобразования.</param>
    /// <returns>Наиболее подходящий TimeRange.</returns>
    /// <exception cref="ArgumentException">
    /// Выбрасывается, если TimeSpan превышает максимальный TimeRange (Month) или указано 2 и более параметров (мин. + ч. и т.п.)
    /// </exception>
    /// <remarks>
    /// Преобразование выполняется по следующим правилам:
    /// <list type="bullet">
    ///   <item><description>≤ 15 минут → Min15</description></item>
    ///   <item><description>≤ 1 час → Hour</description></item>
    ///   <item><description>≤ 1 день → Day</description></item>
    ///   <item><description>≤ 7 дней → Week</description></item>
    ///   <item><description>≤ 30 дней → Month</description></item>
    /// </list>
    /// </remarks>
    public static TimeRange ToTimeRange(this TimeSpan timeSpan) =>
        (timeSpan.Minutes, timeSpan.Hours, timeSpan.Days) switch
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