using Microsoft.Extensions.Logging;
using Renci.SshNet;
using SSHHelper.Core.Models;
using SSHHelper.Core.Services.Interfaces;
using System.Threading.Tasks;

namespace SSHHelper.Core.Services;

/// <summary>
/// 远程公钥部署服务实现
/// </summary>
public class RemoteKeyDeployService : IRemoteKeyDeployService
{
    private readonly ISshSessionService _sshService;
    private readonly ILogger<RemoteKeyDeployService> _logger;
    
    public RemoteKeyDeployService(
        ISshSessionService sshService,
        ILogger<RemoteKeyDeployService> logger)
    {
        _sshService = sshService;
        _logger = logger;
    }
    
    /// <summary>
    /// 部署公钥到远程服务器（幂等）
    /// </summary>
    public async Task<DeployResult> DeployPublicKeyAsync(
        ServerProfile profile,
        string rootPassword,
        string publicKeyContent,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connectionInfo = new ConnectionInfo(
                profile.IpAddress,
                profile.Port,
                profile.UserName,
                new PasswordAuthenticationMethod(profile.UserName, rootPassword));
            
            // 设置连接超时
            connectionInfo.Timeout = TimeSpan.FromSeconds(30);
            
            using var client = new SshClient(connectionInfo);
            
            // 建立SSH连接
            await Task.Run(() => client.Connect(), cancellationToken);
            
            if (!client.IsConnected)
            {
                return new DeployResult 
                { 
                    IsSuccess = false, 
                    ErrorMessage = "SSH连接失败" 
                };
            }
            
            // 检查 authorized_keys 是否已包含该公钥
            var checkCmd = $"grep -F '{publicKeyContent.Trim()}' ~/.ssh/authorized_keys";
            var checkResult = client.RunCommand(checkCmd);
            
            if (checkResult.ExitStatus == 0)
            {
                return new DeployResult 
                { 
                    IsSuccess = true,
                    OutputLines = new List<string> { "公钥已存在，跳过部署" }
                };
            }
            
            // 部署公钥（幂等操作）
            var commands = new[]
            {
                "mkdir -p ~/.ssh",
                "chmod 700 ~/.ssh",
                $"echo '{publicKeyContent}' >> ~/.ssh/authorized_keys",
                "chmod 600 ~/.ssh/authorized_keys",
                "sort -u ~/.ssh/authorized_keys -o ~/.ssh/authorized_keys"  // 去重
            };
            
            var outputLines = new List<string>();
            foreach (var cmd in commands)
            {
                var result = client.RunCommand(cmd);
                if (!string.IsNullOrEmpty(result.Result))
                    outputLines.Add(result.Result);
                if (result.ExitStatus != 0 && !cmd.Contains("mkdir"))
                {
                    return new DeployResult
                    {
                        IsSuccess = false,
                        ErrorMessage = $"命令执行失败: {cmd}",
                        OutputLines = outputLines
                    };
                }
            }
            
            client.Disconnect();
            
            return new DeployResult
            {
                IsSuccess = true,
                OutputLines = outputLines
            };
        }
        catch (Exception ex)
        {
            return new DeployResult
            {
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }
    
    /// <summary>
    /// 检查公钥是否已部署
    /// </summary>
    public async Task<bool> IsKeyDeployedAsync(
        ServerProfile profile, 
        string publicKeyContent, 
        CancellationToken cancellationToken = default)
    {
        // 使用 ExecuteCommandAsync 来检查公钥是否已部署
        var checkCmd = $"grep -F '{publicKeyContent.Trim()}' ~/.ssh/authorized_keys";
        
        var result = await _sshService.ExecuteCommandAsync(profile, checkCmd, cancellationToken);
        
        // 如果命令成功执行且有输出，则表示公钥已部署
        return result.IsSuccess && result.OutputLines.Any(line => !string.IsNullOrWhiteSpace(line));
    }
}