namespace GloboClima.API.Models
{
    public class CountryInfo
    {
        public string Name { get; set; }
        public string Region { get; set; }
        public long Population { get; set; }
        public List<string> Languages { get; set; }
        public List<string> Currencies { get; set; }
        public string FlagUrl { get; set; }
    }
}
