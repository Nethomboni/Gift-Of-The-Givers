using GiftOfTheGivers.Web.Data;
using GiftOfTheGivers.Web.Models;
using GiftOfTheGivers.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Tests.Unit.Services
{
    public class VolunteerServiceTests
    {
        private static VolunteerService CreateSut()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            return new VolunteerService(new ApplicationDbContext(options));
        }

        private static VolunteerViewModel Model() => new()
        {
            FullName = "  Sipho Dlamini ",
            SelectedSkills = new() { "IT", "Driving" },
            SelectedAvailability = new() { "Weekends" },
            Email = " ", Phone = "0821234567"
        };

        [Fact]
        public async Task CreateVolunteerAsync_ValidModel_TrimsNameJoinsSkillsAndSetsReference()
        {
            var sut = CreateSut();
            var result = await sut.CreateVolunteerAsync(Model());
            Assert.Equal("Sipho Dlamini", result.FullName);
            Assert.Equal("IT, Driving", result.Skills);
            Assert.Null(result.Email);
            Assert.Matches(@"^VOL-\d{4}-00001$", result.ReferenceNumber);
            Assert.Equal("New", result.Status);
        }

        [Fact]
        public async Task GetByReferenceAsync_UnknownReference_ReturnsNull()
        {
            Assert.Null(await CreateSut().GetByReferenceAsync("VOL-0000-00000"));
        }

        [Fact]
        public async Task GetCountAsync_AfterTwoSignUps_ReturnsTwo()
        {
            var sut = CreateSut();
            await sut.CreateVolunteerAsync(Model());
            await sut.CreateVolunteerAsync(Model());
            Assert.Equal(2, await sut.GetCountAsync());
        }
    }
}
