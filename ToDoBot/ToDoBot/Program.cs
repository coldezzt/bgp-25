using System.Threading;
using Telegram.Bot;
using ToDoBot.Services;
using ToDoBot;

var cts = new CancellationTokenSource();

var botClient = new TelegramBotClient("8061428940:AAEl1rjzqqHQ_xIHvJEiISN6RsmpaU3n4RE", cancellationToken: cts.Token);
var apiService = new ApiService("https://localhost:5001/api/");// api url
var dialogService = new DialogService(apiService);

var botHandler = new BotHandler(botClient, apiService, dialogService);

var reminderService = new ReminderService(botClient, apiService);
_ = Task.Run(() => reminderService.RunAsync(cts.Token));


botHandler.Start(cts.Token);

Console.WriteLine("Бот запущен и принимает обновления...");
await Task.Delay(Timeout.Infinite);


cts.Cancel();
