import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';

// O proxy repassa tudo que começa com /api para a API .NET.
// Assim o navegador acha que front e API estão no mesmo endereço (sem CORS).
// Padrão: API no Docker (porta 8080). Para `dotnet run`, use API_URL=http://localhost:5121.
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');
  return {
    plugins: [react()],
    server: {
      port: 5173,
      proxy: {
        '/api': { target: env.API_URL || 'http://localhost:8080', changeOrigin: true },
        '/health': { target: env.API_URL || 'http://localhost:8080', changeOrigin: true },
      },
    },
  };
});
