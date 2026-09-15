using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using MarketPlaceApi.Controllers;
using MarketPlaceApi.Dtos;
using MarketPlaceApi.Models;
using MarketPlaceApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace MarketPlaceApi.Tests
{
    public class AdminRoastersControllerTests
    {
        private readonly Mock<IRoasterProfileService> _mockRoasterService;
        private readonly AdminRoastersController _controller;

        public AdminRoastersControllerTests()
        {
            _mockRoasterService = new Mock<IRoasterProfileService>();
            _controller = new AdminRoastersController(_mockRoasterService.Object);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "admin-user-id"),
                new(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
        }

        [Fact]
        public void RoasterProfile_DefaultsToPendingApproval()
        {
            var profile = new RoasterProfile
            {
                UserId = "user-1"
            };

            Assert.Equal(ApprovalStatus.Pending, profile.ApprovalStatus);
        }

        [Fact]
        public async Task UpdateStatus_ReturnsOk_WhenStatusApproved()
        {
            var expectedDto = new RoasterProfileDto
            {
                Id = 1,
                UserId = "seller-1",
                CompanyName = "Test Roastery",
                ApprovalStatus = ApprovalStatus.Approved
            };

            _mockRoasterService
                .Setup(s => s.UpdateApprovalStatusAsync("1", ApprovalStatus.Approved))
                .ReturnsAsync(expectedDto);

            var dto = new UpdateRoasterStatusDto { Status = ApprovalStatus.Approved };
            var result = await _controller.UpdateStatus("1", dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedDto, okResult.Value);
        }

        [Fact]
        public async Task UpdateStatus_ReturnsOk_WhenStatusRejected()
        {
            var expectedDto = new RoasterProfileDto
            {
                Id = 2,
                UserId = "seller-2",
                CompanyName = "Rejected Roastery",
                ApprovalStatus = ApprovalStatus.Rejected
            };

            _mockRoasterService
                .Setup(s => s.UpdateApprovalStatusAsync("2", ApprovalStatus.Rejected))
                .ReturnsAsync(expectedDto);

            var dto = new UpdateRoasterStatusDto { Status = ApprovalStatus.Rejected };
            var result = await _controller.UpdateStatus("2", dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedDto, okResult.Value);
        }

        [Fact]
        public async Task UpdateStatus_ReturnsNotFound_WhenProfileDoesNotExist()
        {
            _mockRoasterService
                .Setup(s => s.UpdateApprovalStatusAsync("999", ApprovalStatus.Approved))
                .ThrowsAsync(new KeyNotFoundException("Roaster profile '999' not found."));

            var dto = new UpdateRoasterStatusDto { Status = ApprovalStatus.Approved };
            var result = await _controller.UpdateStatus("999", dto);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);
        }
    }
}
