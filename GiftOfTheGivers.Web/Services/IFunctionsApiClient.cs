using GiftOfTheGivers.Web.Models;

namespace GiftOfTheGivers.Web.Services
{
    // Thin wrapper around the two Azure Functions added in Part 2, Section A.
    // Both calls are best-effort: if the Functions host isn't running (e.g.
    // during a quick demo of the Web project alone), the site should keep
    // working rather than fail the donation/update the caller was doing.
    public interface IFunctionsApiClient
    {
        Task<(string? CertificateNumber, string? ValidationCode)> GenerateTaxCertificateAsync(Donation donation);

        Task<bool> LogProjectUpdateAsync(ProjectUpdate update, string projectName);
    }
}
