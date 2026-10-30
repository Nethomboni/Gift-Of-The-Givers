using System.Net;
using System.Net.Http.Json;
using GiftOfTheGivers.Web.Models;
using GiftOfTheGivers.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace GiftOfTheGivers.Tests.Unit.Services
{
    public class FunctionsApiClientTests
    {
        // Stub HTTP handler: no real network calls are made.
        private sealed class StubHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;
            public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) => _responder = responder;
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => Task.FromResult(_responder(request));
        }

        private sealed class ThrowingHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => throw new HttpRequestException("Connection refused");
        }

        private static FunctionsApiClient CreateSut(HttpMessageHandler handler) =>
            new(new HttpClient(handler) { BaseAddress = new Uri("http://localhost:7071/api/") },
                NullLogger<FunctionsApiClient>.Instance);

        private static Donation SampleDonation() => new()
        {
            ReferenceNumber = "GOTG-2026-000001", Amount = 100m, Currency = "ZAR", Cause = "Food Support", DonorName = "Thandi"
        };

        [Fact]
        public async Task GenerateTaxCertificateAsync_SuccessResponse_ReturnsCertificateAndCode()
        {
            var sut = CreateSut(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new { CertificateNumber = "TC-1", ValidationCode = "VAL" })
            }));

            var (number, code) = await sut.GenerateTaxCertificateAsync(SampleDonation());

            Assert.Equal("TC-1", number);
            Assert.Equal("VAL", code);
        }

        [Fact]
        public async Task GenerateTaxCertificateAsync_ServerError_ReturnsNulls()
        {
            var sut = CreateSut(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError)));
            var (number, code) = await sut.GenerateTaxCertificateAsync(SampleDonation());
            Assert.Null(number);
            Assert.Null(code);
        }

        [Fact]
        public async Task GenerateTaxCertificateAsync_HostUnreachable_ReturnsNullsWithoutThrowing()
        {
            var sut = CreateSut(new ThrowingHandler());
            var (number, code) = await sut.GenerateTaxCertificateAsync(SampleDonation());
            Assert.Null(number);
            Assert.Null(code);
        }

        [Fact]
        public async Task LogProjectUpdateAsync_SuccessResponse_ReturnsTrue()
        {
            var sut = CreateSut(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)));
            Assert.True(await sut.LogProjectUpdateAsync(new ProjectUpdate { Title = "T" }, "Project"));
        }

        [Fact]
        public async Task LogProjectUpdateAsync_ServerError_ReturnsFalse()
        {
            var sut = CreateSut(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)));
            Assert.False(await sut.LogProjectUpdateAsync(new ProjectUpdate { Title = "T" }, "Project"));
        }

        [Fact]
        public async Task LogProjectUpdateAsync_HostUnreachable_ReturnsFalseWithoutThrowing()
        {
            var sut = CreateSut(new ThrowingHandler());
            Assert.False(await sut.LogProjectUpdateAsync(new ProjectUpdate { Title = "T" }, "Project"));
        }
    }
}
