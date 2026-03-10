using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SSHHelper.Auth.Interfaces;
using System.Windows;

namespace SSHHelper.App.ViewModels;

public partial class LicenseViewModel : ObservableObject
{
    private readonly ILicenseValidator _licenseValidator;
    private readonly IMachineIdGenerator _machineIdGenerator;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string _licenseKey = string.Empty;

    [ObservableProperty]
    private string _machineId = string.Empty;

    [ObservableProperty]
    private bool _isValidating;

    [ObservableProperty]
    private bool _isValid;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _showSuccess;

    public bool IsValidButtonEnabled => !string.IsNullOrWhiteSpace(LicenseKey) && !IsValidating;

    public LicenseViewModel(
        ILicenseValidator licenseValidator,
        IMachineIdGenerator machineIdGenerator,
        INavigationService navigationService)
    {
        _licenseValidator = licenseValidator;
        _machineIdGenerator = machineIdGenerator;
        _navigationService = navigationService;
        
        // 初始化时生成机器码
        _machineId = _machineIdGenerator.Generate();
    }

    partial void OnLicenseKeyChanged(string value)
    {
        OnPropertyChanged(nameof(IsValidButtonEnabled));
    }

    partial void OnIsValidatingChanged(bool value)
    {
        OnPropertyChanged(nameof(IsValidButtonEnabled));
    }

    /// <summary>
    /// 验证授权命令
    /// </summary>
    [RelayCommand]
    private async Task ValidateLicense()
    {
        if (string.IsNullOrWhiteSpace(LicenseKey))
        {
            ErrorMessage = "请输入授权码";
            return;
        }

        IsValidating = true;
        ErrorMessage = string.Empty;
        ShowSuccess = false;

        try
        {
            // 先尝试在线验证
            var result = await _licenseValidator.ValidateOnlineAsync(LicenseKey);

            if (result != null)
            {
                IsValid = true;
                ShowSuccess = true;
                ErrorMessage = string.Empty;
                
                // 验证成功，延迟导航到主界面
                await Task.Delay(500);
                _navigationService.NavigateToMainView();
            }
            else
            {
                ErrorMessage = "授权码无效或已过期";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"验证失败: {ex.Message}";
        }
        finally
        {
            IsValidating = false;
        }
    }

    /// <summary>
    /// 复制机器码命令
    /// </summary>
    [RelayCommand]
    private void CopyMachineId()
    {
        Clipboard.SetText(MachineId);
        MessageBox.Show("机器码已复制到剪贴板", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}