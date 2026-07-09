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

const TeamLeaveApprovals = () => {
  const { user } = useAuth();
  const toast = useToast();
  const [pendingLeaves, setPendingLeaves] = useState([]);
  const [loading, setLoading] = useState(true);
  const [actionModal, setActionModal] = useState(null);
  const [actionType, setActionType] = useState('');
  const [rejectionReason, setRejectionReason] = useState('');
  const [actionLoading, setActionLoading] = useState(false);

  const hasProfile = !!user?.employeeId;

  useEffect(() => { if (hasProfile) loadPending(); else setLoading(false); }, []);

  const loadPending = async () => {
    setLoading(true);
    try {
      const res = await leaveService.getPendingForMe();
      setPendingLeaves(res.data?.data || res.data || []);
    } catch (e) { toast.error('Failed to load pending leaves'); }
    setLoading(false);
  };

  const handleAction = async () => {
    setActionLoading(true);
    try {
      await leaveService.action(actionModal.leaveRequestId, {
        action: actionType,
        rejectionReason: actionType === 'Rejected' ? rejectionReason : undefined,
      });
      toast.success(`Leave ${actionType.toLowerCase()}`);
      setActionModal(null);
      setRejectionReason('');
      loadPending();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Action failed');
    } finally { setActionLoading(false); }
  };

  if (!hasProfile) {
    return (
      <div>
        <div className="page-header"><h1 className="page-title">Leave Approvals</h1><div className="page-title-accent" /></div>
        <div className="no-profile-box"><div className="empty-icon">👤</div><div className="empty-title">No Employee Profile</div></div>
      </div>
    );
  }

  return (
    <div>
      <div className="page-header">
        <h1 className="page-title">Team Leave Approvals</h1>
        <div className="page-title-accent" />
        <p className="page-subtitle">{pendingLeaves.length} pending request{pendingLeaves.length !== 1 ? 's' : ''}</p>
      </div>

      <div className="card">
        {loading ? (
          <div className="loading-spinner"><div className="spinner" /> Loading...</div>
        ) : pendingLeaves.length === 0 ? (
          <div className="empty-state">
            <div className="empty-icon">✅</div><div className="empty-title">All caught up!</div>
            <div className="empty-message">No pending leave requests from your team.</div>
          </div>
        ) : (
          <table className="data-table">
            <thead><tr><th>Employee</th><th>Type</th><th>From</th><th>To</th><th>Days</th><th>Reason</th><th className="text-right">Actions</th></tr></thead>
            <tbody>
              {pendingLeaves.map(l => (
                <tr key={l.leaveRequestId}>
                  <td><strong>{l.employeeName}</strong><div className="sub-text">{l.departmentName}</div></td>
                  <td><span className="badge badge-info">{l.leaveTypeName}</span></td>
                  <td>{formatDate(l.fromDate)}</td>
                  <td>{formatDate(l.toDate)}</td>
                  <td>{l.totalDays}{l.isHalfDay ? ' (½)' : ''}</td>
                  <td className="text-secondary">{l.reason || '—'}</td>
                  <td className="text-right">
                    <div style={{ display: 'flex', gap: 6, justifyContent: 'flex-end' }}>
                      <button className="btn-ghost" onClick={() => { setActionModal(l); setActionType('Approved'); }}>Approve</button>
                      <button className="btn-ghost danger" onClick={() => { setActionModal(l); setActionType('Rejected'); }}>Reject</button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      <Modal isOpen={!!actionModal} onClose={() => { setActionModal(null); setRejectionReason(''); }}
        title={actionType === 'Approved' ? 'Approve Leave' : 'Reject Leave'}
        footer={<>
          <button className="btn btn-secondary" onClick={() => setActionModal(null)}>Cancel</button>
          <button className={`btn ${actionType === 'Approved' ? 'btn-success' : 'btn-danger'}`}
            onClick={handleAction} disabled={actionLoading || (actionType === 'Rejected' && !rejectionReason.trim())}>
            {actionLoading ? 'Processing...' : actionType === 'Approved' ? 'Approve' : 'Reject'}
          </button>
        </>}>
        <p style={{ marginBottom: 16 }}>
          <strong>{actionModal?.employeeName}</strong> — {actionModal?.leaveTypeName}<br />
          {formatDate(actionModal?.fromDate)} to {formatDate(actionModal?.toDate)} ({actionModal?.totalDays} days)
        </p>
        {actionType === 'Rejected' && (
          <div className="form-group">
            <label className="form-label">Rejection Reason *</label>
            <textarea className="form-input" rows={3} value={rejectionReason}
              onChange={e => setRejectionReason(e.target.value)} placeholder="Provide a reason" />
          </div>
        )}
      </Modal>
    </div>
  );
};

export default TeamLeaveApprovals;
