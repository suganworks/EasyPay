import React, { useState, useEffect } from 'react';
import { useAuth } from '../../context/AuthContext';
import { useToast } from '../../context/ToastContext';
import { leaveService } from '../../api/leaveService';
import Modal from '../../components/Modal';

const formatDate = (dateStr) => {
  if (!dateStr) return '—';
  const d = new Date(dateStr + (dateStr.includes('T') ? '' : 'T00:00:00'));
  return d.toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
};
const getStatusBadge = (status) => {
  const map = { Pending: 'badge-warning', Approved: 'badge-success', Rejected: 'badge-danger', Cancelled: 'badge-danger' };
  return map[status] || 'badge-info';
};

const LEAVE_TYPES = [
  { id: 1, name: 'Casual Leave' }, { id: 2, name: 'Sick Leave' },
  { id: 3, name: 'Privilege Leave' }, { id: 4, name: 'Loss of Pay' },
  { id: 5, name: 'Compensatory Off' },
];

const MyLeaves = () => {
  const { user } = useAuth();
  const toast = useToast();
  const [leaves, setLeaves] = useState([]);
  const [balance, setBalance] = useState([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [applyModal, setApplyModal] = useState(false);
  const [saving, setSaving] = useState(false);
  const [cancelling, setCancelling] = useState(null);
  const [form, setForm] = useState({
    leaveTypeId: '', fromDate: '', toDate: '', reason: '', isHalfDay: false, halfDayType: 'AM',
  });
  const [formErrors, setFormErrors] = useState({});

  const hasProfile = !!user?.employeeId;

  useEffect(() => { if (hasProfile) loadData(); else setLoading(false); }, [page]);

  const loadData = async () => {
    setLoading(true);
    try {
      const res = await leaveService.getMyLeaves({ PageNumber: page, PageSize: 10 });
      setLeaves(res.data?.data || []);
      setTotalPages(res.data?.totalPages || 1);
    } catch (e) { toast.error('Failed to load leaves'); }
    try {
      const bRes = await leaveService.getMyBalance(new Date().getFullYear());
      setBalance(bRes.data?.data || bRes.data || []);
    } catch (e) { /* ignore */ }
    setLoading(false);
  };

  const validateForm = () => {
    const errors = {};
    if (!form.leaveTypeId) errors.leaveTypeId = 'Required';
    if (!form.fromDate) errors.fromDate = 'Required';
    if (!form.toDate) errors.toDate = 'Required';
    if (form.fromDate && form.toDate && form.toDate < form.fromDate) {
      errors.toDate = 'End date must be >= start date';
    }
    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleApply = async (e) => {
    e.preventDefault();
    if (!validateForm()) return;
    setSaving(true);
    try {
      await leaveService.create({
        leaveTypeId: Number(form.leaveTypeId),
        fromDate: form.fromDate,
        toDate: form.toDate,
        reason: form.reason || undefined,
        isHalfDay: form.isHalfDay,
        halfDayType: form.isHalfDay ? form.halfDayType : undefined,
      });
      toast.success('Leave request submitted');
      setApplyModal(false);
      setForm({ leaveTypeId: '', fromDate: '', toDate: '', reason: '', isHalfDay: false, halfDayType: 'AM' });
      loadData();
    } catch (e) {
      if (e.response?.status === 409) {
        toast.error('Overlapping leave request exists for this period');
      } else {
        toast.error(e.response?.data?.message || 'Failed to submit leave');
      }
    } finally { setSaving(false); }
  };

  const handleCancel = async (id) => {
    setCancelling(id);
    try {
      await leaveService.cancel(id);
      toast.success('Leave cancelled');
      loadData();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Failed to cancel');
    } finally { setCancelling(null); }
  };

  if (!hasProfile) {
    return (
      <div>
        <div className="page-header"><h1 className="page-title">My Leaves</h1><div className="page-title-accent" /></div>
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
          <h1 className="page-title">My Leaves</h1>
          <div className="page-title-accent" />
        </div>
        <button className="btn btn-primary" onClick={() => setApplyModal(true)}>+ Apply for Leave</button>
      </div>

      {/* Balance Cards */}
      {balance.length > 0 && (
        <div className="stat-grid">
          {balance.map(lb => (
            <div key={lb.leaveTypeId} className="stat-card accent-blue">
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                <div>
                  <div className="stat-card-value">{lb.remainingDays}</div>
                  <div className="stat-card-label">{lb.leaveTypeName}</div>
                </div>
                <div style={{ fontSize: 12, color: 'var(--text-secondary)' }}>{lb.usedDays}/{lb.maxDaysPerYear}</div>
              </div>
            </div>
          ))}
        </div>
      )}

      <div className="card">
        {loading ? (
          <div className="loading-spinner"><div className="spinner" /> Loading...</div>
        ) : leaves.length === 0 ? (
          <div className="empty-state">
            <div className="empty-icon">📅</div>
            <div className="empty-title">No leave requests</div>
            <div className="empty-message">Apply for leave to see your requests here.</div>
          </div>
        ) : (
          <div style={{ overflowX: 'auto' }}>
            <table className="data-table">
              <thead><tr><th>Type</th><th>From</th><th>To</th><th>Days</th><th>Reason</th><th>Status</th><th className="text-right">Actions</th></tr></thead>
              <tbody>
                {leaves.map(l => (
                  <tr key={l.leaveRequestId}>
                    <td><span className="badge badge-info">{l.leaveTypeName}</span></td>
                    <td>{formatDate(l.fromDate)}</td>
                    <td>{formatDate(l.toDate)}</td>
                    <td>{l.totalDays}{l.isHalfDay ? ' (½)' : ''}</td>
                    <td className="text-secondary">{l.reason || '—'}</td>
                    <td>
                      <span className={`badge ${getStatusBadge(l.status)}`}>{l.status}</span>
                      {l.rejectionReason && <div className="sub-text" style={{ color: 'var(--danger)' }}>{l.rejectionReason}</div>}
                    </td>
                    <td className="text-right">
                      {l.status === 'Pending' && (
                        <button className="btn-ghost danger" onClick={() => handleCancel(l.leaveRequestId)}
                          disabled={cancelling === l.leaveRequestId}>
                          {cancelling === l.leaveRequestId ? '...' : 'Cancel'}
                        </button>
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

      <Modal isOpen={applyModal} onClose={() => setApplyModal(false)} title="Apply for Leave"
        footer={<>
          <button className="btn btn-secondary" onClick={() => setApplyModal(false)} disabled={saving}>Cancel</button>
          <button className="btn btn-primary" onClick={handleApply} disabled={saving}>{saving ? 'Submitting...' : 'Submit'}</button>
        </>}>
        <form onSubmit={handleApply}>
          <div className="form-group">
            <label className="form-label">Leave Type *</label>
            <select className="form-input" value={form.leaveTypeId}
              onChange={e => setForm(f => ({ ...f, leaveTypeId: e.target.value }))}>
              <option value="">Select Type</option>
              {LEAVE_TYPES.map(lt => <option key={lt.id} value={lt.id}>{lt.name}</option>)}
            </select>
            {formErrors.leaveTypeId && <div className="form-error">{formErrors.leaveTypeId}</div>}
          </div>
          <div className="form-grid">
            <div className="form-group">
              <label className="form-label">From Date *</label>
              <input type="date" className="form-input" value={form.fromDate}
                max={form.toDate || undefined}
                onChange={e => setForm(f => ({ ...f, fromDate: e.target.value }))} />
              {formErrors.fromDate && <div className="form-error">{formErrors.fromDate}</div>}
            </div>
            <div className="form-group">
              <label className="form-label">To Date *</label>
              <input type="date" className="form-input" value={form.toDate}
                min={form.fromDate || undefined}
                onChange={e => setForm(f => ({ ...f, toDate: e.target.value }))} />
              {formErrors.toDate && <div className="form-error">{formErrors.toDate}</div>}
            </div>
          </div>
          <div className="form-group">
            <label className="checkbox-label">
              <input type="checkbox" checked={form.isHalfDay}
                onChange={e => setForm(f => ({ ...f, isHalfDay: e.target.checked }))} />
              Half Day
            </label>
          </div>
          {form.isHalfDay && (
            <div className="form-group">
              <label className="form-label">Half Day Type</label>
              <select className="form-input" value={form.halfDayType}
                onChange={e => setForm(f => ({ ...f, halfDayType: e.target.value }))}>
                <option value="AM">First Half (AM)</option>
                <option value="PM">Second Half (PM)</option>
              </select>
            </div>
          )}
          <div className="form-group">
            <label className="form-label">Reason</label>
            <textarea className="form-input" rows={3} value={form.reason}
              onChange={e => setForm(f => ({ ...f, reason: e.target.value }))} placeholder="Optional reason" />
          </div>
        </form>
      </Modal>
    </div>
  );
};

export default MyLeaves;
