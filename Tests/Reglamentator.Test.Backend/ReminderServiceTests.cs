using AutoFixture;
using Moq;
using Reglamentator.Application.Abstractions;
using Reglamentator.Application.Dtos;
using Reglamentator.Application.Services;
using Reglamentator.Domain.Entities;
using Reglamentator.Domain.Interfaces;

namespace Reglamentator.Test.Backend;

public class ReminderServiceTests
{
    private readonly Mock<IReminderRepository> _reminderRepositoryMock = new();
    private readonly Mock<IOperationRepository> _operationRepositoryMock = new();
    private readonly Mock<ITelegramUserRepository> _telegramUserRepositoryMock = new();
    private readonly Mock<IHangfireReminderJobHelper> _hangfireReminderJobHelperMock = new();
    private readonly Fixture _fixture = new();
    private readonly CancellationToken _cancellationToken = new();
    private readonly ReminderService _reminderService;
    public ReminderServiceTests()
    {
        _reminderService = new ReminderService(_telegramUserRepositoryMock.Object, _operationRepositoryMock.Object,
            _reminderRepositoryMock.Object, _hangfireReminderJobHelperMock.Object);
    }

    [Fact]
    public async Task Creating_Reminder_With_Valid_Operation_And_User_Saves_It_And_Launch_Periodic_Notifications()
    {
        var reminderDto = _fixture.Create<CreateReminderDto>();
        var telegramId = _fixture.Create<long>();
        var operationId = _fixture.Create<long>();
        var telegram = _fixture
            .Build<TelegramUser>()
            .With(u => u.TelegramId, telegramId)
            .With(u => u.Operations, new List<Operation>())
            .Create();
        var operation = _fixture
            .Build<Operation>()
            .With(op => op.Id, operationId)
            .With(op => op.TelegramUserId, telegramId)
            .With(op => op.History, new List<OperationInstance>())
            .With(op => op.Reminders, new List<Reminder>())
            .With(op => op.TelegramUser, telegram)
            .With(op => op.NextOperationInstance, new OperationInstance())
            .Create();
        telegram.Operations.Add(operation);
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId, _cancellationToken))
            .ReturnsAsync(true);
        _telegramUserRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(u => u.Id == telegramId, _cancellationToken))
            .ReturnsAsync(telegram);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId, _cancellationToken))
            .ReturnsAsync(true);
        _operationRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(op => op.Id == operationId, _cancellationToken))
            .ReturnsAsync(operation);
        
        var result = await _reminderService.AddReminderAsync(telegramId, operationId, reminderDto, _cancellationToken);
        
        _reminderRepositoryMock.Verify(repo => 
                repo.InsertEntityAsync(It.IsAny<Reminder>(), _cancellationToken), 
            Times.Once);
        _hangfireReminderJobHelperMock.Verify(helper => 
                helper.CreateJobForReminder(It.IsAny<Operation>(), It.IsAny<Reminder>()),
            Times.Once);
        Assert.Equal(operationId, result.ValueOrDefault?.OperationId);
    }
    
    [Fact]
    public async Task Creating_Reminder_With_Invalid_User_And_Operation_Is_Fail()
    {
        var reminderDto = _fixture.Create<CreateReminderDto>();
        var telegramId = _fixture.Create<long>();
        var operationId = _fixture.Create<long>();
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId, _cancellationToken))
            .ReturnsAsync(false);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(operationId, _cancellationToken))
            .ReturnsAsync(false);
        
        var result = await _reminderService.AddReminderAsync(telegramId, operationId, reminderDto, _cancellationToken);
        
        _reminderRepositoryMock.Verify(repo => 
                repo.InsertEntityAsync(It.IsAny<Reminder>(), _cancellationToken), 
            Times.Never);
        _hangfireReminderJobHelperMock.Verify(helper => 
                helper.CreateJobForReminder(It.IsAny<Operation>(), It.IsAny<Reminder>()),
            Times.Never);
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task Creating_Reminder_With_Invalid_User_And_Existing_Operation_Is_Fail()
    {
        var reminderDto = _fixture.Create<CreateReminderDto>();
        var telegramId = _fixture.Create<long>();
        var operationTelegramId = telegramId+1;
        var operationId = _fixture.Create<long>();
        var operation = _fixture
            .Build<Operation>()
            .With(op => op.Id, operationId)
            .With(op => op.TelegramUserId, telegramId)
            .With(op => op.History, new List<OperationInstance>())
            .With(op => op.Reminders, new List<Reminder>())
            .With(op => op.TelegramUser, new TelegramUser{TelegramId = operationTelegramId})
            .With(op => op.NextOperationInstance, new OperationInstance())
            .Create();
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId, _cancellationToken))
            .ReturnsAsync(false);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(operationId, _cancellationToken))
            .ReturnsAsync(true);
        _operationRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(op => op.Id == operationId, _cancellationToken))
            .ReturnsAsync(operation);
        
        var result = await _reminderService.AddReminderAsync(telegramId, operationId, reminderDto, _cancellationToken);
        
        _reminderRepositoryMock.Verify(repo => 
                repo.InsertEntityAsync(It.IsAny<Reminder>(), _cancellationToken), 
            Times.Never);
        _hangfireReminderJobHelperMock.Verify(helper => 
                helper.CreateJobForReminder(It.IsAny<Operation>(), It.IsAny<Reminder>()),
            Times.Never);
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task Creating_Reminder_With_Invalid_Operation_Is_Fail()
    {
        var reminderDto = _fixture.Create<CreateReminderDto>();
        var telegramId = _fixture.Create<long>();
        var operationId = _fixture.Create<long>();
        var telegram = _fixture
            .Build<TelegramUser>()
            .With(u => u.TelegramId, telegramId)
            .With(u => u.Operations, new List<Operation>())
            .Create();
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId, _cancellationToken))
            .ReturnsAsync(true);
        _telegramUserRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(u => u.Id == telegramId, _cancellationToken))
            .ReturnsAsync(telegram);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(operationId, _cancellationToken))
            .ReturnsAsync(false);
        
        var result = await _reminderService.AddReminderAsync(telegramId, operationId, reminderDto, _cancellationToken);
        
        _reminderRepositoryMock.Verify(repo => 
                repo.InsertEntityAsync(It.IsAny<Reminder>(), _cancellationToken), 
            Times.Never);
        _hangfireReminderJobHelperMock.Verify(helper => 
                helper.CreateJobForReminder(It.IsAny<Operation>(), It.IsAny<Reminder>()),
            Times.Never);
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task Creating_Reminder_With_Operation_That_Does_Not_Belong_To_User_Is_Fail()
    {
        var reminderDto = _fixture.Create<CreateReminderDto>();
        var telegramId = _fixture.Create<long>();
        var operationTelegramId = telegramId + 1;
        var operationId = _fixture.Create<long>();
        var telegram = _fixture
            .Build<TelegramUser>()
            .With(u => u.TelegramId, telegramId)
            .With(u => u.Operations, new List<Operation>())
            .Create();
        var operation = _fixture
            .Build<Operation>()
            .With(op => op.Id, operationId)
            .With(op => op.TelegramUserId, operationTelegramId)
            .With(op => op.History, new List<OperationInstance>())
            .With(op => op.Reminders, new List<Reminder>())
            .With(op => op.TelegramUser, new TelegramUser{TelegramId = operationTelegramId})
            .With(op => op.NextOperationInstance, new OperationInstance())
            .Create();
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId, _cancellationToken))
            .ReturnsAsync(true);
        _telegramUserRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(u => u.Id == telegramId, _cancellationToken))
            .ReturnsAsync(telegram);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(operationId, _cancellationToken))
            .ReturnsAsync(true);
        _operationRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(op => op.Id == operationId, _cancellationToken))
            .ReturnsAsync(operation);
        
        var result = await _reminderService.AddReminderAsync(telegramId, operationId, reminderDto, _cancellationToken);
        
        _reminderRepositoryMock.Verify(repo => 
                repo.InsertEntityAsync(It.IsAny<Reminder>(), _cancellationToken), 
            Times.Never);
        _hangfireReminderJobHelperMock.Verify(helper => 
                helper.CreateJobForReminder(It.IsAny<Operation>(), It.IsAny<Reminder>()),
            Times.Never);
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task Updating_Existing_Reminder_With_Valid_Operation_And_User_Saves_Changes_And_Relaunch_Periodic_Notifications()
    {
        var reminderDto = _fixture.Create<UpdateReminderDto>();
        var telegramId = _fixture.Create<long>();
        var operationId = _fixture.Create<long>();
        var telegram = _fixture
            .Build<TelegramUser>()
            .With(u => u.TelegramId, telegramId)
            .With(u => u.Operations, new List<Operation>())
            .Create();
        var operation = _fixture
            .Build<Operation>()
            .With(op => op.Id, operationId)
            .With(op => op.TelegramUserId, telegramId)
            .With(op => op.History, new List<OperationInstance>())
            .With(op => op.Reminders, new List<Reminder>())
            .With(op => op.TelegramUser, telegram)
            .With(op => op.NextOperationInstance, new OperationInstance())
            .Create();
        telegram.Operations.Add(operation);
        var reminder = _fixture
            .Build<Reminder>()
            .With(r => r.Id, reminderDto.Id)
            .With(r => r.Operation, operation)
            .With(r => r.OperationId, operationId)
            .Create();
        operation.Reminders.Add(reminder);
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId, _cancellationToken))
            .ReturnsAsync(true);
        _telegramUserRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(u => u.Id == telegramId, _cancellationToken))
            .ReturnsAsync(telegram);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(operationId, _cancellationToken))
            .ReturnsAsync(true);
        _operationRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(op => op.Id == operationId, _cancellationToken))
            .ReturnsAsync(operation);
        _reminderRepositoryMock
            .Setup(repo => repo.IsExistAsync(reminderDto.Id, _cancellationToken))
            .ReturnsAsync(true);
        _reminderRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(r => r.Id == reminderDto.Id, _cancellationToken))
            .ReturnsAsync(reminder);
        
        var result = await _reminderService.UpdateReminderAsync(telegramId, operationId, reminderDto, _cancellationToken);
        
        _reminderRepositoryMock.Verify(repo => 
                repo.UpdateEntityAsync(It.IsAny<Reminder>(), _cancellationToken), 
            Times.Once);
        _hangfireReminderJobHelperMock.Verify(helper => 
                helper.UpdateJobForReminder(It.IsAny<Operation>(), It.IsAny<Reminder>()),
            Times.Once);
        Assert.True(result.IsSuccess);
        Assert.Equal(operationId, result.ValueOrDefault?.OperationId);
    }
    
    [Fact]
    public async Task Updating_Existing_Reminder_With_Invalid_User_And_Operation_Is_Fail()
    {
        var reminderDto = _fixture.Create<UpdateReminderDto>();
        var telegramId = _fixture.Create<long>();
        var operationId = _fixture.Create<long>();
        var reminder = _fixture
            .Build<Reminder>()
            .With(r => r.Id, reminderDto.Id)
            .With(r => r.OperationId, operationId)
            .With(r => r.Operation, new Operation{Id = operationId + 1})
            .Create();
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId, _cancellationToken))
            .ReturnsAsync(false);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(operationId, _cancellationToken))
            .ReturnsAsync(false);
        _reminderRepositoryMock
            .Setup(repo => repo.IsExistAsync(reminderDto.Id, _cancellationToken))
            .ReturnsAsync(true);
        _reminderRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(op => op.Id == reminderDto.Id, _cancellationToken))
            .ReturnsAsync(reminder);
        
        var result = await _reminderService.UpdateReminderAsync(telegramId, operationId, reminderDto, _cancellationToken);
        
        _reminderRepositoryMock.Verify(repo => 
                repo.UpdateEntityAsync(It.IsAny<Reminder>(), _cancellationToken), 
            Times.Never);
        _hangfireReminderJobHelperMock.Verify(helper => 
                helper.UpdateJobForReminder(It.IsAny<Operation>(), It.IsAny<Reminder>()),
            Times.Never);
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task Updating_Existing_Reminder_With_Invalid_User_And_Existing_Operation_Is_Fail()
    {
        var reminderDto = _fixture.Create<UpdateReminderDto>();
        var telegramId = _fixture.Create<long>();
        var operationId = _fixture.Create<long>();
        var operation = _fixture
            .Build<Operation>()
            .With(op => op.Id, operationId)
            .With(op => op.TelegramUserId, telegramId)
            .With(op => op.History, new List<OperationInstance>())
            .With(op => op.Reminders, new List<Reminder>())
            .With(op => op.TelegramUser, new TelegramUser{TelegramId = telegramId + 1})
            .With(op => op.NextOperationInstance, new OperationInstance())
            .Create();
        var reminder = _fixture
            .Build<Reminder>()
            .With(r => r.Id, reminderDto.Id)
            .With(r => r.OperationId, operationId)
            .With(r => r.Operation, operation)
            .Create();
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId, _cancellationToken))
            .ReturnsAsync(false);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(operationId, _cancellationToken))
            .ReturnsAsync(true);
        _operationRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(op => op.Id == operationId, _cancellationToken))
            .ReturnsAsync(operation);
        _reminderRepositoryMock
            .Setup(repo => repo.IsExistAsync(reminderDto.Id, _cancellationToken))
            .ReturnsAsync(true);
        _reminderRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(op => op.Id == reminderDto.Id, _cancellationToken))
            .ReturnsAsync(reminder);
        
        var result = await _reminderService.UpdateReminderAsync(telegramId, operationId, reminderDto, _cancellationToken);
        
        _reminderRepositoryMock.Verify(repo => 
                repo.UpdateEntityAsync(It.IsAny<Reminder>(), _cancellationToken), 
            Times.Never);
        _hangfireReminderJobHelperMock.Verify(helper => 
                helper.UpdateJobForReminder(It.IsAny<Operation>(), It.IsAny<Reminder>()),
            Times.Never);
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task Updating_Existing_Reminder_With_Invalid_Operation_Is_Fail()
    {
        var reminderDto = _fixture.Create<UpdateReminderDto>();
        var telegramId = _fixture.Create<long>();
        var operationId = _fixture.Create<long>();
        var telegram = _fixture
            .Build<TelegramUser>()
            .With(u => u.TelegramId, telegramId)
            .With(u => u.Operations, new List<Operation>())
            .Create();
        var reminder = _fixture
            .Build<Reminder>()
            .With(r => r.Id, reminderDto.Id)
            .With(r => r.OperationId, operationId)
            .With(r => r.Operation, new Operation{Id = operationId + 1})
            .Create();
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId, _cancellationToken))
            .ReturnsAsync(true);
        _telegramUserRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(op => op.Id == telegramId, _cancellationToken))
            .ReturnsAsync(telegram);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(operationId, _cancellationToken))
            .ReturnsAsync(false);
        _reminderRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(op => op.Id == reminderDto.Id, _cancellationToken))
            .ReturnsAsync(reminder);
        _reminderRepositoryMock
            .Setup(repo => repo.IsExistAsync(reminderDto.Id, _cancellationToken))
            .ReturnsAsync(true);
        
        var result = await _reminderService.UpdateReminderAsync(telegramId, operationId, reminderDto, _cancellationToken);
        
        _reminderRepositoryMock.Verify(repo => 
                repo.UpdateEntityAsync(It.IsAny<Reminder>(), _cancellationToken), 
            Times.Never);
        _hangfireReminderJobHelperMock.Verify(helper => 
                helper.UpdateJobForReminder(It.IsAny<Operation>(), It.IsAny<Reminder>()),
            Times.Never);
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task Updating_Existing_Reminder_With_Operation_That_Does_Not_Belong_To_User_Is_Fail()
    {
        var reminderDto = _fixture.Create<UpdateReminderDto>();
        var telegramId = _fixture.Create<long>();
        var operationTelegramId = telegramId + 1;
        var operationId = _fixture.Create<long>();
        var telegram = _fixture
            .Build<TelegramUser>()
            .With(u => u.TelegramId, telegramId)
            .With(u => u.Operations, new List<Operation>())
            .Create();
        var operation = _fixture
            .Build<Operation>()
            .With(op => op.Id, operationId)
            .With(op => op.TelegramUserId, operationTelegramId)
            .With(op => op.History, new List<OperationInstance>())
            .With(op => op.Reminders, new List<Reminder>())
            .With(op => op.TelegramUser, new TelegramUser{TelegramId = operationTelegramId})
            .With(op => op.NextOperationInstance, new OperationInstance())
            .Create();
        var reminder = _fixture
            .Build<Reminder>()
            .With(r => r.Id, reminderDto.Id)
            .With(r => r.OperationId, operationId)
            .With(r => r.Operation, operation)
            .Create();
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId, _cancellationToken))
            .ReturnsAsync(true);
        _telegramUserRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(op => op.Id == telegramId, _cancellationToken))
            .ReturnsAsync(telegram);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(operationId, _cancellationToken))
            .ReturnsAsync(true);
        _operationRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(op => op.Id == operationId, _cancellationToken))
            .ReturnsAsync(operation);
        _reminderRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(op => op.Id == reminderDto.Id, _cancellationToken))
            .ReturnsAsync(reminder);
        _reminderRepositoryMock
            .Setup(repo => repo.IsExistAsync(reminderDto.Id, _cancellationToken))
            .ReturnsAsync(true);
        
        var result = await _reminderService.UpdateReminderAsync(telegramId, operationId, reminderDto, _cancellationToken);
        
        _reminderRepositoryMock.Verify(repo => 
                repo.UpdateEntityAsync(It.IsAny<Reminder>(), _cancellationToken), 
            Times.Never);
        _hangfireReminderJobHelperMock.Verify(helper => 
                helper.UpdateJobForReminder(It.IsAny<Operation>(), It.IsAny<Reminder>()),
            Times.Never);
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task Updating_Existing_Reminder_That_Does_Not_Belong_To_Operation_Is_Fail()
    {
        var reminderDto = _fixture.Create<UpdateReminderDto>();
        var telegramId = _fixture.Create<long>();
        var operationId = _fixture.Create<long>();
        var telegram = _fixture
            .Build<TelegramUser>()
            .With(u => u.TelegramId, telegramId)
            .With(u => u.Operations, new List<Operation>())
            .Create();
        var operation = _fixture
            .Build<Operation>()
            .With(op => op.Id, operationId)
            .With(op => op.TelegramUserId, telegramId)
            .With(op => op.History, new List<OperationInstance>())
            .With(op => op.Reminders, new List<Reminder>())
            .With(op => op.TelegramUser, telegram)
            .With(op => op.NextOperationInstance, new OperationInstance())
            .Create();
        telegram.Operations.Add(operation);
        var reminder = _fixture
            .Build<Reminder>()
            .With(r => r.Id, reminderDto.Id)
            .With(r => r.OperationId, operationId + 1)
            .With(r => r.Operation, new Operation{Id = operationId + 1})
            .Create();
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId, _cancellationToken))
            .ReturnsAsync(true);
        _telegramUserRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(op => op.Id == telegramId, _cancellationToken))
            .ReturnsAsync(telegram);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(operationId, _cancellationToken))
            .ReturnsAsync(true);
        _operationRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(op => op.Id == operationId, _cancellationToken))
            .ReturnsAsync(operation);
        _reminderRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(op => op.Id == reminderDto.Id, _cancellationToken))
            .ReturnsAsync(reminder);
        _reminderRepositoryMock
            .Setup(repo => repo.IsExistAsync(reminderDto.Id, _cancellationToken))
            .ReturnsAsync(true);
        
        var result = await _reminderService.UpdateReminderAsync(telegramId, operationId, reminderDto, _cancellationToken);
        
        _reminderRepositoryMock.Verify(repo => 
                repo.UpdateEntityAsync(It.IsAny<Reminder>(), _cancellationToken), 
            Times.Never);
        _hangfireReminderJobHelperMock.Verify(helper => 
                helper.UpdateJobForReminder(It.IsAny<Operation>(), It.IsAny<Reminder>()),
            Times.Never);
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task Updating_Not_Existing_Reminder_Is_Fail()
    {
        var reminderDto = _fixture.Create<UpdateReminderDto>();
        var telegramId = _fixture.Create<long>();
        var operationId = _fixture.Create<long>();
        var telegram = _fixture
            .Build<TelegramUser>()
            .With(u => u.TelegramId, telegramId)
            .With(u => u.Operations, new List<Operation>())
            .Create();
        var operation = _fixture
            .Build<Operation>()
            .With(op => op.Id, operationId)
            .With(op => op.TelegramUserId, telegramId)
            .With(op => op.History, new List<OperationInstance>())
            .With(op => op.Reminders, new List<Reminder>())
            .With(op => op.TelegramUser, new TelegramUser{TelegramId = telegramId})
            .With(op => op.NextOperationInstance, new OperationInstance())
            .Create();
        telegram.Operations.Add(operation);
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId, _cancellationToken))
            .ReturnsAsync(true);
        _telegramUserRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(op => op.Id == telegramId, _cancellationToken))
            .ReturnsAsync(telegram);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(operationId, _cancellationToken))
            .ReturnsAsync(true);
        _operationRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(op => op.Id == operationId, _cancellationToken))
            .ReturnsAsync(operation);
        _reminderRepositoryMock
            .Setup(repo => repo.IsExistAsync(reminderDto.Id, _cancellationToken))
            .ReturnsAsync(false);
        
        var result = await _reminderService.UpdateReminderAsync(telegramId, operationId, reminderDto, _cancellationToken);
        
        _reminderRepositoryMock.Verify(repo => 
                repo.UpdateEntityAsync(It.IsAny<Reminder>(), _cancellationToken), 
            Times.Never);
        _hangfireReminderJobHelperMock.Verify(helper => 
                helper.UpdateJobForReminder(It.IsAny<Operation>(), It.IsAny<Reminder>()),
            Times.Never);
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task Deleting_Existing_Reminder_With_Valid_Operation_And_User_Remove_It_And_Stop_Periodic_Notifications()
    {
        var reminderId = _fixture.Create<long>();
        var telegramId = _fixture.Create<long>();
        var operationId = _fixture.Create<long>();
        var telegram = _fixture
            .Build<TelegramUser>()
            .With(u => u.TelegramId, telegramId)
            .With(u => u.Operations, new List<Operation>())
            .Create();
        var operation = _fixture
            .Build<Operation>()
            .With(op => op.Id, operationId)
            .With(op => op.TelegramUserId, telegramId)
            .With(op => op.History, new List<OperationInstance>())
            .With(op => op.Reminders, new List<Reminder>())
            .With(op => op.TelegramUser, telegram)
            .With(op => op.NextOperationInstance, new OperationInstance())
            .Create();
        telegram.Operations.Add(operation);
        var reminder = _fixture
            .Build<Reminder>()
            .With(r => r.Id, reminderId)
            .With(r => r.Operation, operation)
            .With(r => r.OperationId, operationId)
            .Create();
        operation.Reminders.Add(reminder);
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId, _cancellationToken))
            .ReturnsAsync(true);
        _telegramUserRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(u => u.Id == telegramId, _cancellationToken))
            .ReturnsAsync(telegram);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(operationId, _cancellationToken))
            .ReturnsAsync(true);
        _operationRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(op => op.Id == operationId, _cancellationToken))
            .ReturnsAsync(operation);
        _reminderRepositoryMock
            .Setup(repo => repo.IsExistAsync(reminderId, _cancellationToken))
            .ReturnsAsync(true);
        _reminderRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(r => r.Id == reminderId, _cancellationToken))
            .ReturnsAsync(reminder);
        
        var result = await _reminderService.DeleteReminderAsync(telegramId, operationId, reminderId, _cancellationToken);
        
        _reminderRepositoryMock.Verify(repo => 
                repo.DeleteEntityAsync(It.IsAny<Reminder>(), _cancellationToken), 
            Times.Once);
        _hangfireReminderJobHelperMock.Verify(helper => 
                helper.DeleteJobForReminder(It.IsAny<Reminder>()),
            Times.Once);
        Assert.True(result.IsSuccess);
        Assert.Equal(operationId, result.ValueOrDefault?.OperationId);
    }
    
    [Fact]
    public async Task Deleting_Existing_Reminder_With_Invalid_User_And_Operation_Is_Fail()
    {
        var reminderId = _fixture.Create<long>();
        var telegramId = _fixture.Create<long>();
        var operationId = _fixture.Create<long>();
        var reminder = _fixture
            .Build<Reminder>()
            .With(r => r.Id, reminderId)
            .With(r => r.OperationId, operationId)
            .With(r => r.Operation, new Operation{Id = operationId + 1})
            .Create();
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId, _cancellationToken))
            .ReturnsAsync(false);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(operationId, _cancellationToken))
            .ReturnsAsync(false);
        _reminderRepositoryMock
            .Setup(repo => repo.IsExistAsync(reminderId, _cancellationToken))
            .ReturnsAsync(true);
        _reminderRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(op => op.Id == reminderId, _cancellationToken))
            .ReturnsAsync(reminder);
        
        var result = await _reminderService.DeleteReminderAsync(telegramId, operationId, reminderId, _cancellationToken);
        
        _reminderRepositoryMock.Verify(repo => 
                repo.DeleteEntityAsync(It.IsAny<Reminder>(), _cancellationToken), 
            Times.Never);
        _hangfireReminderJobHelperMock.Verify(helper => 
                helper.DeleteJobForReminder(It.IsAny<Reminder>()),
            Times.Never);
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task Deleting_Existing_Reminder_With_Invalid_User_And_Existing_Operation_Is_Fail()
    {
        var reminderId = _fixture.Create<long>();
        var telegramId = _fixture.Create<long>();
        var operationId = _fixture.Create<long>();
        var operation = _fixture
            .Build<Operation>()
            .With(op => op.Id, operationId)
            .With(op => op.TelegramUserId, telegramId)
            .With(op => op.History, new List<OperationInstance>())
            .With(op => op.Reminders, new List<Reminder>())
            .With(op => op.TelegramUser, new TelegramUser{TelegramId = telegramId + 1})
            .With(op => op.NextOperationInstance, new OperationInstance())
            .Create();
        var reminder = _fixture
            .Build<Reminder>()
            .With(r => r.Id, reminderId)
            .With(r => r.OperationId, operationId)
            .With(r => r.Operation, operation)
            .Create();
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId, _cancellationToken))
            .ReturnsAsync(false);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(operationId, _cancellationToken))
            .ReturnsAsync(true);
        _operationRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(op => op.Id == operationId, _cancellationToken))
            .ReturnsAsync(operation);
        _reminderRepositoryMock
            .Setup(repo => repo.IsExistAsync(reminderId, _cancellationToken))
            .ReturnsAsync(true);
        _reminderRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(op => op.Id == reminderId, _cancellationToken))
            .ReturnsAsync(reminder);
        
        var result = await _reminderService.DeleteReminderAsync(telegramId, operationId, reminderId, _cancellationToken);
        
        _reminderRepositoryMock.Verify(repo => 
                repo.DeleteEntityAsync(It.IsAny<Reminder>(), _cancellationToken), 
            Times.Never);
        _hangfireReminderJobHelperMock.Verify(helper => 
                helper.DeleteJobForReminder(It.IsAny<Reminder>()),
            Times.Never);
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task Deleting_Existing_Reminder_With_Invalid_Operation_Is_Fail()
    {
        var reminderId = _fixture.Create<long>();
        var telegramId = _fixture.Create<long>();
        var operationId = _fixture.Create<long>();
        var telegram = _fixture
            .Build<TelegramUser>()
            .With(u => u.TelegramId, telegramId)
            .With(u => u.Operations, new List<Operation>())
            .Create();
        var reminder = _fixture
            .Build<Reminder>()
            .With(r => r.Id, reminderId)
            .With(r => r.OperationId, operationId)
            .With(r => r.Operation, new Operation{Id = operationId + 1})
            .Create();
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId, _cancellationToken))
            .ReturnsAsync(true);
        _telegramUserRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(op => op.Id == telegramId, _cancellationToken))
            .ReturnsAsync(telegram);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(operationId, _cancellationToken))
            .ReturnsAsync(false);
        _reminderRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(op => op.Id == reminderId, _cancellationToken))
            .ReturnsAsync(reminder);
        _reminderRepositoryMock
            .Setup(repo => repo.IsExistAsync(reminderId, _cancellationToken))
            .ReturnsAsync(true);
        
        var result = await _reminderService.DeleteReminderAsync(telegramId, operationId, reminderId, _cancellationToken);
        
        _reminderRepositoryMock.Verify(repo => 
                repo.DeleteEntityAsync(It.IsAny<Reminder>(), _cancellationToken), 
            Times.Never);
        _hangfireReminderJobHelperMock.Verify(helper => 
                helper.DeleteJobForReminder(It.IsAny<Reminder>()),
            Times.Never);
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task Deleting_Existing_Reminder_With_Operation_That_Does_Not_Belong_To_User_Is_Fail()
    {
        var reminderId = _fixture.Create<long>();
        var telegramId = _fixture.Create<long>();
        var operationTelegramId = telegramId + 1;
        var operationId = _fixture.Create<long>();
        var telegram = _fixture
            .Build<TelegramUser>()
            .With(u => u.TelegramId, telegramId)
            .With(u => u.Operations, new List<Operation>())
            .Create();
        var operation = _fixture
            .Build<Operation>()
            .With(op => op.Id, operationId)
            .With(op => op.TelegramUserId, operationTelegramId)
            .With(op => op.History, new List<OperationInstance>())
            .With(op => op.Reminders, new List<Reminder>())
            .With(op => op.TelegramUser, new TelegramUser{TelegramId = operationTelegramId})
            .With(op => op.NextOperationInstance, new OperationInstance())
            .Create();
        var reminder = _fixture
            .Build<Reminder>()
            .With(r => r.Id, reminderId)
            .With(r => r.OperationId, operationId)
            .With(r => r.Operation, operation)
            .Create();
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId, _cancellationToken))
            .ReturnsAsync(true);
        _telegramUserRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(op => op.Id == telegramId, _cancellationToken))
            .ReturnsAsync(telegram);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(operationId, _cancellationToken))
            .ReturnsAsync(true);
        _operationRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(op => op.Id == operationId, _cancellationToken))
            .ReturnsAsync(operation);
        _reminderRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(op => op.Id == reminderId, _cancellationToken))
            .ReturnsAsync(reminder);
        _reminderRepositoryMock
            .Setup(repo => repo.IsExistAsync(reminderId, _cancellationToken))
            .ReturnsAsync(true);
        
        var result = await _reminderService.DeleteReminderAsync(telegramId, operationId, reminderId, _cancellationToken);
        
        _reminderRepositoryMock.Verify(repo => 
                repo.DeleteEntityAsync(It.IsAny<Reminder>(), _cancellationToken), 
            Times.Never);
        _hangfireReminderJobHelperMock.Verify(helper => 
                helper.DeleteJobForReminder(It.IsAny<Reminder>()),
            Times.Never);
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task Deleting_Existing_Reminder_That_Does_Not_Belong_To_Operation_Is_Fail()
    {
        var reminderId = _fixture.Create<long>();
        var telegramId = _fixture.Create<long>();
        var operationId = _fixture.Create<long>();
        var telegram = _fixture
            .Build<TelegramUser>()
            .With(u => u.TelegramId, telegramId)
            .With(u => u.Operations, new List<Operation>())
            .Create();
        var operation = _fixture
            .Build<Operation>()
            .With(op => op.Id, operationId)
            .With(op => op.TelegramUserId, telegramId)
            .With(op => op.History, new List<OperationInstance>())
            .With(op => op.Reminders, new List<Reminder>())
            .With(op => op.TelegramUser, telegram)
            .With(op => op.NextOperationInstance, new OperationInstance())
            .Create();
        telegram.Operations.Add(operation);
        var reminder = _fixture
            .Build<Reminder>()
            .With(r => r.Id, reminderId)
            .With(r => r.OperationId, operationId + 1)
            .With(r => r.Operation, new Operation{Id = operationId + 1})
            .Create();
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId, _cancellationToken))
            .ReturnsAsync(true);
        _telegramUserRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(op => op.Id == telegramId, _cancellationToken))
            .ReturnsAsync(telegram);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(operationId, _cancellationToken))
            .ReturnsAsync(true);
        _operationRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(op => op.Id == operationId, _cancellationToken))
            .ReturnsAsync(operation);
        _reminderRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(op => op.Id == reminderId, _cancellationToken))
            .ReturnsAsync(reminder);
        _reminderRepositoryMock
            .Setup(repo => repo.IsExistAsync(reminderId, _cancellationToken))
            .ReturnsAsync(true);
        
        var result = await _reminderService.DeleteReminderAsync(telegramId, operationId, reminderId, _cancellationToken);
        
        _reminderRepositoryMock.Verify(repo => 
                repo.DeleteEntityAsync(It.IsAny<Reminder>(), _cancellationToken), 
            Times.Never);
        _hangfireReminderJobHelperMock.Verify(helper => 
                helper.DeleteJobForReminder(It.IsAny<Reminder>()),
            Times.Never);
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task Deleting_Not_Existing_Reminder_Is_Fail()
    {
        var reminderId = _fixture.Create<long>();
        var telegramId = _fixture.Create<long>();
        var operationId = _fixture.Create<long>();
        var telegram = _fixture
            .Build<TelegramUser>()
            .With(u => u.TelegramId, telegramId)
            .With(u => u.Operations, new List<Operation>())
            .Create();
        var operation = _fixture
            .Build<Operation>()
            .With(op => op.Id, operationId)
            .With(op => op.TelegramUserId, telegramId)
            .With(op => op.History, new List<OperationInstance>())
            .With(op => op.Reminders, new List<Reminder>())
            .With(op => op.TelegramUser, new TelegramUser{TelegramId = telegramId})
            .With(op => op.NextOperationInstance, new OperationInstance())
            .Create();
        telegram.Operations.Add(operation);
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId, _cancellationToken))
            .ReturnsAsync(true);
        _telegramUserRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(op => op.Id == telegramId, _cancellationToken))
            .ReturnsAsync(telegram);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(operationId, _cancellationToken))
            .ReturnsAsync(true);
        _operationRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(op => op.Id == operationId, _cancellationToken))
            .ReturnsAsync(operation);
        _reminderRepositoryMock
            .Setup(repo => repo.IsExistAsync(reminderId, _cancellationToken))
            .ReturnsAsync(false);
        
        var result = await _reminderService.DeleteReminderAsync(telegramId, operationId, reminderId, _cancellationToken);
        
        _reminderRepositoryMock.Verify(repo => 
                repo.DeleteEntityAsync(It.IsAny<Reminder>(), _cancellationToken), 
            Times.Never);
        _hangfireReminderJobHelperMock.Verify(helper => 
                helper.DeleteJobForReminder(It.IsAny<Reminder>()),
            Times.Never);
        Assert.True(result.IsFailed);
    }
}