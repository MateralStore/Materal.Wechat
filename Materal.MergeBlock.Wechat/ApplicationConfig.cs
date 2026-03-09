namespace Materal.MergeBlock.Wechat;

/// <summary>
/// 应用程序配置
/// </summary>
[Options("Wechat")]
public class ApplicationConfig : IOptions
{
    /// <summary>
    /// 微信配置
    /// </summary>
    public List<WechatOptions> WechatOptions { get; set; } = [];
}
