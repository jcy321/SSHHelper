using Microsoft.Extensions.Logging;
using SSHHelper.Core.Models;
using SSHHelper.Core.Services.Interfaces;
using System.Threading.Tasks;

namespace SSHHelper.Core.Services;

/// <summary>
/// SSH配置管理实现
/// </summary>
public class ConfigManager : IConfigManager
{
    private readonly ILogger<ConfigManager> _logger;
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private readonly string _configPath;
    private volatile string? _cachedConfig;
    
    public ConfigManager(ILogger<ConfigManager> logger)
    {
        _logger = logger;
        var sshDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
            ".ssh");
        _configPath = Path.Combine(sshDir, "config");
        
        // 确保 .ssh 目录存在
        if (!Directory.Exists(sshDir))
        {
            Directory.CreateDirectory(sshDir);
        }
    }
    
    /// <summary>
    /// 生成SSH config条目
    /// </summary>
    public string GenerateConfigEntry(ServerProfile profile, string keyPath)
    {
        return $@"# {profile.MachineMark}
Host {profile.Alias}
    HostName {profile.IpAddress}
    User {profile.UserName}
    Port {profile.Port}
    IdentityFile {keyPath}
    IdentitiesOnly yes
";
    }
    
    /// <summary>
    /// 添加配置到SSH config文件（线程安全）
    /// </summary>
    public async Task AddConfigEntryAsync(ServerProfile profile, string keyPath)
    {
        // 写入锁保护
        await _writeLock.WaitAsync();
        try
        {
            var entry = GenerateConfigEntry(profile, keyPath);
            
            // 原子替换：先写临时文件，再替换
            var tempPath = $"{_configPath}.tmp";
            var existingContent = File.Exists(_configPath) 
                ? await File.ReadAllTextAsync(_configPath) 
                : "";
            
            // 检查是否已存在
            if (existingContent.Contains($"Host {profile.Alias}"))
            {
                _logger.LogWarning("配置 {Alias} 已存在", profile.Alias);
                return;
            }
            
            // 写入新内容
            var newContent = existingContent + Environment.NewLine + entry;
            await File.WriteAllTextAsync(tempPath, newContent);
            
            // 原子替换
            File.Move(tempPath, _configPath, overwrite: true);
            
            // 更新缓存（原子操作）
            Interlocked.Exchange(ref _cachedConfig, newContent);
            
            _logger.LogInformation("已添加配置: {Alias}", profile.Alias);
        }
        finally
        {
            _writeLock.Release();
        }
    }
    
    /// <summary>
    /// 读取所有服务器配置
    /// </summary>
    public async Task<IReadOnlyList<ServerProfile>> LoadServerProfilesAsync()
    {
        // 不可变快照读取（无锁）
        var content = _cachedConfig;
        if (content == null)
        {
            content = File.Exists(_configPath) 
                ? await File.ReadAllTextAsync(_configPath) 
                : "";
            Interlocked.Exchange(ref _cachedConfig, content);
        }
        
        // 解析配置文件
        return ParseConfigFile(content);
    }
    
    /// <summary>
    /// 保存服务器配置
    /// </summary>
    public Task SaveServerProfileAsync(ServerProfile profile)
    {
        // 在当前实现中，配置是通过AddConfigEntryAsync添加的
        // 这里可以扩展为支持修改现有配置
        throw new NotImplementedException("将在后续版本中实现配置修改功能");
    }
    
    /// <summary>
    /// 解析SSH配置文件
    /// </summary>
    private IReadOnlyList<ServerProfile> ParseConfigFile(string content)
    {
        var profiles = new List<ServerProfile>();
        var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        
        ServerProfile? currentProfile = null;
        string? currentMark = null;
        
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
            {
                // 处理注释行，提取MachineMark
                if (trimmedLine.StartsWith("#"))
                {
                    currentMark = trimmedLine.Substring(1).Trim();
                }
                continue;
            }
            
            var parts = trimmedLine.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) continue;
            
            var key = parts[0].Trim();
            var value = parts[1].Trim();
            
            if (key.Equals("Host", StringComparison.OrdinalIgnoreCase))
            {
                // 创建新的服务器配置
                currentProfile = new ServerProfile
                {
                    Alias = value,
                    MachineMark = currentMark ?? "",
                    CreatedAt = DateTime.UtcNow
                };
                profiles.Add(currentProfile);
            }
            else if (currentProfile != null)
            {
                // 设置当前配置的属性
                switch (key.ToLower())
                {
                    case "hostname":
                        currentProfile.IpAddress = value;
                        break;
                    case "user":
                        currentProfile.UserName = value;
                        break;
                    case "port":
                        if (int.TryParse(value, out var port))
                        {
                            currentProfile.Port = port;
                        }
                        break;
                    case "identityfile":
                        currentProfile.KeyPath = value;
                        break;
                }
            }
        }
        
        return profiles.AsReadOnly();
    }
}