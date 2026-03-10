import { Outlet, useNavigate, useLocation } from 'react-router-dom'
import './Layout.css'

const Layout: React.FC = () => {
  const navigate = useNavigate()
  const location = useLocation()
  const isHome = location.pathname === '/'

  const getTitle = () => {
    if (isHome) return '微信接口测试'
    const path = location.pathname.replace('/test/', '')
    const titles: Record<string, string> = {
      'wechat-auth': '微信授权测试',
      'jsapi-signature': 'JSAPI签名测试',
      'user-info': '用户信息测试',
      'template-message': '模板消息测试'
    }
    return titles[path] || '测试页面'
  }

  return (
    <div className="layout">
      <header className="layout-header">
        {!isHome && (
          <button className="back-btn" onClick={() => navigate('/')}>‹</button>
        )}
        <h1>{getTitle()}</h1>
      </header>
      <main className="layout-main">
        <Outlet />
      </main>
    </div>
  )
}

export default Layout
