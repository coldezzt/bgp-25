using Grpc.Net.Client.Balancer;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Reglamentator.Bot.Services;

/// <summary>
/// Основной сервис Telegram-бота, реализующий обработку команд, диалогов и работу с gRPC backend.
/// </summary>
public class TelegramBotService
{
    private static readonly string InfoMessage = File.ReadAllText("Resources/infoMessage.md");
    private readonly ITelegramBotClient _botClient;
    private readonly Operation.OperationClient _operationClient;
    private readonly Reminder.ReminderClient _reminderClient;
    private readonly User.UserClient _userClient;
    private readonly DialogService _dialogService;

    /// <summary>
    /// Создаёт экземпляр TelegramBotService.
    /// </summary>
    /// <param name="botClient">Клиент Telegram-бота</param>
    /// <param name="operationClient">gRPC клиент операций</param>
    /// <param name="reminderClient">gRPC клиент напоминаний</param>
    /// <param name="userClient">gRPC клиент пользователей</param>
    public TelegramBotService(ITelegramBotClient botClient, Operation.OperationClient operationClient,
        Reminder.ReminderClient reminderClient, User.UserClient userClient)
    {
        _botClient = botClient;
        _operationClient = operationClient;
        _reminderClient = reminderClient;
        _userClient = userClient;
        _dialogService = new DialogService(_botClient, _operationClient, reminderClient);
    }

    /// <summary>
    /// Запускает основной цикл Telegram-бота.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var receiverOptions = new ReceiverOptions();
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
        var text = message.Text?.Trim() ?? "";

        if (await TryHandleCancelCommand(chatId, text, ct)) return;
        if (await TryHandleActiveDialog(chatId, message, ct)) return;
        if (await TryHandleSlashCommand(chatId, text, ct)) return;
        if (await TryHandleMainMenuButton(chatId, text, ct)) return;

        await SendMessage(chatId, "Неизвестная команда. Используйте /start", null, ct);
    }
    
    private async Task<bool> TryHandleCancelCommand(long chatId, string text, CancellationToken ct)
    {
        if (!text.Equals("/cancel", StringComparison.OrdinalIgnoreCase))
            return false;
        
        _dialogService.CancelDialog(chatId);
        await SendMessage(chatId, "Диалог отменён.", MainKeyboard, ct);
        return true;
    }
    
    private async Task<bool> TryHandleActiveDialog(long chatId, Message message, CancellationToken ct)
    {
        if (!_dialogService.HasActiveDialog(chatId))
            return false;

        if (await _dialogService.HandleDialogMessage(message, ct))
            return true;

        await SendMessage(chatId, "Пожалуйста, завершите текущий диалог или введите /cancel для отмены.", null, ct);
        return true;
    }
    
    private async Task<bool> TryHandleSlashCommand(long chatId, string text, CancellationToken ct)
    {
        if (!text.StartsWith("/"))
            return false;

        var command = text.Split(' ')[0];
        switch (command)
        {
            case "/start":
                await HandleStartCommand(chatId, ct);
                return true;
            case "/list":
                await HandleListCommand(chatId, ct);
                return true;
            case "/add":
                await _dialogService.StartAddDialog(chatId, ct);
                return true;
            case "/delete":
                await HandleDeleteCommand(chatId, text, ct);
                return true;
            case "/today":
                await HandleFilteredListCommand(chatId, "today", ct);
                return true;
            case "/week":
                await HandleFilteredListCommand(chatId, "week", ct);
                return true;
            case "/month":
                await HandleFilteredListCommand(chatId, "month", ct);
                return true;
            case "/edit":
                await _dialogService.StartEditDialog(chatId, ct);
                return true;
            case "/history":
                await HandleHistoryCommand(chatId, ct);
                return true;
            default:
                return false;
        }
    }
    private async Task<bool> TryHandleMainMenuButton(long chatId, string text,CancellationToken ct)
    {
        switch (text)
        {
            case "📋 Список задач":
                await HandleListCommand(chatId, ct);
                return true;
            case "➕ Добавить":
                await _dialogService.StartAddDialog(chatId, ct);
                return true;
            case "📅 Сегодня":
                await HandleFilteredListCommand(chatId, "today", ct);
                return true;
            case "🗓️ Неделя":
                await HandleFilteredListCommand(chatId, "week", ct);
                return true;
            case "📆 Месяц":
                await HandleFilteredListCommand(chatId, "month", ct);
                return true;
            case "✏️ Изменить":
                await _dialogService.StartEditDialog(chatId, ct);
                return true;
            case "❌ Удалить":
                await HandleDeleteCommand(chatId, text, ct);
                return true;
            case "ℹ️ Инструкция":
                await SendInfoMessage(chatId, ct);
                return true;
            default:
                return false;
        }
    }
    
    private async Task HandleStartCommand(long chatId, CancellationToken ct)
    {
        var result = await _userClient.CreateUserAsync(new CreateUserRequest{TelegramId = chatId});
        if (result.Status.IsSuccess)
        {
            await SendMessage(chatId, "Добро пожаловать! Выберите действие:", MainKeyboard, ct);
            return;
        }

        await _botClient.SendMessage(chatId, "Не удалось зарегистрироваться", cancellationToken: ct);
    }

    private async Task HandleHistoryCommand(long chatId, CancellationToken ct)
    {
        var result = await _operationClient.GetOperationHistoryAsync(new OperationHistoryRequest { TelegramId = chatId });
        if (!result.Status.IsSuccess)
        {
            await SendMessage(chatId, "Не удалось получить историю операций", ct: ct);
            return;
        }
        var history = result.History;
        if (history.Count == 0)
        {
            await SendMessage(chatId, "Нет задач.", null, ct);
            return;
        }
        var operation = $"{history[0].Operation.Theme} - {history[0].Operation.StartDate} \n";
        var list = history[0].Operation.Id + string.Join("\n",
            history.Select(op => $"• [{op.Id}] {op.Result} : {op.ScheduledAt} - {op.ExecutedAt}"));
        await SendMessage(chatId, "Ваща история задач", operation + list, ct);
        
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
            await _operationClient.DeleteOperationAsync(request);
            await SendMessage(chatId, $"✅ Задача {id} удалена.", null, ct);
        }
        catch
        {
            await SendMessage(chatId, $"❌ Не удалось удалить задачу {id}.", null, ct);
        }
    }   
    
    private async Task HandleListCommand(long chatId, CancellationToken ct)
    {
        var request = new PlanedOperationsRequest { TelegramId = chatId, Range = TimeRange.Month };
        var response = await _operationClient.GetPlanedOperationsAsync(request);
        
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

        var response = await _operationClient.GetPlanedOperationsAsync(request);

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
        await SendMessage(chatId, InfoMessage, MainKeyboard, ct);
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