using SSHHelper.Core.Models;

namespace SSHHelper.Core.Services.Interfaces;

/// <summary>
/// SSH配置管理接口
/// </summary>
public interface IConfigManager
{
    /// <summary>
    /// 生成SSH config条目
    /// </summary>
    string GenerateConfigEntry(ServerProfile profile, string keyPath);
    
    /// <summary>
    /// 添加配置到SSH config文件
    /// </summary>
    Task AddConfigEntryAsync(ServerProfile profile, string keyPath);
    
    /// <summary>
    /// 读取所有服务器配置
    /// </summary>
    Task<IReadOnlyList<ServerProfile>> LoadServerProfilesAsync();
    
    /// <summary>
    /// 保存服务器配置
    /// </summary>
    Task SaveServerProfileAsync(ServerProfile profile);
}