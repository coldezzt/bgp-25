using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Types;
using Google.Protobuf.WellKnownTypes;
using Telegram.Bot.Types.ReplyMarkups;
using Enum = System.Enum;

namespace Reglamentator.Bot.Services;

/// <summary>
/// Сервис для управления диалогами бота.
/// </summary>
public class DialogService
{
    private readonly ITelegramBotClient _botClient;
    private readonly Operation.OperationClient _operationClient;
    private readonly Reminder.ReminderClient _reminderClient;
    private readonly ConcurrentDictionary<long, DialogState> _userDialogs = new();

    /// <summary>
    /// Инициализирует новый экземпляр DialogService.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="operationClient">Клиент gRPC для взаимодействия с API операции.</param>
    /// <param name="reminderClient">Клиент gRPC для взаимодействия с API напоминаний</param>
    public DialogService(ITelegramBotClient botClient, Operation.OperationClient operationClient, 
        Reminder.ReminderClient reminderClient)
    {
        _botClient = botClient;
        _operationClient = operationClient;
        _reminderClient = reminderClient;
    }

    /// <summary>
    /// Запускает диалог добавления задачи.
    /// </summary>
    /// <param name="chatId">ID чата пользователя</param>
    /// <param name="ct">Токен отмены</param>
    public async Task StartAddDialog(long chatId, CancellationToken ct = default)
    {
        _userDialogs[chatId] = new DialogState
        {
            Step = DialogStep.AskTheme, 
            ActionType = ActionType.Create,
            DialogObject = DialogObject.Operation
        };
        await _botClient.SendMessage(chatId, "Введите тему задачи:", cancellationToken: ct);
    }
    
    /// <summary>
    /// Запускает диалог изменения задачи
    /// </summary>
    /// <param name="chatId">ID чата пользователя</param>
    /// <param name="text">Текст сообщения</param>
    /// <param name="ct">Токен отмены</param>
    public async Task StartEditDialog(long chatId, CancellationToken ct = default)
    {
        _userDialogs[chatId] = new DialogState
        {
            Step = DialogStep.AskOperationId,
            ActionType = ActionType.Update,
            DialogObject = DialogObject.Operation
        };
        await _botClient.SendMessage(chatId, "Введите id задачи:", cancellationToken: ct);
    }
    
    /// <summary>
    /// Запускает диалог создания напоминания
    /// </summary>
    /// <param name="chatId">ID чата пользователя</param>
    /// <param name="ct">Токен отмены</param>
    public async Task StartAddReminderDialog(long chatId, CancellationToken ct = default)
    {
        _userDialogs[chatId] = new DialogState
        {
            Step = DialogStep.AskOperationId, 
            ActionType = ActionType.Create,
            DialogObject = DialogObject.Reminder
        };
        await _botClient.SendMessage(chatId, "Введите id операции", cancellationToken: ct);
    }
    /// <summary>
    /// Запускает диалог обновления напоминания
    /// </summary>
    /// <param name="chatId">ID чата пользователя</param>
    /// <param name="ct">токен отмены</param>
    public async Task StartEditReminderDialog(long chatId, CancellationToken ct = default)
    {
        _userDialogs[chatId] = new DialogState
        {
            Step = DialogStep.AskOperationId, 
            ActionType = ActionType.Update,
            DialogObject = DialogObject.Reminder
        };
        await _botClient.SendMessage(chatId, "Введите id операции", cancellationToken: ct);
    }
    
