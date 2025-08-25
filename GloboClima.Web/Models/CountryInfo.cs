namespace GloboClima.Web.Models
{
    public class CountryInfo
    {
        public string Name { get; set; } = string.Empty;
        public string NativeName { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string Subregion { get; set; } = string.Empty;
        public string Capital { get; set; } = string.Empty;
        public int Population { get; set; }
        public double Area { get; set; }
        public List<string> Languages { get; set; } = new();
        public List<string> Currencies { get; set; } = new();
        public List<string> Timezones { get; set; } = new();
        public string Flag { get; set; } = string.Empty;
    }
}
