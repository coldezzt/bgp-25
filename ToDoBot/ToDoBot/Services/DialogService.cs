using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using ToDoBot.Models;

namespace ToDoBot.Services;


public class DialogService
{
    private record OperationBuilder(string ChatId)
    {
        public int Id { get; set; } = 0;
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public RepeatType Repeat { get; set; } = RepeatType.Once;
        public ReminderType Reminder { get; set; } = ReminderType.None;
    }




    private readonly Dictionary<string, OperationBuilder> _builders = new();
    private readonly IApiService _api;

    public DialogService(IApiService api) => _api = api;

    public async Task StartAddOperation(Chat chat, Func<ChatId, string, InlineKeyboardMarkup?, Task<Message>> sendMessage)
    {
        var chatId = chat.Id.ToString();
        _builders[chatId] = new OperationBuilder(chatId);
        await sendMessage(chat.Id, "Введите название задачи:", null);
    }

    public async Task HandleNextStep(Chat chat, string text, Func<ChatId, string, InlineKeyboardMarkup?, Task<Message>> sendMessage)
    {
        var chatId = chat.Id.ToString();

        if (!_builders.TryGetValue(chatId, out var builder))
        {
            await sendMessage(chat.Id, "Сначала нажмите 'Добавить задачу'", null);
            return;
        }

        if (builder.Title == null)
        {
            builder.Title = text;
            await sendMessage(chat.Id, "Введите описание задачи:", null);
        }
        else if (builder.Description == null)
        {
            builder.Description = text;
            await sendMessage(chat.Id, "Введите дату выполнения (формат: ГГГГ-ММ-ДД ЧЧ:ММ):", null);
        }
        else if (builder.DueDate == null)
        {
            if (DateTime.TryParse(text, out var dueDate))
            {
                builder.DueDate = dueDate;
                await ShowRepeatMenu(chat, sendMessage);
            }
            else
            {
                await sendMessage(chat.Id, "❌ Неверный формат даты. Попробуйте снова.", null);
            }
        }
        else
        {
            var op = new Operation(0, builder.Title!, builder.Description!, builder.DueDate!.Value, builder.Repeat, builder.Reminder);



            if (builder.Id == 0)
                await _api.AddOperationAsync(op);
            else
                await _api.UpdateOperationAsync(op);
            //await _api.AddOperationAsync(op);
            await sendMessage(chat.Id, "✅ Задача успешно добавлена!", null);
            _builders.Remove(chatId);
        }
    }

    public async Task HandleRepeatSelection(Chat chat, string callbackData, Func<ChatId, string, InlineKeyboardMarkup?, Task<Message>> sendMessage)
    {
        var chatId = chat.Id.ToString();
        if (!_builders.TryGetValue(chatId, out var builder)) return;

        if (Enum.TryParse<RepeatType>(callbackData, out var repeat))
        {
            builder.Repeat = repeat;
            await ShowReminderMenu(chat, sendMessage);
        }
    }

    public async Task HandleReminderSelection(Chat chat, string callbackData, Func<ChatId, string, InlineKeyboardMarkup?, Task<Message>> sendMessage)
    {
        var chatId = chat.Id.ToString();
        if (!_builders.TryGetValue(chatId, out var builder)) return;

        if (Enum.TryParse<ReminderType>(callbackData, out var reminder))
        {
            builder.Reminder = reminder;
            var op = new Operation(0, builder.Title!, builder.Description!, builder.DueDate!.Value, builder.Repeat, builder.Reminder);
            await _api.AddOperationAsync(op);
            await sendMessage(chat.Id, "✅ Задача успешно добавлена!", null);
            _builders.Remove(chatId);
        }
    }

    private async Task ShowRepeatMenu(Chat chat, Func<ChatId, string, InlineKeyboardMarkup?, Task<Message>> sendMessage)
    {
        var buttons = new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("Одноразовая", "Once") },
            new[] { InlineKeyboardButton.WithCallbackData("Каждый час", "Hourly") },
            new[] { InlineKeyboardButton.WithCallbackData("Каждый день", "Daily") },
            new[] { InlineKeyboardButton.WithCallbackData("Каждую неделю", "Weekly") },
            new[] { InlineKeyboardButton.WithCallbackData("Каждый месяц", "Monthly") },
            new[] { InlineKeyboardButton.WithCallbackData("Каждый год", "Yearly") },
        };
        await sendMessage(chat.Id, "Выберите периодичность:", new InlineKeyboardMarkup(buttons));
    }

    private async Task ShowReminderMenu(Chat chat, Func<ChatId, string, InlineKeyboardMarkup?, Task<Message>> sendMessage)
    {
        var buttons = new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("Без напоминания", "None") },
            new[] { InlineKeyboardButton.WithCallbackData("За 15 минут", "FifteenMinutes") },
            new[] { InlineKeyboardButton.WithCallbackData("За 1 час", "OneHour") },
            new[] { InlineKeyboardButton.WithCallbackData("За 1 день", "OneDay") },
        };
        await sendMessage(chat.Id, "Выберите напоминание:", new InlineKeyboardMarkup(buttons));
    }

    public void Cancel(Chat chat)
    {
        var chatId = chat.Id.ToString();
        if (_builders.ContainsKey(chatId))
            _builders.Remove(chatId);
    }





    public async Task StartEditOperation(Chat chat, Operation existingOperation,
    Func<ChatId, string, InlineKeyboardMarkup?, Task<Message>> sendMessage)
    {
        var chatId = chat.Id.ToString();
        _builders[chatId] = new OperationBuilder(chatId)
        {
            Id = existingOperation.Id,
            Title = null,
            Description = null,
            DueDate = null,
            Repeat = existingOperation.Repeat,
            Reminder = existingOperation.Reminder
        };

        await sendMessage(chat.Id, "Введите новое название задачи:", null);
    }

}