    /// <summary>
    ///Запускает диалог создания 
    /// </summary>
    /// <param name="chatId">ID чата пользователя</param>
    /// <param name="ct">токен отмены</param>
    public async Task StartDeleteReminderDialog(long chatId, CancellationToken ct = default)
    {
        _userDialogs[chatId] = new DialogState
        {
            Step = DialogStep.AskOperationId,
            ActionType = ActionType.Delete,
            DialogObject = DialogObject.Reminder
        };
        await _botClient.SendMessage(chatId, "Введите id операции", cancellationToken: ct);
    }
    /// <summary>
    /// Обрабатывает шаг диалога.
    /// </summary>
    /// <param name="message">Сообщение пользователя</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>True, если сообщение обработано как часть диалога</returns>
    public async Task<bool> HandleDialogMessage(Message message, CancellationToken ct = default)
    {
        var chatId = message.Chat.Id;
        if (!_userDialogs.TryGetValue(chatId, out var state))
            return false;
        
        switch (state.Step)
        {
            case DialogStep.AskOperationId:
                await HandleOperationIdStep(message, state, chatId, ct);
                break;
            case DialogStep.AskReminderId:
                await HandleReminderIdStep(message, state, chatId, ct);
                break;
            case DialogStep.AskTheme:
                await HandleThemeStep(message, state, chatId, ct);
                break;
            case DialogStep.AskDescription:
                await HandleDescriptionStep(message, state, chatId, ct);
                break;
            case DialogStep.AskDate:
                await HandleDateStep(message, state, chatId, ct);
                break;
            case DialogStep.AskTimeRange:
                await HandleTimeRangeStep(message, state, chatId, ct);
                break;
            case DialogStep.AskAddReminder:
                await HandleAddReminderStep(message, state, chatId, ct);
                break;
            case DialogStep.AskReminderMessage:
                await HandleReminderMessageStep(message, state, chatId, ct);
                break;
            case DialogStep.AskReminderTime:
                await HandleReminderTimeStep(message, state, chatId, ct);
                break;
        }
        return true;
    }
    
    /// <summary>
    /// Проверяет, активен ли диалог для пользователя.
    /// </summary>
    /// <param name="chatId">ID чата пользователя</param>
    /// <returns>True, если диалог активен</returns>
    public bool HasActiveDialog(long chatId) => _userDialogs.ContainsKey(chatId);

    /// <summary>
    /// Отменяет текущий диалог для пользователя.
    /// </summary>
    /// <param name="chatId">ID чата пользователя</param>
    public void CancelDialog(long chatId)
    {
        _userDialogs.TryRemove(chatId, out _);
    }

