using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SSHHelper.App.ViewModels;
using SSHHelper.Auth;
using SSHHelper.Auth.Interfaces;
using SSHHelper.Core.Services;
using SSHHelper.Core.Services.Interfaces;
using System.Windows;

namespace SSHHelper.App;

public partial class App : Application
{
    private IServiceProvider _serviceContainer = null!;
    private INavigationService _navigationService = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceContainer = services.BuildServiceProvider();
            
            AppServices.Initialize(_serviceContainer);
            
            _navigationService = _serviceContainer.GetRequiredService<INavigationService>();
            
            CheckLicenseAndStart();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"程序启动失败: {ex.Message}\n\n详细信息: {ex.StackTrace}", 
                "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    private void ConfigureServices(ServiceCollection services)
    {
        services.AddLogging(builder => builder.AddDebug());

        services.AddSingleton<IKeyPairService, KeyPairService>();
        services.AddSingleton<IConfigManager, ConfigManager>();
        services.AddSingleton<IRemoteKeyDeployService, RemoteKeyDeployService>();
        services.AddSingleton<ISshSessionService, SshSessionService>();

        services.AddSingleton<IMachineIdGenerator, MachineIdGenerator>();
        services.AddSingleton<ISecureStorage, SecureStorage>();
        services.AddSingleton<ILicenseValidator, LicenseValidator>();

        services.AddSingleton<INavigationService, NavigationService>();

        services.AddTransient<LicenseViewModel>();
        services.AddTransient<MainViewModel>();
    }

    private async void CheckLicenseAndStart()
    {
        try
        {
            var licenseValidator = _serviceContainer.GetRequiredService<ILicenseValidator>();
            var license = await licenseValidator.ValidateOfflineAsync();
            
            if (license != null)
            {
                _navigationService.NavigateToMainView();
            }
            else
            {
                _navigationService.NavigateToLicenseView();
            }

            _navigationService.CurrentViewChanged += OnCurrentViewChanged;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"授权验证失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            _navigationService.NavigateToLicenseView();
        }
    }

    private void OnCurrentViewChanged(object? sender, EventArgs e)
    {
        var newWindow = _navigationService.CurrentView as Window;
        var oldWindow = Current.MainWindow;
        
        Current.MainWindow = newWindow;
        
        // 关闭旧窗口
        oldWindow?.Close();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        (_serviceContainer as IDisposable)?.Dispose();
        base.OnExit(e);
    }
}