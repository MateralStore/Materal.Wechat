using SKIT.FlurlHttpClient.Wechat.Api;

namespace Materal.MergeBlock.Wechat;

/// <summary>
/// 微信模块异常
/// </summary>
public class WechatModuleException : MergeBlockModuleException
{
    /// <summary>
    /// 构造方法
    /// </summary>
    public WechatModuleException()
    {
    }

    /// <summary>
    /// 构造方法
    /// </summary>
    /// <param name="message"></param>
    public WechatModuleException(string message) : base(message)
    {
    }

    /// <summary>
    /// 构造方法
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public WechatModuleException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// 构造方法
    /// </summary>
    /// <param name="response"></param>
    public WechatModuleException(WechatApiResponse response) : this($"{response.ErrorCode}:{response.ErrorMessage}")
    {

    }
}
