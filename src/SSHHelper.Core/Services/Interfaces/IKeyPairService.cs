using SSHHelper.Core.Models;

namespace SSHHelper.Core.Services.Interfaces;

/// <summary>
/// SSH密钥对生成服务接口
/// </summary>
public interface IKeyPairService
{
    /// <summary>
    /// 生成SSH密钥对
    /// </summary>
    Task<KeyPairInfo> GenerateKeyPairAsync(
        string alias, 
        string keyType = "ed25519", 
        string? passphrase = null, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 检查密钥对是否存在
    /// </summary>
    Task<bool> KeyPairExistsAsync(string alias);
    
    /// <summary>
    /// 获取密钥对信息
    /// </summary>
    Task<KeyPairInfo?> GetKeyPairInfoAsync(string alias);
    
    /// <summary>
    /// 读取公钥内容
    /// </summary>
    Task<string> ReadPublicKeyAsync(string alias);
}