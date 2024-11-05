import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  server: {
    proxy: {
      "/drunken-master": "http://localhost:5001",
    }
  },
  plugins: [react()],
})