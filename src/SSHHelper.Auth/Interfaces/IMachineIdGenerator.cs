namespace SSHHelper.Auth.Interfaces;

/// <summary>
/// 机器码生成接口
/// </summary>
public interface IMachineIdGenerator
{
    /// <summary>
    /// 生成唯一机器码
    /// </summary>
    string Generate();
}