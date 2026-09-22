using System.Net.Http.Json;
using GiftOfTheGivers.Web.Models;
using Microsoft.Extensions.Logging;

namespace GiftOfTheGivers.Web.Services
{
    public class FunctionsApiClient : IFunctionsApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FunctionsApiClient> _logger;

        public FunctionsApiClient(HttpClient httpClient, ILogger<FunctionsApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<(string? CertificateNumber, string? ValidationCode)> GenerateTaxCertificateAsync(Donation donation)
        {
            try
            {
                var payload = new
                {
                    donation.ReferenceNumber,
                    DonorName = donation.DisplayName,
                    donation.Amount,
                    donation.Currency,
                    donation.Cause,
                    DonationDate = donation.CreatedAt
                };

                var response = await _httpClient.PostAsJsonAsync("certificates/generate", payload);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "GenerateTaxCertificate returned {StatusCode} for donation {ReferenceNumber}.",
                        response.StatusCode, donation.ReferenceNumber);
                    return (null, null);
                }

                var result = await response.Content.ReadFromJsonAsync<CertificateResult>();
                return (result?.CertificateNumber, result?.ValidationCode);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                // The Functions host may simply not be running - this is a
                // prototype integration, not a hard dependency of the donation
                // flow, so log and let the donation succeed regardless.
                _logger.LogWarning(ex, "Could not reach GenerateTaxCertificate function.");
                return (null, null);
            }
        }

        public async Task<bool> LogProjectUpdateAsync(ProjectUpdate update, string projectName)
        {
            try
            {
                var payload = new
                {
                    update.ProjectId,
                    ProjectName = projectName,
                    update.Title,
                    update.Description,
                    update.Location,
                    update.PostedByName,
                    update.CreatedAt
                };

                var response = await _httpClient.PostAsJsonAsync("projectupdates/log", payload);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                _logger.LogWarning(ex, "Could not reach LogProjectUpdate function.");
                return false;
            }
        }

        private class CertificateResult
        {
            public string? CertificateNumber { get; set; }
            public string? ValidationCode { get; set; }
        }
    }
}
