using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ThoiTietApp.Models;
using ThoiTietApp.Services;

namespace ThoiTietApp.ViewModels;

// ViewModel: chứa dữ liệu và logic, kết nối Model với View qua data binding
// Kế thừa ObservableObject để tự động thông báo cho UI khi property thay đổi
public partial class MainViewModel : ObservableObject
{
    private readonly DatabaseService _db;
    private readonly WeatherService _weather;

    // [ObservableProperty]: source generator tự tạo public property + gọi OnPropertyChanged()
    // Khi ViewModel gán lại giá trị → UI binding nhận được thông báo → tự render lại
    [ObservableProperty] private string searchText = string.Empty;
    [ObservableProperty] private string cityName = string.Empty;
    [ObservableProperty] private string temperature = "--";
    [ObservableProperty] private string description = string.Empty;
    [ObservableProperty] private string humidity = "--";
    [ObservableProperty] private string windSpeed = "--";
    [ObservableProperty] private string feelsLike = "--";
    [ObservableProperty] private string weatherIcon = "cloudy.svg";
    [ObservableProperty] private bool isLoading;

    // ObservableCollection: khác List<T> ở chỗ tự bắn sự kiện CollectionChanged
    // CollectionView trên XAML lắng nghe sự kiện này → tự cập nhật khi Add/Clear
    public ObservableCollection<DailyForecast> DailyForecasts { get; } = new();
    public ObservableCollection<FavoriteCity> Favorites { get; } = new();
    public ObservableCollection<SearchHistory> RecentSearches { get; } = new();

    // Constructor nhận service qua Dependency Injection — MauiProgram.cs đã đăng ký
    public MainViewModel(DatabaseService db, WeatherService weather)
    {
        _db = db;
        _weather = weather;
        // _ = : "fire and forget" — constructor không thể async nên dùng cách này
        // Lỗi nếu có sẽ được bắt bên trong InitAsync()
        _ = InitAsync();
    }

    // Async: không làm đơ UI khi đang chờ DB hoặc HTTP — chạy trên background thread
    private async Task InitAsync()
    {
        await LoadFavoritesAsync();
        await LoadRecentSearchesAsync();
        await SearchCityAsync("Ho Chi Minh City");
    }

    // [RelayCommand]: source generator tự tạo SearchCommand (ICommand) để bind trong XAML
    // XAML dùng: Command="{Binding SearchCommand}"
    [RelayCommand]
    private async Task Search()
    {
        if (string.IsNullOrWhiteSpace(SearchText)) return;
        await SearchCityAsync(SearchText.Trim());
        SearchText = string.Empty;
    }

    // Lưu thành phố đang xem vào SQLite, sau đó reload danh sách Favorites
    [RelayCommand]
    private async Task AddFavorite()
    {
        if (string.IsNullOrWhiteSpace(CityName)) return;
        await _db.AddFavoriteAsync(CityName);
        await LoadFavoritesAsync();
    }

    // Nhận FavoriteCity làm tham số — XAML truyền qua CommandParameter="{Binding .}"
    [RelayCommand]
    private async Task RemoveFavorite(FavoriteCity city)
    {
        await _db.RemoveFavoriteAsync(city.CityName);
        await LoadFavoritesAsync();
    }

    [RelayCommand]
    private async Task LoadFavoriteCity(FavoriteCity city) => await SearchCityAsync(city.CityName);

    [RelayCommand]
    private async Task LoadFromHistory(SearchHistory item) => await SearchCityAsync(item.CityName);

    // Phương thức trung tâm: gọi API → cập nhật properties → UI tự render lại qua binding
    private async Task SearchCityAsync(string city)
    {
        IsLoading = true;  // binding IsVisible của ActivityIndicator → spinner hiện
        try
        {
            var coords = await _weather.GetCoordinatesAsync(city);
            if (coords == null)
            {
                await Shell.Current.DisplayAlertAsync("Không tìm thấy", $"'{city}' không có trong dữ liệu.", "Đóng");
                return;
            }
            var data = await _weather.GetWeatherAsync(coords.Value.Lat, coords.Value.Lon);
            if (data?.Current == null) return;

            // Gán property → OnPropertyChanged() → XAML Label/Image tự cập nhật
            CityName = coords.Value.Name;
            Temperature = $"{data.Current.Temperature:F0}°";
            Description = WeatherService.GetDescription(data.Current.WeatherCode);
            Humidity = $"{data.Current.Humidity}%";
            WindSpeed = $"{data.Current.WindSpeed:F0} km/h";
            FeelsLike = $"{data.Current.ApparentTemperature:F0}°";
            WeatherIcon = WeatherService.GetIcon(data.Current.WeatherCode);

            // Clear + Add vào ObservableCollection → CollectionView tự render lại
            DailyForecasts.Clear();
            if (data.Daily != null)
                foreach (var f in WeatherService.ParseDailyForecasts(data.Daily))
                    DailyForecasts.Add(f);

            // Lưu vào SQLite sau khi tìm thành công
            await _db.SaveSearchAsync(CityName);
            await LoadRecentSearchesAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Lỗi kết nối", ex.Message, "Đóng");
        }
        finally { IsLoading = false; }  // finally đảm bảo spinner luôn tắt dù thành công hay lỗi
    }

    private async Task LoadFavoritesAsync()
    {
        var list = await _db.GetFavoritesAsync();
        Favorites.Clear();
        foreach (var item in list) Favorites.Add(item);
    }

    private async Task LoadRecentSearchesAsync()
    {
        var list = await _db.GetRecentSearchesAsync(5);
        RecentSearches.Clear();
        foreach (var item in list) RecentSearches.Add(item);
    }
}
