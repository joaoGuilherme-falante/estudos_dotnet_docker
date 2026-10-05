import { useCallback, useEffect, useState } from 'react';
import { api, canManage, PRIORITIES, TASK_STATUSES } from '../api.js';
import TaskModal from './TaskModal.jsx';
import AssigneeField from './AssigneeField.jsx';

export default function ProjectsView({ user }) {
  const [projects, setProjects] = useState([]);
  const [selectedId, setSelectedId] = useState(null);
  const [users, setUsers] = useState(null);
  const [error, setError] = useState('');

  const loadProjects = useCallback(async () => {
    try {
      const list = await api('/projects');
      setProjects(list);
      setSelectedId((current) => current ?? list[0]?.id ?? null);
    } catch (err) {
      setError(err.message);
    }
  }, []);

  useEffect(() => {
    loadProjects();
    // GET /api/users exige role Admin; para os outros perfis o responsável é digitado pelo Id.
    if (user.role === 'Admin') api('/users').then(setUsers).catch(() => setUsers(null));
  }, [loadProjects, user.role]);

  const selected = projects.find((p) => p.id === selectedId);

  return (
    <div className="projects">
      <section className="sidebar">
        <h2>Projetos</h2>
        {error && <p className="error">{error}</p>}
        <ul className="project-list">
          {projects.map((project) => (
            <li key={project.id}>
              <button
                className={project.id === selectedId ? 'active' : ''}
                onClick={() => setSelectedId(project.id)}
              >
                <span>{project.name}</span>
                <span className={`badge project-${project.status}`}>{project.status}</span>
              </button>
            </li>
          ))}
          {projects.length === 0 && <li className="muted">Nenhum projeto.</li>}
        </ul>
        {canManage(user) ? (
          <CreateProjectForm
            onCreated={(project) => {
              setProjects((list) => [...list, project]);
              setSelectedId(project.id);
            }}
          />
        ) : (
          <p className="muted small">Só Admin/Manager criam projetos.</p>
        )}
      </section>

      <section className="content">
        {selected ? (
          <ProjectDetail
            key={selected.id}
            project={selected}
            user={user}
            users={users}
            onChanged={(updated) => setProjects((list) => list.map((p) => (p.id === updated.id ? updated : p)))}
            onDeleted={() => {
              setProjects((list) => list.filter((p) => p.id !== selected.id));
              setSelectedId(null);
            }}
          />
        ) : (
          <p className="muted">Selecione ou crie um projeto.</p>
        )}
      </section>
    </div>
  );
}

