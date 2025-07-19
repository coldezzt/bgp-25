namespace ToDoBot.Models;

public record Operation(
    int Id,
    string Title,
    string Description,
    DateTime DueDate,
    RepeatType Repeat,
    ReminderType Reminder
);

public enum RepeatType
{
    Once,
    Hourly,
    Daily,
    Weekly,
    Monthly,
    Yearly
}

public enum ReminderType
{
    None,
    FifteenMinutes,
    OneHour,
    OneDay
}