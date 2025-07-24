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
    private readonly Operation.OperationClient _grpcClient;
    private readonly ConcurrentDictionary<long, DialogState> _userDialogs = new();

    /// <summary>
    /// Инициализирует новый экземпляр DialogService.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="grpcClient">Клиент gRPC для взаимодействия с API.</param>
    public DialogService(ITelegramBotClient botClient, Operation.OperationClient grpcClient)
    {
        _botClient = botClient;
        _grpcClient = grpcClient;
    }

    /// <summary>
    /// Запускает диалог добавления задачи.
    /// </summary>
    /// <param name="chatId">ID чата пользователя</param>
    /// <param name="ct">Токен отмены</param>
    public async Task StartAddDialog(long chatId, CancellationToken ct = default)
    {
        _userDialogs[chatId] = new DialogState { Step = DialogStep.AskTheme };
        await _botClient.SendMessage(chatId, "Введите тему задачи:", cancellationToken: ct);
    }
    
    /// <summary>
    /// Запускает диалог изменения задачи
    /// </summary>
    /// <param name="chatId">ID чата пользователя</param>
    /// <param name="text">Текст сообщения</param>
    /// <param name="ct">Токен отмены</param>
    public async Task StartEditDialog(long chatId, string text, CancellationToken ct = default)
    {
        _userDialogs[chatId] = new DialogState { Step = DialogStep.AskOperationId };
        await _botClient.SendMessage(chatId, "Введите id задачи:", cancellationToken: ct);
    }
    private async Task SendCurrentOperationState(long chatId, CancellationToken ct)
    {
        if (!_userDialogs.TryGetValue(chatId, out var state)) return;

        var message = $"Текущие значения операции:\n\n" +
                      $"Тема: {state.Theme}\n" +
                      $"Описание: {state.Description}\n" +
                      $"Дата: {state.Date:yyyy-MM-dd}\n" +
                      $"Периодичность: {state.Cron}";

        await _botClient.SendMessage(chatId, message, cancellationToken: ct);
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

    private async Task HandleOperationIdStep(Message message, DialogState state, long chatId, CancellationToken ct)
    {
        if ( !int.TryParse(message.Text, out int operationId))
        {
            await _botClient.SendMessage(chatId, "Используйте правильный формат id", cancellationToken: ct);
            return;
        }
        var operation = await _grpcClient.GetOperationAsync(new GetOperationRequest {OperationId = operationId, TelegramId = chatId});
        if (!operation.Status.IsSuccess)
        {
            await _botClient.SendMessage(chatId, "Не удалось получить операцию, возможно она не существует", cancellationToken: ct);
            return;
        }
        state.Step = DialogStep.AskOperationId;
        state.OperationType = OperationType.Update;
        state.OperationId = operationId;
        state.Theme = operation.Operation.Theme;
        state.Description = operation.Operation.Description;
        state.Date = operation.Operation.StartDate.ToDateTime();
        state.Cron = operation.Operation.Cron;
        await SendCurrentOperationState(chatId, ct);
        await _botClient.SendMessage(chatId, "Введите новую тему задачи (или оставьте текущую):", cancellationToken: ct);
    }
    private async Task HandleDescriptionStep(Message message, DialogState state, long chatId, CancellationToken ct)
    {
        state.Description = message.Text ?? "";
        state.Step = DialogStep.AskDate;
        if (state.OperationType == OperationType.Create)
        {
            await _botClient.SendMessage(chatId, "Введите дату (гггг-мм-дд):", cancellationToken: ct);
        }
        else if (state.OperationType == OperationType.Update)
        {
            await SendCurrentOperationState(chatId, ct);
            await _botClient.SendMessage(chatId, "Введите новую дату (гггг-мм-дд) (или оставьте текущую):", cancellationToken: ct);
        }
    }
    private async Task HandleThemeStep(Message message, DialogState state, long chatId, CancellationToken ct)
    {
        state.Theme = message.Text ?? "";
        state.Step = DialogStep.AskDescription;
        if (state.OperationType == OperationType.Create)
        {
            await _botClient.SendMessage(chatId, "Введите описание задачи:", cancellationToken: ct);
        }
        else if (state.OperationType == OperationType.Update)
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
            if (state.OperationType == OperationType.Create)
            {
                await AskTimeRangeSelection(chatId, ct);
            }
            else if (state.OperationType == OperationType.Update)
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
            state.Cron = timeRange;
            if (state.OperationType == OperationType.Create)
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
                Cron = state.Cron
            }
        };
        
        var result = await _grpcClient.CreateOperationAsync(request, cancellationToken: ct);
        if (!result.Status.IsSuccess)
        {
            await _botClient.SendMessage(chatId, "Не удалось создать операцию", cancellationToken: ct);
            return;
        }
        await _botClient.SendMessage(
            chatId, 
            $"✅ Задача создана!{(state.Cron != TimeRange.None ? $" Периодичность: {state.Cron}" : "")}", 
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: ct);
        _userDialogs.TryRemove(chatId, out _);
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
                Cron = state.Cron
            }
        };
        
        var result = await _grpcClient.UpdateOperationAsync(request, cancellationToken: ct);
        if (!result.Status.IsSuccess)
        {
            await _botClient.SendMessage(chatId, "Не удалось обновить операцию", cancellationToken: ct);
            return;
        }
        await _botClient.SendMessage(
            chatId, 
            $"✅ Задача обновлена!{(state.Cron != TimeRange.None ? $" Периодичность: {state.Cron}" : "")}", 
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: ct);
        _userDialogs.TryRemove(chatId, out _);
    }
    private class DialogState
    {
        public DialogStep Step { get; set; }
        public OperationType OperationType { get; set; }
        public long OperationId { get; set; }
        public string? Theme { get; set; }
        public string? Description { get; set; }
        public DateTime Date { get; set; }
        public TimeRange Cron { get; set; }
    }

    private enum DialogStep
    {
        AskOperationId,
        AskTheme,
        AskDescription,
        AskDate,
        AskTimeRange
    }

    private enum OperationType
    {
        Create,
        Update
    }
}