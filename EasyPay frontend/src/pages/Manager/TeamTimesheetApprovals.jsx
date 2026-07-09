import React, { useState, useEffect } from 'react';
import { useAuth } from '../../context/AuthContext';
import { useToast } from '../../context/ToastContext';
import { timesheetService } from '../../api/services';

const formatDate = (dateStr) => {
  if (!dateStr) return '—';
  const d = new Date(dateStr + (dateStr.includes('T') ? '' : 'T00:00:00'));
  return d.toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
};
const getStatusBadge = (status) => {
  const map = { Pending: 'badge-warning', Approved: 'badge-success', Rejected: 'badge-danger' };
  return map[status] || 'badge-info';
};

const TeamTimesheetApprovals = () => {
  const { user } = useAuth();
  const toast = useToast();
  const [timesheets, setTimesheets] = useState([]);
  const [loading, setLoading] = useState(true);
  const [selectedIds, setSelectedIds] = useState([]);
  const [actionLoading, setActionLoading] = useState(false);

  const hasProfile = !!user?.employeeId;

  useEffect(() => { if (hasProfile) loadTimesheets(); else setLoading(false); }, []);

  const loadTimesheets = async () => {
    setLoading(true);
    try {
      const res = await timesheetService.getAll({ PageNumber: 1, PageSize: 100, status: 'Pending' });
      setTimesheets(res.data?.data || []);
    } catch (e) { toast.error('Failed to load timesheets'); }
    setLoading(false);
  };

  const handleAction = async (id, action) => {
    setActionLoading(true);
    try {
      await timesheetService.action(id, { action });
      toast.success(`Timesheet ${action.toLowerCase()}`);
      loadTimesheets();
    } catch (e) { toast.error(e.response?.data?.message || 'Failed'); }
    finally { setActionLoading(false); }
  };

  const handleBulkApprove = async () => {
    if (selectedIds.length === 0) return;
    setActionLoading(true);
    try {
      await timesheetService.bulkApprove({ timesheetIds: selectedIds });
      toast.success(`${selectedIds.length} timesheets approved`);
      setSelectedIds([]);
      loadTimesheets();
    } catch (e) { toast.error(e.response?.data?.message || 'Failed'); }
    finally { setActionLoading(false); }
  };

  const toggleSelect = (id) => setSelectedIds(prev => prev.includes(id) ? prev.filter(x => x !== id) : [...prev, id]);

  if (!hasProfile) {
    return (
      <div>
        <div className="page-header"><h1 className="page-title">Timesheet Approvals</h1><div className="page-title-accent" /></div>
        <div className="no-profile-box"><div className="empty-icon">👤</div><div className="empty-title">No Employee Profile</div></div>
      </div>
    );
  }

  return (
    <div>
      <div className="page-actions">
        <div className="page-header" style={{ marginBottom: 0 }}>
          <h1 className="page-title">Timesheet Approvals</h1>
          <div className="page-title-accent" />
          <p className="page-subtitle">{timesheets.length} pending</p>
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
            <div className="empty-icon">✅</div><div className="empty-title">All caught up!</div>
            <div className="empty-message">No pending timesheets to review.</div>
          </div>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th style={{ width: 40 }}>
                  <input type="checkbox" checked={selectedIds.length === timesheets.length && timesheets.length > 0}
                    onChange={() => setSelectedIds(prev => prev.length === timesheets.length ? [] : timesheets.map(t => t.timesheetId))}
                    style={{ accentColor: 'var(--hex-blue)' }} />
                </th>
                <th>Employee</th><th>Date</th><th>Hours</th><th>OT</th><th>Notes</th><th className="text-right">Actions</th>
              </tr>
            </thead>
            <tbody>
              {timesheets.map(t => (
                <tr key={t.timesheetId}>
                  <td><input type="checkbox" checked={selectedIds.includes(t.timesheetId)}
                    onChange={() => toggleSelect(t.timesheetId)} style={{ accentColor: 'var(--hex-blue)' }} /></td>
                  <td><strong>{t.employeeName}</strong><div className="sub-text">{t.employeeCode}</div></td>
                  <td>{formatDate(t.workDate)}</td>
                  <td>{t.hoursWorked}h</td>
                  <td>{t.overtimeHours > 0 ? `${t.overtimeHours}h` : '—'}</td>
                  <td className="text-secondary">{t.notes || '—'}</td>
                  <td className="text-right">
                    <div style={{ display: 'flex', gap: 6, justifyContent: 'flex-end' }}>
                      <button className="btn-ghost" onClick={() => handleAction(t.timesheetId, 'Approved')} disabled={actionLoading}>Approve</button>
                      <button className="btn-ghost danger" onClick={() => handleAction(t.timesheetId, 'Rejected')} disabled={actionLoading}>Reject</button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
};

export default TeamTimesheetApprovals;
