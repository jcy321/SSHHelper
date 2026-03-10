namespace SSHHelper.Core.Helpers;

/// <summary>
/// 安全相关辅助方法
/// </summary>
public static class SecurityHelper
{
    /// <summary>
    /// 清除敏感数据
    /// </summary>
    public static void ClearSensitiveData(ref string? data)
    {
        if (!string.IsNullOrEmpty(data))
        {
            var chars = data.ToCharArray();
            Array.Clear(chars, 0, chars.Length);
            data = null;
        }
    }
}