    private async Task HandleReminderIdStep(Message message, DialogState state, long chatId, CancellationToken ct)
    {
        if ( !long.TryParse(message.Text, out long reminderId))
        {
            await _botClient.SendMessage(chatId, "Используйте правильный формат id", cancellationToken: ct);
            return;
        }

        if (state.ActionType == ActionType.Delete)
        {
            var result = _reminderClient.DeleteOperation(new DeleteReminderRequest
            {
                OperationId = state.OperationId, 
                ReminderId = reminderId, 
                TelegramId = chatId
            });
            if (!result.Status.IsSuccess)
            {
                await CompleteDialog(chatId, "Не удалось удалить напоминание",ct);
                return;
            }

            await CompleteDialog(chatId, "Напоминание удалено", ct);
        }
        var reminder = state.Reminders.FirstOrDefault(x => x.Id == reminderId);
        if (reminder == null)
        {
            await CompleteDialog(chatId, "Не удалось найти напоминание",ct);
            return;
        }
        state.ReminderId = reminderId;
        state.ReminderCron = reminder.OffsetBeforeExecution;
        state.ReminderMessage = reminder.MessageTemplate;
        state.Step = DialogStep.AskReminderMessage;
        await SendCurrentReminderState(chatId, ct);
        await _botClient.SendMessage(chatId, "Введите сообщение нового напоминания", cancellationToken: ct);
    }
    private async Task SendCurrentOperationState(long chatId, CancellationToken ct)
    {
        if (!_userDialogs.TryGetValue(chatId, out var state)) return;
        var remindersInfo = string.Join("\n", state.Reminders.Select(r => 
            $"  ⏰ [{r.Id}] {r.MessageTemplate} (за {r.OffsetBeforeExecution} до)"));
        var message = $"Текущие значения операции:\n\n" +
                      $"Тема: {state.Theme}\n" +
                      $"Описание: {state.Description}\n" +
                      $"Дата: {state.Date:yyyy-MM-dd}\n" +
                      $"Периодичность: {state.OperationCron}"+
                      $"Напоминания:\n{remindersInfo}";;
        
        await _botClient.SendMessage(chatId, message, cancellationToken: ct);
    }
    private async Task SendCurrentReminderState(long chatId, CancellationToken ct)
    {
        if (!_userDialogs.TryGetValue(chatId, out var state)) return;

        var message = $"Текущие значения напоминания:\n\n" +
                      $"Текст: {state.ReminderMessage}\n" +
                      $"Время напоминания: {state.ReminderCron} до события";

        await _botClient.SendMessage(chatId, message, cancellationToken: ct);
    }
    private async Task HandleOperationIdStep(Message message, DialogState state, long chatId, CancellationToken ct)
    {
        if ( !long.TryParse(message.Text, out long operationId))
        {
            await _botClient.SendMessage(chatId, "Используйте правильный формат id", cancellationToken: ct);
            return;
        }
        var operation = await _operationClient.GetOperationAsync(new GetOperationRequest {OperationId = operationId, TelegramId = chatId});
        if (!operation.Status.IsSuccess)
        {
            await _botClient.SendMessage(chatId, "Не удалось получить операцию, возможно она не существует", cancellationToken: ct);
            return;
        }
        state.OperationId = operationId;
        state.Theme = operation.Operation.Theme;
        state.Description = operation.Operation.Description;
        state.Date = operation.Operation.StartDate.ToDateTime();
        state.OperationCron = operation.Operation.Cron;
        state.Reminders = operation.Operation.Reminders;
        await SendCurrentOperationState(chatId, ct);
        if (state.DialogObject == DialogObject.Operation)
        {
            state.Step = DialogStep.AskTheme;
            await _botClient.SendMessage(chatId, "Введите новую тему задачи (или оставьте текущую):", cancellationToken: ct);
        }
        else if (state is { DialogObject: DialogObject.Reminder, ActionType: ActionType.Create })
        {
            state.Step = DialogStep.AskReminderMessage;
            await _botClient.SendMessage(chatId, "Введите сообщение нового напоминания", cancellationToken: ct);
        }
        else if (state is {DialogObject: DialogObject.Reminder, ActionType:ActionType.Update})
        {
            state.Step = DialogStep.AskReminderId;
            await _botClient.SendMessage(chatId, "Введите id напоминания, которое хотите изменить", cancellationToken: ct);
        }
        else
        {
            state.Step = DialogStep.AskReminderId;
            await _botClient.SendMessage(chatId, "Введите id напоминания, которое хотите удалить", cancellationToken: ct);
        }
        
    }
    private async Task HandleDescriptionStep(Message message, DialogState state, long chatId, CancellationToken ct)
    {
        state.Description = message.Text ?? "";
        state.Step = DialogStep.AskDate;
        if (state.ActionType == ActionType.Create)
        {
            await _botClient.SendMessage(chatId, "Введите дату (гггг-мм-дд):", cancellationToken: ct);
        }
        else if (state.ActionType == ActionType.Update)
        {
            await SendCurrentOperationState(chatId, ct);
            await _botClient.SendMessage(chatId, "Введите новую дату (гггг-мм-дд) (или оставьте текущую):", cancellationToken: ct);
        }
    }
    private async Task HandleThemeStep(Message message, DialogState state, long chatId, CancellationToken ct)
    {
        state.Theme = message.Text ?? "";
        state.Step = DialogStep.AskDescription;
        if (state.ActionType == ActionType.Create)
        {
            await _botClient.SendMessage(chatId, "Введите описание задачи:", cancellationToken: ct);
        }
        else if (state.ActionType == ActionType.Update)
        {
            await SendCurrentOperationState(chatId, ct);
            await _botClient.SendMessage(chatId, "Введите новое описание задачи (или оставьте текущее):", cancellationToken: ct);
        }
    }
    
    private async Task HandleDateStep(Message message, DialogState state, long chatId, CancellationToken ct)
    {
        if (DateTime.TryParse(message.Text, out var date))
        {
            state.Date = date;
            state.Step = DialogStep.AskTimeRange;
            if (state.ActionType == ActionType.Create)
            {
                await AskTimeRangeSelection(chatId, ct);
            }
            else if (state.ActionType == ActionType.Update)
            {
                await SendCurrentOperationState(chatId, ct);
                await AskTimeRangeSelection(chatId, ct, "Выберите новую периодичность (или оставьте текущую):");
            }
        }
        else
        {
            await _botClient.SendMessage(chatId, "Некорректная дата. Введите в формате гггг-мм-дд:", cancellationToken: ct);
        }
    }

