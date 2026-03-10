using Microsoft.Extensions.Logging;
using Renci.SshNet;
using Renci.SshNet.Common;
using SSHHelper.Core.Models;
using SSHHelper.Core.Services.Interfaces;
using System.Threading.Tasks;

namespace SSHHelper.Core.Services;

/// <summary>
/// SSH会话服务实现
/// </summary>
public class SshSessionService : ISshSessionService
{
    private readonly ILogger<SshSessionService> _logger;
    
    public SshSessionService(ILogger<SshSessionService> logger)
    {
        _logger = logger;
    }
    
    public async Task<SshConnectionResult> TestConnectionAsync(
        ServerProfile profile, 
        string password, 
        CancellationToken cancellationToken = default)
    {
        var connectionInfo = new ConnectionInfo(
            profile.IpAddress,
            profile.Port,
            profile.UserName,
            new PasswordAuthenticationMethod(profile.UserName, password));
        
        // 设置连接超时
        connectionInfo.Timeout = TimeSpan.FromSeconds(30);
        
        using var client = new SshClient(connectionInfo);
        var startTime = DateTime.UtcNow;
        
        try
        {
            // 建立连接
            await Task.Run(() => client.Connect(), cancellationToken);
            
            var duration = DateTime.UtcNow - startTime;
            
            return new SshConnectionResult
            {
                IsSuccess = client.IsConnected,
                ConnectionDuration = duration
            };
        }
        catch (SshAuthenticationException)
        {
            return new SshConnectionResult
            {
                IsSuccess = false,
                ErrorMessage = "认证失败",
                ConnectionDuration = DateTime.UtcNow - startTime
            };
        }
        catch (SshOperationTimeoutException)
        {
            return new SshConnectionResult
            {
                IsSuccess = false,
                ErrorMessage = "连接超时",
                ConnectionDuration = DateTime.UtcNow - startTime
            };
        }
        catch (Exception ex)
        {
            return new SshConnectionResult
            {
                IsSuccess = false,
                ErrorMessage = $"连接失败: {ex.Message}",
                ConnectionDuration = DateTime.UtcNow - startTime
            };
        }
    }
    
    public async Task<DeployResult> ExecuteCommandAsync(
        ServerProfile profile, 
        string command, 
        CancellationToken cancellationToken = default)
    {
        var connectionInfo = new ConnectionInfo(
            profile.IpAddress,
            profile.Port,
            profile.UserName,
            new PasswordAuthenticationMethod(profile.UserName, profile.KeyPath ?? ""));
        
        // 设置命令执行超时
        connectionInfo.Timeout = TimeSpan.FromSeconds(30);
        
        using var client = new SshClient(connectionInfo);
        
        try
        {
            // 建立连接
            await Task.Run(() => client.Connect(), cancellationToken);
            
            if (!client.IsConnected)
            {
                return new DeployResult
                {
                    IsSuccess = false,
                    ErrorMessage = "SSH连接失败"
                };
            }
            
            // 执行命令
            var cmd = client.RunCommand(command);
            
            return new DeployResult
            {
                IsSuccess = cmd.ExitStatus == 0,
                OutputLines = new List<string> 
                { 
                    cmd.Result ?? "", 
                    cmd.Error ?? "" 
                }
            };
        }
        catch (Exception ex)
        {
            return new DeployResult
            {
                IsSuccess = false,
                ErrorMessage = $"命令执行失败: {ex.Message}"
            };
        }
    }
}