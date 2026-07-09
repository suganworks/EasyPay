import React, { useState, useEffect } from 'react';
import { auditLogService } from '../../api/services';
import { useToast } from '../../context/ToastContext';

const AuditLogs = () => {
  const [logs, setLogs] = useState([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [filters, setFilters] = useState({ action: '', entityName: '', fromDate: '', toDate: '' });
  const toast = useToast();

  useEffect(() => { fetchLogs(); }, [page, filters]);

  const fetchLogs = async () => {
    setLoading(true);
    try {
      const params = { PageNumber: page, PageSize: 20 };
      if (filters.action) params.action = filters.action;
      if (filters.entityName) params.entityName = filters.entityName;
      if (filters.fromDate) params.fromDate = filters.fromDate;
      if (filters.toDate) params.toDate = filters.toDate;
      const res = await auditLogService.getAll(params);
      setLogs(res.data?.data || []);
      setTotalPages(res.data?.totalPages || 1);
    } catch (e) {
      toast.error('Failed to load audit logs');
    } finally { setLoading(false); }
  };

  const formatDateTime = (dateStr) => {
    if (!dateStr) return '—';
    return new Date(dateStr).toLocaleString('en-IN', {
      day: '2-digit', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit',
    });
  };

  return (
    <div>
      <div className="page-header">
        <h1 className="page-title">Audit Logs</h1>
        <div className="page-title-accent" />
        <p className="page-subtitle">System activity log</p>
      </div>

      <div className="filter-bar">
        <div className="form-group" style={{ marginBottom: 0 }}>
          <label className="form-label">Action</label>
          <input type="text" className="form-input" style={{ minWidth: 150 }} placeholder="e.g. Login, CreateEmployee"
            value={filters.action} onChange={e => setFilters(f => ({ ...f, action: e.target.value }))} />
        </div>
        <div className="form-group" style={{ marginBottom: 0 }}>
          <label className="form-label">Entity</label>
          <input type="text" className="form-input" style={{ minWidth: 150 }} placeholder="e.g. Employee, Payroll"
            value={filters.entityName} onChange={e => setFilters(f => ({ ...f, entityName: e.target.value }))} />
        </div>
        <div className="form-group" style={{ marginBottom: 0 }}>
          <label className="form-label">From Date</label>
          <input type="date" className="form-input" value={filters.fromDate}
            max={filters.toDate || undefined}
            onChange={e => setFilters(f => ({ ...f, fromDate: e.target.value }))} />
        </div>
        <div className="form-group" style={{ marginBottom: 0 }}>
          <label className="form-label">To Date</label>
          <input type="date" className="form-input" value={filters.toDate}
            min={filters.fromDate || undefined}
            onChange={e => setFilters(f => ({ ...f, toDate: e.target.value }))} />
        </div>
        <button className="btn btn-secondary" onClick={() => { setFilters({ action: '', entityName: '', fromDate: '', toDate: '' }); setPage(1); }}>
          Clear
        </button>
      </div>

      <div className="card">
        {loading ? (
          <div className="loading-spinner"><div className="spinner" /> Loading...</div>
        ) : logs.length === 0 ? (
          <div className="empty-state">
            <div className="empty-icon">📝</div>
            <div className="empty-title">No audit logs found</div>
            <div className="empty-message">No logs match your filter criteria.</div>
          </div>
        ) : (
          <div style={{ overflowX: 'auto' }}>
            <table className="data-table">
              <thead>
                <tr>
                  <th>Timestamp</th>
                  <th>User</th>
                  <th>Action</th>
                  <th>Entity</th>
                  <th>Entity ID</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {logs.map(log => (
                  <tr key={log.auditLogId}>
                    <td style={{ whiteSpace: 'nowrap' }}>{formatDateTime(log.createdAt)}</td>
                    <td>{log.userEmail || `User #${log.userId}` || '—'}</td>
                    <td><span className="badge badge-info">{log.action}</span></td>
                    <td>{log.entityName || '—'}</td>
                    <td>{log.entityId || '—'}</td>
                    <td>
                      <span className={`badge ${log.isSuccess ? 'badge-success' : 'badge-danger'}`}>
                        {log.isSuccess ? 'Success' : 'Failed'}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        {totalPages > 1 && (
          <div className="pagination">
            <button className="pagination-btn" disabled={page <= 1} onClick={() => setPage(p => p - 1)}>← Prev</button>
            <span className="pagination-info">Page {page} of {totalPages}</span>
            <button className="pagination-btn" disabled={page >= totalPages} onClick={() => setPage(p => p + 1)}>Next →</button>
          </div>
        )}
      </div>
    </div>
  );
};

export default AuditLogs;
