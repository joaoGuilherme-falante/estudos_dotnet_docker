// Admin vê a lista de usuários; os outros perfis não podem chamar GET /api/users, então digitam o Id.
export default function AssigneeField({ users, value, onChange }) {
  if (users) {
    return (
      <select value={value} onChange={onChange}>
        <option value="">Sem responsável</option>
        {users.map((u) => (
          <option key={u.id} value={u.id}>{u.name} ({u.role})</option>
        ))}
      </select>
    );
  }
  return <input type="number" min="1" placeholder="Id do responsável" value={value} onChange={onChange} />;
}
