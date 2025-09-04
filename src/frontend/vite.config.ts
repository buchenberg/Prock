import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  server: {
    proxy: {
      "/prock": "http://localhost:5001",
      "/swagger": "http://localhost:5001",
    },
    watch: {
      usePolling: true
    },
    port: 8080
  },
  plugins: [react()],
})
