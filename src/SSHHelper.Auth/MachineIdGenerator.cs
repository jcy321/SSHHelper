using System.Diagnostics;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace SSHHelper.Auth;

/// <summary>
/// 机器码生成器
/// 基于硬件信息生成唯一ID
/// </summary>
public class MachineIdGenerator : Interfaces.IMachineIdGenerator
{
    /// <summary>
    /// 生成唯一机器码
    /// </summary>
    /// <returns>16位十六进制字符串</returns>
    public string Generate()
    {
        var components = new List<string>
        {
            GetCpuId(),
            GetMotherboardSerial(),
            GetSystemDiskSerial(),
            GetFirstMacAddress()
        };

        var combined = string.Join("|", components.Where(c => !string.IsNullOrEmpty(c)));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(combined));

        // 返回16位十六进制字符串
        return Convert.ToHexString(hash)[..16];
    }

    /// <summary>
    /// 获取CPU ID
    /// </summary>
    private string GetCpuId()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
#if NET8_0_OR_GREATER
                return GetWindowsCpuId();
#endif
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return GetLinuxCpuId();
            }
        }
        catch
        {
            // 忽略异常，返回空字符串
        }

        return string.Empty;
    }

    /// <summary>
    /// 获取主板序列号
    /// </summary>
    private string GetMotherboardSerial()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
#if NET8_0_OR_GREATER
                return GetWindowsMotherboardSerial();
#endif
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return GetLinuxMotherboardSerial();
            }
        }
        catch
        {
            // 忽略异常，返回空字符串
        }

        return string.Empty;
    }

    /// <summary>
    /// 获取系统磁盘序列号
    /// </summary>
    private string GetSystemDiskSerial()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
#if NET8_0_OR_GREATER
                return GetWindowsSystemDiskSerial();
#endif
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return GetLinuxSystemDiskSerial();
            }
        }
        catch
        {
            // 忽略异常，返回空字符串
        }

        return string.Empty;
    }

    /// <summary>
    /// 获取第一个网卡MAC地址
    /// </summary>
    private string GetFirstMacAddress()
    {
        try
        {
            var nic = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .OrderByDescending(n => n.Speed)
                .FirstOrDefault();

            return nic?.GetPhysicalAddress().ToString() ?? string.Empty;
        }
        catch
        {
            // 忽略异常，返回空字符串
        }

        return string.Empty;
    }

    #region Windows实现

    private string GetWindowsCpuId()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
            foreach (var obj in searcher.Get())
            {
                return obj["ProcessorId"]?.ToString() ?? string.Empty;
            }
        }
        catch
        {
            // 忽略异常
        }

        return string.Empty;
    }

    private string GetWindowsMotherboardSerial()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
            foreach (var obj in searcher.Get())
            {
                return obj["SerialNumber"]?.ToString() ?? string.Empty;
            }
        }
        catch
        {
            // 忽略异常
        }

        return string.Empty;
    }

    private string GetWindowsSystemDiskSerial()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT VolumeSerialNumber FROM Win32_LogicalDisk WHERE DeviceID='C:'");
            foreach (var obj in searcher.Get())
            {
                return obj["VolumeSerialNumber"]?.ToString() ?? string.Empty;
            }
        }
        catch
        {
            // 忽略异常
        }

        return string.Empty;
    }

    #endregion

    #region Linux实现

    private string GetLinuxCpuId()
    {
        try
        {
            // 尝试读取CPU信息
            if (File.Exists("/proc/cpuinfo"))
            {
                var lines = File.ReadAllLines("/proc/cpuinfo");
                foreach (var line in lines)
                {
                    if (line.StartsWith("Serial"))
                    {
                        var parts = line.Split(':');
                        if (parts.Length > 1)
                        {
                            return parts[1].Trim();
                        }
                    }
                }
            }
        }
        catch
        {
            // 忽略异常
        }

        return string.Empty;
    }

    private string GetLinuxMotherboardSerial()
    {
        try
        {
            // 尝试读取主板序列号
            if (File.Exists("/sys/class/dmi/id/board_serial"))
            {
                return File.ReadAllText("/sys/class/dmi/id/board_serial").Trim();
            }
        }
        catch
        {
            // 忽略异常
        }

        return string.Empty;
    }

    private string GetLinuxSystemDiskSerial()
    {
        try
        {
            // 尝试读取根分区的UUID
            if (Directory.Exists("/dev/disk/by-uuid"))
            {
                var links = Directory.GetFiles("/dev/disk/by-uuid");
                if (links.Length > 0)
                {
                    // 返回第一个UUID作为示例
                    return Path.GetFileName(links[0]);
                }
            }
        }
        catch
        {
            // 忽略异常
        }

        return string.Empty;
    }

    #endregion
}