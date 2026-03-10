using Microsoft.Extensions.Logging;
using SSHHelper.Core.Helpers;
using SSHHelper.Core.Models;
using SSHHelper.Core.Services.Interfaces;
using System.Threading.Tasks;

namespace SSHHelper.Core.Services;

/// <summary>
/// SSH密钥对生成服务实现
/// </summary>
public class KeyPairService : IKeyPairService
{
    private readonly ILogger<KeyPairService> _logger;
    private readonly ProcessRunner _processRunner;
    private readonly string _sshDirectory;
    
    public KeyPairService(ILogger<KeyPairService> logger)
    {
        _logger = logger;
        _processRunner = new ProcessRunner();
        _sshDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
            ".ssh");
    }
    
    public async Task<KeyPairInfo> GenerateKeyPairAsync(
        string alias, 
        string keyType = "ed25519", 
        string? passphrase = null, 
        CancellationToken cancellationToken = default)
    {
        var keyPath = Path.Combine(_sshDirectory, alias);
        var pubKeyPath = $"{keyPath}.pub";
        
        // 检查是否已存在
        if (File.Exists(keyPath))
        {
            throw new InvalidOperationException($"密钥 {alias} 已存在");
        }
        
        // 构建 ssh-keygen 命令
        var args = $"-t {keyType} -f \"{keyPath}\" -N \"{passphrase ?? ""}\" -C \"{alias}\"";
        
        // 执行命令（使用ProcessRunner）
        var result = await _processRunner.ExecuteAsync("ssh-keygen", args, cancellationToken);
        
        if (!result.IsSuccess)
        {
            throw new Exception($"密钥生成失败: {result.Error}");
        }
        
        // 读取指纹
        var fingerprint = await GetFingerprintAsync(keyPath);
        
        return new KeyPairInfo
        {
            PrivateKeyPath = keyPath,
            PublicKeyPath = pubKeyPath,
            KeyType = keyType,
            Comment = alias,
            CreatedAt = DateTime.UtcNow,
            Fingerprint = fingerprint
        };
    }
    
    public Task<bool> KeyPairExistsAsync(string alias)
    {
        var keyPath = Path.Combine(_sshDirectory, alias);
        return Task.FromResult(File.Exists(keyPath));
    }
    
    public async Task<KeyPairInfo?> GetKeyPairInfoAsync(string alias)
    {
        var keyPath = Path.Combine(_sshDirectory, alias);
        var pubKeyPath = $"{keyPath}.pub";
        
        // 检查密钥对是否存在
        if (!File.Exists(keyPath))
        {
            return null;
        }
        
        // 读取指纹
        var fingerprint = await GetFingerprintAsync(keyPath);
        
        // 读取公钥内容获取注释
        string? comment = null;
        if (File.Exists(pubKeyPath))
        {
            var pubKeyContent = await File.ReadAllTextAsync(pubKeyPath);
            var parts = pubKeyContent.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3)
            {
                comment = parts[2]; // 注释通常是第三部分
            }
        }
        
        return new KeyPairInfo
        {
            PrivateKeyPath = keyPath,
            PublicKeyPath = pubKeyPath,
            KeyType = "ed25519", // 默认值，实际可以通过解析进一步确定
            Comment = comment,
            CreatedAt = File.GetCreationTimeUtc(keyPath),
            Fingerprint = fingerprint
        };
    }
    
    public async Task<string> ReadPublicKeyAsync(string alias)
    {
        var pubKeyPath = Path.Combine(_sshDirectory, $"{alias}.pub");
        return await File.ReadAllTextAsync(pubKeyPath);
    }
    
    /// <summary>
    /// 获取密钥指纹
    /// </summary>
    private async Task<string?> GetFingerprintAsync(string keyPath)
    {
        try
        {
            var result = await _processRunner.ExecuteAsync(
                "ssh-keygen", 
                $"-lf \"{keyPath}\"", 
                CancellationToken.None);
            
            if (result.IsSuccess)
            {
                // 输出格式: "256 SHA256:... comment (ED25519)"
                var parts = result.Output.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    return parts[1]; // 返回指纹部分
                }
            }
            
            return null;
        }
        catch
        {
            return null; // 静默失败
        }
    }
}