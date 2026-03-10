import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'
import { execSync } from 'child_process'
import path from 'path'
import fs from 'fs'

const zipPlugin = () => ({
  name: 'zip-plugin',
  buildStart() {
    const distPath = path.resolve('dist')
    if (fs.existsSync(distPath)) {
      fs.rmSync(distPath, { recursive: true })
      console.log('\nCleared dist folder')
    }
  },
  closeBundle() {
    const timestamp = new Date().toISOString().replace(/[-:T]/g, '').slice(0, 14)
    const zipName = `WechatTest${timestamp}.zip`
    const distPath = path.resolve('dist')

    execSync(`powershell -Command "cd '${distPath}'; Compress-Archive -Path 'WechatTest' -DestinationPath '${zipName}'"`, {
      stdio: 'inherit'
    })
    console.log(`\nZipped to dist/${zipName}`)
  }
})

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd())

  return {
    base: '/WechatTest/',
    plugins: [react(), zipPlugin()],
    build: {
      outDir: 'dist/WechatTest'
    },
    server: {
      proxy: {
        '/WechatAPI': {
          target: env.VITE_API_TARGET,
          changeOrigin: true
        }
      }
    }
  }
})
