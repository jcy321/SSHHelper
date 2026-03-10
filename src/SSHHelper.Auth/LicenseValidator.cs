using System.Net.Http.Json;
using System.Text.Json;
using SSHHelper.Auth.Interfaces;
using SSHHelper.Auth.Models;

namespace SSHHelper.Auth;

public class LicenseValidator : ILicenseValidator
{
    private readonly HttpClient _httpClient;
    private readonly IMachineIdGenerator _machineIdGenerator;
    private readonly ISecureStorage _secureStorage;
    private LicenseInfo? _currentLicense;

    private const string LicenseServerUrl = "https://license.sxls.fun";

    public LicenseValidator(IMachineIdGenerator machineIdGenerator, ISecureStorage secureStorage)
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        _machineIdGenerator = machineIdGenerator;
        _secureStorage = secureStorage;
    }

    public LicenseInfo? CurrentLicense => _currentLicense;

    public async Task<LicenseInfo?> ValidateOnlineAsync(string licenseKey)
    {
        var machineId = _machineIdGenerator.Generate();

        try
        {
            var request = new
            {
                license_key = licenseKey,
                machine_id = machineId,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"{LicenseServerUrl}/api/v1/license/validate",
                request);

            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<LicenseInfo>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result != null)
                {
                    result.Type = LicenseType.Professional;
                    await _secureStorage.SaveAsync(result);
                    _currentLicense = result;
                    return result;
                }
            }

            return null;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"网络请求失败: {ex.Message}");
            return null;
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("请求超时，请检查网络连接");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"在线验证失败: {ex.Message}");
            return null;
        }
    }

    public async Task<LicenseInfo?> ValidateOfflineAsync()
    {
        try
        {
            var cached = await _secureStorage.LoadAsync();

            if (cached != null && cached.ExpiresAt > DateTime.UtcNow)
            {
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
            Console.WriteLine($"离线验证失败: {ex.Message}");
            return null;
        }
    }
}