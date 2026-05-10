using Moq;
using Xunit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using MarketPlaceApi.Dtos;
using MarketPlaceApi.Models;
using MarketPlaceApi.Services;

public class UserServiceTests
{
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<SignInManager<User>> _mockSignInManager;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        var store = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(
            store.Object, null, null, null, null, null, null, null, null);

        _mockSignInManager = new Mock<SignInManager<User>>(
            _mockUserManager.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<User>>(),
            null, null, null, null);

        _userService = new UserService(_mockUserManager.Object, _mockSignInManager.Object);
    }

    // ── Register ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_ReturnsUser_WhenRegistrationSucceeds()
    {
        var dto = new RegisterDto
        {
            Email = "alice@test.com", Password = "Password1!", ConfirmPassword = "Password1!",
            FirstName = "Alice", LastName = "Smith", Role = "Buyer",
            AddressOne = "1 Road", City = "Leeds", Country = "UK", PostalCode = "LS1 1AA"
        };
        _mockUserManager
            .Setup(m => m.CreateAsync(It.IsAny<User>(), dto.Password))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager
            .Setup(m => m.AddToRoleAsync(It.IsAny<User>(), dto.Role))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _userService.Register(dto);

        Assert.NotNull(result);
        Assert.Equal(dto.Email, result.Email);
    }

    [Fact]
    public async Task Register_ThrowsException_WhenUserCreationFails()
    {
        var dto = new RegisterDto
        {
            Email = "fail@test.com", Password = "weak", ConfirmPassword = "weak",
            FirstName = "Bob", LastName = "Jones", Role = "Buyer"
        };
        _mockUserManager
            .Setup(m => m.CreateAsync(It.IsAny<User>(), dto.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak." }));

        await Assert.ThrowsAsync<Exception>(() => _userService.Register(dto));
    }

    [Fact]
    public async Task Register_ThrowsException_WhenRoleAssignmentFails()
    {
        var dto = new RegisterDto
        {
            Email = "norole@test.com", Password = "Password1!", ConfirmPassword = "Password1!",
            FirstName = "Carol", LastName = "White", Role = "UnknownRole"
        };
        _mockUserManager
            .Setup(m => m.CreateAsync(It.IsAny<User>(), dto.Password))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager
            .Setup(m => m.AddToRoleAsync(It.IsAny<User>(), dto.Role))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role does not exist." }));

        await Assert.ThrowsAsync<Exception>(() => _userService.Register(dto));
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_ReturnsUser_WhenCredentialsAreValid()
    {
        var dto  = new LoginDto { Email = "user@test.com", Password = "Password1!" };
        var user = new User { Email = dto.Email };

        _mockUserManager.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
        _mockSignInManager
            .Setup(m => m.CheckPasswordSignInAsync(user, dto.Password, false))
            .ReturnsAsync(SignInResult.Success);

        var result = await _userService.Login(dto);

        Assert.Equal(user, result);
    }

    [Fact]
    public async Task Login_ThrowsException_WhenUserNotFound()
    {
        var dto = new LoginDto { Email = "ghost@test.com", Password = "Password1!" };
        _mockUserManager.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<Exception>(() => _userService.Login(dto));
    }

    [Fact]
    public async Task Login_ThrowsException_WhenPasswordIsWrong()
    {
        var dto  = new LoginDto { Email = "user@test.com", Password = "WrongPassword!" };
        var user = new User { Email = dto.Email };

        _mockUserManager.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
        _mockSignInManager
            .Setup(m => m.CheckPasswordSignInAsync(user, dto.Password, false))
            .ReturnsAsync(SignInResult.Failed);

        await Assert.ThrowsAsync<Exception>(() => _userService.Login(dto));
    }
}