function CreateProjectForm({ onCreated }) {
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [error, setError] = useState('');

  async function submit(event) {
    event.preventDefault();
    setError('');
    try {
      const project = await api('/projects', { method: 'POST', body: { name, description: description || null } });
      onCreated(project);
      setName('');
      setDescription('');
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <form className="card compact" onSubmit={submit}>
      <h3>Novo projeto</h3>
      <input placeholder="Nome" value={name} onChange={(e) => setName(e.target.value)} required />
      <textarea placeholder="Descrição (opcional)" value={description} onChange={(e) => setDescription(e.target.value)} />
      {error && <p className="error">{error}</p>}
      <button type="submit">Criar projeto</button>
    </form>
  );
}

function ProjectDetail({ project, user, users, onChanged, onDeleted }) {
  const [tasks, setTasks] = useState([]);
  const [error, setError] = useState('');
  const [editing, setEditing] = useState(false);
  const [openTaskId, setOpenTaskId] = useState(null);

  const loadTasks = useCallback(async () => {
    try {
      setTasks(await api(`/tasks?projectId=${project.id}`));
    } catch (err) {
      setError(err.message);
    }
  }, [project.id]);

  useEffect(() => {
    loadTasks();
  }, [loadTasks]);

  async function run(action) {
    setError('');
    try {
      await action();
    } catch (err) {
      setError(err.message);
    }
  }

  const complete = () =>
    run(async () => onChanged(await api(`/projects/${project.id}/complete`, { method: 'POST' })));

  const remove = () =>
    run(async () => {
      if (!confirm(`Excluir o projeto "${project.name}"?`)) return;
      await api(`/projects/${project.id}`, { method: 'DELETE' });
      onDeleted();
    });

  const advance = (task, status) =>
    run(async () => {
      const updated = await api(`/tasks/${task.id}/status`, { method: 'PATCH', body: { status } });
      setTasks((list) => list.map((t) => (t.id === updated.id ? updated : t)));
    });

  const removeTask = (task) =>
    run(async () => {
      if (!confirm(`Excluir a tarefa "${task.title}"?`)) return;
      await api(`/tasks/${task.id}`, { method: 'DELETE' });
      setTasks((list) => list.filter((t) => t.id !== task.id));
    });

  const userName = (id) => users?.find((u) => u.id === id)?.name ?? `#${id}`;
  const openTask = tasks.find((t) => t.id === openTaskId);

  return (
    <div>
      <div className="project-header">
        {editing ? (
          <EditProjectForm
            project={project}
            onSaved={(updated) => {
              onChanged(updated);
              setEditing(false);
            }}
            onCancel={() => setEditing(false)}
          />
        ) : (
          <div>
            <h2>
              {project.name} <span className={`badge project-${project.status}`}>{project.status}</span>
            </h2>
            {project.description && <p className="muted">{project.description}</p>}
          </div>
        )}
        {canManage(user) && !editing && (
          <div className="actions">
            <button className="secondary" onClick={() => setEditing(true)}>Editar</button>
            <button
              className="secondary"
              onClick={complete}
              title="Regra: só conclui se todas as tarefas estiverem Done (Project.Complete)"
            >
              Concluir projeto
            </button>
            <button className="danger" onClick={remove}>Excluir</button>
          </div>
        )}
      </div>

      {error && <p className="error">{error}</p>}

      <div className="board">
        {TASK_STATUSES.map((status, index) => {
          const column = tasks.filter((t) => t.status === status);
          return (
            <div key={status} className="column">
              <h3>
                {status} <span className="muted">({column.length})</span>
              </h3>
              {column.map((task) => (
                <div key={task.id} className="task-card">
                  <button className="task-title" onClick={() => setOpenTaskId(task.id)}>
                    {task.title}
                  </button>
                  <div className="task-meta">
                    <span className={`badge priority-${task.priority}`}>{task.priority}</span>
                    {task.assignedUserId && <span>👤 {userName(task.assignedUserId)}</span>}
                    {task.dueDate && <span>📅 {new Date(task.dueDate).toLocaleDateString()}</span>}
                  </div>
                  <div className="task-actions">
                    {index < TASK_STATUSES.length - 1 && (
                      <button onClick={() => advance(task, TASK_STATUSES[index + 1])}>
                        → {TASK_STATUSES[index + 1]}
                      </button>
                    )}
                    {index < TASK_STATUSES.length - 2 && (
                      <button
                        className="secondary"
                        title="Testa a regra de avançar só um passo — deve dar erro 400"
                        onClick={() => advance(task, TASK_STATUSES[index + 2])}
                      >
                        pular ⇢
                      </button>
                    )}
                    {canManage(user) && (
                      <button className="danger" onClick={() => removeTask(task)}>✕</button>
                    )}
                  </div>
                </div>
              ))}
            </div>
          );
        })}
      </div>

      <CreateTaskForm
        projectId={project.id}
        users={users}
        onCreated={(task) => setTasks((list) => [...list, task])}
      />

      {openTask && (
        <TaskModal
          task={openTask}
          user={user}
          users={users}
          onClose={() => setOpenTaskId(null)}
          onSaved={(updated) => setTasks((list) => list.map((t) => (t.id === updated.id ? updated : t)))}
        />
      )}
    </div>
  );
}

function EditProjectForm({ project, onSaved, onCancel }) {
  const [name, setName] = useState(project.name);
  const [description, setDescription] = useState(project.description ?? '');
  const [error, setError] = useState('');

  async function submit(event) {
    event.preventDefault();
    try {
      onSaved(await api(`/projects/${project.id}`, { method: 'PUT', body: { name, description: description || null } }));
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <form className="inline-form" onSubmit={submit}>
      <input value={name} onChange={(e) => setName(e.target.value)} required />
      <input placeholder="Descrição" value={description} onChange={(e) => setDescription(e.target.value)} />
      <button type="submit">Salvar</button>
      <button type="button" className="secondary" onClick={onCancel}>Cancelar</button>
      {error && <p className="error">{error}</p>}
    </form>
  );
}

function CreateTaskForm({ projectId, users, onCreated }) {
  const empty = { title: '', description: '', priority: 'Medium', assignedUserId: '', dueDate: '' };
  const [form, setForm] = useState(empty);
  const [error, setError] = useState('');
  const set = (field) => (e) => setForm((f) => ({ ...f, [field]: e.target.value }));

  async function submit(event) {
    event.preventDefault();
    setError('');
    try {
      const task = await api('/tasks', {
        method: 'POST',
        body: {
          title: form.title,
          description: form.description || null,
          priority: form.priority,
          projectId,
          assignedUserId: form.assignedUserId ? Number(form.assignedUserId) : null,
          dueDate: toIsoDate(form.dueDate),
        },
      });
      onCreated(task);
      setForm(empty);
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <form className="card task-form" onSubmit={submit}>
      <h3>Nova tarefa</h3>
      <div className="grid">
        <input placeholder="Título" value={form.title} onChange={set('title')} required />
        <select value={form.priority} onChange={set('priority')}>
          {PRIORITIES.map((p) => <option key={p}>{p}</option>)}
        </select>
        <AssigneeField users={users} value={form.assignedUserId} onChange={set('assignedUserId')} />
        <label className="inline">
          Prazo
          <input type="date" value={form.dueDate} onChange={set('dueDate')} />
        </label>
      </div>
      <textarea placeholder="Descrição (opcional)" value={form.description} onChange={set('description')} />
      {error && <p className="error">{error}</p>}
      <button type="submit">Criar tarefa</button>
    </form>
  );
}

// "2026-10-05" (input date) → ISO com meio-dia local, para o fuso não mudar o dia no servidor.
export const toIsoDate = (value) => (value ? new Date(`${value}T12:00:00`).toISOString() : null);
