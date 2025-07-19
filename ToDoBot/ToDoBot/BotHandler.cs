using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using ToDoBot.Models;
using ToDoBot.Services;



namespace ToDoBot
{
    public class BotHandler
    {
        private readonly ITelegramBotClient _bot;
        private readonly ApiService _api;
        private readonly DialogService _dialog;
        private readonly Dictionary<long, string> _userStates = new();

        public BotHandler(ITelegramBotClient bot, ApiService api, DialogService dialog)
        {
            _bot = bot;
            _api = api;
            _dialog = dialog;
        }

        public void Start(CancellationToken cancellationToken)
        {
            var receiverOptions = new Telegram.Bot.Polling.ReceiverOptions
            {
                AllowedUpdates = new UpdateType[]
                {
                    UpdateType.Message,
                    UpdateType.CallbackQuery,
                    UpdateType.EditedMessage,
                }
            };

            var updateHandler = new BotUpdateHandler(this);

            _bot.StartReceiving(
                updateHandler,
                receiverOptions,
                cancellationToken
            );

            Console.WriteLine("Бот запущен и принимает обновления...");
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                await HandleMessage(update.Message, cancellationToken);
            }
            else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
            {
                await HandleCallbackQuery(update.CallbackQuery, cancellationToken);
            }
        }

        public async Task HandleMessage(Message message, CancellationToken cancellationToken)
        {
            var chatId = message.Chat.Id;
            var text = message.Text ?? "";

            if (text == "/start")
            {
                _userStates.Remove(chatId);
                var buttons = new InlineKeyboardMarkup(new[]
                {
                    new[] { InlineKeyboardButton.WithCallbackData("Список задач", "list") },
                    new[] { InlineKeyboardButton.WithCallbackData("Добавить задачу", "add") }
                });

                _userStates.Remove(chatId);
                await _bot.SendMessage(
                    chatId,
                    "Добро пожаловать! Выберите действие:",
                    replyMarkup: _mainKeyboard,
                    cancellationToken: cancellationToken);
                return;
            }

            if (text == "/list")
            {
                var operations = await _api.GetOperationsAsync(chatId);
                if (operations.Count == 0)
                {
                    await _bot.SendMessage(chatId, "Нет задач.", cancellationToken: cancellationToken);
                    return;
                }

                var list = string.Join("\n", operations.Select(op => $"• [{op.Id}] {op.Title} — {op.DueDate:G}"));
                await _bot.SendMessage(chatId, "Ваши задачи:\n" + list, cancellationToken: cancellationToken);
                return;
            }

            if (text.StartsWith("/delete"))
            {
                var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2 || !int.TryParse(parts[1], out int id))
                {
                    await _bot.SendMessage(chatId, "Используйте: /delete <ID>", cancellationToken: cancellationToken);
                    return;
                }

                try
                {
                    await _api.DeleteOperationAsync(id);
                    await _bot.SendMessage(chatId, $"✅ Задача {id} удалена.", cancellationToken: cancellationToken);
                }
                catch
                {
                    await _bot.SendMessage(chatId, $"❌ Не удалось удалить задачу {id}.", cancellationToken: cancellationToken);
                }
                return;
            }


            if (text == "/add")
            {
                await _dialog.StartAddOperation(message.Chat,
                    (chatId_, text_, keyboard) => _bot.SendMessage(chatId_, text_, replyMarkup: keyboard, cancellationToken: cancellationToken));
                _userStates[chatId] = "adding";
                return;
            }

            if (text == "/today")
            {
                var operations = await _api.GetOperationsAsync(chatId);
                var today = DateTime.Today;
                var todayOps = operations.Where(op => op.DueDate.Date == today).ToList();

                if (todayOps.Count == 0)
                {
                    await _bot.SendMessage(chatId, "На сегодня задач нет.", cancellationToken: cancellationToken);
                }
                else
                {
                    var list = string.Join("\n", todayOps.Select(op => $"• [{op.Id}] {op.Title} — {op.DueDate:t}"));
                    await _bot.SendMessage(chatId, "Задачи на сегодня:\n" + list, cancellationToken: cancellationToken);
                }
                return;
            }

            if (text == "/week")
            {
                var operations = await _api.GetOperationsAsync(chatId);
                var today = DateTime.Today;
                var endOfWeek = today.AddDays(7);
                var weekOps = operations.Where(op => op.DueDate.Date >= today && op.DueDate.Date <= endOfWeek).ToList();

                if (weekOps.Count == 0)
                {
                    await _bot.SendMessage(chatId, "На неделю задач нет.", cancellationToken: cancellationToken);
                }
                else
                {
                    var list = string.Join("\n", weekOps.Select(op => $"• [{op.Id}] {op.Title} — {op.DueDate:g}"));
                    await _bot.SendMessage(chatId, "Задачи на неделю:\n" + list, cancellationToken: cancellationToken);
                }
                return;
            }

            if (text == "/month")
            {
                var operations = await _api.GetOperationsAsync(chatId);
                var today = DateTime.Today;
                var endOfMonth = today.AddMonths(1);
                var monthOps = operations.Where(op => op.DueDate.Date >= today && op.DueDate.Date <= endOfMonth).ToList();

                if (monthOps.Count == 0)
                {
                    await _bot.SendMessage(chatId, "На месяц задач нет.", cancellationToken: cancellationToken);
                }
                else
                {
                    var list = string.Join("\n", monthOps.Select(op => $"• [{op.Id}] {op.Title} — {op.DueDate:g}"));
                    await _bot.SendMessage(chatId, "Задачи на месяц:\n" + list, cancellationToken: cancellationToken);
                }
                return;
            }

