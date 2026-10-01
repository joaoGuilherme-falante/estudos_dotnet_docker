# TaskFlow

API de gerenciamento de projetos e tarefas com ASP.NET Core, EF Core, PostgreSQL e JWT.

## Executar com Docker Compose

1. Copie `.env.example` para `.env` e substitua os valores de desenvolvimento.
2. Execute `docker compose up --build`.
3. A API fica em `http://localhost:8080`; health check em `/health` e OpenAPI JSON em `/openapi/v1.json`.
4. O primeiro Admin é criado no primeiro start quando `BOOTSTRAP_ADMIN_*` está configurado. A API aplica migrations na inicialização.

Não use os valores de exemplo em produção. Mantenha `.env` fora do Git e forneça segredos de produção por um gerenciador de segredos.

## Executar localmente

Com PostgreSQL disponível, defina `ConnectionStrings__TaskFlowDb`, `Jwt__Key` (mínimo 32 bytes), `BootstrapAdmin__Email` e `BootstrapAdmin__Password`, então execute:

```powershell
dotnet run --project TaskFlow.Api
```

## Endpoints principais

- `POST /api/auth/login` — autenticação e emissão de JWT.
- `/api/users` — CRUD de usuários, apenas Admin.
- `/api/projects` — leitura para autenticados; criação/edição/finalização para Admin ou Manager.
- `/api/tasks` — CRUD autenticado; alteração de status respeita responsável/Manager/Admin e o fluxo.
- `/api/tasks/{id}/comments` — listar/adicionar comentários autenticado.
- `GET /health` — verifica disponibilidade da API e conexão ao banco.

Enums são trafegados como strings. A coleção de chamadas de exemplo está em `TaskFlow.Api/TaskFlow.Api.http`.

## Testes

```powershell
dotnet test TaskFlow.Tests\TaskFlow.Tests.csproj
```

Os testes incluem regras de domínio e uma verificação HTTP de autenticação/autorização usando banco EF Core InMemory. A integração real com PostgreSQL deve ser validada com `docker compose up --build` quando o Docker estiver disponível.
