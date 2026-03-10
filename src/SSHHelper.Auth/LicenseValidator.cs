using System.Net.Http.Json;
using SSHHelper.Auth.Interfaces;
using SSHHelper.Auth.Models;

namespace SSHHelper.Auth;

/// <summary>
/// 授权验证器实现
/// 提供在线和离线授权验证功能
/// </summary>
public class LicenseValidator : ILicenseValidator
{
    private readonly HttpClient _httpClient;
    private readonly IMachineIdGenerator _machineIdGenerator;
    private readonly ISecureStorage _secureStorage;
    private LicenseInfo? _currentLicense;

    /// <summary>
    /// 初始化授权验证器
    /// </summary>
    /// <param name="machineIdGenerator">机器码生成器</param>
    /// <param name="secureStorage">安全存储</param>
    public LicenseValidator(IMachineIdGenerator machineIdGenerator, ISecureStorage secureStorage)
    {
        _httpClient = new HttpClient();
        _machineIdGenerator = machineIdGenerator;
        _secureStorage = secureStorage;
    }

    /// <summary>
    /// 当前授权信息
    /// </summary>
    public LicenseInfo? CurrentLicense => _currentLicense;

    /// <summary>
    /// 验证授权码（在线）
    /// </summary>
    /// <param name="licenseKey">授权码</param>
    /// <returns>授权信息</returns>
    public async Task<LicenseInfo?> ValidateOnlineAsync(string licenseKey)
    {
        var machineId = _machineIdGenerator.Generate();

        try
        {
            // 构造验证请求
            var request = new
            {
                licenseKey,
                machineId,
                timestamp = DateTime.UtcNow
            };

            // 调用授权服务器验证（URL：暂时硬编码为 https://your-license-server.com/api/license/validate）
            var response = await _httpClient.PostAsJsonAsync(
                "https://your-license-server.com/api/license/validate",
                request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LicenseInfo>();

                if (result != null)
                {
                    // 保存到安全存储
                    await _secureStorage.SaveAsync(result);

                    _currentLicense = result;
                    return result;
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            // 记录日志（在实际应用中应使用ILogger）
            Console.WriteLine($"在线验证失败: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 验证授权码（离线缓存）
    /// </summary>
    /// <returns>授权信息</returns>
    public async Task<LicenseInfo?> ValidateOfflineAsync()
    {
        try
        {
            var cached = await _secureStorage.LoadAsync();

            if (cached != null && cached.ExpiresAt > DateTime.UtcNow)
            {
                // 验证机器码
                var currentMachineId = _machineIdGenerator.Generate();
                if (cached.MachineId == currentMachineId)
                {
                    _currentLicense = cached;
                    return cached;
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            // 记录日志（在实际应用中应使用ILogger）
            Console.WriteLine($"离线验证失败: {ex.Message}");
            return null;
        }
    }
}