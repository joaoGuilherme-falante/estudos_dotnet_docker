import { useState } from 'react';
import { api } from '../api.js';

export default function Login({ onLogin }) {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  async function submit(event) {
    event.preventDefault();
    setError('');
    setLoading(true);
    try {
      // POST /api/auth/login → AuthController.Login → AuthService.LoginAsync
      const result = await api('/auth/login', { method: 'POST', body: { email, password } });
      onLogin(result.accessToken);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  return (
    <form className="card login" onSubmit={submit}>
      <h2>Entrar</h2>
      <p className="muted">
        Use o admin definido no <code>.env</code> (<code>BOOTSTRAP_ADMIN_EMAIL</code> /{' '}
        <code>BOOTSTRAP_ADMIN_PASSWORD</code>).
      </p>
      <label>
        E-mail
        <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required autoFocus />
      </label>
      <label>
        Senha
        <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
      </label>
      {error && <p className="error">{error}</p>}
      <button type="submit" disabled={loading}>{loading ? 'Entrando…' : 'Entrar'}</button>
    </form>
  );
}
