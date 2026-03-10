namespace SSHHelper.Auth.Models;

/// <summary>
/// 授权信息
/// </summary>
public class LicenseInfo
{
    public string LicenseKey { get; set; } = string.Empty;
    public LicenseType Type { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string MachineId { get; set; } = string.Empty;
    public DateTime? ActivatedAt { get; set; }
    public string[] Features { get; set; } = Array.Empty<string>();
}

/// <summary>
/// 授权类型
/// </summary>
public enum LicenseType
{
    Trial,
    Professional,
    Enterprise
}