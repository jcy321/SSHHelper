using SSHHelper.Core.Models;

namespace SSHHelper.Core.Services.Interfaces;

/// <summary>
/// SSH会话服务接口
/// </summary>
public interface ISshSessionService
{
    /// <summary>
    /// 测试SSH连接
    /// </summary>
    Task<SshConnectionResult> TestConnectionAsync(
        ServerProfile profile, 
        string password, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 执行远程命令
    /// </summary>
    Task<DeployResult> ExecuteCommandAsync(
        ServerProfile profile, 
        string command, 
        CancellationToken cancellationToken = default);
}