import React, { useState, useEffect } from 'react';
import { payrollService } from '../../api/payrollService';
import { employeeService } from '../../api/employeeService';
import { salaryStructureService } from '../../api/services';
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

const ProcessorPayroll = () => {
  const [activeTab, setActiveTab] = useState('list');
  const [payrolls, setPayrolls] = useState([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [filters, setFilters] = useState({ status: '' });
  const [actionLoading, setActionLoading] = useState(false);
  const [rejectModal, setRejectModal] = useState(null);
  const [rejectRemarks, setRejectRemarks] = useState('');
  const [payDateModal, setPayDateModal] = useState(null);
  const [paymentDate, setPaymentDate] = useState(new Date().toISOString().split('T')[0]);

  // Process form
  const [employees, setEmployees] = useState([]);
  const [processing, setProcessing] = useState(false);
  const [processForm, setProcessForm] = useState({
    employeeId: '', payPeriodStart: '', payPeriodEnd: '', bonusAmount: 0, otherDeductions: 0, remarks: '',
  });
  const [processResult, setProcessResult] = useState(null);
  const toast = useToast();

  useEffect(() => { loadPayrolls(); }, [page, filters]);
  useEffect(() => { if (activeTab === 'process') loadEmployees(); }, [activeTab]);

  const loadPayrolls = async () => {
    setLoading(true);
    try {
      const params = { PageNumber: page, PageSize: 15 };
      if (filters.status) params.status = filters.status;
      const res = await payrollService.getAll(params);
      setPayrolls(res.data?.data || []);
      setTotalPages(res.data?.totalPages || 1);
    } catch (e) {
      toast.error('Failed to load payrolls');
    } finally { setLoading(false); }
  };

  const loadEmployees = async () => {
    try {
      const res = await employeeService.getAll({ PageNumber: 1, PageSize: 200 });
      setEmployees(res.data?.data || []);
    } catch (e) { /* ignore */ }
  };

  const handleApprove = async (id) => {
    setActionLoading(true);
    try { await payrollService.approve(id); toast.success('Payroll approved'); loadPayrolls(); }
    catch (e) { toast.error(e.response?.data?.message || 'Failed'); }
    finally { setActionLoading(false); }
  };

  const handleReject = async () => {
    setActionLoading(true);
    try { await payrollService.reject(rejectModal, { remarks: rejectRemarks }); toast.success('Payroll rejected'); setRejectModal(null); setRejectRemarks(''); loadPayrolls(); }
    catch (e) { toast.error(e.response?.data?.message || 'Failed'); }
    finally { setActionLoading(false); }
  };

  const handleMarkPaid = async () => {
    setActionLoading(true);
    try { await payrollService.markAsPaid(payDateModal, { paymentDate }); toast.success('Marked as paid'); setPayDateModal(null); loadPayrolls(); }
    catch (e) { toast.error(e.response?.data?.message || 'Failed'); }
    finally { setActionLoading(false); }
  };

  const handleProcess = async (e) => {
    e.preventDefault();
    setProcessing(true);
    try {
      const res = await payrollService.process({
        employeeId: Number(processForm.employeeId),
        payPeriodStart: processForm.payPeriodStart, payPeriodEnd: processForm.payPeriodEnd,
        bonusAmount: Number(processForm.bonusAmount) || undefined,
        otherDeductions: Number(processForm.otherDeductions) || undefined,
        remarks: processForm.remarks || undefined,
      });
      toast.success('Payroll processed');
      setProcessResult(res.data?.data || res.data);
    } catch (e) {
      if (e.response?.status === 409) toast.error('Payroll already processed for this period');
      else toast.error(e.response?.data?.message || 'Failed');
    } finally { setProcessing(false); }
  };

  return (
    <div>
      <div className="page-header">
        <h1 className="page-title">Payroll Management</h1>
        <div className="page-title-accent" />
      </div>

      <div className="tab-nav">
        <button className={`tab-btn ${activeTab === 'list' ? 'active' : ''}`} onClick={() => setActiveTab('list')}>All Payrolls</button>
        <button className={`tab-btn ${activeTab === 'process' ? 'active' : ''}`} onClick={() => { setActiveTab('process'); setProcessResult(null); }}>Process Payroll</button>
      </div>

      {activeTab === 'list' ? (
        <>
          <div className="filter-bar">
            <div className="form-group" style={{ marginBottom: 0 }}>
              <label className="form-label">Status</label>
              <select className="form-input" style={{ minWidth: 140 }} value={filters.status}
                onChange={e => { setFilters(f => ({ ...f, status: e.target.value })); setPage(1); }}>
                <option value="">All</option>
                <option value="Pending">Pending</option>
                <option value="Approved">Approved</option>
                <option value="Paid">Paid</option>
              </select>
            </div>
          </div>

          <div className="card">
            {loading ? (
              <div className="loading-spinner"><div className="spinner" /> Loading...</div>
            ) : payrolls.length === 0 ? (
              <div className="empty-state"><div className="empty-icon">💰</div><div className="empty-title">No payrolls</div></div>
            ) : (
              <div style={{ overflowX: 'auto' }}>
                <table className="data-table">
                  <thead><tr><th>Employee</th><th>Period</th><th>Gross</th><th>Net</th><th>Status</th><th className="text-right">Actions</th></tr></thead>
                  <tbody>
                    {payrolls.map(pr => (
                      <tr key={pr.payrollId}>
                        <td><strong>{pr.employeeName}</strong><div className="sub-text">{pr.employeeCode}</div></td>
                        <td>{formatDate(pr.payPeriodStart)} — {formatDate(pr.payPeriodEnd)}</td>
                        <td className="currency">{formatCurrency(pr.grossEarnings)}</td>
                        <td className="currency" style={{ fontWeight: 700 }}>{formatCurrency(pr.netSalary)}</td>
                        <td><span className={`badge ${getStatusBadge(pr.status)}`}>{pr.status}</span></td>
                        <td className="text-right">
                          <div style={{ display: 'flex', gap: 6, justifyContent: 'flex-end' }}>
                            {pr.status === 'Pending' && <>
                              <button className="btn-ghost" onClick={() => handleApprove(pr.payrollId)} disabled={actionLoading}>Approve</button>
                              <button className="btn-ghost danger" onClick={() => setRejectModal(pr.payrollId)} disabled={actionLoading}>Reject</button>
                            </>}
                            {pr.status === 'Approved' && <button className="btn-ghost" onClick={() => setPayDateModal(pr.payrollId)} disabled={actionLoading}>Mark Paid</button>}
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
        </>
      ) : !processResult ? (
        <div className="card">
          <form onSubmit={handleProcess}>
            <div className="form-grid">
              <div className="form-group full-width">
                <label className="form-label">Employee *</label>
                <select className="form-input" value={processForm.employeeId}
                  onChange={e => setProcessForm(f => ({ ...f, employeeId: e.target.value }))} required>
                  <option value="">Select Employee</option>
                  {employees.map(emp => <option key={emp.employeeId} value={emp.employeeId}>
                    {emp.fullName || `${emp.firstName} ${emp.lastName}`} ({emp.employeeCode})
                  </option>)}
                </select>
              </div>
              <div className="form-group"><label className="form-label">Period Start *</label>
                <input type="date" className="form-input" value={processForm.payPeriodStart}
                  max={processForm.payPeriodEnd || undefined}
                  onChange={e => setProcessForm(f => ({ ...f, payPeriodStart: e.target.value }))} required /></div>
              <div className="form-group"><label className="form-label">Period End *</label>
                <input type="date" className="form-input" value={processForm.payPeriodEnd}
                  min={processForm.payPeriodStart || undefined}
                  onChange={e => setProcessForm(f => ({ ...f, payPeriodEnd: e.target.value }))} required /></div>
              <div className="form-group"><label className="form-label">Bonus</label>
                <input type="number" className="form-input" min="0" value={processForm.bonusAmount}
                  onChange={e => setProcessForm(f => ({ ...f, bonusAmount: e.target.value }))} /></div>
              <div className="form-group"><label className="form-label">Other Deductions</label>
                <input type="number" className="form-input" min="0" value={processForm.otherDeductions}
                  onChange={e => setProcessForm(f => ({ ...f, otherDeductions: e.target.value }))} /></div>
            </div>
            <div style={{ display: 'flex', justifyContent: 'flex-end', marginTop: 16 }}>
              <button type="submit" className="btn btn-primary" disabled={processing}>
                {processing ? 'Processing...' : 'Process Payroll'}
              </button>
            </div>
          </form>
        </div>
      ) : (
        <div className="card">
          <div className="card-header">Processing Complete ✅</div>
          <div className="detail-grid">
            <div className="detail-item"><div className="detail-label">Employee</div><div className="detail-value">{processResult.employeeName}</div></div>
            <div className="detail-item"><div className="detail-label">Net Salary</div><div className="detail-value currency" style={{ fontSize: 20, fontWeight: 800 }}>{formatCurrency(processResult.netSalary)}</div></div>
          </div>
          <div style={{ marginTop: 16 }}><button className="btn btn-secondary" onClick={() => setProcessResult(null)}>Process Another</button></div>
        </div>
      )}

      <Modal isOpen={!!rejectModal} onClose={() => setRejectModal(null)} title="Reject Payroll"
        footer={<><button className="btn btn-secondary" onClick={() => setRejectModal(null)}>Cancel</button>
          <button className="btn btn-danger" onClick={handleReject} disabled={actionLoading || !rejectRemarks.trim()}>{actionLoading ? '...' : 'Reject'}</button></>}>
        <div className="form-group"><label className="form-label">Reason *</label>
          <textarea className="form-input" rows={3} value={rejectRemarks} onChange={e => setRejectRemarks(e.target.value)} /></div>
      </Modal>

      <Modal isOpen={!!payDateModal} onClose={() => setPayDateModal(null)} title="Mark as Paid"
        footer={<><button className="btn btn-secondary" onClick={() => setPayDateModal(null)}>Cancel</button>
          <button className="btn btn-primary" onClick={handleMarkPaid} disabled={actionLoading}>{actionLoading ? '...' : 'Confirm'}</button></>}>
        <div className="form-group"><label className="form-label">Payment Date</label>
          <input type="date" className="form-input" value={paymentDate} onChange={e => setPaymentDate(e.target.value)} /></div>
      </Modal>
    </div>
  );
};

export default ProcessorPayroll;
