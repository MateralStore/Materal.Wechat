namespace Materal.MergeBlock.Wechat.DTO;

/// <summary>
/// 用户AccessTokenDTO
/// OpenID在这
/// </summary>
public class UserAccessTokenDTO
{
    /// <summary>
    /// 网页授权接口调用凭证
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;
    /// <summary>
    /// 凭证有效期（单位：秒）
    /// </summary>
    public int ExpiresIn { get; set; }
    /// <summary>
    /// 用户刷新 AccessToken
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
    /// <summary>
    /// 用户唯一标识
    /// </summary>
    public string OpenID { get; set; } = string.Empty;
    /// <summary>
    /// 用户授权的作用域，使用逗号分隔
    /// </summary>
    public string Scope { get; set; } = string.Empty;
    /// <summary>
    /// 用户全局标识
    /// </summary>
    public string? UnionID { get; set; } = string.Empty;
}
