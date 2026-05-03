#nullable disable
using System.Text.Json.Serialization;

namespace ThoiTietApp.Models
{
    // Kết quả geocoding từ Open-Meteo: 1 thành phố tìm được
    public class GeoResult
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("latitude")]
        public double? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double? Longitude { get; set; }

        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("admin1")]
        public string Admin1 { get; set; }
    }

    // Danh sách kết quả trả về từ geocoding API
    public class GeoCodingResponse
    {
        [JsonPropertyName("results")]
        public List<GeoResult> Results { get; set; }
    }
}
