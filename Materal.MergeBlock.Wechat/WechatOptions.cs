namespace Materal.MergeBlock.Wechat;

/// <summary>
/// 微信配置
/// </summary>
public class WechatOptions
{
    /// <summary>
    /// Key
    /// </summary>
    public string Key { get; set; } = string.Empty;
    /// <summary>
    /// AppID
    /// </summary>
    public string AppID { get; set; } = string.Empty;
    /// <summary>
    /// AppSecret
    /// </summary>
    public string AppSecret { get; set; } = string.Empty;
    /// <summary>
    /// 微信商户号
    /// </summary>
    public string? MchId { get; set; }
    /// <summary>
    /// 微信商户V3 API密钥
    /// </summary>
    public string? ApiV3Key { get; set; }
    /// <summary>
    /// 商户证书序列号
    /// </summary>
    public string? CertificateSerialNumber { get; set; }
    /// <summary>
    /// 商户私钥文件路径（PEM格式）
    /// </summary>
    public string? PrivateKeyPath { get; set; }
    /// <summary>
    /// 支付回调通知地址
    /// </summary>
    public string? NotifyUrl { get; set; }
    /// <summary>
    /// 微信支付平台公钥ID（新商户使用，2024年10月后注册）
    /// </summary>
    public string? PlatformPublicKeyId { get; set; }
    /// <summary>
    /// 微信支付平台公钥文件路径（新商户使用，2024年10月后注册）
    /// </summary>
    public string? PlatformPublicKeyPath { get; set; }
}
