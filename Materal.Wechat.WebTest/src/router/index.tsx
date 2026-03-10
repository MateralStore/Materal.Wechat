import { createBrowserRouter } from 'react-router-dom'
import Layout from '../components/Layout'
import Home from '../pages/Home'
import WechatAuth from '../pages/WechatAuth'
import WechatPay from '../pages/WechatPay'
import Settings from '../pages/Settings'

const router = createBrowserRouter([
  {
    path: '/',
    element: <Layout />,
    children: [
      {
        index: true,
        element: <Home />
      },
      {
        path: 'test/wechat-auth',
        element: <WechatAuth />
      },
      {
        path: 'test/wechat-pay',
        element: <WechatPay />
      },
      {
        path: 'settings',
        element: <Settings />
      }
    ]
  }
], {
  basename: '/WechatTest'
})

export default router
