namespace GiftOfTheGivers.Tests.UI.Support
{
    public static class TestSettings
    {
        public static string BaseUrl =>
            Environment.GetEnvironmentVariable("GOTG_BASE_URL")
            ?? "https://giftofthegiverssa-gubpd7bqbed6dvdn.southafricanorth-01.azurewebsites.net";

        public static string EmployeeEmail =>
            Environment.GetEnvironmentVariable("GOTG_EMPLOYEE_EMAIL") ?? string.Empty;

        public static string EmployeePassword =>
            Environment.GetEnvironmentVariable("GOTG_EMPLOYEE_PASSWORD") ?? string.Empty;
    }
}
