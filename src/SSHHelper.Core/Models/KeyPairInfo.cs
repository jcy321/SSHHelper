namespace SSHHelper.Core.Models;

/// &lt;summary&gt;
/// SSH密钥对信息
/// &lt;/summary&gt;
public class KeyPairInfo
{
    public string PrivateKeyPath { get; set; } = string.Empty;
    public string PublicKeyPath { get; set; } = string.Empty;
    public string KeyType { get; set; } = "ed25519";        // ed25519, rsa
    public string? Comment { get; set; }
    public int? KeySize { get; set; }                       // RSA时使用
    public DateTime CreatedAt { get; set; }
    public string? Fingerprint { get; set; }
}