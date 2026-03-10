namespace Materal.MergeBlock.Wechat.DTO;

/// <summary>
/// 订单金额DTO
/// </summary>
public class OrderAmountDTO
{
    /// <summary>
    /// 订单总金额（单位：分）
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// 用户支付金额（单位：分）
    /// </summary>
    public int PayerTotal { get; set; }

    /// <summary>
    /// 货币类型
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// 用户支付币种
    /// </summary>
    public string? PayerCurrency { get; set; }
}
