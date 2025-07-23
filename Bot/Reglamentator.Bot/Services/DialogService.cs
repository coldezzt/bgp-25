using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Types;
using Google.Protobuf.WellKnownTypes;

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
            case DialogStep.AskTheme:
                state.Theme = message.Text ?? "";
                state.Step = DialogStep.AskDescription;
                await _botClient.SendMessage(chatId, "Введите описание задачи:", cancellationToken: ct);
                break;

            case DialogStep.AskDescription:
                state.Description = message.Text ?? "";
                state.Step = DialogStep.AskDate;
                await _botClient.SendMessage(chatId, "Введите дату (гггг-мм-дд):", cancellationToken: ct);
                break;

            case DialogStep.AskDate:
                if (DateTime.TryParse(message.Text, out var date))
                {
                    state.Date = date;
                    var request = new CreateOperationRequest
                    {
                        TelegramId = chatId,
                        Operation = new CreateOperationDto
                        {
                            Theme = state.Theme,
                            Description = state.Description,
                            StartDate = Timestamp.FromDateTime(DateTime.SpecifyKind(state.Date, DateTimeKind.Utc))
                        }
                    };
                    await _grpcClient.CreateOperationAsync(request, cancellationToken: ct);
                    await _botClient.SendMessage(chatId, "✅ Задача создана!", cancellationToken: ct);
                    _userDialogs.TryRemove(chatId, out _);
                }
                else
                {
                    await _botClient.SendMessage(chatId, "Некорректная дата. Введите в формате гггг-мм-дд:", cancellationToken: ct);
                }
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

    private class DialogState
    {
        public DialogStep Step { get; set; }
        public string? Theme { get; set; }
        public string? Description { get; set; }
        public DateTime Date { get; set; }
    }

    private enum DialogStep
    {
        AskTheme,
        AskDescription,
        AskDate
    }
}