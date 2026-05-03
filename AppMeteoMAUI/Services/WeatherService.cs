using System.Globalization;
using System.Net.Http.Json;
using System.Web;
using ThoiTietApp.Models;

namespace ThoiTietApp.Services
{
    // WeatherService: chịu trách nhiệm gọi HTTP đến Open-Meteo API
    public class WeatherService
    {
        // static: dùng chung 1 instance HttpClient cho toàn app — tránh "socket exhaustion"
        // (mở quá nhiều kết nối TCP nếu new HttpClient() mỗi lần gọi API)
        private static readonly HttpClient _http = new();

        // Geocoding: chuyển tên thành phố → tọa độ (lat, lon) để gọi weather API
        public async Task<(double Lat, double Lon, string Name)?> GetCoordinatesAsync(string cityName)
        {
            string encoded = HttpUtility.UrlEncode(cityName);
            string url = $"https://geocoding-api.open-meteo.com/v1/search?name={encoded}&count=1&language=vi";
            var response = await _http.GetFromJsonAsync<GeoCodingResponse>(url);
            var result = response?.Results?.FirstOrDefault();
            if (result == null) return null;
            return (result.Latitude ?? 0, result.Longitude ?? 0, result.Name ?? cityName);
        }

        // Lấy thời tiết theo tọa độ
        // ToString(InvariantCulture): ép lat/lon dùng dấu "." (10.823) thay vì "," (10,823)
        // vì máy cài locale tiếng Việt sẽ format số thập phân với dấu phẩy → URL sai → API lỗi
        public async Task<WeatherApiResponse?> GetWeatherAsync(double lat, double lon)
        {
            string latStr = lat.ToString(CultureInfo.InvariantCulture);
            string lonStr = lon.ToString(CultureInfo.InvariantCulture);
            string url = $"https://api.open-meteo.com/v1/forecast?latitude={latStr}&longitude={lonStr}" +
                "&current=temperature_2m,relative_humidity_2m,apparent_temperature,wind_speed_10m,weather_code" +
                "&daily=weather_code,temperature_2m_max,temperature_2m_min" +
                "&timezone=auto&forecast_days=7";
            return await _http.GetFromJsonAsync<WeatherApiResponse>(url);
        }

        // Chuyển mã WMO (chuẩn quốc tế) sang mô tả tiếng Việt
        public static string GetDescription(int code) => code switch
        {
            0 => "Trời quang",
            1 or 2 => "Ít mây",
            3 => "Nhiều mây",
            45 or 48 => "Sương mù",
            51 or 53 or 55 => "Mưa phùn",
            61 or 63 or 65 => "Mưa",
            71 or 73 or 75 => "Tuyết",
            80 or 81 or 82 => "Mưa rào",
            95 => "Dông",
            96 or 99 => "Dông + mưa đá",
            _ => "Không rõ"
        };

        // Chuyển mã WMO sang tên file icon SVG trong Resources/Images/
        public static string GetIcon(int code) => code switch
        {
            0 => "clear_day.svg",
            1 or 2 => "partly_cloudy_day.svg",
            3 => "cloudy.svg",
            45 or 48 => "fog.svg",
            51 or 53 or 55 => "drizzle.svg",
            61 or 63 or 65 => "drizzle.svg",
            71 or 73 or 75 => "snow.svg",
            80 or 81 or 82 => "extreme_drizzle.svg",
            95 or 96 or 99 => "sleet.svg",
            _ => "cloudy.svg"
        };

        // Ghép 4 mảng daily API thành danh sách DailyForecast để CollectionView hiển thị
        public static List<DailyForecast> ParseDailyForecasts(DailyConditions daily)
        {
            var list = new List<DailyForecast>();
            for (int i = 0; i < daily.Time.Count; i++)
            {
                var date = DateTime.Parse(daily.Time[i]);
                list.Add(new DailyForecast
                {
                    DayName = i == 0 ? "Hôm nay" : date.ToString("ddd", CultureInfo.CurrentCulture),
                    Icon = GetIcon(daily.WeatherCode[i]),
                    Description = GetDescription(daily.WeatherCode[i]),
                    TempMax = daily.TemperatureMax[i],
                    TempMin = daily.TemperatureMin[i],
                });
            }
            return list;
        }
    }
}
