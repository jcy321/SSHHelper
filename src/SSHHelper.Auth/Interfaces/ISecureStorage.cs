using SSHHelper.Auth.Models;

namespace SSHHelper.Auth.Interfaces;

/// <summary>
/// 安全存储接口
/// </summary>
public interface ISecureStorage
{
    /// <summary>
    /// 保存授权信息
    /// </summary>
    Task SaveAsync(LicenseInfo license);
    
    /// <summary>
    /// 加载授权信息
    /// </summary>
    Task<LicenseInfo?> LoadAsync();
    
    /// <summary>
    /// 清除授权信息
    /// </summary>
    Task ClearAsync();
}