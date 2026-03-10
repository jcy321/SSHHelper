using SSHHelper.Core.Models;

namespace SSHHelper.Core.Services.Interfaces;

/// <summary>
/// 远程公钥部署服务接口
/// </summary>
public interface IRemoteKeyDeployService
{
    /// <summary>
    /// 部署公钥到远程服务器
    /// </summary>
    Task<DeployResult> DeployPublicKeyAsync(
        ServerProfile profile,
        string rootPassword,
        string publicKeyContent,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 检查公钥是否已部署
    /// </summary>
    Task<bool> IsKeyDeployedAsync(
        ServerProfile profile, 
        string publicKeyContent, 
        CancellationToken cancellationToken = default);
}