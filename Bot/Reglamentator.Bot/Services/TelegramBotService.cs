using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Reglamentator.Bot;
using Grpc.Net.Client;
using Reglamentator.Bot.Services;

/// <summary>
/// Основной сервис Telegram-бота, реализующий обработку команд, диалогов и работу с gRPC backend.
/// </summary>
public class TelegramBotService
{
    private readonly ITelegramBotClient _botClient;
    private readonly Operation.OperationClient _grpcClient;
    private readonly Dictionary<long, string> _userStates = new();
    private readonly DialogService _dialogService;

    /// <summary>
    /// Создаёт экземпляр TelegramBotService.
    /// </summary>
    /// <param name="botClient">Клиент Telegram-бота</param>
    /// <param name="grpcClient">gRPC клиент</param>
    public TelegramBotService(ITelegramBotClient botClient, Operation.OperationClient grpcClient)
    {
        _botClient = botClient;
        _grpcClient = grpcClient;
        _dialogService = new DialogService(_botClient, _grpcClient);
    }

    /// <summary>
    /// Запускает основной цикл Telegram-бота.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var receiverOptions = new ReceiverOptions { AllowedUpdates = { } };
        _botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cancellationToken
        );
        var me = await _botClient.GetMe();
        Console.WriteLine($"Start listening for @{me.Username}");
    }

    private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        if (update.Type == UpdateType.Message && update.Message?.Text != null)
        {
            await HandleMessage(update.Message, ct);
        }
    }

    private async Task HandleMessage(Message message, CancellationToken ct)
    {
        var chatId = message.Chat.Id;
        var text = message.Text ?? "";

        if (text.StartsWith("/") || IsMainMenuButton(text))
            _dialogService.CancelDialog(chatId);

        if (text.Equals("/cancel", StringComparison.OrdinalIgnoreCase))
        {
            _dialogService.CancelDialog(chatId);
            await SendMessage(chatId, "Диалог отменён.", MainKeyboard, ct);
            return;
        }

        if (_dialogService.HasActiveDialog(chatId))
        {
            if (await _dialogService.HandleDialogMessage(message, ct))
                return;
        }

        if (text.StartsWith("/"))
        {
            var command = text.Split(' ')[0];
            switch (command)
            {
                case "/start":
                    await HandleStartCommand(chatId, ct);
                    return;
                case "/list":
                    await HandleListCommand(chatId, ct);
                    return;
                case "/add":
                    await _dialogService.StartAddDialog(chatId, ct);
                    return;
                case "/delete":
                    await HandleDeleteCommand(chatId, text, ct);
                    return;
                case "/today":
                    await HandleFilteredListCommand(chatId, "today", ct);
                    return;
                case "/week":
                    await HandleFilteredListCommand(chatId, "week", ct);
                    return;
                case "/month":
                    await HandleFilteredListCommand(chatId, "month", ct);
                    return;
                case "/edit":
                    await HandleEditCommand(message, text, ct);
                    return;
            }
        }
        else
        {
            switch (text)
            {
                case "📋 Список задач":
                    await HandleListCommand(chatId, ct);
                    return;
                case "➕ Добавить":
                    await _dialogService.StartAddDialog(chatId, ct);
                    return;
                case "📅 Сегодня":
                    await HandleFilteredListCommand(chatId, "today", ct);
                    return;
                case "🗓️ Неделя":
                    await HandleFilteredListCommand(chatId, "week", ct);
                    return;
                case "📆 Месяц":
                    await HandleFilteredListCommand(chatId, "month", ct);
                    return;
                case "✏️ Изменить":
                    await SendMessage(chatId, "Введите: /edit <ID>", MainKeyboard, ct);
                    return;
                case "❌ Удалить":
                    await SendMessage(chatId, "Введите: /delete <ID>", MainKeyboard, ct);
                    return;
                case "ℹ️ Инструкция":
                    await SendInfoMessage(chatId, ct);
                    return;
            }
        }

        await SendMessage(chatId, "Неизвестная команда. Используйте /start", null, ct);
    }

    private bool IsMainMenuButton(string text)
    {
        return text == "📋 Список задач" || text == "➕ Добавить" || text == "📅 Сегодня" ||
               text == "🗓️ Неделя" || text == "📆 Месяц" || text == "✏️ Изменить" || text == "❌ Удалить";
    }

    private async Task HandleStartCommand(long chatId, CancellationToken ct)
    {
        _userStates.Remove(chatId);
        await SendMessage(chatId, "Добро пожаловать! Выберите действие:", MainKeyboard, ct);
    }

    private async Task HandleDeleteCommand(long chatId, string text, CancellationToken ct)
    {
        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2 || !int.TryParse(parts[1], out int id))
        {
            await SendMessage(chatId, "Используйте: /delete <ID>", null, ct);
            return;
        }

        try
        {
            var request = new DeleteOperationRequest
            {
                TelegramId = chatId,
                OperationId = id
            };
            await _grpcClient.DeleteOperationAsync(request);
            await SendMessage(chatId, $"✅ Задача {id} удалена.", null, ct);
        }
        catch
        {
            await SendMessage(chatId, $"❌ Не удалось удалить задачу {id}.", null, ct);
        }
    }

    private async Task HandleEditCommand(Message message, string text, CancellationToken ct)
    {
        var chatId = message.Chat.Id;
        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2 || !int.TryParse(parts[1], out int id))
        {
            await SendMessage(chatId, "Используйте: /edit <ID>", null, ct);
            return;
        }

        try
        {
            var request = new UpdateOperationRequest
            {
                Operation = new UpdateOperationDto
                {
                    Id = id,
                    Theme = "Обновленная задача",
                    Description = "Обновленное описание",
                    StartDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(
                        DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc))
                }
            };
            await _grpcClient.UpdateOperationAsync(request);
            await SendMessage(chatId, $"✅ Задача {id} обновлена.", null, ct);
        }
        catch
        {
            await SendMessage(chatId, $"❌ Ошибка при обновлении задачи {id}.", null, ct);
        }
    }

    private async Task HandleListCommand(long chatId, CancellationToken ct)
    {
        var request = new PlanedOperationsRequest { TelegramId = chatId, Range = TimeRange.Month };
        var response = await _grpcClient.GetPlanedOperationsAsync(request);

        if (response.Instances.Count == 0)
        {
            await SendMessage(chatId, "Нет задач.", null, ct);
            return;
        }

        var list = string.Join("\n", response.Instances.Select(op =>
        {
            var dto = op.Operation;
            var dueDate = dto.StartDate?.ToDateTime().ToString("g") ?? "нет даты";
            return $"• [{dto.Id}] {dto.Theme} — {dueDate}";
        }));

        await SendMessage(chatId, "Ваши задачи:\n" + list, null, ct);
    }

    private async Task HandleFilteredListCommand(long chatId, string filter, CancellationToken ct)
    {
        var request = new PlanedOperationsRequest
        {
            TelegramId = chatId,
            Range = filter switch
            {
                "today" => TimeRange.Day,
                "week" => TimeRange.Week,
                "month" => TimeRange.Month,
                _ => TimeRange.Month
            }
        };

        var response = await _grpcClient.GetPlanedOperationsAsync(request);

        if (response.Instances.Count == 0)
        {
            string msg = filter switch
            {
                "today" => "На сегодня задач нет.",
                "week" => "На неделю задач нет.",
                "month" => "На месяц задач нет.",
                _ => "Нет задач."
            };
            await SendMessage(chatId, msg, null, ct);
            return;
        }

        var list = string.Join("\n", response.Instances.Select(op =>
        {
            var dto = op.Operation;
            var dueDate = dto.StartDate?.ToDateTime().ToString(filter == "today" ? "t" : "g") ?? "нет даты";
            return $"• [{dto.Id}] {dto.Theme} — {dueDate}";
        }));

        string header = filter switch
        {
            "today" => "Задачи на сегодня:\n",
            "week" => "Задачи на неделю:\n",
            "month" => "Задачи на месяц:\n",
            _ => "Задачи:\n"
        };

        await SendMessage(chatId, header + list, null, ct);
    }

    private async Task SendMessage(long chatId, string text, ReplyKeyboardMarkup? markup = null, CancellationToken ct = default)
    {
        await _botClient.SendMessage(chatId, text, replyMarkup: markup, cancellationToken: ct);
    }

    private async Task SendInfoMessage(long chatId, CancellationToken ct)
    {
        string info = "ℹ️ <b>Инструкция по использованию бота:</b>\n\n" +
                      "• <b>Добавить задачу</b>: нажмите \"➕ Добавить\" и следуйте диалогу.\n" +
                      "• <b>Список задач</b>: нажмите \"📋 Список задач\".\n" +
                      "• <b>Фильтры</b>: используйте кнопки \"Сегодня\", \"Неделя\", \"Месяц\".\n" +
                      "• <b>Изменить/Удалить</b>: используйте кнопки \"✏️ Изменить\" или \"❌ Удалить\" и следуйте подсказкам.\n" +
                      "• <b>Отмена диалога</b>: введите /cancel.\n\n" +
                      "Бот поддерживает команды: /start, /list, /add, /delete, /edit, /today, /week, /month.\n" +
                      "Если возникли вопросы — напишите /start для повторной инструкции.";
        await SendMessage(chatId, info, MainKeyboard, ct);
    }

    private Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, HandleErrorSource source, CancellationToken ct)
    {
        Console.WriteLine(exception.ToString());
        return Task.CompletedTask;
    }

    private static readonly ReplyKeyboardMarkup MainKeyboard = new(new[]
    {
        new KeyboardButton[] { "📋 Список задач", "➕ Добавить" },
        new KeyboardButton[] { "📅 Сегодня", "🗓️ Неделя", "📆 Месяц" },
        new KeyboardButton[] { "✏️ Изменить", "❌ Удалить", "ℹ️ Инструкция"  },
    })
    {
        ResizeKeyboard = true,
        OneTimeKeyboard = false
    };
}
