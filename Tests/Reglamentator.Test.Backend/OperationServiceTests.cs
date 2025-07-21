using AutoFixture;
using Moq;
using Reglamentator.Application.Abstractions;
using Reglamentator.Application.Dtos;
using Reglamentator.Application.Services;
using Reglamentator.Domain.Entities;
using Reglamentator.Domain.Interfaces;

namespace Reglamentator.Test.Backend;

public class OperationServiceTests
{
    private readonly Mock<IOperationRepository> _operationRepositoryMock = new();
    private readonly Mock<IOperationInstanceRepository> _operationInstanceRepositoryMock = new();
    private readonly Mock<ITelegramUserRepository> _telegramUserRepositoryMock = new();
    private readonly Mock<IHangfireOperationJobHelper> _hangfireOperationJobHelperMock = new();
    private readonly Fixture _fixture = new();
    private readonly CancellationToken _cancellationToken = new();
    private readonly OperationService _operationService;

    public OperationServiceTests()
    {
        _operationService = new OperationService(_telegramUserRepositoryMock.Object, _operationRepositoryMock.Object, 
            _operationInstanceRepositoryMock.Object, _hangfireOperationJobHelperMock.Object);
    }

    [Fact]
    public async Task Getting_Planed_Operations_With_Valid_Telegram_User_Id_Is_Successful()
    {
        var range = _fixture.Create<TimeRange>();
        var telegramUser = _fixture
            .Build<TelegramUser>()
            .With(u => u.Operations, new List<Operation>())
            .Create();
        var operation = _fixture
            .Build<Operation>()
            .With(o => o.TelegramUser, telegramUser)
            .With(o => o.TelegramUserId, telegramUser.Id)
            .With(o => o.Reminders, new List<Reminder>())
            .With(o => o.NextOperationInstance, new OperationInstance())
            .With(o => o.History, new List<OperationInstance>())
            .Create();
        telegramUser.Operations.Add(operation);
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramUser.TelegramId,_cancellationToken))
            .ReturnsAsync(true);
        _telegramUserRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(u => u.TelegramId == telegramUser.TelegramId, _cancellationToken))
            .ReturnsAsync(telegramUser);
        var expected = _fixture
            .Build<OperationInstance>()
            .With(o => o.Operation, operation)
            .With(o => o.OperationId, operation.Id)
            .CreateMany()
            .ToList();
        _operationInstanceRepositoryMock
            .Setup(repo => repo.GetPlanedUserOperationsAsync(telegramUser.TelegramId, range, _cancellationToken))
            .ReturnsAsync(expected);
        
        var result = await _operationService.GetPlanedOperationsAsync(telegramUser.TelegramId, range, _cancellationToken);
        
        Assert.True(result.IsSuccess);
        Assert.All(result.Value, item => Assert.Equal(operation.Id, item.OperationId));
    }
    
    [Fact]
    public async Task Getting_Planed_Operations_With_Invalid_Telegram_User_Id_Is_Fail()
    {
        var range = _fixture.Create<TimeRange>();
        var telegramId = _fixture.Create<long>();
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId,_cancellationToken))
            .ReturnsAsync(false);
        
        var result = await _operationService.GetPlanedOperationsAsync(telegramId, range, _cancellationToken);
        
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task Getting_Operations_History_With_Valid_Telegram_User_Id_Is_Successful()
    {
        var telegramUser = _fixture
            .Build<TelegramUser>()
            .With(u => u.Operations, new List<Operation>())
            .Create();
        var operation = _fixture
            .Build<Operation>()
            .With(o => o.TelegramUser, telegramUser)
            .With(o => o.TelegramUserId, telegramUser.Id)
            .With(o => o.Reminders, new List<Reminder>())
            .With(o => o.NextOperationInstance, new OperationInstance())
            .With(o => o.History, new List<OperationInstance>())
            .Create();
        telegramUser.Operations.Add(operation);
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramUser.TelegramId,_cancellationToken))
            .ReturnsAsync(true);
        _telegramUserRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(u => u.TelegramId == telegramUser.TelegramId, _cancellationToken))
            .ReturnsAsync(telegramUser);
        var expected = _fixture
            .Build<OperationInstance>()
            .With(o => o.Operation, operation)
            .With(o => o.OperationId, operation.Id)
            .CreateMany()
            .ToList();
        _operationInstanceRepositoryMock
            .Setup(repo => repo.GetExecutedUserOperationsAsync(telegramUser.TelegramId, _cancellationToken))
            .ReturnsAsync(expected);
        
        var result = await _operationService.GetOperationHistoryAsync(telegramUser.TelegramId, _cancellationToken);
        
        Assert.True(result.IsSuccess);
        Assert.All(result.Value, item => Assert.Equal(operation.Id, item.OperationId));
    }
    
    [Fact]
    public async Task Getting_Operations_History_With_Invalid_Telegram_User_Id_Is_Fail()
    {
        var telegramId = _fixture.Create<long>();
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId,_cancellationToken))
            .ReturnsAsync(false);
        
        var result = await _operationService.GetOperationHistoryAsync(telegramId, _cancellationToken);
        
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task Getting_Operation_With_Invalid_Telegram_User_And_Operation_Is_Fail()
    {
        var telegramId = _fixture.Create<long>();
        var operationId = _fixture.Create<long>();
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId,_cancellationToken))
            .ReturnsAsync(false);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(operationId,_cancellationToken))
            .ReturnsAsync(false);
        
        var result = await _operationService.GetOperationAsync(telegramId, operationId, _cancellationToken);
        
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task Getting_Operation_With_Invalid_Telegram_User_And_Existing_Operation_Is_Fail()
    {
        var telegramId = _fixture.Create<long>();
        var operationId = _fixture.Create<long>();
        var operation = _fixture
            .Build<Operation>()
            .With(o => o.TelegramUser, new TelegramUser{TelegramId = telegramId+1})
            .With(o => o.TelegramUserId, telegramId + 1)
            .With(o => o.Reminders, new List<Reminder>())
            .With(o => o.NextOperationInstance, new OperationInstance())
            .With(o => o.History, new List<OperationInstance>())
            .Create();
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId,_cancellationToken))
            .ReturnsAsync(false);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(operationId,_cancellationToken))
            .ReturnsAsync(true);
        _operationRepositoryMock
            .Setup(repo => repo.GetWithRemindersAsync(o => o.Id == operationId, _cancellationToken))
            .ReturnsAsync(operation);
        
        var result = await _operationService.GetOperationAsync(telegramId, operationId, _cancellationToken);
        
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task Getting_Operation_With_Invalid_Operation_Is_Fail()
    {
        var telegramId = _fixture.Create<long>();
        var operationId = _fixture.Create<long>();
        var telegram = _fixture
            .Build<TelegramUser>()
            .With(u => u.TelegramId, telegramId)
            .With(u => u.Operations, new List<Operation>())
            .Create();
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId,_cancellationToken))
            .ReturnsAsync(true);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(operationId,_cancellationToken))
            .ReturnsAsync(false);
        _telegramUserRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(u => u.TelegramId == telegramId,_cancellationToken))
            .ReturnsAsync(telegram);
        
        var result = await _operationService.GetOperationAsync(telegramId, operationId, _cancellationToken);
        
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task Getting_Operation_That_Does_Not_Belong_To_User_Is_Fail()
    {
        var telegramId = _fixture.Create<long>();
        var operationId = _fixture.Create<long>();
        var telegram = _fixture
            .Build<TelegramUser>()
            .With(u => u.TelegramId, telegramId)
            .With(u => u.Operations, new List<Operation>())
            .Create();
        var operation = _fixture
            .Build<Operation>()
            .With(o => o.TelegramUser, new TelegramUser{TelegramId = telegramId+1})
            .With(o => o.TelegramUserId, telegramId + 1)
            .With(o => o.Reminders, new List<Reminder>())
            .With(o => o.NextOperationInstance, new OperationInstance())
            .With(o => o.History, new List<OperationInstance>())
            .Create();
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId,_cancellationToken))
            .ReturnsAsync(true);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(operationId,_cancellationToken))
            .ReturnsAsync(true);
        _telegramUserRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(u => u.TelegramId == telegramId,_cancellationToken))
            .ReturnsAsync(telegram);
        _operationRepositoryMock
            .Setup(repo => repo.GetWithRemindersAsync(o => o.Id == operationId, _cancellationToken))
            .ReturnsAsync(operation);
        
        var result = await _operationService.GetOperationAsync(telegramId, operationId, _cancellationToken);
        
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task Getting_Operation_With_Valid_User_And_Operation_Is_Successful()
    {
        var telegramId = _fixture.Create<long>();
        var operationId = _fixture.Create<long>();
        var telegram = _fixture
            .Build<TelegramUser>()
            .With(u => u.TelegramId, telegramId)
            .With(u => u.Operations, new List<Operation>())
            .Create();
        var operation = _fixture
            .Build<Operation>()
            .With(o => o.TelegramUser, telegram)
            .With(o => o.TelegramUserId, telegramId)
            .With(o => o.Reminders, new List<Reminder>())
            .With(o => o.NextOperationInstance, new OperationInstance())
            .With(o => o.History, new List<OperationInstance>())
            .Create();
        telegram.Operations.Add(operation);
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId,_cancellationToken))
            .ReturnsAsync(true);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(operationId,_cancellationToken))
            .ReturnsAsync(true);
        _telegramUserRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(u => u.TelegramId == telegramId,_cancellationToken))
            .ReturnsAsync(telegram);
        _operationRepositoryMock
            .Setup(repo => repo.GetWithRemindersAsync(o => o.Id == operationId, _cancellationToken))
            .ReturnsAsync(operation);
        
        var result = await _operationService.GetOperationAsync(telegramId, operationId, _cancellationToken);
        
        Assert.True(result.IsSuccess);
        Assert.Equal(operation.Id, result.ValueOrDefault.Id);
    }

    [Fact]
    public async Task Creating_Operation_With_Valid_User_And_Start_Date_Saves_It_And_Launch_Scheduling()
    {
        var telegramId = _fixture.Create<long>();
        var operationDto = _fixture
            .Build<CreateOperationDto>()
            .With(o => o.StartDate, DateTime.Now.AddDays(1))
            .Create();
        var telegram = _fixture
            .Build<TelegramUser>()
            .With(u => u.TelegramId, telegramId)
            .With(u => u.Operations, new List<Operation>())
            .Create();
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId,_cancellationToken))
            .ReturnsAsync(true);
        _telegramUserRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(u => u.TelegramId == telegramId,_cancellationToken))
            .ReturnsAsync(telegram);
        
        var result = await _operationService.CreateOperationAsync(telegramId, operationDto, _cancellationToken);
        
        _operationRepositoryMock.Verify(repo => 
                repo.InsertEntityAsync(It.IsAny<Operation>(), _cancellationToken), 
            Times.Once);
        _hangfireOperationJobHelperMock.Verify(helper => 
                helper.CreateJobsForOperation(It.IsAny<Operation>()),
            Times.Once);
        Assert.True(result.IsSuccess);
        Assert.Equal(telegramId, result.ValueOrDefault.TelegramUserId);
    }
    
    [Fact]
    public async Task? Creating_Operation_With_Valid_User_And_Invalid_Start_Date_Is_Fail()
    {
        var telegramId = _fixture.Create<long>();
        var operationDto = _fixture
            .Build<CreateOperationDto>()
            .With(o => o.StartDate, DateTime.Now.AddDays(-1))
            .Create();
        var telegram = _fixture
            .Build<TelegramUser>()
            .With(u => u.TelegramId, telegramId)
            .With(u => u.Operations, new List<Operation>())
            .Create();
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId,_cancellationToken))
            .ReturnsAsync(true);
        _telegramUserRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(u => u.TelegramId == telegramId,_cancellationToken))
            .ReturnsAsync(telegram);
        
        var result = await _operationService.CreateOperationAsync(telegramId, operationDto, _cancellationToken);
        
        _operationRepositoryMock.Verify(repo => 
                repo.InsertEntityAsync(It.IsAny<Operation>(), _cancellationToken), 
            Times.Never);
        _hangfireOperationJobHelperMock.Verify(helper => 
                helper.CreateJobsForOperation(It.IsAny<Operation>()),
            Times.Never);
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task? Creating_Operation_With_Invalid_User_Is_Fail()
    {
        var telegramId = _fixture.Create<long>();
        var operationDto = _fixture
            .Build<CreateOperationDto>()
            .With(o => o.StartDate, DateTime.Now.AddDays(-1))
            .Create();
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId,_cancellationToken))
            .ReturnsAsync(false);
        
        var result = await _operationService.CreateOperationAsync(telegramId, operationDto, _cancellationToken);
        
        _operationRepositoryMock.Verify(repo => 
                repo.InsertEntityAsync(It.IsAny<Operation>(), _cancellationToken), 
            Times.Never);
        _hangfireOperationJobHelperMock.Verify(helper => 
                helper.CreateJobsForOperation(It.IsAny<Operation>()),
            Times.Never);
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task Updating_Existing_Operation_With_Valid_User_And_Start_Date_Save_Changes_And_Relaunch_Scheduling()
    {
        var telegramId = _fixture.Create<long>();
        var operationId = _fixture.Create<long>();
        var operationDto = _fixture
            .Build<UpdateOperationDto>()
            .With(o => o.Id, operationId)
            .With(o => o.StartDate, DateTime.Now.AddDays(1))
            .Create();
        var telegram = _fixture
            .Build<TelegramUser>()
            .With(u => u.TelegramId, telegramId)
            .With(u => u.Operations, new List<Operation>())
            .Create();
        var operation = _fixture
            .Build<Operation>()
            .With(o => o.Id, operationId)
            .With(o => o.TelegramUserId, telegramId)
            .With(o => o.TelegramUser, telegram)
            .With(o => o.Reminders, new List<Reminder>())
            .With(o => o.NextOperationInstance, new OperationInstance())
            .With(o => o.History, new List<OperationInstance>())
            .Create();
        telegram.Operations.Add(operation);
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId,_cancellationToken))
            .ReturnsAsync(true);
        _telegramUserRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(u => u.TelegramId == telegramId,_cancellationToken))
            .ReturnsAsync(telegram);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(operationId, _cancellationToken))
            .ReturnsAsync(true);
        _operationRepositoryMock
            .Setup(repo => repo.GetWithDetailsForProcessJobAsync(o => o.Id == operationDto.Id, _cancellationToken))
            .ReturnsAsync(operation);
        
        var result = await _operationService.UpdateOperationAsync(telegramId, operationDto, _cancellationToken);
        
        _operationRepositoryMock.Verify(repo => 
                repo.UpdateEntityAsync(It.IsAny<Operation>(), _cancellationToken), 
            Times.Once);
        _hangfireOperationJobHelperMock.Verify(helper => 
                helper.UpdateJobsForOperation(It.IsAny<Operation>()),
            Times.Once);
        Assert.True(result.IsSuccess);
        Assert.Equal(telegramId, result.ValueOrDefault.TelegramUserId);
    }
    
    [Fact]
    public async Task Updating_Existing_Operation_With_Valid_User_And_Invalid_Start_Date_Is_Fail()
    {
        var telegramId = _fixture.Create<long>();
        var operationId = _fixture.Create<long>();
        var operationDto = _fixture
            .Build<UpdateOperationDto>()
            .With(o => o.Id, operationId)
            .With(o => o.StartDate, DateTime.Now.AddDays(-1))
            .Create();
        var telegram = _fixture
            .Build<TelegramUser>()
            .With(u => u.TelegramId, telegramId)
            .With(u => u.Operations, new List<Operation>())
            .Create();
        var operation = _fixture
            .Build<Operation>()
            .With(o => o.Id, operationId)
            .With(o => o.TelegramUserId, telegramId)
            .With(o => o.TelegramUser, telegram)
            .With(o => o.Reminders, new List<Reminder>())
            .With(o => o.NextOperationInstance, new OperationInstance())
            .With(o => o.History, new List<OperationInstance>())
            .Create();
        telegram.Operations.Add(operation);
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId,_cancellationToken))
            .ReturnsAsync(true);
        _telegramUserRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(u => u.TelegramId == telegramId,_cancellationToken))
            .ReturnsAsync(telegram);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(operationId, _cancellationToken))
            .ReturnsAsync(true);
        _operationRepositoryMock
            .Setup(repo => repo.GetWithDetailsForProcessJobAsync(o => o.Id == operationDto.Id, _cancellationToken))
            .ReturnsAsync(operation);
        
        var result = await _operationService.UpdateOperationAsync(telegramId, operationDto, _cancellationToken);
        
        _operationRepositoryMock.Verify(repo => 
                repo.UpdateEntityAsync(It.IsAny<Operation>(), _cancellationToken), 
            Times.Never);
        _hangfireOperationJobHelperMock.Verify(helper => 
                helper.UpdateJobsForOperation(It.IsAny<Operation>()),
            Times.Never);
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task Updating_Existing_Operation_With_Invalid_User_Is_Fail()
    {
        var telegramId = _fixture.Create<long>();
        var operationId = _fixture.Create<long>();
        var operationDto = _fixture
            .Build<UpdateOperationDto>()
            .With(o => o.Id, operationId)
            .With(o => o.StartDate, DateTime.Now.AddDays(1))
            .Create();
        var operation = _fixture
            .Build<Operation>()
            .With(o => o.Id, operationId)
            .With(o => o.TelegramUserId, telegramId+1)
            .With(o => o.TelegramUser, new TelegramUser{TelegramId = telegramId+1})
            .With(o => o.Reminders, new List<Reminder>())
            .With(o => o.NextOperationInstance, new OperationInstance())
            .With(o => o.History, new List<OperationInstance>())
            .Create();
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId,_cancellationToken))
            .ReturnsAsync(false);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(operationId, _cancellationToken))
            .ReturnsAsync(true);
        _operationRepositoryMock
            .Setup(repo => repo.GetWithDetailsForProcessJobAsync(o => o.Id == operationDto.Id, _cancellationToken))
            .ReturnsAsync(operation);
        
        var result = await _operationService.UpdateOperationAsync(telegramId, operationDto, _cancellationToken);
        
        _operationRepositoryMock.Verify(repo => 
                repo.UpdateEntityAsync(It.IsAny<Operation>(), _cancellationToken), 
            Times.Never);
        _hangfireOperationJobHelperMock.Verify(helper => 
                helper.UpdateJobsForOperation(It.IsAny<Operation>()),
            Times.Never);
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task Updating_Not_Existing_Operation_With_Valid_User_Is_Fail()
    {
        var telegramId = _fixture.Create<long>();
        var operationId = _fixture.Create<long>();
        var operationDto = _fixture
            .Build<UpdateOperationDto>()
            .With(o => o.StartDate, DateTime.Now.AddDays(1))
            .Create();
        var telegram = _fixture
            .Build<TelegramUser>()
            .With(u => u.TelegramId, telegramId)
            .With(u => u.Operations, new List<Operation>())
            .Create();
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId,_cancellationToken))
            .ReturnsAsync(true);
        _telegramUserRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(u => u.TelegramId == telegramId,_cancellationToken))
            .ReturnsAsync(telegram);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(operationId, _cancellationToken))
            .ReturnsAsync(false);
        
        var result = await _operationService.UpdateOperationAsync(telegramId, operationDto, _cancellationToken);
        
        _operationRepositoryMock.Verify(repo => 
                repo.UpdateEntityAsync(It.IsAny<Operation>(), _cancellationToken), 
            Times.Never);
        _hangfireOperationJobHelperMock.Verify(helper => 
                helper.UpdateJobsForOperation(It.IsAny<Operation>()),
            Times.Never);
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task Updating_Not_Existing_Operation_With_Invalid_User_Is_Fail()
    {
        var telegramId = _fixture.Create<long>();
        var operationId = _fixture.Create<long>();
        var operationDto = _fixture
            .Build<UpdateOperationDto>()
            .With(o => o.StartDate, DateTime.Now.AddDays(1))
            .Create();
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId,_cancellationToken))
            .ReturnsAsync(false);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(operationId, _cancellationToken))
            .ReturnsAsync(false);
        
        var result = await _operationService.UpdateOperationAsync(telegramId, operationDto, _cancellationToken);
        
        _operationRepositoryMock.Verify(repo => 
                repo.UpdateEntityAsync(It.IsAny<Operation>(), _cancellationToken), 
            Times.Never);
        _hangfireOperationJobHelperMock.Verify(helper => 
                helper.UpdateJobsForOperation(It.IsAny<Operation>()),
            Times.Never);
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task Updating_Existing_Operation_That_Does_Not_Belong_To_User_Is_Fail()
    {
        var telegramId = _fixture.Create<long>();
        var operationId = _fixture.Create<long>();
        var operationDto = _fixture
            .Build<UpdateOperationDto>()
            .With(o => o.Id, operationId)
            .With(o => o.StartDate, DateTime.Now.AddDays(1))
            .Create();
        var telegram = _fixture
            .Build<TelegramUser>()
            .With(u => u.TelegramId, telegramId)
            .With(u => u.Operations, new List<Operation>())
            .Create();
        var operation = _fixture
            .Build<Operation>()
            .With(o => o.Id, operationId)
            .With(o => o.TelegramUserId, telegramId + 1)
            .With(o => o.TelegramUser, new TelegramUser{ TelegramId = telegramId +1})
            .With(o => o.Reminders, new List<Reminder>())
            .With(o => o.NextOperationInstance, new OperationInstance())
            .With(o => o.History, new List<OperationInstance>())
            .Create();
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId,_cancellationToken))
            .ReturnsAsync(true);
        _telegramUserRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(u => u.TelegramId == telegramId,_cancellationToken))
            .ReturnsAsync(telegram);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(operationId, _cancellationToken))
            .ReturnsAsync(true);
        _operationRepositoryMock
            .Setup(repo => repo.GetWithDetailsForProcessJobAsync(o => o.Id == operationDto.Id, _cancellationToken))
            .ReturnsAsync(operation);
        
        var result = await _operationService.UpdateOperationAsync(telegramId, operationDto, _cancellationToken);
        
        _operationRepositoryMock.Verify(repo => 
                repo.UpdateEntityAsync(It.IsAny<Operation>(), _cancellationToken), 
            Times.Never);
        _hangfireOperationJobHelperMock.Verify(helper => 
                helper.UpdateJobsForOperation(It.IsAny<Operation>()),
            Times.Never);
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task Deleting_Existing_Operation_With_Valid_User_Remove_It_And_Stop_Scheduling()
    {
        var telegramId = _fixture.Create<long>();
        var operationId = _fixture.Create<long>();
        var telegram = _fixture
            .Build<TelegramUser>()
            .With(u => u.TelegramId, telegramId)
            .With(u => u.Operations, new List<Operation>())
            .Create();
        var operation = _fixture
            .Build<Operation>()
            .With(o => o.Id, operationId)
            .With(o => o.TelegramUserId, telegramId)
            .With(o => o.TelegramUser, telegram)
            .With(o => o.Reminders, new List<Reminder>())
            .With(o => o.NextOperationInstance, new OperationInstance())
            .With(o => o.History, new List<OperationInstance>())
            .Create();
        telegram.Operations.Add(operation);
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId,_cancellationToken))
            .ReturnsAsync(true);
        _telegramUserRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(u => u.TelegramId == telegramId,_cancellationToken))
            .ReturnsAsync(telegram);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(operationId, _cancellationToken))
            .ReturnsAsync(true);
        _operationRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(o => o.Id == operationId, _cancellationToken))
            .ReturnsAsync(operation);
        
        var result = await _operationService.DeleteOperationAsync(telegramId, operationId, _cancellationToken);
        
        _operationRepositoryMock.Verify(repo => 
                repo.DeleteEntityAsync(It.IsAny<Operation>(), _cancellationToken), 
            Times.Once);
        _hangfireOperationJobHelperMock.Verify(helper => 
                helper.DeleteJobsForOperation(It.IsAny<Operation>()),
            Times.Once);
        Assert.True(result.IsSuccess);
        Assert.Equal(telegramId, result.ValueOrDefault.TelegramUserId);
    }
    
    [Fact]
    public async Task Deleting_Existing_Operation_With_Invalid_User_Is_Fail()
    {
        var telegramId = _fixture.Create<long>();
        var operationId = _fixture.Create<long>();
        var operation = _fixture
            .Build<Operation>()
            .With(o => o.Id, operationId)
            .With(o => o.TelegramUserId, telegramId+1)
            .With(o => o.TelegramUser, new TelegramUser{TelegramId = telegramId+1})
            .With(o => o.Reminders, new List<Reminder>())
            .With(o => o.NextOperationInstance, new OperationInstance())
            .With(o => o.History, new List<OperationInstance>())
            .Create();
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId,_cancellationToken))
            .ReturnsAsync(false);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(operationId, _cancellationToken))
            .ReturnsAsync(true);
        _operationRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(o => o.Id == operationId, _cancellationToken))
            .ReturnsAsync(operation);
        
        var result = await _operationService.DeleteOperationAsync(telegramId, operationId, _cancellationToken);
        
        _operationRepositoryMock.Verify(repo => 
                repo.DeleteEntityAsync(It.IsAny<Operation>(), _cancellationToken), 
            Times.Never);
        _hangfireOperationJobHelperMock.Verify(helper => 
                helper.DeleteJobsForOperation(It.IsAny<Operation>()),
            Times.Never);
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task Deleting_Not_Existing_Operation_With_Valid_User_Is_Fail()
    {
        var telegramId = _fixture.Create<long>();
        var operationId = _fixture.Create<long>();
        var telegram = _fixture
            .Build<TelegramUser>()
            .With(u => u.TelegramId, telegramId)
            .With(u => u.Operations, new List<Operation>())
            .Create();
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId,_cancellationToken))
            .ReturnsAsync(true);
        _telegramUserRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(u => u.TelegramId == telegramId,_cancellationToken))
            .ReturnsAsync(telegram);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(operationId, _cancellationToken))
            .ReturnsAsync(false);
        
        var result = await _operationService.DeleteOperationAsync(telegramId, operationId, _cancellationToken);
        
        _operationRepositoryMock.Verify(repo => 
                repo.DeleteEntityAsync(It.IsAny<Operation>(), _cancellationToken), 
            Times.Never);
        _hangfireOperationJobHelperMock.Verify(helper => 
                helper.DeleteJobsForOperation(It.IsAny<Operation>()),
            Times.Never);
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task Deleting_Not_Existing_Operation_With_Invalid_User_Is_Fail()
    {
        var telegramId = _fixture.Create<long>();
        var operationId = _fixture.Create<long>();
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId,_cancellationToken))
            .ReturnsAsync(false);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(operationId, _cancellationToken))
            .ReturnsAsync(false);
        
        var result = await _operationService.DeleteOperationAsync(telegramId, operationId, _cancellationToken);
        
        _operationRepositoryMock.Verify(repo => 
                repo.DeleteEntityAsync(It.IsAny<Operation>(), _cancellationToken), 
            Times.Never);
        _hangfireOperationJobHelperMock.Verify(helper => 
                helper.DeleteJobsForOperation(It.IsAny<Operation>()),
            Times.Never);
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task Deleting_Existing_Operation_That_Does_Not_Belong_To_User_Is_Fail()
    {
        var telegramId = _fixture.Create<long>();
        var operationId = _fixture.Create<long>();
        var telegram = _fixture
            .Build<TelegramUser>()
            .With(u => u.TelegramId, telegramId)
            .With(u => u.Operations, new List<Operation>())
            .Create();
        var operation = _fixture
            .Build<Operation>()
            .With(o => o.TelegramUserId, telegramId + 1)
            .With(o => o.Id, operationId)
            .With(o => o.TelegramUser, new TelegramUser{ TelegramId = telegramId +1})
            .With(o => o.Reminders, new List<Reminder>())
            .With(o => o.NextOperationInstance, new OperationInstance())
            .With(o => o.History, new List<OperationInstance>())
            .Create();
        _telegramUserRepositoryMock
            .Setup(repo => repo.IsExistAsync(telegramId,_cancellationToken))
            .ReturnsAsync(true);
        _telegramUserRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(u => u.TelegramId == telegramId,_cancellationToken))
            .ReturnsAsync(telegram);
        _operationRepositoryMock
            .Setup(repo => repo.IsExistAsync(operation.Id, _cancellationToken))
            .ReturnsAsync(true);
        _operationRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(o => o.Id == operation.Id, _cancellationToken))
            .ReturnsAsync(operation);
        
        var result = await _operationService.DeleteOperationAsync(telegramId, operationId, _cancellationToken);
        
        _operationRepositoryMock.Verify(repo => 
                repo.DeleteEntityAsync(It.IsAny<Operation>(), _cancellationToken), 
            Times.Never);
        _hangfireOperationJobHelperMock.Verify(helper => 
                helper.DeleteJobsForOperation(It.IsAny<Operation>()),
            Times.Never);
        Assert.True(result.IsFailed);
    }
}