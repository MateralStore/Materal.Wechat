import { useState, useEffect } from 'react'
import axios from 'axios'
import { isWechatEnv, invokeWechatPay } from '../utils/wechat-jsapi'
import type { JsapiPayParameters } from '../utils/wechat-jsapi'
import { getApiBaseUrl } from '../utils/api-config'
import './WechatPay.css'

const STORAGE_KEY_AUTH = 'wechat_auth'

// 从 localStorage 获取已保存的 OpenID
const getStoredOpenId = (): string => {
  const stored = localStorage.getItem(STORAGE_KEY_AUTH)
  if (stored) {
    const auth = JSON.parse(stored)
    if (auth.ResultType === 0 && auth.Data?.OpenID) {
      return auth.Data.OpenID
    }
  }
  return ''
}

interface CreateOrderResponse {
  ResultType: number
  Message?: string
  Data: {
    PrepayID: string
    AppID: string
    TimeStamp: string
    NonceStr: string
    Package: string
    SignType: string
    PaySign: string
  } | null
}

interface QueryOrderResponse {
  ResultType: number
  Message?: string
  Data: {
    TransactionId: string
    OutTradeNo: string
    TradeState: string
    TradeStateDesc: string
    BankType: string
    Attach: string
    SuccessTime: string
    OpenID: string
    Amount: {
      Total: number
      PayerTotal: number
      Currency: string
      PayerCurrency: string
    }
  } | null
}

