import { useEffect, useState } from 'react';
import { onRequest } from '../api.js';

// Mostra cada chamada HTTP feita à API — útil para ligar o que você clica com o código C#.
export default function RequestLog() {
  const [entries, setEntries] = useState([]);
  const [openId, setOpenId] = useState(null);

  useEffect(() => onRequest((entry) => setEntries((list) => [entry, ...list].slice(0, 50))), []);

  return (
    <aside className="log">
      <div className="log-header">
        <h3>Requisições</h3>
        <button className="link" onClick={() => setEntries([])}>Limpar</button>
      </div>
      {entries.length === 0 && <p className="muted">Nenhuma requisição ainda.</p>}
      <ul>
        {entries.map((entry) => (
          <li key={entry.id}>
            <button className="log-line" onClick={() => setOpenId(openId === entry.id ? null : entry.id)}>
              <span className={`method method-${entry.method}`}>{entry.method}</span>
              <span className="path">{entry.path}</span>
              <span className={`status status-${String(entry.status)[0]}`}>{entry.status}</span>
            </button>
            {openId === entry.id && (
              <div className="log-detail">
                <small className="muted">{entry.at.toLocaleTimeString()} · {entry.ms} ms</small>
                {entry.request !== undefined && (
                  <>
                    <strong>Body enviado</strong>
                    <pre>{JSON.stringify(entry.request, null, 2)}</pre>
                  </>
                )}
                <strong>Resposta</strong>
                <pre>{entry.response === null ? '(vazia)' : JSON.stringify(entry.response, null, 2)}</pre>
              </div>
            )}
          </li>
        ))}
      </ul>
    </aside>
  );
}
