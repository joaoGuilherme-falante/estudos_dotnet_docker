// Toda conversa com a API .NET passa por aqui.

const TOKEN_KEY = 'taskflow.token';
const listeners = new Set();

export const getToken = () => localStorage.getItem(TOKEN_KEY);

export function setToken(token) {
  if (token) localStorage.setItem(TOKEN_KEY, token);
  else localStorage.removeItem(TOKEN_KEY);
}

// Permite que o painel "Requisições" mostre cada chamada HTTP feita.
export function onRequest(listener) {
  listeners.add(listener);
  return () => listeners.delete(listener);
}

export class ApiError extends Error {
  constructor(status, message) {
    super(message);
    this.status = status;
  }
}

export async function api(path, { method = 'GET', body } = {}) {
  const headers = {};
  if (body !== undefined) headers['Content-Type'] = 'application/json';
  const token = getToken();
  if (token) headers.Authorization = `Bearer ${token}`;

  const started = performance.now();
  const response = await fetch(`/api${path}`, {
    method,
    headers,
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });

  const text = await response.text();
  let data = null;
  try {
    data = text ? JSON.parse(text) : null;
  } catch {
    data = text;
  }

  const entry = {
    id: crypto.randomUUID(),
    at: new Date(),
    method,
    path: `/api${path}`,
    status: response.status,
    ms: Math.round(performance.now() - started),
    request: hidePasswords(body),
    response: data,
  };
  listeners.forEach((listener) => listener(entry));

  if (response.status === 401 && token) {
    window.dispatchEvent(new Event('taskflow:unauthorized'));
  }
  if (!response.ok) throw new ApiError(response.status, describeError(response.status, data));
  return data;
}

function hidePasswords(body) {
  if (!body || typeof body !== 'object') return body;
  return Object.fromEntries(
    Object.entries(body).map(([key, value]) => [key, /password/i.test(key) && value ? '••••••' : value]),
  );
}

// Converte o ProblemDetails do ApiExceptionHandler (ou os erros de validação do [ApiController])
// em uma mensagem legível.
function describeError(status, data) {
  if (data && typeof data === 'object') {
    if (data.errors) {
      const messages = Object.entries(data.errors).flatMap(([field, list]) =>
        list.map((message) => `${field}: ${message}`),
      );
      return `${status} – ${messages.join(' | ')}`;
    }
    if (data.detail || data.title) return `${status} – ${data.detail ?? data.title}`;
  }
  const fallback = {
    401: 'Não autenticado (token ausente, inválido ou expirado)',
    403: 'Sem permissão para esta ação',
    404: 'Não encontrado',
  };
  return `${status} – ${fallback[status] ?? 'Erro inesperado'}`;
}

// O JWT tem 3 partes separadas por "."; a do meio (payload) é JSON em base64url.
// Ler o payload no front serve só para exibir dados — quem valida a assinatura é a API.
export function decodeToken(token) {
  try {
    const payload = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
    const json = decodeURIComponent(
      atob(payload)
        .split('')
        .map((c) => '%' + c.charCodeAt(0).toString(16).padStart(2, '0'))
        .join(''),
    );
    const claims = JSON.parse(json);
    return {
      id: Number(claims.nameid ?? claims.sub ?? claims['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']),
      name: claims.unique_name ?? claims.name ?? claims['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'],
      email: claims.email ?? claims['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'],
      role: claims.role,
      expiresAt: new Date(claims.exp * 1000),
      claims,
    };
  } catch {
    return null;
  }
}

export const TASK_STATUSES = ['Backlog', 'Todo', 'InProgress', 'Review', 'Done'];
export const PRIORITIES = ['Low', 'Medium', 'High', 'Critical'];
export const ROLES = ['Admin', 'Manager', 'Developer'];

export const canManage = (user) => user?.role === 'Admin' || user?.role === 'Manager';
