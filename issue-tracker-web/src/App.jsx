import { useEffect, useState } from 'react'

function App() {
  const [issues, setIssues] = useState(null)
  const [title, setTitle] = useState('')
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    const controller = new AbortController()

    fetch('/issues', { signal: controller.signal })
      .then((response) => {
        if (!response.ok) throw new Error('Could not load issues')
        return response.json()
      })
      .then(setIssues)
      .catch((err) => {
        if (err.name !== 'AbortError') setError(err.message)
      })

    return () => controller.abort()
  }, [])

  async function addIssue(event) {
    event.preventDefault()
    const cleanTitle = title.trim()
    if (!cleanTitle) return

    setSaving(true)
    setError('')

    try {
      const response = await fetch('/issues', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ title: cleanTitle }),
      })
      if (!response.ok) throw new Error('Could not add issue')

      const created = await response.json()
      setIssues((current) => [...current, created])
      setTitle('')
    } catch {
      setError('Could not add issue. Check both servers are running.')
    } finally {
      setSaving(false)
    }
  }

  async function closeIssue(id) {
    setError('')

    try {
      const response = await fetch(`/issues/${id}/close`, {
        method: 'PATCH',
      })
      if (!response.ok) throw new Error('Could not close issue')

      const updated = await response.json()
      setIssues((current) =>
        current.map((issue) => issue.id === id ? updated : issue)
      )
    } catch {
      setError('Could not close issue. Check both servers are running.')
    }
  }

  return (
    <main style={{ padding: '2rem', textAlign: 'left' }}>
      <h1>Issue Tracker</h1>

      {issues !== null && (
        <form onSubmit={addIssue}>
          <label htmlFor="new-issue">New issue: </label>
          <input
            id="new-issue"
            value={title}
            onChange={(event) => setTitle(event.target.value)}
            required
          />
          <button type="submit" disabled={saving}>
            {saving ? 'Adding...' : 'Add issue'}
          </button>
        </form>
      )}

      {error && <p role="alert">{error}</p>}
      {issues === null && !error && <p>Loading issues...</p>}
      {issues?.length === 0 && <p>No issues yet.</p>}

      {issues?.length > 0 && (
        <ul>
          {issues.map((issue) => (
            <li key={issue.id}>
              {issue.title} — {issue.status}{' '}
              {issue.status === 'Open' && (
                <button type="button" onClick={() => closeIssue(issue.id)}>
                  Close
                </button>
              )}
            </li>
          ))}
        </ul>
      )}
    </main>
  )
}

export default App