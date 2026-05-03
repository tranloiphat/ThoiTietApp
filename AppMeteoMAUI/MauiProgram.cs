using System.Globalization;
using ThoiTietApp.Services;
using ThoiTietApp.ViewModels;
using ThoiTietApp.Views;

namespace ThoiTietApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        // Định dạng ngày/giờ/số theo tiếng Việt (thứ Hai, tháng 1...)
        CultureInfo.CurrentCulture = new CultureInfo("vi-VN");
        CultureInfo.CurrentUICulture = new CultureInfo("vi-VN");

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("times new roman.ttf", "TimesNewRoman");
                fonts.AddFont("FiraSans-Bold.otf", "FiraSansBold");
            });

        // Singleton: chỉ tạo 1 instance duy nhất trong suốt vòng đời app
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<WeatherService>();

        // Transient: tạo instance mới mỗi khi được yêu cầu (phù hợp với Page/ViewModel)
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<MainPage>();

        return builder.Build();
    }
}