            if (text.StartsWith("/edit"))
            {
                var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2 || !int.TryParse(parts[1], out int id))
                {
                    await _bot.SendMessage(chatId, "Используйте: /edit <ID>", cancellationToken: cancellationToken);
                    return;
                }

                try
                {
                    var operations = await _api.GetOperationsAsync(chatId);
                    var op = operations.FirstOrDefault(o => o.Id == id);
                    if (op == null)
                    {
                        await _bot.SendMessage(chatId, $"❌ Задача с ID {id} не найдена.", cancellationToken: cancellationToken);
                        return;
                    }

                    await _dialog.StartEditOperation(message.Chat,
                        op,
                        (chatId_, text_, keyboard) => _bot.SendMessage(chatId_, text_, replyMarkup: keyboard, cancellationToken: cancellationToken));

                    _userStates[chatId] = "editing";
                }
                catch
                {
                    await _bot.SendMessage(chatId, $"❌ Ошибка при получении задачи {id}.", cancellationToken: cancellationToken);
                }

                return;
            }

            switch (text)
            {
                case "📋 Список задач":
                    await HandleMessage(new Message { Chat = message.Chat, Text = "/list" }, cancellationToken);
                    return;
                case "➕ Добавить":
                    await HandleMessage(new Message { Chat = message.Chat, Text = "/add" }, cancellationToken);
                    return;
                case "📅 Сегодня":
                    await HandleMessage(new Message { Chat = message.Chat, Text = "/today" }, cancellationToken);
                    return;
                case "🗓️ Неделя":
                    await HandleMessage(new Message { Chat = message.Chat, Text = "/week" }, cancellationToken);
                    return;
                case "📆 Месяц":
                    await HandleMessage(new Message { Chat = message.Chat, Text = "/month" }, cancellationToken);
                    return;
                case "✏️ Изменить":
                    await _bot.SendMessage(chatId, "Введите: /edit <ID>", replyMarkup: _mainKeyboard, cancellationToken: cancellationToken);
                    return;
                case "❌ Удалить":
                    await _bot.SendMessage(chatId, "Введите: /delete <ID>", replyMarkup: _mainKeyboard, cancellationToken: cancellationToken);
                    return;
            }


            if (_userStates.ContainsKey(chatId))
            {
                await _dialog.HandleNextStep(message.Chat, text,
                    (chatId_, text_, keyboard) => _bot.SendMessage(chatId_, text_, replyMarkup: keyboard, cancellationToken: cancellationToken));
            }
            else
            {
                await _bot.SendMessage(chatId, "Неизвестная команда. Используйте /start", cancellationToken: cancellationToken);
            }
        }

        public async Task HandleCallbackQuery(Telegram.Bot.Types.CallbackQuery query, CancellationToken cancellationToken)
        {
            var chatId = query.Message?.Chat.Id ?? 0;
            var data = query.Data ?? "";


            if (data == "list")
            {
                var operations = await _api.GetOperationsAsync(chatId);
                if (operations.Count == 0)
                {
                    await _bot.SendMessage(chatId, "Нет задач.", cancellationToken: cancellationToken);
                }
                else
                {
                    var list = string.Join("\n", operations.Select(op => $"• [{op.Id}] {op.Title} — {op.DueDate:G}"));
                    await _bot.SendMessage(chatId, "Ваши задачи:\n" + list, cancellationToken: cancellationToken);
                }
            }
            else if (data == "add")
            {
                await _dialog.StartAddOperation(query.Message.Chat,
                    (chatId_, text_, keyboard) => _bot.SendMessage(chatId_, text_, replyMarkup: keyboard, cancellationToken: cancellationToken));
                _userStates[chatId] = "adding";
            }
            else if (Enum.TryParse<RepeatType>(data, out var repeat))
            {
                await _dialog.HandleRepeatSelection(query.Message.Chat, data,
                    (chatId_, text_, keyboard) => _bot.SendMessage(chatId_, text_, replyMarkup: keyboard, cancellationToken: cancellationToken));
            }
            else if (Enum.TryParse<ReminderType>(data, out var reminder))
            {
                await _dialog.HandleReminderSelection(query.Message.Chat, data,
                    (chatId_, text_, keyboard) => _bot.SendMessage(chatId_, text_, replyMarkup: keyboard, cancellationToken: cancellationToken));
            }

            if (_bot is TelegramBotClient botClient)
            {
                await TelegramBotClientExtensions.AnswerCallbackQuery(_bot, query.Id, cancellationToken: cancellationToken);
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource errorSource, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                Telegram.Bot.Exceptions.ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }


        private static readonly ReplyKeyboardMarkup _mainKeyboard = new(new[]
        {
    new KeyboardButton[] { "📋 Список задач", "➕ Добавить" },
    new KeyboardButton[] { "📅 Сегодня", "🗓️ Неделя", "📆 Месяц" },
    new KeyboardButton[] { "✏️ Изменить", "❌ Удалить" }
})
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = false
        };


    }
}
