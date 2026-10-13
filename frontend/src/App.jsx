import React, { useCallback, useEffect, useState } from 'react';

const API = (import.meta.env.VITE_API_BASE_URL || 'https://localhost:56410').replace(/\/$/, '');

const STATUSES = ['New', 'InProgress', 'Blocked', 'Completed'];
const PRIORITIES = ['Low', 'Medium', 'High'];
const emptyForm = { title: '', clientName: '', description: '', priority: 'Medium', status: 'New', dueDate: '', notes: '' };

async function request(path, options = {}) {
  const response = await fetch(`${API}${path}`, {
    ...options,
    headers: { 'Content-Type': 'application/json', ...(options.headers || {}) }
  });
  const body = response.status === 204 ? null : await response.json().catch(() => null);
  if (!response.ok) throw new Error(body?.error?.message || `Request failed (${response.status})`);
  return body;
}

export default function App() {
  const [items, setItems] = useState([]);
  const [status, setStatus] = useState('');
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [total, setTotal] = useState(0);
  const [form, setForm] = useState(emptyForm);
  const [noteInputs, setNoteInputs] = useState({});
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [message, setMessage] = useState('');

  const load = useCallback(async () => {
    setLoading(true); setError('');
    try {
      const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
      if (status) params.set('status', status);
      if (search.trim()) params.set('search', search.trim());
      const result = await request(`/api/work-requests?${params.toString()}`);
      setItems(result.items || []); setTotal(result.totalCount || 0);
    } catch (e) { setError(`${e.message}. Check that the API is running and the database connection is configured.`); }
    finally { setLoading(false); }
  }, [page, pageSize, status, search]);

  useEffect(() => { load(); }, [load]);

  function changeFilter(setter, value) { setter(value); setPage(1); setMessage(''); }
  function updateForm(e) { setForm({ ...form, [e.target.name]: e.target.value }); }

  async function createRequest(e) {
    e.preventDefault(); setError(''); setMessage('');
    if (!form.title.trim() || !form.clientName.trim() || !form.dueDate) {
      setError('Title, client name, and due date are required.'); return;
    }
    setSaving(true);
    try {
      await request('/api/work-requests', {
        method: 'POST',
        body: JSON.stringify({ ...form, dueDate: new Date(`${form.dueDate}T00:00:00`).toISOString() })
      });
      setForm(emptyForm); setPage(1); setMessage('Work request created successfully.'); await load();
    } catch (e) { setError(e.message); }
    finally { setSaving(false); }
  }

  async function changeStatus(id, nextStatus) {
    setError(''); setMessage('');
    try {
      await request(`/api/work-requests/${id}/status`, { method: 'PATCH', body: JSON.stringify({ status: nextStatus }) });
      setMessage('Status updated.'); await load();
    } catch (e) { setError(e.message); }
  }

  async function addNote(id) {
    const text = (noteInputs[id] || '').trim();
    if (!text) { setError('Enter a note before submitting.'); return; }
    setError(''); setMessage('');
    try {
      await request(`/api/work-requests/${id}/notes`, { method: 'POST', body: JSON.stringify({ text }) });
      setNoteInputs({ ...noteInputs, [id]: '' }); setMessage('Note added.'); await load();
    } catch (e) { setError(e.message); }
  }

  const pages = Math.max(1, Math.ceil(total / pageSize));

  return <main className="shell">
    <header className="hero">
      <div><p className="eyebrow">OPERATIONS WORKSPACE</p><h1>Work Request Tracker</h1>
        <p className="subtitle">Create, prioritize, search, and track client work requests.</p></div>
      <div className="stat"><strong>{total}</strong><span>Total requests</span></div>
    </header>

    {error && <div className="alert error" role="alert">{error}</div>}
    {message && <div className="alert success" role="status">{message}</div>}

    <section className="panel">
      <div className="section-title"><div><h2>Create a work request</h2><p>Required fields are marked with an asterisk.</p></div></div>
      <form className="form-grid" onSubmit={createRequest}>
        <label>Title *<input name="title" maxLength="150" value={form.title} onChange={updateForm} required /></label>
        <label>Client name *<input name="clientName" maxLength="150" value={form.clientName} onChange={updateForm} required /></label>
        <label>Priority<select name="priority" value={form.priority} onChange={updateForm}>{PRIORITIES.map(x => <option key={x}>{x}</option>)}</select></label>
        <label>Status<select name="status" value={form.status} onChange={updateForm}>{STATUSES.map(x => <option key={x}>{x}</option>)}</select></label>
        <label>Due date *<input type="date" name="dueDate" value={form.dueDate} onChange={updateForm} required /></label>
        <label className="wide">Description<textarea name="description" maxLength="2000" rows="2" value={form.description} onChange={updateForm} /></label>
        <label className="wide">Initial note (optional)<input name="notes" maxLength="1000" value={form.notes} onChange={updateForm} /></label>
        <div className="wide"><button disabled={saving}>{saving ? 'Saving…' : 'Create request'}</button></div>
      </form>
    </section>

    <section className="panel">
      <div className="section-title"><div><h2>Work requests</h2><p>Search by title or client, or filter by status.</p></div></div>
      <div className="filters">
        <label className="search">Search<input value={search} onChange={e => changeFilter(setSearch, e.target.value)} placeholder="Title or client name" /></label>
        <label>Status<select value={status} onChange={e => changeFilter(setStatus, e.target.value)}><option value="">All statuses</option>{STATUSES.map(x => <option key={x}>{x}</option>)}</select></label>
        <button className="secondary" onClick={load} disabled={loading}>{loading ? 'Loading…' : 'Refresh'}</button>
      </div>
      {loading ? <div className="empty">Loading work requests…</div> :
       items.length === 0 ? <div className="empty">No work requests found. Create one above or change your filters.</div> :
       <div className="cards">{items.map(item => <article className="request-card" key={item.id}>
         <div className="card-top"><div><span className={`priority ${item.priority.toLowerCase()}`}>{item.priority}</span><h3>#{item.id} · {item.title}</h3><p className="client">{item.clientName}</p></div>
           <label className="status-label">Status<select value={item.status} onChange={e => changeStatus(item.id, e.target.value)}>{STATUSES.map(x => <option key={x}>{x}</option>)}</select></label>
         </div>
         {item.description && <p className="description">{item.description}</p>}
         <div className="meta"><span>Due {new Date(item.dueDate).toLocaleDateString()}</span><span>Updated {new Date(item.updatedDate).toLocaleString()}</span></div>
         <div className="notes">
           <strong>Notes ({item.notes?.length || 0})</strong>
           {(item.notes || []).map(n => <p className="note" key={n.id}>{n.text}<small>{new Date(n.createdDate).toLocaleString()}</small></p>)}
           <div className="note-entry"><input aria-label={`Add note to request ${item.id}`} maxLength="1000" placeholder="Add a note…" value={noteInputs[item.id] || ''} onChange={e => setNoteInputs({ ...noteInputs, [item.id]: e.target.value })} /><button className="secondary" onClick={() => addNote(item.id)}>Add note</button></div>
         </div>
       </article>)}</div>}
      <div className="pagination"><span>Page {page} of {pages} · {total} results</span><div><button className="secondary" disabled={page <= 1 || loading} onClick={() => setPage(p => p - 1)}>Previous</button><button className="secondary" disabled={page >= pages || loading} onClick={() => setPage(p => p + 1)}>Next</button></div></div>
    </section>
    <footer>Assignment sample · ASP.NET Core Web API · React · SQL Server</footer>
  </main>;
}
