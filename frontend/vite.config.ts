import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwind from '@tailwindcss/vite'

export default defineConfig({
  plugins: [react(), tailwind()],
  server: {
    port: 5042, // frontend runs here
    proxy: {
      '/api': {
        target: 'http://localhost:5041', // backend runs here
        changeOrigin: true,
        secure: false // allow self-signed HTTPS certs
      }
    }
  }
})
