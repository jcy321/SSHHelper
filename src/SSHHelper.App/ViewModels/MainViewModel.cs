using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SSHHelper.Core.Models;
using SSHHelper.Core.Services.Interfaces;
using System.Windows;

namespace SSHHelper.App.ViewModels;

/// <summary>
/// 主界面ViewModel - SSH密钥配置
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IKeyPairService _keyPairService;
    private readonly IConfigManager _configManager;
    private readonly IRemoteKeyDeployService _remoteKeyDeployService;
    private readonly ISshSessionService _sshSessionService;

    // 表单字段
    [ObservableProperty]
    private string _alias = string.Empty;

    [ObservableProperty]
    private string _machineMark = string.Empty;

    [ObservableProperty]
    private string _ipAddress = string.Empty;

    [ObservableProperty]
    private string _userName = "root";

    [ObservableProperty]
    private int _port = 22;

    [ObservableProperty]
    private string _password = string.Empty;

    // 状态字段
    [ObservableProperty]
    private string _sshConfigOutput = string.Empty;

    [ObservableProperty]
    private int _currentStep;

    [ObservableProperty]
    private int _totalSteps = 4;

    [ObservableProperty]
    private string _currentStepMessage = string.Empty;

    [ObservableProperty]
    private bool _isConfiguring;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isSuccess;

    [ObservableProperty]
    private double _progressValue;

    public bool IsConfigureButtonEnabled => 
        !string.IsNullOrWhiteSpace(Alias) && 
        !string.IsNullOrWhiteSpace(IpAddress) && 
        !string.IsNullOrWhiteSpace(Password) && 
        !IsConfiguring;

    public MainViewModel(
        IKeyPairService keyPairService,
        IConfigManager configManager,
        IRemoteKeyDeployService remoteKeyDeployService,
        ISshSessionService sshSessionService)
    {
        _keyPairService = keyPairService;
        _configManager = configManager;
        _remoteKeyDeployService = remoteKeyDeployService;
        _sshSessionService = sshSessionService;
    }

    partial void OnAliasChanged(string value) => OnPropertyChanged(nameof(IsConfigureButtonEnabled));
    partial void OnIpAddressChanged(string value) => OnPropertyChanged(nameof(IsConfigureButtonEnabled));
    partial void OnPasswordChanged(string value) => OnPropertyChanged(nameof(IsConfigureButtonEnabled));
    partial void OnIsConfiguringChanged(bool value) => OnPropertyChanged(nameof(IsConfigureButtonEnabled));

    /// <summary>
    /// 一键配置命令
    /// </summary>
    [RelayCommand]
    private async Task Configure()
    {
        if (!ValidateInput())
            return;

        IsConfiguring = true;
        IsSuccess = false;
        ErrorMessage = string.Empty;
        SshConfigOutput = string.Empty;
        CurrentStep = 0;
        ProgressValue = 0;

        var profile = new ServerProfile
        {
            Alias = Alias.Trim(),
            MachineMark = MachineMark.Trim(),
            IpAddress = IpAddress.Trim(),
            UserName = UserName.Trim(),
            Port = Port,
            CreatedAt = DateTime.UtcNow
        };

        string? keyPath = null;

        try
        {
            // Step 1: 生成密钥对
            CurrentStep = 1;
            CurrentStepMessage = "正在生成SSH密钥对...";
            ProgressValue = 0;

            if (await _keyPairService.KeyPairExistsAsync(profile.Alias))
            {
                var existingKey = await _keyPairService.GetKeyPairInfoAsync(profile.Alias);
                keyPath = existingKey?.PrivateKeyPath;
                CurrentStepMessage = "密钥对已存在，跳过生成";
            }
            else
            {
                var keyInfo = await _keyPairService.GenerateKeyPairAsync(profile.Alias, "ed25519", null);
                keyPath = keyInfo.PrivateKeyPath;
            }
            ProgressValue = 25;

            // Step 2: 测试SSH连接
            CurrentStep = 2;
            CurrentStepMessage = "正在连接服务器...";
            
            var connectionResult = await _sshSessionService.TestConnectionAsync(profile, Password);
            if (!connectionResult.IsSuccess)
            {
                ErrorMessage = $"SSH连接失败: {connectionResult.ErrorMessage ?? "未知错误"}";
                return;
            }
            ProgressValue = 50;

            // Step 3: 部署公钥到远程服务器
            CurrentStep = 3;
            CurrentStepMessage = "正在部署公钥到服务器...";

            var publicKeyContent = await _keyPairService.ReadPublicKeyAsync(profile.Alias);
            var deployResult = await _remoteKeyDeployService.DeployPublicKeyAsync(profile, Password, publicKeyContent);
            
            if (!deployResult.IsSuccess)
            {
                ErrorMessage = $"公钥部署失败: {deployResult.ErrorMessage ?? "未知错误"}";
                return;
            }
            ProgressValue = 75;

            // Step 4: 配置SSH config
            CurrentStep = 4;
            CurrentStepMessage = "正在配置SSH config...";

            await _configManager.AddConfigEntryAsync(profile, keyPath!);
            
            // 生成配置输出
            SshConfigOutput = _configManager.GenerateConfigEntry(profile, keyPath!);
            ProgressValue = 100;

            CurrentStepMessage = "配置完成！";
            IsSuccess = true;

            MessageBox.Show(
                "SSH密钥配置成功！\n\n您现在可以使用以下命令连接服务器：\n" +
                $"ssh {profile.Alias}\n\n" +
                "配置已写入 C:\\Users\\<用户名>\\.ssh\\config",
                "配置成功",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"配置失败: {ex.Message}";
        }
        finally
        {
            IsConfiguring = false;
        }
    }

    /// <summary>
    /// 复制SSH配置命令
    /// </summary>
    [RelayCommand]
    private void CopyConfig()
    {
        if (string.IsNullOrEmpty(SshConfigOutput))
            return;

        Clipboard.SetText(SshConfigOutput);
        MessageBox.Show("SSH配置已复制到剪贴板", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>
    /// 测试连接命令
    /// </summary>
    [RelayCommand]
    private async Task TestConnection()
    {
        if (string.IsNullOrWhiteSpace(IpAddress) || string.IsNullOrWhiteSpace(Password))
        {
            MessageBox.Show("请输入IP地址和密码", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var profile = new ServerProfile
        {
            IpAddress = IpAddress.Trim(),
            UserName = UserName.Trim(),
            Port = Port
        };

        try
        {
            var result = await _sshSessionService.TestConnectionAsync(profile, Password);
            if (result.IsSuccess)
            {
                MessageBox.Show($"连接成功！耗时: {result.ConnectionDuration.TotalMilliseconds:F0}ms", 
                    "连接测试", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"连接失败: {result.ErrorMessage}", 
                    "连接测试", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"连接测试失败: {ex.Message}", 
                "连接测试", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 重置表单命令
    /// </summary>
    [RelayCommand]
    private void ResetForm()
    {
        Alias = string.Empty;
        MachineMark = string.Empty;
        IpAddress = string.Empty;
        UserName = "root";
        Port = 22;
        Password = string.Empty;
        SshConfigOutput = string.Empty;
        ErrorMessage = string.Empty;
        IsSuccess = false;
        CurrentStep = 0;
        ProgressValue = 0;
        CurrentStepMessage = string.Empty;
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(Alias))
        {
            ErrorMessage = "请输入服务器别名";
            return false;
        }

        // 验证别名格式（SSH config中的Host名称不能包含空格和特殊字符）
        if (Alias.Contains(' ') || Alias.Contains('\t'))
        {
            ErrorMessage = "别名不能包含空格";
            return false;
        }

        if (string.IsNullOrWhiteSpace(IpAddress))
        {
            ErrorMessage = "请输入IP地址";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "请输入root密码";
            return false;
        }

        return true;
    }
}