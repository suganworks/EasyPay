import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { payrollService } from '../../api/payrollService';
import Modal from '../../components/Modal';
import { useToast } from '../../context/ToastContext';

const formatCurrency = (val) => {
  if (val == null || isNaN(val)) return '—';
  return new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(val);
};

const formatDate = (dateStr) => {
  if (!dateStr) return '—';
  const d = new Date(dateStr + (dateStr.includes('T') ? '' : 'T00:00:00'));
  return d.toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
};

const getStatusBadge = (status) => {
  const map = { Pending: 'badge-warning', Approved: 'badge-success', Paid: 'badge-success', Cancelled: 'badge-danger' };
  return map[status] || 'badge-info';
};

const PayrollList = () => {
  const [payrolls, setPayrolls] = useState([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [filters, setFilters] = useState({ status: '', year: '', month: '' });
  const [rejectModal, setRejectModal] = useState(null);
  const [rejectRemarks, setRejectRemarks] = useState('');
  const [payDateModal, setPayDateModal] = useState(null);
  const [paymentDate, setPaymentDate] = useState(new Date().toISOString().split('T')[0]);
  const [actionLoading, setActionLoading] = useState(false);
  const toast = useToast();

  useEffect(() => { fetchPayrolls(); }, [page, filters]);

  const fetchPayrolls = async () => {
    setLoading(true);
    try {
      const params = { PageNumber: page, PageSize: 10 };
      if (filters.status) params.status = filters.status;
      if (filters.year) params.year = Number(filters.year);
      if (filters.month) params.month = Number(filters.month);
      const res = await payrollService.getAll(params);
      setPayrolls(res.data?.data || []);
      setTotalPages(res.data?.totalPages || 1);
    } catch (e) {
      toast.error('Failed to load payrolls');
    } finally { setLoading(false); }
  };

  const handleApprove = async (id) => {
    setActionLoading(true);
    try {
      await payrollService.approve(id);
      toast.success('Payroll approved');
      fetchPayrolls();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Approve failed');
    } finally { setActionLoading(false); }
  };

  const handleReject = async () => {
    setActionLoading(true);
    try {
      await payrollService.reject(rejectModal, { remarks: rejectRemarks });
      toast.success('Payroll rejected');
      setRejectModal(null);
      setRejectRemarks('');
      fetchPayrolls();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Reject failed');
    } finally { setActionLoading(false); }
  };

  const handleMarkPaid = async () => {
    setActionLoading(true);
    try {
      await payrollService.markAsPaid(payDateModal, { paymentDate });
      toast.success('Marked as paid');
      setPayDateModal(null);
      fetchPayrolls();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Failed to mark as paid');
    } finally { setActionLoading(false); }
  };

  const currentYear = new Date().getFullYear();
  const years = Array.from({ length: 5 }, (_, i) => currentYear - i);
  const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];

  return (
    <div>
      <div className="page-actions">
        <div className="page-header" style={{ marginBottom: 0 }}>
          <h1 className="page-title">Payroll Records</h1>
          <div className="page-title-accent" />
        </div>
        <Link to="/admin/payroll/process" className="btn btn-primary">Process Payroll</Link>
      </div>

      <div className="filter-bar">
        <div className="form-group" style={{ marginBottom: 0 }}>
          <label className="form-label">Status</label>
          <select className="form-input" style={{ minWidth: 140 }} value={filters.status}
            onChange={e => { setFilters(f => ({ ...f, status: e.target.value })); setPage(1); }}>
            <option value="">All</option>
            <option value="Pending">Pending</option>
            <option value="Approved">Approved</option>
            <option value="Paid">Paid</option>
            <option value="Cancelled">Cancelled</option>
          </select>
        </div>
        <div className="form-group" style={{ marginBottom: 0 }}>
          <label className="form-label">Year</label>
          <select className="form-input" style={{ minWidth: 100 }} value={filters.year}
            onChange={e => { setFilters(f => ({ ...f, year: e.target.value })); setPage(1); }}>
            <option value="">All</option>
            {years.map(y => <option key={y} value={y}>{y}</option>)}
          </select>
        </div>
        <div className="form-group" style={{ marginBottom: 0 }}>
          <label className="form-label">Month</label>
          <select className="form-input" style={{ minWidth: 100 }} value={filters.month}
            onChange={e => { setFilters(f => ({ ...f, month: e.target.value })); setPage(1); }}>
            <option value="">All</option>
            {months.map((m, i) => <option key={i} value={i + 1}>{m}</option>)}
          </select>
        </div>
      </div>

      <div className="card">
        {loading ? (
          <div className="loading-spinner"><div className="spinner" /> Loading...</div>
        ) : payrolls.length === 0 ? (
          <div className="empty-state">
            <div className="empty-icon">💰</div>
            <div className="empty-title">No payroll records</div>
            <div className="empty-message">Process payroll to see records here.</div>
          </div>
        ) : (
          <div style={{ overflowX: 'auto' }}>
            <table className="data-table">
              <thead>
                <tr>
                  <th>Employee</th>
                  <th>Department</th>
                  <th>Period</th>
                  <th>Gross</th>
                  <th>Deductions</th>
                  <th>Net Salary</th>
                  <th>Status</th>
                  <th className="text-right">Actions</th>
                </tr>
              </thead>
              <tbody>
                {payrolls.map(pr => (
                  <tr key={pr.payrollId}>
                    <td>
                      <strong>{pr.employeeName}</strong>
                      <div className="sub-text">{pr.employeeCode}</div>
                    </td>
                    <td>{pr.departmentName || '—'}</td>
                    <td>{formatDate(pr.payPeriodStart)} — {formatDate(pr.payPeriodEnd)}</td>
                    <td className="currency">{formatCurrency(pr.grossEarnings)}</td>
                    <td className="currency" style={{ color: 'var(--danger)' }}>{formatCurrency(pr.totalDeductions)}</td>
                    <td className="currency" style={{ fontWeight: 700 }}>{formatCurrency(pr.netSalary)}</td>
                    <td><span className={`badge ${getStatusBadge(pr.status)}`}>{pr.status}</span></td>
                    <td className="text-right">
                      <div style={{ display: 'flex', gap: 6, justifyContent: 'flex-end' }}>
                        {pr.status === 'Pending' && (
                          <>
                            <button className="btn-ghost" onClick={() => handleApprove(pr.payrollId)} disabled={actionLoading}>Approve</button>
                            <button className="btn-ghost danger" onClick={() => setRejectModal(pr.payrollId)} disabled={actionLoading}>Reject</button>
                          </>
                        )}
                        {pr.status === 'Approved' && (
                          <button className="btn-ghost" onClick={() => setPayDateModal(pr.payrollId)} disabled={actionLoading}>Mark Paid</button>
                        )}
                      </div>
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

      {/* Reject Modal */}
      <Modal isOpen={!!rejectModal} onClose={() => setRejectModal(null)} title="Reject Payroll"
        footer={<>
          <button className="btn btn-secondary" onClick={() => setRejectModal(null)}>Cancel</button>
          <button className="btn btn-danger" onClick={handleReject} disabled={actionLoading || !rejectRemarks.trim()}>
            {actionLoading ? 'Rejecting...' : 'Reject'}
          </button>
        </>}>
        <div className="form-group">
          <label className="form-label">Rejection Reason *</label>
          <textarea className="form-input" rows={3} value={rejectRemarks}
            onChange={e => setRejectRemarks(e.target.value)} placeholder="Provide a reason for rejection" />
        </div>
      </Modal>

      {/* Mark Paid Modal */}
      <Modal isOpen={!!payDateModal} onClose={() => setPayDateModal(null)} title="Mark as Paid"
        footer={<>
          <button className="btn btn-secondary" onClick={() => setPayDateModal(null)}>Cancel</button>
          <button className="btn btn-primary" onClick={handleMarkPaid} disabled={actionLoading}>
            {actionLoading ? 'Processing...' : 'Confirm Payment'}
          </button>
        </>}>
        <div className="form-group">
          <label className="form-label">Payment Date</label>
          <input type="date" className="form-input" value={paymentDate}
            onChange={e => setPaymentDate(e.target.value)} />
        </div>
      </Modal>
    </div>
  );
};

export default PayrollList;
