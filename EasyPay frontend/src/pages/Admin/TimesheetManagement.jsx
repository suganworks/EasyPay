import React, { useState, useEffect } from 'react';
import { timesheetService } from '../../api/services';
import { useToast } from '../../context/ToastContext';

const formatDate = (dateStr) => {
  if (!dateStr) return '—';
  const d = new Date(dateStr + (dateStr.includes('T') ? '' : 'T00:00:00'));
  return d.toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
};

const getStatusBadge = (status) => {
  const map = { Pending: 'badge-warning', Approved: 'badge-success', Rejected: 'badge-danger' };
  return map[status] || 'badge-info';
};

const TimesheetManagement = () => {
  const [timesheets, setTimesheets] = useState([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [selectedIds, setSelectedIds] = useState([]);
  const [actionLoading, setActionLoading] = useState(false);
  const toast = useToast();

  useEffect(() => { fetchTimesheets(); }, [page]);

  const fetchTimesheets = async () => {
    setLoading(true);
    try {
      const res = await timesheetService.getAll({ PageNumber: page, PageSize: 20 });
      setTimesheets(res.data?.data || []);
      setTotalPages(res.data?.totalPages || 1);
    } catch (e) {
      toast.error('Failed to load timesheets');
    } finally { setLoading(false); }
  };

  const handleAction = async (id, action) => {
    setActionLoading(true);
    try {
      await timesheetService.action(id, { action });
      toast.success(`Timesheet ${action.toLowerCase()}`);
      fetchTimesheets();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Action failed');
    } finally { setActionLoading(false); }
  };

  const handleBulkApprove = async () => {
    if (selectedIds.length === 0) { toast.warning('Select timesheets to approve'); return; }
    setActionLoading(true);
    try {
      await timesheetService.bulkApprove({ timesheetIds: selectedIds });
      toast.success(`${selectedIds.length} timesheets approved`);
      setSelectedIds([]);
      fetchTimesheets();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Bulk approve failed');
    } finally { setActionLoading(false); }
  };

  const toggleSelect = (id) => {
    setSelectedIds(prev => prev.includes(id) ? prev.filter(x => x !== id) : [...prev, id]);
  };

  const pendingTimesheets = timesheets.filter(t => t.status === 'Pending');
  const selectAll = () => {
    const pendingIds = pendingTimesheets.map(t => t.timesheetId);
    setSelectedIds(prev => prev.length === pendingIds.length ? [] : pendingIds);
  };

  return (
    <div>
      <div className="page-actions">
        <div className="page-header" style={{ marginBottom: 0 }}>
          <h1 className="page-title">Timesheet Management</h1>
          <div className="page-title-accent" />
        </div>
        {selectedIds.length > 0 && (
          <button className="btn btn-primary" onClick={handleBulkApprove} disabled={actionLoading}>
            Approve Selected ({selectedIds.length})
          </button>
        )}
      </div>

      <div className="card">
        {loading ? (
          <div className="loading-spinner"><div className="spinner" /> Loading...</div>
        ) : timesheets.length === 0 ? (
          <div className="empty-state">
            <div className="empty-icon">⏱️</div>
            <div className="empty-title">No timesheets</div>
            <div className="empty-message">No timesheet entries found.</div>
          </div>
        ) : (
          <div style={{ overflowX: 'auto' }}>
            <table className="data-table">
              <thead>
                <tr>
                  <th style={{ width: 40 }}>
                    {pendingTimesheets.length > 0 && (
                      <input type="checkbox" checked={selectedIds.length === pendingTimesheets.length && pendingTimesheets.length > 0}
                        onChange={selectAll} style={{ accentColor: 'var(--hex-blue)' }} />
                    )}
                  </th>
                  <th>Employee</th>
                  <th>Date</th>
                  <th>Hours</th>
                  <th>Overtime</th>
                  <th>Notes</th>
                  <th>Status</th>
                  <th className="text-right">Actions</th>
                </tr>
              </thead>
              <tbody>
                {timesheets.map(t => (
                  <tr key={t.timesheetId}>
                    <td>
                      {t.status === 'Pending' && (
                        <input type="checkbox" checked={selectedIds.includes(t.timesheetId)}
                          onChange={() => toggleSelect(t.timesheetId)} style={{ accentColor: 'var(--hex-blue)' }} />
                      )}
                    </td>
                    <td>
                      <strong>{t.employeeName}</strong>
                      <div className="sub-text">{t.employeeCode}</div>
                    </td>
                    <td>{formatDate(t.workDate)}</td>
                    <td>{t.hoursWorked}h</td>
                    <td>{t.overtimeHours > 0 ? `${t.overtimeHours}h` : '—'}</td>
                    <td className="text-secondary" style={{ maxWidth: 200, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                      {t.notes || '—'}
                    </td>
                    <td><span className={`badge ${getStatusBadge(t.status)}`}>{t.status}</span></td>
                    <td className="text-right">
                      {t.status === 'Pending' && (
                        <div style={{ display: 'flex', gap: 6, justifyContent: 'flex-end' }}>
                          <button className="btn-ghost" onClick={() => handleAction(t.timesheetId, 'Approved')} disabled={actionLoading}>Approve</button>
                          <button className="btn-ghost danger" onClick={() => handleAction(t.timesheetId, 'Rejected')} disabled={actionLoading}>Reject</button>
                        </div>
                      )}
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

export default TimesheetManagement;
