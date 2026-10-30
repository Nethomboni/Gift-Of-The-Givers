using GiftOfTheGivers.Tests.Unit.TestSupport;
using GiftOfTheGivers.Web.Controllers;
using GiftOfTheGivers.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GiftOfTheGivers.Tests.Unit.Controllers
{
    public class AccountControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManager = TestFactory.MockUserManager();
        private readonly Mock<SignInManager<ApplicationUser>> _signInManager;
        private readonly AccountController _sut;

        public AccountControllerTests()
        {
            _signInManager = TestFactory.MockSignInManager(_userManager.Object);
            _sut = new AccountController(_userManager.Object, _signInManager.Object)
            {
                ControllerContext = TestFactory.ControllerContext()
            };
        }

        private static LoginViewModel LoginModel(string? returnUrl = null) => new()
        {
            Email = "user@test.com", Password = "Pass1!", ReturnUrl = returnUrl
        };

        [Fact]
        public void Login_Get_ReturnsViewWithReturnUrl()
        {
            var view = Assert.IsType<ViewResult>(_sut.Login("/Donation"));
            Assert.Equal("/Donation", ((LoginViewModel)view.Model!).ReturnUrl);
        }

        [Fact]
        public async Task Login_InvalidModelState_ReturnsViewAndDoesNotSignIn()
        {
            _sut.ModelState.AddModelError("Email", "Required");
            var result = await _sut.Login(LoginModel());
            Assert.IsType<ViewResult>(result);
            _signInManager.Verify(s => s.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task Login_WrongPassword_ReturnsViewWithError()
        {
            _signInManager.Setup(s => s.PasswordSignInAsync("user@test.com", "Pass1!", false, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            var result = await _sut.Login(LoginModel());

            Assert.IsType<ViewResult>(result);
            Assert.False(_sut.ModelState.IsValid);
        }

        [Fact]
        public async Task Login_EmployeeSucceeds_RedirectsToEmployeeDashboard()
        {
            var user = new ApplicationUser { Email = "user@test.com" };
            _signInManager.Setup(s => s.PasswordSignInAsync("user@test.com", "Pass1!", false, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
            _userManager.Setup(u => u.FindByEmailAsync("user@test.com")).ReturnsAsync(user);
            _userManager.Setup(u => u.IsInRoleAsync(user, "Employee")).ReturnsAsync(true);

            var redirect = Assert.IsType<RedirectToActionResult>(await _sut.Login(LoginModel()));

            Assert.Equal("Dashboard", redirect.ActionName);
            Assert.Equal("Employee", redirect.ControllerName);
        }

        [Fact]
        public async Task Login_DonorSucceeds_RedirectsToDonorDashboard()
        {
            var user = new ApplicationUser { Email = "user@test.com" };
            _signInManager.Setup(s => s.PasswordSignInAsync("user@test.com", "Pass1!", false, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
            _userManager.Setup(u => u.FindByEmailAsync("user@test.com")).ReturnsAsync(user);
            _userManager.Setup(u => u.IsInRoleAsync(user, "Employee")).ReturnsAsync(false);

            var redirect = Assert.IsType<RedirectToActionResult>(await _sut.Login(LoginModel()));

            Assert.Equal("Donor", redirect.ControllerName);
        }

        [Fact]
        public async Task Login_SucceedsWithLocalReturnUrl_RedirectsToThatUrl()
        {
            _signInManager.Setup(s => s.PasswordSignInAsync("user@test.com", "Pass1!", false, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
            var url = new Mock<IUrlHelper>();
            url.Setup(u => u.IsLocalUrl("/Donation")).Returns(true);
            _sut.Url = url.Object;

            var redirect = Assert.IsType<RedirectResult>(await _sut.Login(LoginModel("/Donation")));

            Assert.Equal("/Donation", redirect.Url);
        }

        [Fact]
        public void Register_Get_ReturnsView()
        {
            Assert.IsType<ViewResult>(_sut.Register());
        }

        [Fact]
        public async Task Register_InvalidModelState_ReturnsViewAndNeverCreatesUser()
        {
            _sut.ModelState.AddModelError("Email", "Required");
            var result = await _sut.Register(new RegisterViewModel());
            Assert.IsType<ViewResult>(result);
            _userManager.Verify(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Register_ValidModel_CreatesDonorSignsInAndRedirects()
        {
            _userManager.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), "Pass1!"))
                .ReturnsAsync(IdentityResult.Success);
            _userManager.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Donor"))
                .ReturnsAsync(IdentityResult.Success);

            var model = new RegisterViewModel { FullName = "New Donor", Email = "new@test.com", Password = "Pass1!", ConfirmPassword = "Pass1!" };
            var redirect = Assert.IsType<RedirectToActionResult>(await _sut.Register(model));

            Assert.Equal("Donor", redirect.ControllerName);
            _userManager.Verify(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Donor"), Times.Once);
            _signInManager.Verify(s => s.SignInAsync(It.IsAny<ApplicationUser>(), false, null), Times.Once);
        }

        [Fact]
        public async Task Register_CreateFails_ReturnsViewWithIdentityErrors()
        {
            _userManager.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Email already taken." }));

            var model = new RegisterViewModel { FullName = "X", Email = "x@test.com", Password = "Pass1!", ConfirmPassword = "Pass1!" };
            var result = await _sut.Register(model);

            Assert.IsType<ViewResult>(result);
            Assert.False(_sut.ModelState.IsValid);
            _userManager.Verify(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Logout_SignsOutAndRedirectsToHome()
        {
            var redirect = Assert.IsType<RedirectToActionResult>(await _sut.Logout());

            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
            _signInManager.Verify(s => s.SignOutAsync(), Times.Once);
        }

        [Fact]
        public void AccessDenied_ReturnsView()
        {
            Assert.IsType<ViewResult>(_sut.AccessDenied());
        }
    }
}
