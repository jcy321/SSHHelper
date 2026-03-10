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
    private ServiceProvider _serviceProvider = null!;
    private INavigationService _navigationService = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
        
        ServiceProvider.Initialize(_serviceProvider);
        
        _navigationService = _serviceProvider.GetRequiredService<INavigationService>();
        
        CheckLicenseAndStart();
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
        var licenseValidator = _serviceProvider.GetRequiredService<ILicenseValidator>();
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

    private void OnCurrentViewChanged(object? sender, EventArgs e)
    {
        Current.MainWindow = _navigationService.CurrentView as Window;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
