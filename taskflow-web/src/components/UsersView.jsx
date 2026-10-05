import { useEffect, useState } from 'react';
import { api, ROLES } from '../api.js';

// Tudo aqui usa UsersController, que tem [Authorize(Roles = "Admin")].
export default function UsersView({ user }) {
  const [users, setUsers] = useState([]);
  const [editingId, setEditingId] = useState(null);
  const [error, setError] = useState('');

  useEffect(() => {
    api('/users').then(setUsers).catch((err) => setError(err.message));
  }, []);

  async function remove(target) {
    if (!confirm(`Excluir ${target.name}?`)) return;
    setError('');
    try {
      await api(`/users/${target.id}`, { method: 'DELETE' });
      setUsers((list) => list.filter((u) => u.id !== target.id));
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <div className="users">
      <h2>Usuários</h2>
      {error && <p className="error">{error}</p>}
      <table>
        <thead>
          <tr>
            <th>Id</th>
            <th>Nome</th>
            <th>E-mail</th>
            <th>Role</th>
            <th />
          </tr>
        </thead>
        <tbody>
          {users.map((u) =>
            editingId === u.id ? (
              <EditUserRow
                key={u.id}
                target={u}
                onSaved={(updated) => {
                  setUsers((list) => list.map((x) => (x.id === updated.id ? updated : x)));
                  setEditingId(null);
                }}
                onCancel={() => setEditingId(null)}
              />
            ) : (
              <tr key={u.id}>
                <td>{u.id}</td>
                <td>{u.name} {u.id === user.id && <span className="muted">(você)</span>}</td>
                <td>{u.email}</td>
                <td><span className={`badge role-${u.role}`}>{u.role}</span></td>
                <td className="row-actions">
                  <button className="secondary" onClick={() => setEditingId(u.id)}>Editar</button>
                  <button className="danger" onClick={() => remove(u)}>Excluir</button>
                </td>
              </tr>
            ),
          )}
        </tbody>
      </table>

      <CreateUserForm onCreated={(created) => setUsers((list) => [...list, created])} />
      <p className="muted small">
        Dica: crie um Developer e um Manager e faça login com eles para testar as permissões (403).
      </p>
    </div>
  );
}

function EditUserRow({ target, onSaved, onCancel }) {
  const [form, setForm] = useState({ name: target.name, email: target.email, role: target.role, newPassword: '' });
  const [error, setError] = useState('');
  const set = (field) => (e) => setForm((f) => ({ ...f, [field]: e.target.value }));

  async function save() {
    setError('');
    try {
      onSaved(await api(`/users/${target.id}`, {
        method: 'PUT',
        body: { ...form, newPassword: form.newPassword || null },
      }));
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <tr>
      <td>{target.id}</td>
      <td><input value={form.name} onChange={set('name')} /></td>
      <td><input value={form.email} onChange={set('email')} /></td>
      <td>
        <select value={form.role} onChange={set('role')}>
          {ROLES.map((r) => <option key={r}>{r}</option>)}
        </select>
        <input type="password" placeholder="Nova senha (opcional)" value={form.newPassword} onChange={set('newPassword')} />
      </td>
      <td className="row-actions">
        <button onClick={save}>Salvar</button>
        <button className="secondary" onClick={onCancel}>Cancelar</button>
        {error && <p className="error">{error}</p>}
      </td>
    </tr>
  );
}

function CreateUserForm({ onCreated }) {
  const empty = { name: '', email: '', password: '', role: 'Developer' };
  const [form, setForm] = useState(empty);
  const [error, setError] = useState('');
  const set = (field) => (e) => setForm((f) => ({ ...f, [field]: e.target.value }));

  async function submit(event) {
    event.preventDefault();
    setError('');
    try {
      onCreated(await api('/users', { method: 'POST', body: form }));
      setForm(empty);
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <form className="card" onSubmit={submit}>
      <h3>Novo usuário</h3>
      <div className="grid">
        <input placeholder="Nome" value={form.name} onChange={set('name')} required />
        <input type="email" placeholder="E-mail" value={form.email} onChange={set('email')} required />
        <input
          type="password"
          placeholder="Senha (mín. 12 caracteres)"
          value={form.password}
          onChange={set('password')}
          required
        />
        <select value={form.role} onChange={set('role')}>
          {ROLES.map((r) => <option key={r}>{r}</option>)}
        </select>
      </div>
      {error && <p className="error">{error}</p>}
      <button type="submit">Criar usuário</button>
    </form>
  );
}
