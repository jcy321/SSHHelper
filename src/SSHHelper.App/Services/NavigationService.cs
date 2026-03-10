using Microsoft.Extensions.DependencyInjection;
using SSHHelper.App.ViewModels;
using SSHHelper.App.Views;
using System.Windows;

namespace SSHHelper.App;

public static class AppServices
{
    public static IServiceProvider Instance { get; private set; } = null!;

    public static void Initialize(IServiceProvider serviceProvider)
    {
        Instance = serviceProvider;
    }

    public static T GetService<T>() where T : class
    {
        return Instance.GetRequiredService<T>();
    }

    public static T? GetServiceOrNull<T>() where T : class
    {
        return Instance.GetService<T>();
    }
}

public interface INavigationService
{
    void NavigateToLicenseView();
    void NavigateToMainView();
    object CurrentView { get; }
    event EventHandler? CurrentViewChanged;
}

public class NavigationService : INavigationService
{
    private object _currentView = null!;
    
    public object CurrentView
    {
        get => _currentView;
        private set
        {
            _currentView = value;
            CurrentViewChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? CurrentViewChanged;

    public void NavigateToLicenseView()
    {
        var viewModel = AppServices.GetService<LicenseViewModel>();
        CurrentView = new LicenseView { DataContext = viewModel };
    }

    public void NavigateToMainView()
    {
        var viewModel = AppServices.GetService<MainViewModel>();
        CurrentView = new MainView { DataContext = viewModel };
    }
}