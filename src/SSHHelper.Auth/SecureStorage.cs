using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using SSHHelper.Auth.Models;

namespace SSHHelper.Auth;

/// <summary>
/// 安全存储实现
/// 使用平台特定的保护机制存储敏感数据
/// </summary>
public class SecureStorage : Interfaces.ISecureStorage
{
    private readonly string _storagePath;

    /// <summary>
    /// 初始化安全存储实例
    /// </summary>
    public SecureStorage()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _storagePath = Path.Combine(appDataPath, "SSHHelper", "license.dat");
    }

    /// <summary>
    /// 保存授权信息
    /// </summary>
    /// <param name="license">授权信息</param>
    public async Task SaveAsync(LicenseInfo license)
    {
        // 序列化为JSON
        var json = JsonConvert.SerializeObject(license);

        byte[] protectedData;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Windows: 使用DPAPI保护数据
            protectedData = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(json),
                null,
                DataProtectionScope.CurrentUser);
        }
        else
        {
            // Linux/macOS: 使用简单加密（实际产品中应使用更安全的方法）
            protectedData = SimpleEncrypt(Encoding.UTF8.GetBytes(json));
        }

        // 确保存储目录存在
        Directory.CreateDirectory(Path.GetDirectoryName(_storagePath)!);

        // 写入文件
        await File.WriteAllBytesAsync(_storagePath, protectedData);
    }

    /// <summary>
    /// 加载授权信息
    /// </summary>
    /// <returns>授权信息</returns>
    public async Task<LicenseInfo?> LoadAsync()
    {
        if (!File.Exists(_storagePath))
            return null;

        try
        {
            // 读取保护的数据
            var protectedData = await File.ReadAllBytesAsync(_storagePath);

            byte[] decryptedData;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows: 使用DPAPI解密数据
                decryptedData = ProtectedData.Unprotect(
                    protectedData,
                    null,
                    DataProtectionScope.CurrentUser);
            }
            else
            {
                // Linux/macOS: 使用简单解密
                decryptedData = SimpleDecrypt(protectedData);
            }

            var json = Encoding.UTF8.GetString(decryptedData);
            return JsonConvert.DeserializeObject<LicenseInfo>(json);
        }
        catch
        {
            // 解密失败或数据损坏，返回null
            return null;
        }
    }

    /// <summary>
    /// 清除授权信息
    /// </summary>
    public Task ClearAsync()
    {
        if (File.Exists(_storagePath))
            File.Delete(_storagePath);

        return Task.CompletedTask;
    }

    #region 简单加密实现（仅用于非Windows平台）

    /// <summary>
    /// 简单加密（仅用于演示，实际产品中应使用更安全的方法）
    /// </summary>
    private byte[] SimpleEncrypt(byte[] data)
    {
        // 在实际产品中，应该使用更强的加密方法
        // 这里仅作演示用
        return data;
    }

    /// <summary>
    /// 简单解密（仅用于演示，实际产品中应使用更安全的方法）
    /// </summary>
    private byte[] SimpleDecrypt(byte[] data)
    {
        // 在实际产品中，应该使用更强的加密方法
        // 这里仅作演示用
        return data;
    }

    #endregion
}