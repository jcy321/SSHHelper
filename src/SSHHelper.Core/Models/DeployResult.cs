namespace SSHHelper.Core.Models;

/// <summary>
/// 公钥部署结果
/// </summary>
public class DeployResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> OutputLines { get; set; } = new();
}