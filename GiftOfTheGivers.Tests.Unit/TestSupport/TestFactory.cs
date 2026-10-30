using GiftOfTheGivers.Web.Data;
using GiftOfTheGivers.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace GiftOfTheGivers.Tests.Unit.TestSupport
{
    // Shared builders so each test class stays short and readable.
    public static class TestFactory
    {
        public static ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        public static Mock<UserManager<ApplicationUser>> MockUserManager() =>
            new(Mock.Of<IUserStore<ApplicationUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);

        public static Mock<SignInManager<ApplicationUser>> MockSignInManager(UserManager<ApplicationUser> userManager) =>
            new(userManager, Mock.Of<IHttpContextAccessor>(), Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null!, null!, null!, null!);

        public static ControllerContext ControllerContext() =>
            new() { HttpContext = new DefaultHttpContext() };

        public static ITempDataDictionary TempData() =>
            new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
    }
}
