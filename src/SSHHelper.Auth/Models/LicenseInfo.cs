using System.Text.Json.Serialization;

namespace SSHHelper.Auth.Models;

public class LicenseInfo
{
    [JsonPropertyName("licenseKey")]
    public string LicenseKey { get; set; } = string.Empty;
    
    [JsonPropertyName("machineId")]
    public string MachineId { get; set; } = string.Empty;
    
    [JsonPropertyName("expiresAt")]
    public DateTime ExpiresAt { get; set; }
    
    [JsonPropertyName("holderName")]
    public string? HolderName { get; set; }
    
    public LicenseType Type { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public string[] Features { get; set; } = Array.Empty<string>();
}

public enum LicenseType
{
    Trial,
    Professional,
    Enterprise
}