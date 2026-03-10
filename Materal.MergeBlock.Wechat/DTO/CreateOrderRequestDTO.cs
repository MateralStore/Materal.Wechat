using System.ComponentModel.DataAnnotations;

namespace Materal.MergeBlock.Wechat.DTO;

/// <summary>
/// 创建订单请求DTO
/// </summary>
public class CreateOrderRequestDTO
{
    /// <summary>
    /// 商户订单号（必须唯一）
    /// </summary>
    [Required(ErrorMessage = "商户订单号不能为空")]
    public string OutTradeNo { get; set; } = string.Empty;

    /// <summary>
    /// 订单金额（单位：分）
    /// </summary>
    [Required(ErrorMessage = "订单金额不能为空")]
    [Range(1, int.MaxValue, ErrorMessage = "订单金额必须大于0")]
    public int TotalAmount { get; set; }

    /// <summary>
    /// 商品描述
    /// </summary>
    [Required(ErrorMessage = "商品描述不能为空")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 用户标识（OpenId）
    /// </summary>
    [Required(ErrorMessage = "OpenID不能为空")]
    public string OpenID { get; set; } = string.Empty;

    /// <summary>
    /// 附加数据（可选）
    /// </summary>
    public string? Attach { get; set; }
}
