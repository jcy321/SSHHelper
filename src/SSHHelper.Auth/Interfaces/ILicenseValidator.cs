using SSHHelper.Auth.Models;

namespace SSHHelper.Auth.Interfaces;

/// <summary>
/// 授权验证接口
/// </summary>
public interface ILicenseValidator
{
    /// <summary>
    /// 验证授权码（在线）
    /// </summary>
    Task<LicenseInfo?> ValidateOnlineAsync(string licenseKey);
    
    /// <summary>
    /// 验证授权码（离线缓存）
    /// </summary>
    Task<LicenseInfo?> ValidateOfflineAsync();
    
    /// <summary>
    /// 获取当前授权信息
    /// </summary>
    LicenseInfo? CurrentLicense { get; }
}