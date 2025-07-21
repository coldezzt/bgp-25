using AutoFixture;
using Moq;
using Reglamentator.Application.Dtos;
using Reglamentator.Application.Services;
using Reglamentator.Domain.Entities;
using Reglamentator.Domain.Interfaces;

namespace Reglamentator.Test.Backend;

public class UserServiceTests
{
    private readonly Mock<ITelegramUserRepository> _telegramUserRepositoryMock = new ();
    private readonly Fixture _fixture = new ();
    private readonly UserService _userService;
    private readonly CancellationToken _cancellationToken = new();
    public UserServiceTests()
    {
        _userService = new UserService(_telegramUserRepositoryMock.Object);
    }
    
    [Fact]
    public async Task Creating_New_User_Saves_Him()
    {
        var userId = _fixture.Create<int>();
        var userDto = _fixture
            .Build<CreateUserDto>()
            .With(dto => dto.TelegramId, userId)
            .Create();
        
        var userResult = await _userService.CreateUserAsync(userDto, _cancellationToken);
        
        _telegramUserRepositoryMock.Verify(repo => 
                repo.InsertEntityAsync(It.IsAny<TelegramUser>(), _cancellationToken), 
            Times.Once);
        Assert.True(userResult.IsSuccess);
        Assert.Equal(userId, userResult.ValueOrDefault?.TelegramId);
    }

    [Fact]
    public async Task Creating_Existing_User_Does_Not_Saves_Him()
    {
        var userId = _fixture.Create<int>();
        var userDto = _fixture
            .Build<CreateUserDto>()
            .With(dto => dto.TelegramId, userId)
            .Create();
        var expected = _fixture
            .Build<TelegramUser>()
            .With(user => user.Operations, new List<Operation>())
            .With(user => user.TelegramId, userId)
            .Create();
        _telegramUserRepositoryMock
            .Setup(repo => repo.GetEntityByFilterAsync(tu => tu.TelegramId == userDto.TelegramId, _cancellationToken))
            .ReturnsAsync(expected);
        
        var userResult = await _userService.CreateUserAsync(userDto, _cancellationToken);
        
        _telegramUserRepositoryMock.Verify(repo => 
                repo.InsertEntityAsync(It.IsAny<TelegramUser>(), _cancellationToken), 
            Times.Never);
        Assert.True(userResult.IsSuccess);
        Assert.Equal(expected, userResult.ValueOrDefault);
    }
}