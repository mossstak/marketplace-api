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
            Email = "buyer@test.com", Password = "Password1!", ConfirmPassword = "Password1!",
            FirstName = "A", LastName = "B", Role = "Buyer",
            AddressOne = "1 Main St", City = "London", Country = "UK", PostalCode = "E1 1AA"
        };
        _mockUserService
            .Setup(s => s.Register(It.IsAny<RegisterDto>()))
            .ThrowsAsync(new Exception("User creation failed."));

        var result = await _controller.Register(dto);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("User creation failed.", bad.Value);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenRoleIsAdmin()
    {
        // Admin accounts must never be self-service registered.
        var dto = new RegisterDto
        {
            Email = "wannabe-admin@test.com", Password = "Password1!", ConfirmPassword = "Password1!",
            FirstName = "A", LastName = "B", Role = "Admin"
        };

        var result = await _controller.Register(dto);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Role must be Seller or Buyer.", bad.Value);
        _mockUserService.Verify(s => s.Register(It.IsAny<RegisterDto>()), Times.Never);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenRoleIsUnrecognized()
    {
        var dto = new RegisterDto
        {
            Email = "weird@test.com", Password = "Password1!", ConfirmPassword = "Password1!",
            FirstName = "A", LastName = "B", Role = "SuperUser"
        };

        var result = await _controller.Register(dto);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Role must be Seller or Buyer.", bad.Value);
        _mockUserService.Verify(s => s.Register(It.IsAny<RegisterDto>()), Times.Never);
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
        var user = new User { Id = "user-abc", Email = "me@test.com", FirstName = "Ann", PasswordHash = "super-secret-hash" };
        SetUserClaim(user.Id);
        _mockUserService.Setup(s => s.GetUserByIdAsync(user.Id)).ReturnsAsync(user);
        _mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Buyer" });

        var result = await _controller.GetMe();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);

        var valueType = ok.Value!.GetType();
        Assert.Equal(user.Id, valueType.GetProperty("Id")?.GetValue(ok.Value));
        Assert.Equal(user.FirstName, valueType.GetProperty("FirstName")?.GetValue(ok.Value));

        // Regression guard for the /me PasswordHash leak: the response must never
        // expose the raw Identity entity or any of its sensitive fields.
        Assert.Null(valueType.GetProperty("PasswordHash"));
        Assert.Null(valueType.GetProperty("SecurityStamp"));
    }

    // ── EditUser / UpdateUser authorization ─────────────────────────────────────

    [Fact]
    public async Task EditUser_ReturnsForbid_WhenEditingAnotherUsersProfile()
    {
        SetUserClaim("user-1");
        var dto = new EditUserDto { FirstName = "Hacked" };

        var result = await _controller.EditUser("user-2", dto);

        Assert.IsType<ForbidResult>(result);
        _mockUserService.Verify(s => s.EditUserAsync(It.IsAny<string>(), It.IsAny<EditUserDto>()), Times.Never);
    }

    [Fact]
    public async Task EditUser_ReturnsOk_WhenEditingOwnProfile()
    {
        SetUserClaim("user-1");
        var dto = new EditUserDto { FirstName = "Self" };

        var result = await _controller.EditUser("user-1", dto);

        Assert.IsType<OkObjectResult>(result);
        _mockUserService.Verify(s => s.EditUserAsync("user-1", dto), Times.Once);
    }

    [Fact]
    public async Task EditUser_ReturnsOk_WhenAdminEditsAnotherUsersProfile()
    {
        SetUserClaim("admin-1", "Admin");
        var dto = new EditUserDto { FirstName = "ByAdmin" };

        var result = await _controller.EditUser("user-2", dto);

        Assert.IsType<OkObjectResult>(result);
        _mockUserService.Verify(s => s.EditUserAsync("user-2", dto), Times.Once);
    }

    [Fact]
    public async Task UpdateUser_ReturnsForbid_WhenUpdatingAnotherUsersProfile()
    {
        SetUserClaim("user-1");
        var dto = new UpdateUserDto { FirstName = "Hacked", LastName = "Hacker", Email = "h@test.com" };

        var result = await _controller.UpdateUser("user-2", dto);

        Assert.IsType<ForbidResult>(result);
        _mockUserService.Verify(s => s.UpdateUserAsync(It.IsAny<string>(), It.IsAny<UpdateUserDto>()), Times.Never);
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private void SetUserClaim(string userId, string? role = null)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
        if (role != null)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var identity = new ClaimsIdentity(claims, "TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }
}
