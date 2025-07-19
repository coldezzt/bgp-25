using Telegram.Bot;
using ToDoBot.Services;
using ToDoBot.Models;

public class ReminderService
{
    private readonly ITelegramBotClient _bot;
    private readonly ApiService _api;
    private readonly Dictionary<long, DateTime> _lastReminderSent = new();

    public ReminderService(ITelegramBotClient bot, ApiService api)
    {
        _bot = bot;
        _api = api;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndSendRemindersAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Reminder] Ошибка: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken); // Пауза между проверками
        }
    }

    private async Task CheckAndSendRemindersAsync(CancellationToken cancellationToken)
    {
        // Тут можно получить список всех chatId (сохраняй их при создании операций)
        var chatIds = await _api.GetAllUserIdsAsync();

        foreach (var chatId in chatIds)
        {
            var operations = await _api.GetOperationsAsync(chatId);

            foreach (var op in operations)
            {
                var now = DateTime.UtcNow;
                var timeLeft = op.DueDate.ToUniversalTime() - now;

                if (ShouldSendReminder(op.Reminder, timeLeft))
                {
                    if (!_lastReminderSent.TryGetValue(op.Id, out var lastTime) || (DateTime.UtcNow - lastTime).TotalMinutes > 5)
                    {
                        string message = $"⏰ Напоминание! Задача: *{op.Title}*\nДо выполнения осталось: {timeLeft:g}";
                        await _bot.SendMessage(chatId, message, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown, cancellationToken: cancellationToken);
                        _lastReminderSent[op.Id] = DateTime.UtcNow;
                    }
                }
            }
        }
    }

    private bool ShouldSendReminder(ReminderType type, TimeSpan timeLeft)
    {
        return type switch
        {
            ReminderType.FifteenMinutes => timeLeft.TotalMinutes <= 15 && timeLeft.TotalMinutes > 13,
            ReminderType.OneHour => timeLeft.TotalMinutes <= 60 && timeLeft.TotalMinutes > 58,
            ReminderType.OneDay => timeLeft.TotalHours <= 24 && timeLeft.TotalHours > 22,
            _ => false
        };
    }
}
