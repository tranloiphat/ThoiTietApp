#nullable disable
using System.Text.Json.Serialization;

namespace ThoiTietApp.Models
{
    // Phản hồi JSON từ Open-Meteo API — ánh xạ 1-1 với cấu trúc JSON
    public class WeatherApiResponse
    {
        [JsonPropertyName("current")]
        public CurrentConditions Current { get; set; }

        [JsonPropertyName("daily")]
        public DailyConditions Daily { get; set; }
    }

    // Thời tiết hiện tại — từ block "current" trong JSON
    public class CurrentConditions
    {
        [JsonPropertyName("temperature_2m")]
        public double Temperature { get; set; }

        [JsonPropertyName("relative_humidity_2m")]
        public int Humidity { get; set; }

        [JsonPropertyName("apparent_temperature")]
        public double ApparentTemperature { get; set; }

        [JsonPropertyName("wind_speed_10m")]
        public double WindSpeed { get; set; }

        [JsonPropertyName("weather_code")]
        public int WeatherCode { get; set; }
    }

    // Dự báo hàng ngày — từ block "daily" trong JSON (mỗi field là 1 mảng 7 phần tử)
    public class DailyConditions
    {
        [JsonPropertyName("time")]
        public List<string> Time { get; set; }

        [JsonPropertyName("weather_code")]
        public List<int> WeatherCode { get; set; }

        [JsonPropertyName("temperature_2m_max")]
        public List<double> TemperatureMax { get; set; }

        [JsonPropertyName("temperature_2m_min")]
        public List<double> TemperatureMin { get; set; }
    }

    // Model hiển thị UI — được ViewModel tổng hợp từ DailyConditions
    public class DailyForecast
    {
        public string DayName { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public double TempMax { get; set; }
        public double TempMin { get; set; }
    }
}
