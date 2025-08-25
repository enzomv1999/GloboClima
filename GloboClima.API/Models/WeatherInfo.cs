namespace GloboClima.API.Models
{
    public class WeatherInfo
    {
        public string City { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Temperature { get; set; }
        public double FeelsLike { get; set; }
        public double TempMin { get; set; }
        public double TempMax { get; set; }
        public int Pressure { get; set; }
        public int Humidity { get; set; }
        public double WindSpeed { get; set; }
        public int WindDeg { get; set; }
        public int Visibility { get; set; }
        public DateTimeOffset TimeStamp { get; set; } = DateTimeOffset.UtcNow;
        public string Icon { get; set; } = string.Empty;
    }
}
