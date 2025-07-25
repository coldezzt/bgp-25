using Telegram.Bot.Types.ReplyMarkups;

namespace Reglamentator.Bot.Templates;

public static class Keyboards
{
    public static readonly ReplyKeyboardMarkup MainKeyboard = new(new[]
    {
        new KeyboardButton[] { "📋 Список задач", "⏳ История задач", "ℹ️ Инструкция" },
        new KeyboardButton[] { "📅 Сегодня", "🗓️ Неделя", "📆 Месяц" },
        new KeyboardButton[] { "➕ Добавить задачу", "✏️ Изменить задачу", "❌ Удалить задачу" },
        new KeyboardButton[] { "⏰ Добавить напоминание", "🔄 Обновить напоминание", "🗑️ Удалить напоминание" }
    })
    {
        ResizeKeyboard = true,
        OneTimeKeyboard = false
    };

    public static readonly ReplyKeyboardMarkup TimeRangeKeyboard = new(new[]
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
    
    public static readonly ReplyKeyboardMarkup TimeRangeReminderKeyboard = new ReplyKeyboardMarkup(new[]
    {
        new[] { new KeyboardButton(TimeRange.Min15.ToString()) },
        new[] { new KeyboardButton(TimeRange.Hour.ToString()) },
        new[] { new KeyboardButton(TimeRange.Day.ToString()) }
    })
    {
        ResizeKeyboard = true,
        OneTimeKeyboard = true
    };
    public static readonly ReplyKeyboardMarkup YesNoKeyboard = new(new[]
    {
        new[] { new KeyboardButton("Да") },
        new[] { new KeyboardButton("Нет") }
    })
    {
        ResizeKeyboard = true,
        OneTimeKeyboard = true
    };
    
    public static readonly ReplyKeyboardMarkup DialogKeyboard = new(new[]
    {
        new KeyboardButton[] { "❌ Отменить" }
    })
    {
        ResizeKeyboard = true,
        OneTimeKeyboard = true
    };

    public static readonly ReplyKeyboardMarkup EditDialogKeyboard = new(new[]
    {
        new KeyboardButton[] { "🚫 Не изменять" },
        new KeyboardButton[] { "❌ Отменить" }
    })
    {
        ResizeKeyboard = true,
        OneTimeKeyboard = true
    };
}