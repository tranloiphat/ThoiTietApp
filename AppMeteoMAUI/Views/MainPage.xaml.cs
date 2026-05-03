using ThoiTietApp.ViewModels;

namespace ThoiTietApp.Views;

public partial class MainPage : ContentPage
{
    // ViewModel được inject tự động qua Dependency Injection (đăng ký trong MauiProgram)
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
