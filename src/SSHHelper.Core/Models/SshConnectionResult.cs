namespace SSHHelper.Core.Models;

/// &lt;summary&gt;
/// SSH连接结果
/// &lt;/summary&gt;
public class SshConnectionResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan ConnectionDuration { get; set; }
}