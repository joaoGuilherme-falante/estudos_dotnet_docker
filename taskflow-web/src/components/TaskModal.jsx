import { useEffect, useState } from 'react';
import { api, canManage, PRIORITIES } from '../api.js';
import AssigneeField from './AssigneeField.jsx';
import { toIsoDate } from './ProjectsView.jsx';

export default function TaskModal({ task, user, users, onClose, onSaved }) {
  const [form, setForm] = useState({
    title: task.title,
    description: task.description ?? '',
    priority: task.priority,
    assignedUserId: task.assignedUserId ?? '',
    dueDate: task.dueDate ? task.dueDate.slice(0, 10) : '',
  });
  const [comments, setComments] = useState([]);
  const [newComment, setNewComment] = useState('');
  const [error, setError] = useState('');
  const set = (field) => (e) => setForm((f) => ({ ...f, [field]: e.target.value }));

  useEffect(() => {
    api(`/tasks/${task.id}/comments`).then(setComments).catch((err) => setError(err.message));
  }, [task.id]);

  async function run(action) {
    setError('');
    try {
      await action();
    } catch (err) {
      setError(err.message);
    }
  }

  const save = (event) => {
    event.preventDefault();
    run(async () => {
      // PUT não muda o status — isso só acontece pelo PATCH /status (ChangeStatus no WorkItem).
      const updated = await api(`/tasks/${task.id}`, {
        method: 'PUT',
        body: {
          title: form.title,
          description: form.description || null,
          priority: form.priority,
          assignedUserId: form.assignedUserId ? Number(form.assignedUserId) : null,
          dueDate: toIsoDate(form.dueDate),
        },
      });
      onSaved(updated);
    });
  };

  const addComment = (event) => {
    event.preventDefault();
    run(async () => {
      const comment = await api(`/tasks/${task.id}/comments`, { method: 'POST', body: { content: newComment } });
      setComments((list) => [...list, comment]);
      setNewComment('');
    });
  };

  const removeComment = (comment) =>
    run(async () => {
      await api(`/comments/${comment.id}`, { method: 'DELETE' });
      setComments((list) => list.filter((c) => c.id !== comment.id));
    });

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal card" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>
            Tarefa #{task.id} <span className="badge">{task.status}</span>
          </h2>
          <button className="link" onClick={onClose}>Fechar ✕</button>
        </div>
        <p className="muted small">Criada em {new Date(task.createdAt).toLocaleString()}</p>

        <form onSubmit={save}>
          <label>
            Título
            <input value={form.title} onChange={set('title')} required />
          </label>
          <label>
            Descrição
            <textarea value={form.description} onChange={set('description')} />
          </label>
          <div className="grid">
            <label>
              Prioridade
              <select value={form.priority} onChange={set('priority')}>
                {PRIORITIES.map((p) => <option key={p}>{p}</option>)}
              </select>
            </label>
            <label>
              Responsável
              <AssigneeField users={users} value={form.assignedUserId} onChange={set('assignedUserId')} />
            </label>
            <label>
              Prazo
              <input type="date" value={form.dueDate} onChange={set('dueDate')} />
            </label>
          </div>
          <button type="submit">Salvar alterações</button>
        </form>

        {error && <p className="error">{error}</p>}

        <h3>Comentários</h3>
        <ul className="comments">
          {comments.map((comment) => (
            <li key={comment.id}>
              <div>
                <strong>{comment.authorName}</strong>{' '}
                <span className="muted small">{new Date(comment.createdAt).toLocaleString()}</span>
                <p>{comment.content}</p>
              </div>
              {(comment.userId === user.id || canManage(user)) && (
                <button className="link danger-text" onClick={() => removeComment(comment)}>excluir</button>
              )}
            </li>
          ))}
          {comments.length === 0 && <li className="muted">Sem comentários.</li>}
        </ul>
        <form className="inline-form" onSubmit={addComment}>
          <input
            placeholder="Escreva um comentário…"
            value={newComment}
            onChange={(e) => setNewComment(e.target.value)}
            required
          />
          <button type="submit">Comentar</button>
        </form>
      </div>
    </div>
  );
}
