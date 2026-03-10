import { createBrowserRouter } from 'react-router-dom'
import Layout from '../components/Layout'
import Home from '../pages/Home'
import WechatAuth from '../pages/WechatAuth'
import TestPage from '../pages/TestPage'

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
        path: 'test/:moduleId',
        element: <TestPage />
      }
    ]
  }
], {
  basename: '/WechatTest'
})

export default router
