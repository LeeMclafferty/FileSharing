using FileSharing.Controllers;
using FileSharing.Interfaces;
using FileSharing.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Claims;
using Xunit;

namespace FileSharing.Test
{
    public class AccountControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly Mock<IEmailSender> _emailSender;
        private readonly Mock<IRenderService> _renderService;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _mockHttpContext = new Mock<HttpContext>();
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);

            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<IPasswordHasher<ApplicationUser>>(),
                Array.Empty<IUserValidator<ApplicationUser>>(),
                Array.Empty<IPasswordValidator<ApplicationUser>>(),
                Mock.Of<ILookupNormalizer>(),
                Mock.Of<IdentityErrorDescriber>(),
                Mock.Of<IServiceProvider>(),
                Mock.Of<ILogger<UserManager<ApplicationUser>>>()
            );

            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                _userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<ILogger<SignInManager<ApplicationUser>>>(),
                Mock.Of<IAuthenticationSchemeProvider>(),
                Mock.Of<IUserConfirmation<ApplicationUser>>()
            );
            _emailSender = new Mock<IEmailSender>();
            _renderService = new Mock<IRenderService>();

            _controller = new AccountController(_userManagerMock.Object, _signInManagerMock.Object, _emailSender.Object, _renderService.Object);
        }

        [Fact]
        public async Task SignIn_ValidUser_SignsInAndRedirects()
        {
            var model = new SignInViewModel { Email = "test@example.com", Password = "password123" };
            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };

            _userManagerMock.Setup(x => x.FindByEmailAsync(model.Email)).ReturnsAsync(user);
            _signInManagerMock.Setup(x => x.PasswordSignInAsync(model.Email, model.Password, true, false))
                              .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            var result = await _controller.SignIn(model);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Upload", redirectResult.ActionName);
            Assert.Equal("Upload", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Register_ValidModel_UserCreatedAndRedirects()
        {
            var model = new RegisterViewModel { Email = "test@example.com", Password = "password123" };
            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), model.Password))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _controller.Register(model);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Upload", redirectResult.ActionName);
            Assert.Equal("Upload", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Logout_LogoutAndRedirects()
        {
            _signInManagerMock.Setup(x => x.SignOutAsync()).Returns(Task.CompletedTask);

            var result = await _controller.Logout();

            _signInManagerMock.Verify(x => x.SignOutAsync(), Times.Once());

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
        }

        [Fact]
        public async Task RequestPasswordReset_UserNotFound_AddsModelErrorAndReturnsView()
        {
            var model = new PasswordResetViewModel { Email = "test@example.com" };

            // Mock the UserManager's FindByEmailAsync method to return null (user not found)
            _userManagerMock.Setup(x => x.FindByEmailAsync(model.Email)).ReturnsAsync((ApplicationUser?)null);

            var result = await _controller.RequestPasswordReset(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.True(_controller.ModelState.ErrorCount > 0);
            Assert.Contains(_controller.ModelState, e => e.Value?.Errors.Any(error => error.ErrorMessage == "User not found.") == true);
        }
    }
}

