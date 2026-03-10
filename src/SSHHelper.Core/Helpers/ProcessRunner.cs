using System.Diagnostics;
using System.Text;

namespace SSHHelper.Core.Helpers;

/// <summary>
/// 命令行进程执行器
/// </summary>
public class ProcessRunner
{
    /// <summary>
    /// 执行命令并返回结果
    /// </summary>
    public async Task<ProcessResult> ExecuteAsync(
        string command, 
        string arguments, 
        CancellationToken cancellationToken = default)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };
        
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();
        
        process.OutputDataReceived += (s, e) => outputBuilder.AppendLine(e.Data);
        process.ErrorDataReceived += (s, e) => errorBuilder.AppendLine(e.Data);
        
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        
        await process.WaitForExitAsync(cancellationToken);
        
        return new ProcessResult
        {
            ExitCode = process.ExitCode,
            Output = outputBuilder.ToString(),
            Error = errorBuilder.ToString(),
            IsSuccess = process.ExitCode == 0
        };
    }
}

/// <summary>
/// 进程执行结果
/// </summary>
public class ProcessResult
{
    public int ExitCode { get; set; }
    public string Output { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
}