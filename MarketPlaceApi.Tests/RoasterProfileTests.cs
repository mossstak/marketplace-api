using System.Security.Claims;
using MarketPlaceApi.Controllers;
using MarketPlaceApi.Data;
using MarketPlaceApi.Dtos;
using MarketPlaceApi.Models;
using MarketPlaceApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class RoasterProfileTests
{
    private readonly Mock<IRoasterProfileService> _mockRoasterService;
    private readonly RoasterProfileController _controller;

    public RoasterProfileTests()
    {
        _mockRoasterService = new Mock<IRoasterProfileService>();
        _controller = new RoasterProfileController(_mockRoasterService.Object);
    }

    [Fact]
    public async Task BecomeRoaster_ReturnsUnauthorized_WhenNoUserIdClaimPresent()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };

        var dto = new BecomeRoasterDto { CompanyName = "Roast Masters" };
        var result = await _controller.BecomeRoaster(dto);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task BecomeRoaster_ReturnsOk_WhenServiceSucceeds()
    {
        SetUserClaim("user-100");
        var dto = new BecomeRoasterDto
        {
            CompanyName = "Alpine Coffee Roasters",
            City = "Inverness",
            Country = "UK"
        };

        var expectedResponse = new BecomeRoasterResponseDto
        {
            Profile = new RoasterProfileDto { CompanyName = dto.CompanyName, City = dto.City, Country = dto.Country },
            Token = "new-seller-jwt-token",
            Roles = new List<string> { "Buyer", "Seller" },
            OnboardingUrl = "https://connect.stripe.com/setup/s/mock"
        };

        _mockRoasterService
            .Setup(s => s.BecomeRoasterAsync("user-100", dto))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.BecomeRoaster(dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task BecomeRoaster_ReturnsBadRequest_WhenUserAlreadyHasRoasterProfile()
    {
        SetUserClaim("user-100");
        var dto = new BecomeRoasterDto { CompanyName = "Second Roastery" };

        _mockRoasterService
            .Setup(s => s.BecomeRoasterAsync("user-100", dto))
            .ThrowsAsync(new InvalidOperationException("User already has a roaster profile."));

        var result = await _controller.BecomeRoaster(dto);

        var badResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badResult.Value);
    }

    [Fact]
    public async Task GetAllRoaster_ReturnsOnlyApprovedRoasters()
    {
        var approvedRoasters = new List<RoasterProfileDto>
        {
            new() { Id = 1, CompanyName = "Approved Roaster 1", ApprovalStatus = ApprovalStatus.Approved },
            new() { Id = 2, CompanyName = "Approved Roaster 2", ApprovalStatus = ApprovalStatus.Approved }
        };

        _mockRoasterService
            .Setup(s => s.GetAllRoaster())
            .ReturnsAsync(approvedRoasters);

        var result = await _controller.GetAllRoaster();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var roasters = Assert.IsAssignableFrom<IEnumerable<RoasterProfileDto>>(okResult.Value);
        Assert.All(roasters, r => Assert.Equal(ApprovalStatus.Approved, r.ApprovalStatus));
    }

    [Fact]
    public async Task GetPublic_ReturnsNotFound_WhenRoasterNotApproved()
    {
        _mockRoasterService
            .Setup(s => s.GetPublicByUserIdAsync("unapproved-user"))
            .ThrowsAsync(new KeyNotFoundException("Roaster profile not found."));

        var result = await _controller.GetPublic("unapproved-user");

        Assert.IsType<NotFoundObjectResult>(result);
    }

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
