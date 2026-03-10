namespace SSHHelper.Core.Models;

/// &lt;summary&gt;
/// 服务器配置档案
/// &lt;/summary&gt;
public class ServerProfile
{
    public string Alias { get; set; } = string.Empty;      // ECS别名
    public string MachineMark { get; set; } = string.Empty; // 机器标记
    public string IpAddress { get; set; } = string.Empty;   // IP地址
    public string UserName { get; set; } = "root";          // 用户名
    public int Port { get; set; } = 22;                     // SSH端口
    public string? KeyPath { get; set; }                    // 密钥路径
    public DateTime CreatedAt { get; set; }                 // 创建时间
    public DateTime? LastConnectedAt { get; set; }          // 最后连接时间
}