    private async Task HandleTimeRangeStep(Message message, DialogState state, long chatId, CancellationToken ct)
    {
        if (Enum.TryParse<TimeRange>(message.Text, out var timeRange) && 
            Enum.IsDefined(typeof(TimeRange), timeRange))
        {
            state.OperationCron = timeRange;
            if (state.ActionType == ActionType.Create)
            {
                await CreateOperationAndCompleteDialog(state, chatId, ct);
            }
            else
            {
                await UpdateOperationAndCompleteDialog(state, chatId, ct);
            }
        }
        else
        {
            await AskTimeRangeSelection(chatId, ct, "Некорректный выбор. Пожалуйста, выберите периодичность:");
        }
    }
    
   
    private async Task AskTimeRangeSelection(long chatId, CancellationToken ct, string messageText = "Выберите периодичность задачи:")
    {
        var keyboard = new ReplyKeyboardMarkup(new[]
        {
            new[] { new KeyboardButton(TimeRange.Min15.ToString()) },
            new[] { new KeyboardButton(TimeRange.Hour.ToString()) },
            new[] { new KeyboardButton(TimeRange.Day.ToString()) },
            new[] { new KeyboardButton(TimeRange.Week.ToString()) },
            new[] { new KeyboardButton(TimeRange.Month.ToString()) },
            new[] { new KeyboardButton(TimeRange.None.ToString()) }
        })
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true
        };

