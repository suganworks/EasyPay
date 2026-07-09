import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    host: '0.0.0.0',  // needed so Docker can expose the port
    port: 5173,
    proxy: {
      '/api': {
        target: process.env.VITE_API_URL || 'https://localhost:7245',
        changeOrigin: true,
        secure: false, // allow self-signed certs on localhost
      },
    },
  },
})

