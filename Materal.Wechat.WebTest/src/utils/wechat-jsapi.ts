/**
 * 微信 JSAPI 支付参数接口
 */
export interface JsapiPayParameters {
  appId: string
  timeStamp: string
  nonceStr: string
  package: string
  signType: string
  paySign: string
}

/**
 * 检测是否在微信环境中
 */
export function isWechatEnv(): boolean {
  return /micromessenger/i.test(navigator.userAgent)
}

/**
 * 检测 WeixinJSBridge 是否就绪
 */
function isWeixinJSBridgeReady(): boolean {
  return typeof (window as any).WeixinJSBridge !== 'undefined'
}

/**
 * 等待 WeixinJSBridge 就绪
 */
function waitForWeixinJSBridge(timeout = 5000): Promise<void> {
  return new Promise((resolve, reject) => {
    if (isWeixinJSBridgeReady()) {
      resolve()
      return
    }

    const timer = setTimeout(() => {
      reject(new Error('WeixinJSBridge 加载超时'))
    }, timeout)

    ;(document as any).addEventListener('WeixinJSBridgeReady', () => {
      clearTimeout(timer)
      resolve()
    })
  })
}

/**
 * 调起微信支付
 * 优先使用 WeixinJSBridge（无需 config 配置），失败时降级到 wx.chooseWXPay
 */
export async function invokeWechatPay(params: JsapiPayParameters): Promise<void> {
  // 优先使用 WeixinJSBridge 方式（无需 config）
  if (isWeixinJSBridgeReady()) {
    return invokeWechatPayViaBridge(params)
  }

  // 等待 WeixinJSBridge 就绪
  try {
    await waitForWeixinJSBridge()
    return invokeWechatPayViaBridge(params)
  } catch {
    // WeixinJSBridge 不可用，尝试 wx.chooseWXPay 方式
    return invokeWechatPayViaSDK(params)
  }
}

/**
 * 通过 WeixinJSBridge 调起支付
 */
function invokeWechatPayViaBridge(params: JsapiPayParameters): Promise<void> {
  return new Promise((resolve, reject) => {
    const WeixinJSBridge = (window as any).WeixinJSBridge

    WeixinJSBridge.invoke(
      'getBrandWCPayRequest',
      {
        appId: params.appId,
        timeStamp: params.timeStamp,
        nonceStr: params.nonceStr,
        package: params.package,
        signType: params.signType,
        paySign: params.paySign
      },
      (res: { err_msg: string }) => {
        if (res.err_msg === 'get_brand_wcpay_request:ok') {
          resolve()
        } else if (res.err_msg === 'get_brand_wcpay_request:cancel') {
          reject(new Error('用户取消支付'))
        } else {
          reject(new Error(res.err_msg || '支付失败'))
        }
      }
    )
  })
}

/**
 * 通过 wx.chooseWXPay 调起支付（需要先 config 配置）
 */
function invokeWechatPayViaSDK(params: JsapiPayParameters): Promise<void> {
  return new Promise((resolve, reject) => {
    const wx = (window as any).wx

    if (typeof wx === 'undefined' || !wx.chooseWXPay) {
      reject(new Error('微信JS-SDK未加载或未配置'))
      return
    }

    wx.chooseWXPay({
      timestamp: params.timeStamp,
      nonceStr: params.nonceStr,
      package: params.package,
      signType: params.signType,
      paySign: params.paySign,
      success: () => resolve(),
      cancel: () => reject(new Error('用户取消支付')),
      fail: (err: any) => reject(new Error(err.errMsg || '支付失败'))
    })
  })
}