        await _botClient.SendMessage(
            chatId,
            messageText,
            replyMarkup: keyboard,
            cancellationToken: ct);
    }
    
    private async Task HandleAddReminderStep(Message message, DialogState state, long chatId, CancellationToken ct)
    {
        if (message.Text?.Equals("Да", StringComparison.OrdinalIgnoreCase) ?? false)
        {
            state.Step = DialogStep.AskReminderMessage;
            await _botClient.SendMessage(chatId, "Введите текст напоминания:", cancellationToken: ct);
        }
        else
        {
            await CompleteDialog(chatId, "✅ Задача сохранена. Напоминание не добавлено.", ct);
        }
    }
    private async Task HandleReminderMessageStep(Message message, DialogState state, long chatId, CancellationToken ct)
    {
        state.ReminderMessage = message.Text;
        state.Step = DialogStep.AskReminderTime;
        
        var keyboard = new ReplyKeyboardMarkup(new[]
        {
            new[] { new KeyboardButton(TimeRange.Min15.ToString()) },
            new[] { new KeyboardButton(TimeRange.Hour.ToString()) },
            new[] { new KeyboardButton(TimeRange.Day.ToString()) }
        })
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true
        };
        
        if (state.ActionType == ActionType.Update)
        {
            await SendCurrentReminderState(chatId, ct);
                
        }
        await _botClient.SendMessage(
            chatId,
            "За сколько времени напомнить?",
            replyMarkup: keyboard,
            cancellationToken: ct);
    }
    
    private async Task HandleReminderTimeStep(Message message, DialogState state, long chatId, CancellationToken ct)
    {
        TimeRange? timeRange = null;
        
        if (message.Text != "Оставить текущее" && 
            Enum.TryParse<TimeRange>(message.Text, out var parsedTime))
        {
            timeRange = parsedTime;
        }
        else if (message.Text != "Оставить текущее")
        {
            await _botClient.SendMessage(
                chatId, 
                "Неверный выбор. Пожалуйста, выберите время из предложенных вариантов.", 
                cancellationToken: ct);
            return;
        }

        if (state.ActionType == ActionType.Create)
        {
            await CreateReminder(state, chatId, timeRange ?? state.ReminderCron, ct);
        }
        else
        {
            await UpdateReminder(state, chatId, timeRange, ct);
        }
    }

    private async Task CreateReminder(DialogState state, long chatId, TimeRange timeRange, CancellationToken ct)
    {
        var request = new AddReminderRequest
        {
            TelegramId = chatId,
            OperationId = state.OperationId,
            Reminder = new CreateReminderDto
            {
                MessageTemplate = state.ReminderMessage,
                OffsetBeforeExecution = timeRange
            }
        };

        var response = await _reminderClient.AddReminderAsync(request, cancellationToken: ct);
        
        if (response.Status.IsSuccess)
        {
            await CompleteDialog(
                chatId, 
                $"✅ Напоминание добавлено: \"{state.ReminderMessage}\" (за {timeRange} до события)", 
                ct);
        }
        else
        {
            await CompleteDialog(
                chatId, 
                "❌ Не удалось добавить напоминание", 
                ct);
        }
    }

    private async Task UpdateReminder(DialogState state, long chatId, TimeRange? timeRange, CancellationToken ct)
    {
        var request = new UpdateReminderRequest
        {
            TelegramId = chatId,
            Reminder = new UpdateReminderDto
            {
                Id = state.ReminderId,
                MessageTemplate = state.ReminderMessage,
                OffsetBeforeExecution = timeRange ?? state.ReminderCron
            }
        };

        var response = await _reminderClient.UpdateOperationAsync(request, cancellationToken: ct);
        
        if (response.Status.IsSuccess)
        {
            await CompleteDialog(
                chatId, 
                $"✅ Напоминание обновлено: \"{state.ReminderMessage}\" (за {timeRange ?? state.ReminderCron} до события)", 
                ct);
        }
        else
        {
            await CompleteDialog(
                chatId, 
                "❌ Не удалось обновить напоминание", 
                ct);
        }
    }
    
    private async Task CreateOperationAndCompleteDialog(DialogState state, long chatId, CancellationToken ct)
    {
        var request = new CreateOperationRequest
        {
            TelegramId = chatId,
            Operation = new CreateOperationDto
            {
                Theme = state.Theme,
                Description = state.Description,
                StartDate = Timestamp.FromDateTime(DateTime.SpecifyKind(state.Date, DateTimeKind.Utc)),
                Cron = state.OperationCron
            }
        };
        
        var result = await _operationClient.CreateOperationAsync(request, cancellationToken: ct);
        if (!result.Status.IsSuccess)
        {
            await CompleteDialog(chatId, "❌ Не удалось создать задачу", ct);
            return;
        }
        state.OperationId = result.Operation.Id;
        state.Step = DialogStep.AskAddReminder;
        
        var keyboard = new ReplyKeyboardMarkup(new[]
        {
            new[] { new KeyboardButton("Да") },
            new[] { new KeyboardButton("Нет") }
        })
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true
        };

        await _botClient.SendMessage(
            chatId,
            "✅ Задача создана! Хотите добавить напоминание?",
            replyMarkup: keyboard,
            cancellationToken: ct);
    }
    
    private async Task UpdateOperationAndCompleteDialog(DialogState state, long chatId, CancellationToken ct)
    {
        var request = new UpdateOperationRequest
        {
            TelegramId = chatId,
            Operation = new UpdateOperationDto
            {
                Id = state.OperationId,
                Theme = state.Theme,
                Description = state.Description,
                StartDate = Timestamp.FromDateTime(DateTime.SpecifyKind(state.Date, DateTimeKind.Utc)),
                Cron = state.OperationCron
            }
        };
        
        var result = await _operationClient.UpdateOperationAsync(request, cancellationToken: ct);
        if (!result.Status.IsSuccess)
        {
            await CompleteDialog(chatId, "❌ Не удалось обновить задачу", ct);
            return;
        }
        state.Step = DialogStep.AskAddReminder;
        
        var keyboard = new ReplyKeyboardMarkup(new[]
        {
            new[] { new KeyboardButton("Да") },
            new[] { new KeyboardButton("Нет") }
        })
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true
        };

        await _botClient.SendMessage(
            chatId,
            "✅ Задача обновлена! Хотите добавить напоминание?",
            replyMarkup: keyboard,
            cancellationToken: ct);
    }
    private async Task CompleteDialog(long chatId, string message, CancellationToken ct)
    {
        await _botClient.SendMessage(
            chatId, 
            message,
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: ct);
            
        _userDialogs.TryRemove(chatId, out _);
    }
    private class DialogState
    {
        public DialogStep Step { get; set; }
        public ActionType ActionType { get; set; }
        public DialogObject DialogObject { get; set; }
        public long OperationId { get; set; }
        public long ReminderId { get; set; }
        public string? Theme { get; set; }
        public string? Description { get; set; }
        public DateTime Date { get; set; }
        public TimeRange OperationCron { get; set; }
        public TimeRange ReminderCron { get; set; }
        public IEnumerable<ReminderDto> Reminders { get; set; }
        public string? ReminderMessage { get; set; }
        
    }

    private enum DialogStep
    {
        AskOperationId,
        AskReminderId,
        AskTheme,
        AskDescription,
        AskDate,
        AskTimeRange,
        AskAddReminder,
        AskReminderMessage,
        AskReminderTime
    }

    private enum ActionType
    {
        Create,
        Update,
        Delete
    }

    private enum DialogObject
    {
        Reminder,
        Operation
    }
}
