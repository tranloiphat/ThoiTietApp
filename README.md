# 🌤️ Dự Báo Thời Tiết

> Ứng dụng dự báo thời tiết theo thời gian thực, viết bằng **.NET MAUI** trên nền tảng **Android**.  
> Dữ liệu lấy từ **Open-Meteo API** (miễn phí, không cần key). Lịch sử tìm kiếm và thành phố yêu thích lưu cục bộ bằng **SQLite**.

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)
![MAUI](https://img.shields.io/badge/MAUI-10.0.1-blue)
![Platform](https://img.shields.io/badge/Platform-Android-3DDC84?logo=android)
![License](https://img.shields.io/badge/License-MIT-green)


---

## 📋 Mục lục

1. [Giới thiệu](#1-giới-thiệu)
2. [Tính năng chính](#2-tính-năng-chính)
3. [Công nghệ sử dụng](#3-công-nghệ-sử-dụng)
4. [Kiến trúc MVVM](#4-kiến-trúc-mvvm)
5. [Cấu trúc thư mục](#5-cấu-trúc-thư-mục)
6. [Giải thích từng file](#6-giải-thích-từng-file)
7. [Luồng gọi API OpenMeteo](#7-luồng-gọi-api-openmeteo)
8. [Luồng SQLite](#8-luồng-sqlite)
9. [Hướng dẫn cài đặt và chạy](#9-hướng-dẫn-cài-đặt-và-chạy)
10. [Tài liệu tham khảo](#10-tài-liệu-tham-khảo)

---

## 1. Giới thiệu

**Dự Báo Thời Tiết** là ứng dụng Android cho phép người dùng tra cứu thời tiết bất kỳ thành phố nào trên thế giới chỉ bằng một lần gõ tên. Ứng dụng hiển thị nhiệt độ hiện tại, độ ẩm, tốc độ gió, cảm giác nhiệt, cùng dự báo 7 ngày tới có biểu tượng thời tiết trực quan.

**Đối tượng người dùng:** Người dùng phổ thông muốn kiểm tra thời tiết nhanh trên điện thoại Android, không cần tài khoản hay đăng nhập.

**Vấn đề app giải quyết:**
- Tra cứu thời tiết nhanh không cần mở trình duyệt.
- Lưu lại các thành phố hay tra cứu để không phải gõ lại.
- Đánh dấu thành phố yêu thích để truy cập một chạm.



---

## 2. Tính năng chính

- ✅ **Tìm kiếm thời tiết** theo tên thành phố (hỗ trợ tiếng Việt không dấu, tên quốc tế)
- ✅ **Thời tiết hiện tại**: nhiệt độ, mô tả, độ ẩm, tốc độ gió, cảm giác nhiệt, icon SVG
- ✅ **Dự báo 7 ngày** hiển thị dạng scroll ngang với nhiệt độ cao/thấp và icon
- ✅ **Thành phố yêu thích**: lưu vào SQLite, tắt app mở lại vẫn còn
- ✅ **Lịch sử tìm kiếm**: 5 lần gần nhất, kèm thời gian, nhấn để load lại
- ✅ **Dark / Light theme** tự động theo cài đặt hệ điều hành
- ✅ **Giao diện tiếng Việt** hoàn toàn (tên ngày, mô tả thời tiết, thông báo lỗi)
- ✅ **Loading indicator** che toàn màn hình khi đang gọi API

---

## 3. Công nghệ sử dụng

| Công nghệ | Version | Vai trò | Dùng ở đâu trong code |
|-----------|---------|---------|------------------------|
| **.NET MAUI** | 10.0.1 | Framework cross-platform UI | Toàn bộ project, `AppMeteoMAUI.csproj` |
| **C# 13** | built-in | Ngôn ngữ lập trình chính | Tất cả file `.cs` |
| **XAML** | — | Định nghĩa giao diện khai báo | `Views/MainPage.xaml`, `App.xaml`, `AppShell.xaml` |
| **CommunityToolkit.Mvvm** | 8.4.0 | Source generator cho MVVM (`[ObservableProperty]`, `[RelayCommand]`) | `ViewModels/MainViewModel.cs` |
| **sqlite-net-pcl** | 1.9.172 | ORM nhẹ cho SQLite trên mobile | `Services/DatabaseService.cs`, `Models/SearchHistory.cs`, `Models/FavoriteCity.cs` |
| **SQLitePCLRaw.bundle_green** | 2.1.8 | Runtime SQLite native cho Android (bắt buộc) | Dependency gián tiếp của sqlite-net-pcl |
| **HttpClient** | built-in .NET | Gọi REST API qua HTTP/HTTPS | `Services/WeatherService.cs` — field tĩnh `_http` |
| **System.Text.Json** | built-in .NET | Deserialize JSON response từ API | `Services/WeatherService.cs` — method `GetFromJsonAsync<T>()` |
| **Open-Meteo API** | — | Nguồn dữ liệu thời tiết miễn phí, không cần key | `Services/WeatherService.cs` — `GetCoordinatesAsync()`, `GetWeatherAsync()` |
| **SQLite** | on-device | Lưu trữ cục bộ không cần server | `Services/DatabaseService.cs` — file `weather.db` |

---

## 4. Kiến trúc MVVM

### 4.1. MVVM là gì?

**MVVM** (Model – View – ViewModel) là design pattern tách biệt 3 tầng:

| Tầng | Trách nhiệm | Ví dụ trong app |
|------|------------|-----------------|
| **Model** | Cấu trúc dữ liệu thuần, không có logic UI | `WeatherData.cs`, `SearchHistory.cs`, `FavoriteCity.cs` |
| **View** | Hiển thị UI, không xử lý logic | `MainPage.xaml` |
| **ViewModel** | Logic nghiệp vụ, cầu nối Model ↔ View qua Binding | `MainViewModel.cs` |

```
┌──────────────────────────────────────────────────────────────────┐
│                            VIEW                                  │
│                    MainPage.xaml (XAML)                          │
│   Text="{Binding CityName}"   Command="{Binding SearchCommand}"  │
└──────────────────────┬───────────────────────────────────────────┘
                       │   Data Binding (2 chiều, tự động)
                       ▼
┌──────────────────────────────────────────────────────────────────┐
│                         VIEWMODEL                                │
│                   MainViewModel.cs (C#)                          │
│  [ObservableProperty] CityName    [RelayCommand] Search()        │
│  ObservableCollection<DailyForecast> DailyForecasts              │
└──────────┬───────────────────────────────────┬───────────────────┘
           │                                   │
           ▼                                   ▼
┌──────────────────────┐          ┌────────────────────────────┐
│    WeatherService    │          │      DatabaseService        │
│  GetCoordinatesAsync │          │  SaveSearchAsync()          │
│  GetWeatherAsync()   │          │  AddFavoriteAsync()         │
│  GetDescription()    │          │  GetFavoritesAsync()        │
└──────────┬───────────┘          └────────────────────────────┘
           │
           ▼
┌──────────────────────────────────────────────────────────────────┐
│                            MODEL                                 │
│  WeatherApiResponse  CurrentConditions  DailyConditions          │
│  DailyForecast       GeoResult          GeoCodingResponse        │
│  SearchHistory       FavoriteCity                                │
└──────────────────────────────────────────────────────────────────┘
```

**Quy tắc cốt lõi của MVVM:**
- View **không biết** Model tồn tại — chỉ nói chuyện với ViewModel.
- ViewModel **không biết** View trông như thế nào — chỉ cung cấp data và commands.
- Kết nối duy nhất giữa View và ViewModel là **Data Binding**.

### 4.2. MVVM trong app này

```
MainPage.xaml  ←→  MainViewModel.cs  ←→  WeatherApiResponse
                                     ←→  DailyForecast
                                     ←→  SearchHistory
                                     ←→  FavoriteCity
                        ↓
               WeatherService.cs  →  Open-Meteo API
               DatabaseService.cs →  SQLite (weather.db)
```

### 4.3. Data Binding hoạt động ra sao?

Khi `MainViewModel` thay đổi một property, XAML tự cập nhật — không cần gọi `Refresh()` hay thao tác UI thủ công.

**Ví dụ thực tế:**

```csharp
// ViewModels/MainViewModel.cs — dòng 19
[ObservableProperty]
private string cityName = string.Empty;
```

`[ObservableProperty]` là attribute của **CommunityToolkit.Mvvm**. Tại compile time, source generator tự sinh ra:

```csharp
// Code được TỰ ĐỘNG SINH — không cần viết tay
public string CityName
{
    get => cityName;
    set
    {
        if (cityName == value) return;
        cityName = value;
        OnPropertyChanged(nameof(CityName)); // thông báo cho UI
    }
}
```

Trong XAML:
```xml
<!-- Views/MainPage.xaml — dòng 39 -->
<Label Text="{Binding CityName}"
       Style="{StaticResource LargeLabel}"
       HorizontalOptions="Center"
       FontAttributes="Bold"/>
```

**Luồng hoạt động:**
1. `MainViewModel` gọi `CityName = "Hà Nội"`
2. Setter gọi `OnPropertyChanged("CityName")`
3. XAML binding nhận thông báo → tự cập nhật `Label.Text` thành `"Hà Nội"`
4. Người dùng thấy tên thành phố mới trên màn hình — **hoàn toàn tự động**

### 4.4. Command Binding

`[RelayCommand]` sinh ra `ICommand` để XAML có thể bind:

```csharp
// ViewModels/MainViewModel.cs — dòng 52-57
[RelayCommand]
private async Task Search()
{
    if (string.IsNullOrWhiteSpace(SearchText)) return;
    await SearchCityAsync(SearchText.Trim());
    SearchText = string.Empty;
}
```

Source generator tự tạo property `SearchCommand` kiểu `AsyncRelayCommand`. XAML dùng:

```xml
<!-- Views/MainPage.xaml — dòng 21, 25 -->
<Entry ReturnCommand="{Binding SearchCommand}" ... />
<Button Command="{Binding SearchCommand}" Text="Tìm" ... />
```

Cả hai control đều bind vào cùng một command — không cần code-behind.

### 4.5. ObservableCollection và CollectionView

Với danh sách, `ObservableCollection<T>` thay cho `List<T>`:

```csharp
// ViewModels/MainViewModel.cs — dòng 30-32
public ObservableCollection<DailyForecast> DailyForecasts { get; } = new();
public ObservableCollection<FavoriteCity> Favorites { get; } = new();
public ObservableCollection<SearchHistory> RecentSearches { get; } = new();
```

Khi gọi `.Add()` hay `.Clear()`, `ObservableCollection` tự bắn sự kiện `CollectionChanged`. `CollectionView` trong XAML lắng nghe và tự render lại — không cần gọi `Refresh()`.

### 4.6. Dependency Injection

`MauiProgram.cs` đăng ký các service vào DI container:

```csharp
// MauiProgram.cs — dòng 27-32
builder.Services.AddSingleton<DatabaseService>();  // 1 instance duy nhất
builder.Services.AddSingleton<WeatherService>();   // 1 instance duy nhất

builder.Services.AddTransient<MainViewModel>();    // tạo mới mỗi lần
builder.Services.AddTransient<MainPage>();         // tạo mới mỗi lần
```

MAUI tự inject vào constructor — không cần `new`:

```csharp
// ViewModels/MainViewModel.cs — dòng 35-41
public MainViewModel(DatabaseService db, WeatherService weather)
{
    _db = db;
    _weather = weather;
    _ = InitAsync(); // fire-and-forget: constructor không thể async
}
```

---

## 5. Cấu trúc thư mục

```
AppMeteoMAUI/
│
├── 📄 App.xaml / App.xaml.cs          ← Entry point app, định nghĩa màu sắc & style
├── 📄 AppShell.xaml / AppShell.xaml.cs ← Cấu hình điều hướng (Shell navigation)
├── 📄 MauiProgram.cs                   ← Khởi động app, đăng ký DI container
├── 📄 GlobalUsing.cs                   ← Global using cho toàn project
├── 📄 AppMeteoMAUI.csproj              ← Cấu hình project, NuGet packages
│
├── 📁 Models/
│   ├── WeatherData.cs                  ← Schema JSON từ Open-Meteo API
│   ├── GeoCoding.cs                    ← Schema JSON geocoding (tên → tọa độ)
│   ├── SearchHistory.cs                ← Model bảng SQLite lịch sử tìm kiếm
│   └── FavoriteCity.cs                 ← Model bảng SQLite thành phố yêu thích
│
├── 📁 Services/
│   ├── WeatherService.cs               ← Gọi HTTP API, parse JSON, map WMO codes
│   └── DatabaseService.cs              ← CRUD SQLite: lịch sử + yêu thích
│
├── 📁 ViewModels/
│   └── MainViewModel.cs                ← Logic chính, properties binding, commands
│
├── 📁 Views/
│   ├── MainPage.xaml                   ← Giao diện duy nhất (5 section)
│   └── MainPage.xaml.cs                ← Code-behind (chỉ gán BindingContext)
│
├── 📁 Resources/
│   ├── AppIcon/appicon.svg             ← Icon app
│   ├── Splash/splash_screen.svg        ← Màn hình splash
│   ├── Fonts/ (4 font)                 ← OpenSans, Times New Roman, FiraSans
│   ├── Images/ (8 SVG icon)            ← Icon thời tiết (clear_day, cloudy, rain...)
│   └── Styles/
│       ├── Colors.xaml                 ← Bảng màu hệ thống MAUI
│       └── Styles.xaml                 ← Style mặc định cho các control MAUI
│
└── 📁 Platforms/Android/
    ├── MainActivity.cs                 ← Activity Android chính
    ├── MainApplication.cs              ← Khởi tạo MauiApp
    └── AndroidManifest.xml             ← Khai báo quyền INTERNET
```

---

## 6. Giải thích từng file

### `Models/WeatherData.cs`

- **Nhiệm vụ**: Ánh xạ cấu trúc JSON phản hồi từ Open-Meteo sang object C#.
- **Classes**:
  - `WeatherApiResponse`: root object, chứa `Current` và `Daily`
  - `CurrentConditions`: thời tiết hiện tại — `Temperature`, `Humidity`, `ApparentTemperature`, `WindSpeed`, `WeatherCode`
  - `DailyConditions`: mảng 7 ngày — `Time[]`, `WeatherCode[]`, `TemperatureMax[]`, `TemperatureMin[]`
  - `DailyForecast`: model UI được ViewModel tổng hợp từ `DailyConditions` — `DayName`, `Icon`, `TempMax`, `TempMin`
- **Kỹ thuật**: `[JsonPropertyName("...")]` map tên field JSON snake_case sang property C# PascalCase; `#nullable disable` tắt cảnh báo null cho model JSON.
- **Tương tác**: Được `WeatherService` tạo ra, được `MainViewModel` đọc và hiển thị.

### `Models/GeoCoding.cs`

- **Nhiệm vụ**: Ánh xạ response JSON của Geocoding API.
- **Classes**:
  - `GeoResult`: 1 kết quả thành phố — `Name`, `Latitude`, `Longitude`, `Country`, `CountryCode`, `Admin1`
  - `GeoCodingResponse`: wrapper có `Results` (danh sách `GeoResult`)
- **Tương tác**: Được `WeatherService.GetCoordinatesAsync()` deserialize và trả về tuple `(Lat, Lon, Name)`.

### `Models/SearchHistory.cs`

- **Nhiệm vụ**: Định nghĩa schema bảng `SearchHistory` trong SQLite.
- **Attributes**: `[Table("SearchHistory")]` đặt tên bảng; `[PrimaryKey, AutoIncrement]` cho cột `Id`.
- **Fields**: `Id` (int, PK), `CityName` (string), `SearchTime` (DateTime)
- **Tương tác**: `DatabaseService` đọc/ghi; `MainViewModel` hiển thị trong `RecentSearches`.

### `Models/FavoriteCity.cs`

- **Nhiệm vụ**: Định nghĩa schema bảng `FavoriteCity` trong SQLite.
- **Fields**: `Id` (int, PK), `CityName` (string), `AddedTime` (DateTime)
- **Tương tác**: `DatabaseService` đọc/ghi; `MainViewModel` hiển thị trong `Favorites`.

### `Services/WeatherService.cs`

- **Nhiệm vụ**: Toàn bộ logic gọi HTTP đến Open-Meteo API.
- **Methods**:
  - `GetCoordinatesAsync(string cityName)` — gọi Geocoding API, trả về `(double Lat, double Lon, string Name)?`
  - `GetWeatherAsync(double lat, double lon)` — gọi Forecast API, trả về `WeatherApiResponse?`
  - `GetDescription(int code)` — static, map mã WMO → mô tả tiếng Việt
  - `GetIcon(int code)` — static, map mã WMO → tên file SVG
  - `ParseDailyForecasts(DailyConditions daily)` — static, ghép 4 mảng daily thành `List<DailyForecast>`
- **Kỹ thuật quan trọng**:
  - `HttpClient` là `static readonly` — tránh "socket exhaustion" khi tạo nhiều instance
  - `lat.ToString(CultureInfo.InvariantCulture)` — ép dấu thập phân là `.` (tránh lỗi khi locale máy dùng `,`)
  - `HttpUtility.UrlEncode(cityName)` — encode tên thành phố cho URL an toàn
- **Tương tác**: Register Singleton trong `MauiProgram.cs`, inject vào `MainViewModel`.

### `Services/DatabaseService.cs`

- **Nhiệm vụ**: Toàn bộ thao tác đọc/ghi SQLite cho 2 bảng `SearchHistory` và `FavoriteCity`.
- **Methods**:
  - `InitAsync()` — lazy-init: mở kết nối và tạo bảng lần đầu, bỏ qua nếu đã init
  - `SaveSearchAsync(string cityName)` — upsert: cập nhật `SearchTime` nếu đã có, insert mới nếu chưa
  - `GetRecentSearchesAsync(int limit = 10)` — lấy lịch sử, sort giảm dần theo thời gian
  - `AddFavoriteAsync(string cityName)` — insert nếu chưa tồn tại, bỏ qua nếu đã có
  - `GetFavoritesAsync()` — lấy tất cả yêu thích, sort giảm dần theo ngày thêm
  - `RemoveFavoriteAsync(string cityName)` — xóa theo tên thành phố
- **Tương tác**: Register Singleton trong `MauiProgram.cs`, inject vào `MainViewModel`.

### `ViewModels/MainViewModel.cs`

- **Nhiệm vụ**: Toàn bộ logic nghiệp vụ của app — cầu nối giữa Services và View.
- **Observable Properties** (binding đến UI):
  - `SearchText`, `CityName`, `Temperature`, `Description`, `Humidity`, `WindSpeed`, `FeelsLike`, `WeatherIcon`, `IsLoading`
- **Observable Collections**:
  - `DailyForecasts`, `Favorites`, `RecentSearches`
- **Commands**:
  - `SearchCommand` — tìm kiếm theo `SearchText`
  - `AddFavoriteCommand` — lưu `CityName` vào yêu thích
  - `RemoveFavoriteCommand(FavoriteCity)` — xóa yêu thích theo object
  - `LoadFavoriteCityCommand(FavoriteCity)` — load thời tiết thành phố yêu thích
  - `LoadFromHistoryCommand(SearchHistory)` — load thời tiết từ lịch sử
- **Method chính**: `SearchCityAsync(string city)` — orchestrate toàn bộ flow: geocoding → weather API → update properties → lưu DB.
- **Khởi động**: Constructor gọi `_ = InitAsync()` (fire-and-forget) → load favorites, history, auto-search "Ho Chi Minh City".
- **Tương tác**: Nhận `DatabaseService` và `WeatherService` qua DI; được `MainPage` set làm `BindingContext`.

### `Views/MainPage.xaml`

- **Nhiệm vụ**: Giao diện duy nhất của app, 5 section dọc trong `ScrollView`.
- **Sections**:
  1. **Thanh tìm kiếm**: `Entry` bind `SearchText` + nút "Tìm" bind `SearchCommand`
  2. **Thời tiết hiện tại**: tên thành phố, icon SVG, nhiệt độ 64pt, mô tả, 3 cột (gió/độ ẩm/cảm giác), nút yêu thích
  3. **Dự báo 7 ngày**: `CollectionView` nằm ngang, mỗi item là 1 card (ngày, icon, max, min)
  4. **Thành phố yêu thích**: `CollectionView` nằm ngang, tap để load, nút ✕ để xóa
  5. **Lịch sử tìm kiếm**: `CollectionView` dọc, tap để load, hiện thời gian
- **Kỹ thuật**:
  - `x:DataType="vm:MainViewModel"` — compiled binding, kiểm tra tên property lúc build
  - `RelativeSource AncestorType={x:Type vm:MainViewModel}` — truy cập ViewModel từ bên trong `DataTemplate`
  - `ActivityIndicator` đặt trong `Grid` chung để đè lên `ScrollView` khi `IsLoading = true`
  - `EmptyView` hiển thị text khi collection rỗng

### `Views/MainPage.xaml.cs`

- **Nhiệm vụ**: Code-behind tối giản — chỉ gán `BindingContext`.
- **Code toàn bộ**:
  ```csharp
  public MainPage(MainViewModel viewModel)
  {
      InitializeComponent();
      BindingContext = viewModel; // ViewModel inject qua DI
  }
  ```

### `MauiProgram.cs`

- **Nhiệm vụ**: Entry point thực sự của app — cấu hình framework, font, DI container, locale.
- **Điểm quan trọng**:
  - Set `CultureInfo("vi-VN")` để ngày tháng hiển thị tiếng Việt ("Thứ Hai", "tháng 1")
  - `AddSingleton` cho service (chia sẻ 1 instance) vs `AddTransient` cho Page/ViewModel (tạo mới mỗi lần)

### `App.xaml`

- **Nhiệm vụ**: Định nghĩa resource toàn cục — màu sắc và style được dùng trong `MainPage.xaml`.
- **Colors**: `Primary` (#f5f5f5 sáng / #333333 tối), `LightBackground`, `DarkBackground`, `LightLabel`, `DarkLabel`
- **Styles**: `BaseLabel`, `MicroLabel`, `SmallLabel`, `MediumLabel`, `LargeLabel`, `LabelTemperatura`, `ButtonImp`, `CardView`, `CardViewHorizontal`, `SearchBarStyle`
- **Dark/Light theme**: Dùng `AppThemeBinding` — màu tự đổi theo cài đặt hệ thống.

### `AppShell.xaml`

- **Nhiệm vụ**: Định nghĩa cấu trúc điều hướng Shell.
- **Cấu hình**: `FlyoutBehavior="Disabled"` (tắt menu kéo), 1 `ShellContent` duy nhất trỏ đến `MainPage`.

---

## 7. Luồng gọi API OpenMeteo

### 7.1. Giới thiệu Open-Meteo

[Open-Meteo](https://open-meteo.com/) là API thời tiết mã nguồn mở, **hoàn toàn miễn phí**, không cần đăng ký hay API key. App dùng 2 endpoint:

| Endpoint | URL | Mục đích |
|----------|-----|---------|
| **Geocoding** | `https://geocoding-api.open-meteo.com/v1/search` | Chuyển tên thành phố → tọa độ GPS |
| **Forecast** | `https://api.open-meteo.com/v1/forecast` | Lấy thời tiết theo tọa độ |

### 7.2. Code gọi API

**Bước 1 — Geocoding** (`WeatherService.cs`, method `GetCoordinatesAsync`):

```csharp
// Services/WeatherService.cs
public async Task<(double Lat, double Lon, string Name)?> GetCoordinatesAsync(string cityName)
{
    // Encode tên thành phố để dùng trong URL (VD: "Hồ Chí Minh" → "H%e1%bb%93+Ch%c3%ad+Minh")
    string encoded = HttpUtility.UrlEncode(cityName);

    // language=vi: API trả về tên thành phố bằng tiếng Việt
    string url = $"https://geocoding-api.open-meteo.com/v1/search?name={encoded}&count=1&language=vi";

    // GetFromJsonAsync<T>: gọi HTTP GET và tự deserialize JSON → C# object
    var response = await _http.GetFromJsonAsync<GeoCodingResponse>(url);

    var result = response?.Results?.FirstOrDefault();
    if (result == null) return null; // Không tìm thấy thành phố

    return (result.Latitude ?? 0, result.Longitude ?? 0, result.Name ?? cityName);
}
```

**Bước 2 — Forecast** (`WeatherService.cs`, method `GetWeatherAsync`):

```csharp
// Services/WeatherService.cs
public async Task<WeatherApiResponse?> GetWeatherAsync(double lat, double lon)
{
    // ToString(InvariantCulture): bắt buộc dùng dấu "." thay vì ","
    // Không dùng InvariantCulture: máy locale vi-VN sẽ tạo URL "?latitude=10,823" → API báo lỗi
    string latStr = lat.ToString(CultureInfo.InvariantCulture);
    string lonStr = lon.ToString(CultureInfo.InvariantCulture);

    string url = $"https://api.open-meteo.com/v1/forecast?latitude={latStr}&longitude={lonStr}" +
        "&current=temperature_2m,relative_humidity_2m,apparent_temperature,wind_speed_10m,weather_code" +
        "&daily=weather_code,temperature_2m_max,temperature_2m_min" +
        "&timezone=auto&forecast_days=7";

    return await _http.GetFromJsonAsync<WeatherApiResponse>(url);
}
```

**Giải thích tham số URL:**
- `current=temperature_2m,...` — yêu cầu các field thời tiết hiện tại
- `daily=weather_code,...` — yêu cầu dự báo theo ngày
- `timezone=auto` — API tự dùng múi giờ địa phương của tọa độ
- `forecast_days=7` — lấy 7 ngày

**Bước 3 — Map WMO code sang tiếng Việt và icon:**

```csharp
// WMO (World Meteorological Organization): chuẩn quốc tế mã hóa thời tiết
public static string GetDescription(int code) => code switch
{
    0         => "Trời quang",
    1 or 2    => "Ít mây",
    3         => "Nhiều mây",
    45 or 48  => "Sương mù",
    51 or 53 or 55 => "Mưa phùn",
    61 or 63 or 65 => "Mưa",
    80 or 81 or 82 => "Mưa rào",
    95        => "Dông",
    96 or 99  => "Dông + mưa đá",
    _         => "Không rõ"
};
```

### 7.3. Sequence diagram — Luồng tìm kiếm

```
Người dùng gõ "Hà Nội" → nhấn "Tìm"
        │
        ▼
SearchCommand.Execute()
        │
        ▼
MainViewModel.Search()
   └─ SearchCityAsync("Hà Nội")
        │
        ├─ IsLoading = true  ──────────────► ActivityIndicator hiện lên
        │
        ├─ WeatherService.GetCoordinatesAsync("Hà Nội")
        │   └─ HTTP GET geocoding-api.open-meteo.com
        │       └─ Response: { lat: 21.02, lon: 105.83, name: "Hà Nội" }
        │
        ├─ WeatherService.GetWeatherAsync(21.02, 105.83)
        │   └─ HTTP GET api.open-meteo.com
        │       └─ Response: JSON thời tiết 7 ngày
        │
        ├─ Gán properties:
        │   CityName = "Hà Nội"          ──► Label CityName tự cập nhật
        │   Temperature = "32°"          ──► Label Temperature tự cập nhật
        │   WeatherIcon = "clear_day.svg" ──► Image tự cập nhật
        │   ...
        │
        ├─ DailyForecasts.Clear() + Add(7 items)
        │   └──────────────────────────────► CollectionView tự render lại
        │
        ├─ DatabaseService.SaveSearchAsync("Hà Nội")
        │   └─ INSERT hoặc UPDATE bảng SearchHistory trong SQLite
        │
        ├─ LoadRecentSearchesAsync()
        │   └──────────────────────────────► RecentSearches collection cập nhật
        │
        └─ IsLoading = false ─────────────► ActivityIndicator ẩn
```

### 7.4. Lưu ý kỹ thuật khi gọi API

**Tại sao phải dùng `async/await`?**

HTTP request mất 0.5–3 giây. Nếu gọi đồng bộ (blocking), UI thread bị "đơ" — màn hình không phản hồi, người dùng không cuộn được. `async/await` cho phép HTTP chạy trên background thread, UI thread tiếp tục xử lý sự kiện người dùng.

**Permission INTERNET trên Android:**

```xml
<!-- Platforms/Android/AndroidManifest.xml -->
<uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
<uses-permission android:name="android.permission.INTERNET" />
```

Thiếu 2 dòng này → app crash khi gọi HTTP.

**Xử lý lỗi mạng:**

```csharp
// MainViewModel.cs — SearchCityAsync()
try
{
    var coords = await _weather.GetCoordinatesAsync(city);
    if (coords == null)
    {
        await Shell.Current.DisplayAlertAsync("Không tìm thấy",
            $"'{city}' không có trong dữ liệu.", "Đóng");
        return;
    }
    // ...
}
catch (Exception ex)
{
    await Shell.Current.DisplayAlertAsync("Lỗi kết nối", ex.Message, "Đóng");
}
finally
{
    IsLoading = false; // Luôn tắt spinner dù thành công hay lỗi
}
```

---

## 8. Luồng SQLite

### 8.1. File database ở đâu?

```csharp
// Services/DatabaseService.cs — InitAsync()
string dbPath = Path.Combine(FileSystem.AppDataDirectory, "weather.db");
```

`FileSystem.AppDataDirectory` trên Android = `/data/data/com.demo.thoitietapp/files/` — thư mục riêng của app, không cần quyền truy cập bộ nhớ ngoài, tự xóa khi gỡ app.

### 8.2. Schema 2 bảng

**Bảng `SearchHistory`:**

```csharp
[Table("SearchHistory")]
public class SearchHistory
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string CityName { get; set; } = string.Empty; // tên thành phố

    public DateTime SearchTime { get; set; } // thời điểm tìm kiếm
}
```

**Bảng `FavoriteCity`:**

```csharp
[Table("FavoriteCity")]
public class FavoriteCity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string CityName { get; set; } = string.Empty; // tên thành phố

    public DateTime AddedTime { get; set; } // thời điểm thêm vào yêu thích
}
```

SQLite-net-pcl dùng reflection để đọc các attribute `[Table]`, `[PrimaryKey]`, `[AutoIncrement]` và tự tạo bảng trong `CreateTableAsync<T>()`.

### 8.3. Lazy-init pattern

`InitAsync()` được gọi mỗi khi dùng DB, nhưng chỉ thực sự mở kết nối 1 lần:

```csharp
// Services/DatabaseService.cs
public async Task InitAsync()
{
    if (_db != null) return; // Đã init rồi → bỏ qua
    string dbPath = Path.Combine(FileSystem.AppDataDirectory, "weather.db");
    _db = new SQLiteAsyncConnection(dbPath);
    await _db.CreateTableAsync<SearchHistory>(); // Tạo bảng nếu chưa có
    await _db.CreateTableAsync<FavoriteCity>();
}
```

Pattern này đảm bảo kết nối DB chỉ mở 1 lần, dù `DatabaseService` là Singleton.

### 8.4. Upsert lịch sử tìm kiếm

```csharp
// Services/DatabaseService.cs — SaveSearchAsync()
var existing = await _db!.Table<SearchHistory>()
    .Where(s => s.CityName == cityName)
    .FirstOrDefaultAsync();

if (existing != null)
{
    existing.SearchTime = DateTime.Now;  // Cập nhật thời gian nếu đã có
    await _db.UpdateAsync(existing);
}
else
{
    await _db.InsertAsync(new SearchHistory  // Thêm mới nếu chưa có
    {
        CityName = cityName,
        SearchTime = DateTime.Now
    });
}
```

Mỗi thành phố chỉ xuất hiện 1 lần trong lịch sử — tìm lại thì chỉ cập nhật thời gian, không nhân đôi.

### 8.5. Khi nào DB được gọi?

```
Khởi động app
    └─ InitAsync() tự động → LoadFavoritesAsync() + LoadRecentSearchesAsync()

Tìm kiếm thành phố thành công
    └─ SaveSearchAsync(cityName)
    └─ LoadRecentSearchesAsync()

Nhấn "⭐ Thêm vào yêu thích"
    └─ AddFavoriteAsync(CityName)
    └─ LoadFavoritesAsync()

Nhấn "✕" xóa yêu thích
    └─ RemoveFavoriteAsync(city.CityName)
    └─ LoadFavoritesAsync()
```

---

## 9. Hướng dẫn cài đặt và chạy

### 9.1. Yêu cầu hệ thống

| Yêu cầu | Tối thiểu | Khuyến nghị |
|---------|-----------|-------------|
| OS | Windows 10 (build 19041) | Windows 11 |
| RAM | 8 GB | 16 GB |
| Ổ cứng | 20 GB trống | 40 GB trống |
| CPU | Intel/AMD x64 | CPU hỗ trợ virtualization (cho emulator) |

### 9.2. Cài đặt công cụ

**Bước 1:** Tải và cài [Visual Studio 2022/2026 Community](https://visualstudio.microsoft.com/) (miễn phí).

**Bước 2:** Trong installer, chọn workload:
- ✅ **.NET Multi-platform App UI development** (MAUI)

Workload này tự cài .NET 10 SDK, Android SDK, và công cụ cần thiết.

**Bước 3 (Tùy chọn):** Để chạy trên emulator, mở **SDK Manager** trong VS → cài thêm Android Virtual Device.

### 9.3. Clone và build

```bash
# Clone về máy
git clone [URL_REPO]
cd WeatherForecastAppMAUI

# Restore NuGet packages
dotnet restore AppMeteoMAUI/AppMeteoMAUI.csproj

# Build kiểm tra
dotnet build AppMeteoMAUI/AppMeteoMAUI.csproj -f net10.0-android
```

### 9.4. Chạy app

**Cách 1 — Visual Studio (khuyến nghị):**
1. Mở file `ThoiTietApp.sln`
2. Chọn target: Android emulator hoặc thiết bị thật
3. Nhấn **F5** hoặc nút **Run ▶**

**Cách 2 — Command line:**
```bash
dotnet build AppMeteoMAUI/AppMeteoMAUI.csproj -t:Run -f net10.0-android
```

### 9.5. Chạy trên điện thoại Android thật (khuyến nghị cho demo)

1. **Bật Developer Options**: Vào **Cài đặt → Giới thiệu về điện thoại** → nhấn **Số bản dựng** 7 lần liên tiếp
2. **Bật USB Debugging**: Vào **Tuỳ chọn nhà phát triển** → bật **Gỡ lỗi USB**
3. **Kết nối USB**: Cắm cáp vào máy tính → điện thoại hỏi "Cho phép gỡ lỗi USB?" → nhấn **Đồng ý**
4. **Chọn thiết bị**: Trong Visual Studio, danh sách target tự xuất hiện tên điện thoại → chọn → F5

> **Yêu cầu**: Điện thoại Android 7.0 (API 24) trở lên.

---


## 10. Tài liệu tham khảo

| Tài liệu | Link |
|---------|------|
| .NET MAUI Docs | https://learn.microsoft.com/dotnet/maui/ |
| CommunityToolkit.Mvvm | https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/ |
| Open-Meteo API | https://open-meteo.com/en/docs |
| Open-Meteo Geocoding API | https://open-meteo.com/en/docs/geocoding-api |
| SQLite-net-pcl | https://github.com/praeclarum/sqlite-net |
| WMO Weather Codes | https://open-meteo.com/en/docs#weathervariables |
| MAUI Data Binding | https://learn.microsoft.com/dotnet/maui/fundamentals/data-binding/ |

---
