namespace Materal.MergeBlock.Wechat;

/// <summary>
/// 微信模块
/// </summary>
public class WechatModule() : MergeBlockModule("微信模块")
{
    /// <inheritdoc/>
    public override void OnConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMemoryCache();
    }
}
