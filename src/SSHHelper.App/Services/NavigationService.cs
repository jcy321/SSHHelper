using Microsoft.Extensions.DependencyInjection;
using SSHHelper.App.ViewModels;
using SSHHelper.App.Views;
using System.Windows;

namespace SSHHelper.App;

/// <summary>
/// 全局服务提供者
/// </summary>
public static class ServiceProvider
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

/// <summary>
/// 导航服务接口
/// </summary>
public interface INavigationService
{
    void NavigateToLicenseView();
    void NavigateToMainView();
    object CurrentView { get; }
}

/// <summary>
/// 导航服务实现
/// </summary>
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
        var viewModel = ServiceProvider.GetService<LicenseViewModel>();
        CurrentView = new LicenseView { DataContext = viewModel };
    }

    public void NavigateToMainView()
    {
        var viewModel = ServiceProvider.GetService<MainViewModel>();
        CurrentView = new MainView { DataContext = viewModel };
    }
}