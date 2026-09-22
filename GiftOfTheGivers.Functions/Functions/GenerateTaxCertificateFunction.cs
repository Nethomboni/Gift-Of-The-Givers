using System.Net;
using System.Text.Json;
using GiftOfTheGivers.Functions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace GiftOfTheGivers.Functions.Functions
{
    // A.1/A.2 - HTTP-triggered function. The web app calls this the moment a
    // donor's donation form is successfully submitted, and it hands back a
    // dummy tax certificate (no real SARS/tax-authority integration at
    // prototype stage - see the Web project's own "dummy storage" approach
    // in Part 1).
    public class GenerateTaxCertificateFunction
    {
        private readonly ILogger _logger;

        public GenerateTaxCertificateFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GenerateTaxCertificateFunction>();
        }

        [Function("GenerateTaxCertificate")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "certificates/generate")]
            HttpRequestData req)
        {
            _logger.LogInformation("GenerateTaxCertificate triggered.");

            TaxCertificateRequest? donation;
            try
            {
                donation = await JsonSerializer.DeserializeAsync<TaxCertificateRequest>(
                    req.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Request body must be valid JSON.");
                return badRequest;
            }

            if (donation is null || string.IsNullOrWhiteSpace(donation.ReferenceNumber))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("A donation ReferenceNumber is required.");
                return badRequest;
            }

            // Dummy certificate number/validation code - deterministic from the
            // donation reference so the same donation always yields the same
            // certificate number, plus a short random validation code.
            var certificateNumber = $"TC-{donation.ReferenceNumber}";
            var validationCode = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

            var certificate = new TaxCertificateResponse
            {
                CertificateNumber = certificateNumber,
                ValidationCode = validationCode,
                IssuedAt = DateTime.UtcNow,
                DonorName = donation.DonorName,
                Amount = donation.Amount,
                Currency = donation.Currency,
                Cause = donation.Cause
            };

            _logger.LogInformation(
                "Issued dummy certificate {CertificateNumber} for donation {ReferenceNumber}.",
                certificateNumber, donation.ReferenceNumber);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(certificate);
            return response;
        }
    }
}
