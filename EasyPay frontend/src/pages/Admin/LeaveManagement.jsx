import React, { useState, useEffect } from 'react';
import { leaveService } from '../../api/leaveService';
import Modal from '../../components/Modal';
import { useToast } from '../../context/ToastContext';

const formatDate = (dateStr) => {
  if (!dateStr) return '—';
  const d = new Date(dateStr + (dateStr.includes('T') ? '' : 'T00:00:00'));
  return d.toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
};

const getStatusBadge = (status) => {
  const map = { Pending: 'badge-warning', Approved: 'badge-success', Rejected: 'badge-danger', Cancelled: 'badge-danger' };
  return map[status] || 'badge-info';
};

const LeaveManagement = () => {
  const [leaves, setLeaves] = useState([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [statusFilter, setStatusFilter] = useState('');
  const [actionModal, setActionModal] = useState(null);
  const [actionType, setActionType] = useState('');
  const [rejectionReason, setRejectionReason] = useState('');
  const [actionLoading, setActionLoading] = useState(false);
  const [carryForwardLoading, setCarryForwardLoading] = useState(false);
  const toast = useToast();

  useEffect(() => { fetchLeaves(); }, [page, statusFilter]);

  const fetchLeaves = async () => {
    setLoading(true);
    try {
      const params = { PageNumber: page, PageSize: 15 };
      if (statusFilter) params.status = statusFilter;
      const res = await leaveService.getAll(params);
      setLeaves(res.data?.data || []);
      setTotalPages(res.data?.totalPages || 1);
    } catch (e) {
      toast.error('Failed to load leave requests');
    } finally { setLoading(false); }
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
      fetchLeaves();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Action failed');
    } finally { setActionLoading(false); }
  };

  const handleBulkCarryForward = async () => {
    setCarryForwardLoading(true);
    try {
      const res = await leaveService.bulkCarryForward(new Date().getFullYear() - 1);
      const results = res.data?.data || res.data || [];
      toast.success(`Carry-forward completed for ${Array.isArray(results) ? results.length : 0} employees`);
    } catch (e) {
      toast.error(e.response?.data?.message || 'Carry-forward failed');
    } finally { setCarryForwardLoading(false); }
  };

  return (
    <div>
      <div className="page-actions">
        <div className="page-header" style={{ marginBottom: 0 }}>
          <h1 className="page-title">Leave Management</h1>
          <div className="page-title-accent" />
        </div>
        <button className="btn btn-secondary" onClick={handleBulkCarryForward} disabled={carryForwardLoading}>
          {carryForwardLoading ? 'Processing...' : 'Bulk Carry Forward'}
        </button>
      </div>

      <div className="filter-bar">
        <div className="form-group" style={{ marginBottom: 0 }}>
          <label className="form-label">Status</label>
          <select className="form-input" style={{ minWidth: 140 }} value={statusFilter}
            onChange={e => { setStatusFilter(e.target.value); setPage(1); }}>
            <option value="">All</option>
            <option value="Pending">Pending</option>
            <option value="Approved">Approved</option>
            <option value="Rejected">Rejected</option>
            <option value="Cancelled">Cancelled</option>
          </select>
        </div>
      </div>

      <div className="card">
        {loading ? (
          <div className="loading-spinner"><div className="spinner" /> Loading...</div>
        ) : leaves.length === 0 ? (
          <div className="empty-state">
            <div className="empty-icon">📅</div>
            <div className="empty-title">No leave requests</div>
            <div className="empty-message">No leave requests match your filters.</div>
          </div>
        ) : (
          <div style={{ overflowX: 'auto' }}>
            <table className="data-table">
              <thead>
                <tr>
                  <th>Employee</th>
                  <th>Type</th>
                  <th>From</th>
                  <th>To</th>
                  <th>Days</th>
                  <th>Reason</th>
                  <th>Status</th>
                  <th className="text-right">Actions</th>
                </tr>
              </thead>
              <tbody>
                {leaves.map(l => (
                  <tr key={l.leaveRequestId}>
                    <td>
                      <strong>{l.employeeName}</strong>
                      <div className="sub-text">{l.departmentName}</div>
                    </td>
                    <td><span className="badge badge-info">{l.leaveTypeName}</span></td>
                    <td>{formatDate(l.fromDate)}</td>
                    <td>{formatDate(l.toDate)}</td>
                    <td>{l.totalDays}{l.isHalfDay ? ' (½)' : ''}</td>
                    <td className="text-secondary" style={{ maxWidth: 200, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                      {l.reason || '—'}
                    </td>
                    <td><span className={`badge ${getStatusBadge(l.status)}`}>{l.status}</span></td>
                    <td className="text-right">
                      {l.status === 'Pending' && (
                        <div style={{ display: 'flex', gap: 6, justifyContent: 'flex-end' }}>
                          <button className="btn-ghost" onClick={() => { setActionModal(l); setActionType('Approved'); }}>Approve</button>
                          <button className="btn-ghost danger" onClick={() => { setActionModal(l); setActionType('Rejected'); }}>Reject</button>
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

      <Modal isOpen={!!actionModal} onClose={() => { setActionModal(null); setRejectionReason(''); }}
        title={actionType === 'Approved' ? 'Approve Leave' : 'Reject Leave'}
        footer={<>
          <button className="btn btn-secondary" onClick={() => setActionModal(null)}>Cancel</button>
          <button className={`btn ${actionType === 'Approved' ? 'btn-success' : 'btn-danger'}`}
            onClick={handleAction} disabled={actionLoading || (actionType === 'Rejected' && !rejectionReason.trim())}>
            {actionLoading ? 'Processing...' : actionType === 'Approved' ? 'Approve' : 'Reject'}
          </button>
        </>}>
        <div>
          <p style={{ marginBottom: 16 }}>
            <strong>{actionModal?.employeeName}</strong> — {actionModal?.leaveTypeName}<br />
            {formatDate(actionModal?.fromDate)} to {formatDate(actionModal?.toDate)} ({actionModal?.totalDays} days)
          </p>
          {actionType === 'Rejected' && (
            <div className="form-group">
              <label className="form-label">Rejection Reason *</label>
              <textarea className="form-input" rows={3} value={rejectionReason}
                onChange={e => setRejectionReason(e.target.value)} placeholder="Provide a reason for rejection" />
            </div>
          )}
        </div>
      </Modal>
    </div>
  );
};

export default LeaveManagement;
