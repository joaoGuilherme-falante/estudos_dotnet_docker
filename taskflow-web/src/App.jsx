import { useEffect, useState } from 'react';
import { decodeToken, getToken, setToken } from './api.js';
import Login from './components/Login.jsx';
import ProjectsView from './components/ProjectsView.jsx';
import UsersView from './components/UsersView.jsx';
import RequestLog from './components/RequestLog.jsx';

export default function App() {
  const [user, setUser] = useState(() => {
    const token = getToken();
    const decoded = token && decodeToken(token);
    return decoded && decoded.expiresAt > new Date() ? decoded : null;
  });
  const [tab, setTab] = useState('projects');
  const [showLog, setShowLog] = useState(true);

  function logout() {
    setToken(null);
    setUser(null);
    setTab('projects');
  }

  // api.js dispara este evento quando a API responde 401 (ex.: token expirou).
  useEffect(() => {
    window.addEventListener('taskflow:unauthorized', logout);
    return () => window.removeEventListener('taskflow:unauthorized', logout);
  }, []);

  function handleLogin(token) {
    setToken(token);
    setUser(decodeToken(token));
  }

  return (
    <div className={`layout ${showLog ? 'with-log' : ''}`}>
      <main>
        <header className="topbar">
          <h1>TaskFlow</h1>
          {user && (
            <>
              <nav className="tabs">
                <button className={tab === 'projects' ? 'active' : ''} onClick={() => setTab('projects')}>
                  Projetos e tarefas
                </button>
                {user.role === 'Admin' && (
                  <button className={tab === 'users' ? 'active' : ''} onClick={() => setTab('users')}>
                    Usuários
                  </button>
                )}
              </nav>
              <div className="whoami">
                <span>
                  {user.name} <span className={`badge role-${user.role}`}>{user.role}</span> #{user.id}
                </span>
                <button className="link" onClick={logout}>Sair</button>
              </div>
            </>
          )}
          <button className="link" onClick={() => setShowLog((v) => !v)}>
            {showLog ? 'Ocultar requisições' : 'Mostrar requisições'}
          </button>
        </header>

        {!user && <Login onLogin={handleLogin} />}
        {user && tab === 'projects' && <ProjectsView user={user} />}
        {user && tab === 'users' && <UsersView user={user} />}
      </main>
      {showLog && <RequestLog />}
    </div>
  );
}
