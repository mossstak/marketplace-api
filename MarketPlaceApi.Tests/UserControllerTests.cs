using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using MarketPlaceApi.Controllers;
using MarketPlaceApi.Dtos;
using MarketPlaceApi.Models;
using MarketPlaceApi.Services;

public class UserControllerTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly TokenService _tokenService;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _mockUserService = new Mock<IUserService>();

        var store = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(
            store.Object, null, null, null, null, null, null, null, null);

        // Mock IConfiguration so TokenService can build a real JWT token during tests
        var jwtSection = new Mock<IConfigurationSection>();
        jwtSection.Setup(s => s["Key"]).Returns("super-secret-key-32chars-exactly!");
        jwtSection.Setup(s => s["Issuer"]).Returns("TestIssuer");
        jwtSection.Setup(s => s["Audience"]).Returns("TestAudience");
        jwtSection.Setup(s => s["DurationInMinutes"]).Returns("60");
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c.GetSection("Jwt")).Returns(jwtSection.Object);
        _tokenService = new TokenService(mockConfig.Object);

        _controller = new UserController(
            _mockUserService.Object,
            _mockUserManager.Object,
            _tokenService);
    }

    // ── Register ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenPasswordsDontMatch()
    {
        var dto = new RegisterDto
        {
            Email = "test@test.com", Password = "Password1!", ConfirmPassword = "Different!",
            FirstName = "John", LastName = "Doe", Role = "Buyer",
            AddressOne = "1 Road", City = "London", Country = "UK", PostalCode = "E1 1AA"
        };

        var result = await _controller.Register(dto);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Passwords do not match.", bad.Value);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenBuyerHasNoAddress()
    {
        var dto = new RegisterDto
        {
            Email = "buyer@test.com", Password = "Password1!", ConfirmPassword = "Password1!",
            FirstName = "Jane", LastName = "Smith", Role = "Buyer"
            // AddressOne / City / Country / PostalCode intentionally omitted
        };

        var result = await _controller.Register(dto);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Address fields are required for buyers and sellers.", bad.Value);
    }

    [Fact]
    public async Task Register_ReturnsOk_WhenValidBuyerIsRegistered()
    {
        var dto = new RegisterDto
        {
            Email = "buyer@test.com", Password = "Password1!", ConfirmPassword = "Password1!",
            FirstName = "Jane", LastName = "Smith", Role = "Buyer",
            AddressOne = "1 Main St", City = "London", Country = "UK", PostalCode = "E1 1AA"
        };
        _mockUserService.Setup(s => s.Register(It.IsAny<RegisterDto>())).ReturnsAsync(new User());

        var result = await _controller.Register(dto);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenServiceThrowsException()
    {
        var dto = new RegisterDto
        {
            Email = "admin@test.com", Password = "Password1!", ConfirmPassword = "Password1!",
            FirstName = "A", LastName = "B", Role = "Admin"
        };
        _mockUserService
            .Setup(s => s.Register(It.IsAny<RegisterDto>()))
            .ThrowsAsync(new Exception("User creation failed."));

        var result = await _controller.Register(dto);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("User creation failed.", bad.Value);
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_ReturnsOk_WithTokenAndRoles_WhenCredentialsAreValid()
    {
        var dto  = new LoginDto { Email = "user@test.com", Password = "Password1!" };
        var user = new User { Id = "user-123", Email = "user@test.com", UserName = "user@test.com" };

        _mockUserService.Setup(s => s.Login(It.IsAny<LoginDto>())).ReturnsAsync(user);
        _mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Buyer" });

        var result = await _controller.Login(dto);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenServiceThrowsException()
    {
        var dto = new LoginDto { Email = "bad@test.com", Password = "Wrong!" };
        _mockUserService
            .Setup(s => s.Login(It.IsAny<LoginDto>()))
            .ThrowsAsync(new Exception("Invalid credentials."));

        var result = await _controller.Login(dto);

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Invalid credentials.", unauthorized.Value);
    }

    // ── GetMe ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMe_ReturnsUnauthorized_WhenNoUserIdClaimPresent()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };

        var result = await _controller.GetMe();

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task GetMe_ReturnsNotFound_WhenUserDoesNotExist()
    {
        SetUserClaim("nonexistent-id");
        _mockUserService.Setup(s => s.GetUserByIdAsync("nonexistent-id")).ReturnsAsync((User?)null);

        var result = await _controller.GetMe();

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetMe_ReturnsOk_WhenUserExists()
    {
        var user = new User { Id = "user-abc", Email = "me@test.com" };
        SetUserClaim(user.Id);
        _mockUserService.Setup(s => s.GetUserByIdAsync(user.Id)).ReturnsAsync(user);

        var result = await _controller.GetMe();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(user, ok.Value);
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private void SetUserClaim(string userId)
    {
        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }
}
