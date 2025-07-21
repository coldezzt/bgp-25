using AutoFixture;
using Moq;
using Reglamentator.Application.Abstractions;
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
}