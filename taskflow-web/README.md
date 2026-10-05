# TaskFlow Web

Frontend React (Vite) simples para testar a API TaskFlow.

```bash
cd taskflow-web
npm install
npm run dev        # abre em http://localhost:5173
```

O Vite repassa `/api/*` para a API (proxy em `vite.config.js`):

- API no Docker (`docker compose up`): padrão, `http://localhost:8080`.
- API com `dotnet run`: crie `taskflow-web/.env.local` com `API_URL=http://localhost:5121`.

Faça login com o `BOOTSTRAP_ADMIN_EMAIL` / `BOOTSTRAP_ADMIN_PASSWORD` do `.env`.
O painel "Requisições" mostra cada chamada HTTP com o body enviado e a resposta.
