using GiftOfTheGivers.Web.Data;
using GiftOfTheGivers.Web.Models;
using GiftOfTheGivers.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Tests.Unit.Services
{
    public class DonationServiceTests
    {
        // Each test gets its own isolated in-memory database.
        private static (DonationService Service, ApplicationDbContext Context) CreateSut()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            return (new DonationService(context), context);
        }

        private static DonationViewModel Model(string type = "OneOff", string? frequency = null,
            bool anonymous = false, string? name = "Thandi Nkosi", decimal amount = 100m) => new()
        {
            DonationType = type, Frequency = frequency, IsAnonymous = anonymous,
            DonorName = name, Amount = amount, Currency = "ZAR", Cause = "General Relief"
        };

        [Fact]
        public async Task CreateDonationAsync_AnonymousDonation_SetsAnonymousDonorName()
        {
            var (sut, _) = CreateSut();
            var result = await sut.CreateDonationAsync(Model(anonymous: true), null, null);
            Assert.Equal("Anonymous Donor", result.DonorName);
        }

        [Fact]
        public async Task CreateDonationAsync_Recurring_KeepsFrequency()
        {
            var (sut, _) = CreateSut();
            var result = await sut.CreateDonationAsync(Model("Recurring", "Weekly"), null, null);
            Assert.Equal("Weekly", result.Frequency);
        }

        [Fact]
        public async Task CreateDonationAsync_OneOff_SetsFrequencyToNull()
        {
            var (sut, _) = CreateSut();
            var result = await sut.CreateDonationAsync(Model("OneOff", "Monthly"), null, null);
            Assert.Null(result.Frequency);
        }

        [Fact]
        public async Task CreateDonationAsync_LoggedInUser_UsesAccountFullName()
        {
            var (sut, _) = CreateSut();
            var result = await sut.CreateDonationAsync(Model(name: "Typed Name"), "user-1", "Account Holder");
            Assert.Equal("Account Holder", result.DonorName);
            Assert.Equal("user-1", result.UserId);
        }

        [Fact]
        public async Task CreateDonationAsync_AfterSave_SetsReferenceNumber()
        {
            var (sut, _) = CreateSut();
            var result = await sut.CreateDonationAsync(Model(), null, null);
            Assert.Matches(@"^GOTG-\d{4}-000001$", result.ReferenceNumber);
        }

        [Fact]
        public async Task GetTotalAsync_MultipleDonations_SumsAmounts()
        {
            var (sut, _) = CreateSut();
            await sut.CreateDonationAsync(Model(amount: 100m), null, null);
            await sut.CreateDonationAsync(Model(amount: 250.50m), null, null);
            Assert.Equal(350.50m, await sut.GetTotalAsync());
        }

        [Fact]
        public async Task GetTotalAsync_NoDonations_ReturnsZero()
        {
            var (sut, _) = CreateSut();
            Assert.Equal(0m, await sut.GetTotalAsync());
        }

        [Fact]
        public async Task GetByReferenceAsync_UnknownReference_ReturnsNull()
        {
            var (sut, _) = CreateSut();
            Assert.Null(await sut.GetByReferenceAsync("GOTG-0000-999999"));
        }

        [Fact]
        public async Task GetByReferenceAsync_ExistingReference_ReturnsDonation()
        {
            var (sut, _) = CreateSut();
            var created = await sut.CreateDonationAsync(Model(), null, null);
            var found = await sut.GetByReferenceAsync(created.ReferenceNumber);
            Assert.Equal(created.Id, found!.Id);
        }

        [Fact]
        public async Task SetCertificateInfoAsync_UnknownId_DoesNotThrow()
        {
            var (sut, _) = CreateSut();
            var ex = await Record.ExceptionAsync(() => sut.SetCertificateInfoAsync(9999, "TC-1", "ABC"));
            Assert.Null(ex);
        }

        [Fact]
        public async Task SetCertificateInfoAsync_ExistingDonation_PersistsCertificateDetails()
        {
            var (sut, _) = CreateSut();
            var created = await sut.CreateDonationAsync(Model(), null, null);
            await sut.SetCertificateInfoAsync(created.Id, "TC-2026-000001", "VAL123");
            var found = await sut.GetByReferenceAsync(created.ReferenceNumber);
            Assert.Equal("TC-2026-000001", found!.CertificateNumber);
            Assert.Equal("VAL123", found.CertificateValidationCode);
        }

        [Fact]
        public async Task GetByUserAsync_OnlyReturnsThatUsersDonations()
        {
            var (sut, _) = CreateSut();
            await sut.CreateDonationAsync(Model(), "user-1", "A");
            await sut.CreateDonationAsync(Model(), "user-2", "B");
            var result = await sut.GetByUserAsync("user-1");
            Assert.Single(result);
        }

        [Fact]
        public async Task GetRecentAsync_CountLimit_ReturnsAtMostCount()
        {
            var (sut, _) = CreateSut();
            for (var i = 0; i < 5; i++) await sut.CreateDonationAsync(Model(), null, null);
            Assert.Equal(3, (await sut.GetRecentAsync(3)).Count);
        }
    }
}