const WechatPay: React.FC = () => {
  const [outTradeNo, setOutTradeNo] = useState('')
  const [totalAmount, setTotalAmount] = useState('1')
  const [description, setDescription] = useState('测试商品描述')
  const [openId, setOpenId] = useState(() => getStoredOpenId())
  const [attach, setAttach] = useState('')

  const [queryType, setQueryType] = useState<'outTradeNo' | 'transactionId'>('outTradeNo')
  const [queryValue, setQueryValue] = useState('')

  const [loading, setLoading] = useState(false)
  const [queryLoading, setQueryLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)
  const [queryResult, setQueryResult] = useState<QueryOrderResponse['Data'] | null>(null)

  const inWechatEnv = isWechatEnv()

  // 生成商户订单号
  const generateOutTradeNo = () => {
    const timestamp = Date.now()
    const random = Math.floor(Math.random() * 10000).toString().padStart(4, '0')
    const tradeNo = `ORDER${timestamp}${random}`
    setOutTradeNo(tradeNo)
    setAttach(tradeNo)
  }

  // 初始化生成默认商户订单号
  useEffect(() => {
    generateOutTradeNo()
  }, [])

  // 同步商户订单号到查询框
  useEffect(() => {
    if (queryType === 'outTradeNo' && outTradeNo) {
      setQueryValue(outTradeNo)
    }
  }, [outTradeNo, queryType])

  const validateForm = (): boolean => {
    if (!outTradeNo.trim()) {
      setError('请输入商户订单号')
      return false
    }
    if (!totalAmount.trim()) {
      setError('请输入金额')
      return false
    }
    const amount = parseInt(totalAmount, 10)
    if (isNaN(amount) || amount <= 0) {
      setError('金额必须为正整数（单位：分）')
      return false
    }
    if (!description.trim()) {
      setError('请输入商品描述')
      return false
    }
    if (!openId.trim()) {
      setError('请输入用户OpenID')
      return false
    }
    return true
  }

  const handleCreateOrder = async () => {
    setError(null)
    setSuccess(null)

    if (!validateForm()) {
      return
    }

    if (!inWechatEnv) {
      setError('请在微信环境中使用支付功能')
      return
    }

    setLoading(true)

    try {
      const response = await axios.post<CreateOrderResponse>(
        `${getApiBaseUrl()}/WechatAPI/WechatPay/CreateJsapiOrder`,
        {
          OutTradeNo: outTradeNo.trim(),
          TotalAmount: parseInt(totalAmount, 10),
          Description: description.trim(),
          OpenID: openId.trim(),
          Attach: attach.trim() || undefined
        }
      )

      const result = response.data

      if (result.ResultType !== 0 || !result.Data) {
        setError(result.Message || '创建订单失败')
        return
      }

      // 调起微信支付
      const payParams: JsapiPayParameters = {
        appId: result.Data.AppID,
        timeStamp: result.Data.TimeStamp,
        nonceStr: result.Data.NonceStr,
        package: result.Data.Package,
        signType: result.Data.SignType,
        paySign: result.Data.PaySign
      }

      try {
        await invokeWechatPay(payParams)
        setSuccess('支付成功')
      } catch (payError) {
        setError(payError instanceof Error ? payError.message : '支付失败')
      }
    } catch (err) {
      setError(axios.isAxiosError(err) ? err.message : '请求失败')
    } finally {
      setLoading(false)
    }
  }

  const handleQueryOrder = async () => {
    setError(null)
    setSuccess(null)
    setQueryResult(null)

    if (!queryValue.trim()) {
      setError('请输入查询条件')
      return
    }

    setQueryLoading(true)

    try {
      let url: string
      if (queryType === 'outTradeNo') {
        url = `${getApiBaseUrl()}/WechatAPI/WechatPay/QueryOrderByOutTradeNo?outTradeNo=${encodeURIComponent(queryValue.trim())}`
      } else {
        url = `${getApiBaseUrl()}/WechatAPI/WechatPay/QueryOrderByTransactionId?transactionId=${encodeURIComponent(queryValue.trim())}`
      }

      const response = await axios.get<QueryOrderResponse>(url)
      const result = response.data

      if (result.ResultType !== 0 || !result.Data) {
        setError(result.Message || '查询订单失败')
        return
      }

      setQueryResult(result.Data)
    } catch (err) {
      setError(axios.isAxiosError(err) ? err.message : '请求失败')
    } finally {
      setQueryLoading(false)
    }
  }

  const getTradeStateText = (state: string): string => {
    const stateMap: Record<string, string> = {
      SUCCESS: '支付成功',
      REFUND: '转入退款',
      NOTPAY: '未支付',
      CLOSED: '已关闭',
      REVOKED: '已撤销（付款码支付）',
      USERPAYING: '用户支付中',
      PAYERROR: '支付失败'
    }
    return stateMap[state] || state
  }

  const getTradeStateClass = (state: string): string => {
    if (state === 'SUCCESS') return 'success'
    if (state === 'NOTPAY' || state === 'USERPAYING') return 'pending'
    return 'fail'
  }

  return (
    <div className="wechat-pay">
      {!inWechatEnv && (
        <div className="env-warning">
          当前不在微信环境中，支付功能不可用
        </div>
      )}

      <div className="section-title">创建支付订单</div>

      <div className="test-section">
        <label className="input-label">
          商户订单号 <span className="required">*</span>
        </label>
        <div className="input-with-btn">
          <input
            type="text"
            className="input-field"
            value={outTradeNo}
            onChange={(e) => setOutTradeNo(e.target.value)}
            placeholder="请输入商户订单号"
          />
          <button className="generate-btn" onClick={generateOutTradeNo}>
            生成
          </button>
        </div>
      </div>

      <div className="test-section">
        <label className="input-label">
          金额（分） <span className="required">*</span>
        </label>
        <input
          type="number"
          className="input-field"
          value={totalAmount}
          onChange={(e) => setTotalAmount(e.target.value)}
          placeholder="请输入金额，单位：分（1元=100分）"
          min="1"
        />
        {totalAmount && (
          <small className="input-hint">
            约 {(parseInt(totalAmount, 10) / 100).toFixed(2)} 元
          </small>
        )}
      </div>

      <div className="test-section">
        <label className="input-label">
          商品描述 <span className="required">*</span>
        </label>
        <input
          type="text"
          className="input-field"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          placeholder="请输入商品描述"
          maxLength={127}
        />
      </div>

      <div className="test-section">
        <label className="input-label">
          用户OpenID <span className="required">*</span>
        </label>
        <input
          type="text"
          className="input-field"
          value={openId}
          onChange={(e) => setOpenId(e.target.value)}
          placeholder="请输入用户OpenID"
        />
      </div>

      <div className="test-section">
        <label className="input-label">附加数据</label>
        <input
          type="text"
          className="input-field"
          value={attach}
          onChange={(e) => setAttach(e.target.value)}
          placeholder="可选，附加数据，在回调中返回"
        />
      </div>

      <button
        className="test-btn primary"
        onClick={handleCreateOrder}
        disabled={loading || !inWechatEnv}
      >
        {loading ? '处理中...' : '创建订单并支付'}
      </button>

      <div className="divider">
        <span>订单查询</span>
      </div>

      <div className="test-section">
        <label className="input-label">查询方式</label>
        <div className="query-type-options">
          <label className={`query-option ${queryType === 'outTradeNo' ? 'active' : ''}`}>
            <input
              type="radio"
              name="queryType"
              value="outTradeNo"
              checked={queryType === 'outTradeNo'}
              onChange={() => setQueryType('outTradeNo')}
            />
            <span>商户订单号</span>
          </label>
          <label className={`query-option ${queryType === 'transactionId' ? 'active' : ''}`}>
            <input
              type="radio"
              name="queryType"
              value="transactionId"
              checked={queryType === 'transactionId'}
              onChange={() => setQueryType('transactionId')}
            />
            <span>微信订单号</span>
          </label>
        </div>
      </div>

      <div className="test-section">
        <label className="input-label">
          {queryType === 'outTradeNo' ? '商户订单号' : '微信订单号'}
        </label>
        <input
          type="text"
          className="input-field"
          value={queryValue}
          onChange={(e) => setQueryValue(e.target.value)}
          placeholder={queryType === 'outTradeNo' ? '请输入商户订单号' : '请输入微信订单号'}
        />
      </div>

      <button
        className="test-btn secondary"
        onClick={handleQueryOrder}
        disabled={queryLoading}
      >
        {queryLoading ? '查询中...' : '查询订单'}
      </button>

      {error && (
        <div className="result-box error">
          <div className="result-title">错误</div>
          <div className="result-content">{error}</div>
        </div>
      )}

      {success && (
        <div className="result-box success">
          <div className="result-title">成功</div>
          <div className="result-content">{success}</div>
        </div>
      )}

      {queryResult && (
        <div className={`result-box ${getTradeStateClass(queryResult.TradeState)}`}>
          <div className="result-title">
            订单查询结果
            <span className={`trade-state ${getTradeStateClass(queryResult.TradeState)}`}>
              {getTradeStateText(queryResult.TradeState)}
            </span>
          </div>
          <div className="result-content">
            <div className="result-item">
              <span className="item-label">微信订单号:</span>
              <span className="item-value">{queryResult.TransactionId}</span>
            </div>
            <div className="result-item">
              <span className="item-label">商户订单号:</span>
              <span className="item-value">{queryResult.OutTradeNo}</span>
            </div>
            <div className="result-item">
              <span className="item-label">交易状态:</span>
              <span className="item-value">{queryResult.TradeStateDesc}</span>
            </div>
            <div className="result-item">
              <span className="item-label">订单金额:</span>
              <span className="item-value">
                {queryResult.Amount.Total / 100} {queryResult.Amount.Currency}
              </span>
            </div>
            {queryResult.SuccessTime && (
              <div className="result-item">
                <span className="item-label">支付完成时间:</span>
                <span className="item-value">{queryResult.SuccessTime}</span>
              </div>
            )}
            {queryResult.OpenID && (
              <div className="result-item">
                <span className="item-label">付款用户OpenID:</span>
                <span className="item-value">{queryResult.OpenID}</span>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  )
}

export default WechatPay
