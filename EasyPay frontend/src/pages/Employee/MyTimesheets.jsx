import React, { useState, useEffect } from 'react';
import { useAuth } from '../../context/AuthContext';
import { useToast } from '../../context/ToastContext';
import { timesheetService } from '../../api/services';
import Modal from '../../components/Modal';

const formatDate = (dateStr) => {
  if (!dateStr) return '—';
  const d = new Date(dateStr + (dateStr.includes('T') ? '' : 'T00:00:00'));
  return d.toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
};
const getStatusBadge = (status) => {
  const map = { Pending: 'badge-warning', Approved: 'badge-success', Rejected: 'badge-danger' };
  return map[status] || 'badge-info';
};

const MyTimesheets = () => {
  const { user } = useAuth();
  const toast = useToast();
  const [timesheets, setTimesheets] = useState([]);
  const [summary, setSummary] = useState(null);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [logModal, setLogModal] = useState(false);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState({ workDate: '', hoursWorked: 8, overtimeHours: 0, checkIn: '', checkOut: '', notes: '' });
  const [formErrors, setFormErrors] = useState({});

  const hasProfile = !!user?.employeeId;

  useEffect(() => { if (hasProfile) loadData(); else setLoading(false); }, [page]);

  const loadData = async () => {
    setLoading(true);
    try {
      const res = await timesheetService.getMyTimesheets({ PageNumber: page, PageSize: 10 });
      setTimesheets(res.data?.data || []);
      setTotalPages(res.data?.totalPages || 1);
    } catch (e) { toast.error('Failed to load timesheets'); }
    try {
      const now = new Date();
      const sRes = await timesheetService.myMonthlySummary({ year: now.getFullYear(), month: now.getMonth() + 1 });
      setSummary(sRes.data?.data || sRes.data);
    } catch (e) { /* ignore */ }
    setLoading(false);
  };

  const validateForm = () => {
    const errors = {};
    if (!form.workDate) errors.workDate = 'Required';
    const today = new Date().toISOString().split('T')[0];
    if (form.workDate && form.workDate > today) errors.workDate = 'Cannot be in the future';
    if (!form.hoursWorked || Number(form.hoursWorked) <= 0) errors.hoursWorked = 'Must be > 0';
    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleLog = async (e) => {
    e.preventDefault();
    if (!validateForm()) return;
    setSaving(true);
    try {
      await timesheetService.create({
        workDate: form.workDate,
        hoursWorked: Number(form.hoursWorked),
        overtimeHours: Number(form.overtimeHours) || 0,
        checkIn: form.checkIn || undefined,
        checkOut: form.checkOut || undefined,
        notes: form.notes || undefined,
      });
      toast.success('Time logged successfully');
      setLogModal(false);
      setForm({ workDate: '', hoursWorked: 8, overtimeHours: 0, checkIn: '', checkOut: '', notes: '' });
      loadData();
    } catch (e) {
      if (e.response?.status === 409) {
        toast.error('Timesheet already exists for this date');
      } else {
        toast.error(e.response?.data?.message || 'Failed to log time');
      }
    } finally { setSaving(false); }
  };

  if (!hasProfile) {
    return (
      <div>
        <div className="page-header"><h1 className="page-title">My Timesheets</h1><div className="page-title-accent" /></div>
        <div className="no-profile-box">
          <div className="empty-icon">👤</div><div className="empty-title">No Employee Profile</div>
          <div className="empty-message">Your account is not linked to an employee profile.</div>
        </div>
      </div>
    );
  }

  return (
    <div>
      <div className="page-actions">
        <div className="page-header" style={{ marginBottom: 0 }}>
          <h1 className="page-title">My Timesheets</h1>
          <div className="page-title-accent" />
        </div>
        <button className="btn btn-primary" onClick={() => setLogModal(true)}>+ Log Time</button>
      </div>

      {/* Monthly Summary */}
      {summary && (
        <div className="stat-grid">
          <div className="stat-card accent-blue">
            <div className="stat-card-value">{summary.presentDays ?? 0}</div>
            <div className="stat-card-label">Days Present</div>
          </div>
          <div className="stat-card accent-green">
            <div className="stat-card-value">{summary.totalHoursWorked ?? 0}h</div>
            <div className="stat-card-label">Total Hours</div>
          </div>
          <div className="stat-card accent-yellow">
            <div className="stat-card-value">{summary.totalOvertimeHours ?? 0}h</div>
            <div className="stat-card-label">Overtime</div>
          </div>
          <div className="stat-card accent-red">
            <div className="stat-card-value">{summary.pendingTimesheets ?? 0}</div>
            <div className="stat-card-label">Pending Approval</div>
          </div>
        </div>
      )}

      <div className="card">
        {loading ? (
          <div className="loading-spinner"><div className="spinner" /> Loading...</div>
        ) : timesheets.length === 0 ? (
          <div className="empty-state">
            <div className="empty-icon">⏱️</div>
            <div className="empty-title">No timesheets</div>
            <div className="empty-message">Log your work hours to see entries here.</div>
          </div>
        ) : (
          <div style={{ overflowX: 'auto' }}>
            <table className="data-table">
              <thead><tr><th>Date</th><th>Check In</th><th>Check Out</th><th>Hours</th><th>OT</th><th>Notes</th><th>Status</th></tr></thead>
              <tbody>
                {timesheets.map(t => (
                  <tr key={t.timesheetId}>
                    <td>{formatDate(t.workDate)}</td>
                    <td>{t.checkIn || '—'}</td>
                    <td>{t.checkOut || '—'}</td>
                    <td>{t.hoursWorked}h</td>
                    <td>{t.overtimeHours > 0 ? `${t.overtimeHours}h` : '—'}</td>
                    <td className="text-secondary">{t.notes || '—'}</td>
                    <td><span className={`badge ${getStatusBadge(t.status)}`}>{t.status}</span></td>
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

      <Modal isOpen={logModal} onClose={() => setLogModal(false)} title="Log Time"
        footer={<>
          <button className="btn btn-secondary" onClick={() => setLogModal(false)} disabled={saving}>Cancel</button>
          <button className="btn btn-primary" onClick={handleLog} disabled={saving}>{saving ? 'Saving...' : 'Log Time'}</button>
        </>}>
        <form onSubmit={handleLog}>
          <div className="form-group">
            <label className="form-label">Work Date *</label>
            <input type="date" className="form-input" value={form.workDate}
              max={new Date().toISOString().split('T')[0]}
              onChange={e => setForm(f => ({ ...f, workDate: e.target.value }))} />
            {formErrors.workDate && <div className="form-error">{formErrors.workDate}</div>}
          </div>
          <div className="form-grid">
            <div className="form-group">
              <label className="form-label">Check In</label>
              <input type="time" className="form-input" value={form.checkIn}
                onChange={e => setForm(f => ({ ...f, checkIn: e.target.value }))} />
            </div>
            <div className="form-group">
              <label className="form-label">Check Out</label>
              <input type="time" className="form-input" value={form.checkOut}
                onChange={e => setForm(f => ({ ...f, checkOut: e.target.value }))} />
            </div>
          </div>
          <div className="form-grid">
            <div className="form-group">
              <label className="form-label">Hours Worked *</label>
              <input type="number" className="form-input" min="0" max="24" step="0.5"
                value={form.hoursWorked} onChange={e => setForm(f => ({ ...f, hoursWorked: e.target.value }))} />
              {formErrors.hoursWorked && <div className="form-error">{formErrors.hoursWorked}</div>}
            </div>
            <div className="form-group">
              <label className="form-label">Overtime Hours</label>
              <input type="number" className="form-input" min="0" max="24" step="0.5"
                value={form.overtimeHours} onChange={e => setForm(f => ({ ...f, overtimeHours: e.target.value }))} />
            </div>
          </div>
          <div className="form-group">
            <label className="form-label">Notes</label>
            <textarea className="form-input" rows={2} value={form.notes}
              onChange={e => setForm(f => ({ ...f, notes: e.target.value }))} placeholder="What did you work on?" />
          </div>
        </form>
      </Modal>
    </div>
  );
};

export default MyTimesheets;
