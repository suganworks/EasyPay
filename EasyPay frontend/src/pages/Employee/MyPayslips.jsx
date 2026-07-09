import React, { useState, useEffect } from 'react';
import { useAuth } from '../../context/AuthContext';
import { useToast } from '../../context/ToastContext';
import { payrollService } from '../../api/payrollService';

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

const MyPayslips = () => {
  const { user } = useAuth();
  const toast = useToast();
  const [payslips, setPayslips] = useState([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [detail, setDetail] = useState(null);

  const hasProfile = !!user?.employeeId;

  useEffect(() => { if (hasProfile) loadPayslips(); else setLoading(false); }, [page]);

  const loadPayslips = async () => {
    setLoading(true);
    try {
      const res = await payrollService.getMyPayrolls({ PageNumber: page, PageSize: 10 });
      setPayslips(res.data?.data || []);
      setTotalPages(res.data?.totalPages || 1);
    } catch (e) { toast.error('Failed to load payslips'); }
    setLoading(false);
  };

  const viewDetail = async (id) => {
    try {
      const res = await payrollService.getById(id);
      setDetail(res.data?.data || res.data);
    } catch (e) {
      toast.error('Failed to load payslip detail');
    }
  };

  if (!hasProfile) {
    return (
      <div>
        <div className="page-header"><h1 className="page-title">My Payslips</h1><div className="page-title-accent" /></div>
        <div className="no-profile-box">
          <div className="empty-icon">👤</div><div className="empty-title">No Employee Profile</div>
          <div className="empty-message">Your account is not linked to an employee profile.</div>
        </div>
      </div>
    );
  }

  return (
    <div>
      <div className="page-header">
        <h1 className="page-title">My Payslips</h1>
        <div className="page-title-accent" />
        <p className="page-subtitle">View your salary history and pay stubs</p>
      </div>

      {detail ? (
        <div>
          <button className="btn btn-secondary" onClick={() => setDetail(null)} style={{ marginBottom: 20 }}>← Back to List</button>

          <div className="card" style={{ marginBottom: 20 }}>
            <div className="card-header">
              Payslip: {formatDate(detail.payPeriodStart)} — {formatDate(detail.payPeriodEnd)}
              <span className={`badge ${getStatusBadge(detail.status)}`} style={{ float: 'right' }}>{detail.status}</span>
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 20 }}>
              {/* Earnings */}
              <div>
                <h4 style={{ fontFamily: 'var(--font-heading)', fontSize: 14, fontWeight: 700, marginBottom: 12, color: 'var(--success)' }}>💰 Earnings</h4>
                <table className="data-table">
                  <tbody>
                    <tr><td>Basic Salary</td><td className="text-right currency">{formatCurrency(detail.basicSalary)}</td></tr>
                    <tr><td>HRA</td><td className="text-right currency">{formatCurrency(detail.hra)}</td></tr>
                    <tr><td>Conveyance</td><td className="text-right currency">{formatCurrency(detail.conveyanceAllowance)}</td></tr>
                    <tr><td>Medical</td><td className="text-right currency">{formatCurrency(detail.medicalAllowance)}</td></tr>
                    <tr><td>Special Allowance</td><td className="text-right currency">{formatCurrency(detail.specialAllowance)}</td></tr>
                    {detail.bonus > 0 && <tr><td>Bonus</td><td className="text-right currency">{formatCurrency(detail.bonus)}</td></tr>}
                    <tr style={{ fontWeight: 700, borderTop: '2px solid var(--hex-blue)' }}>
                      <td>Gross Earnings</td><td className="text-right currency">{formatCurrency(detail.grossEarnings)}</td>
                    </tr>
                  </tbody>
                </table>
              </div>

              {/* Deductions */}
              <div>
                <h4 style={{ fontFamily: 'var(--font-heading)', fontSize: 14, fontWeight: 700, marginBottom: 12, color: 'var(--danger)' }}>📉 Deductions</h4>
                <table className="data-table">
                  <tbody>
                    <tr><td>PF (Employee)</td><td className="text-right currency">{formatCurrency(detail.pfEmployee)}</td></tr>
                    <tr><td>ESI (Employee)</td><td className="text-right currency">{formatCurrency(detail.esiEmployee)}</td></tr>
                    <tr><td>Income Tax</td><td className="text-right currency">{formatCurrency(detail.incomeTax)}</td></tr>
                    <tr><td>Professional Tax</td><td className="text-right currency">{formatCurrency(detail.professionalTax)}</td></tr>
                    {detail.otherDeductions > 0 && <tr><td>Other Deductions</td><td className="text-right currency">{formatCurrency(detail.otherDeductions)}</td></tr>}
                    <tr style={{ fontWeight: 700, borderTop: '2px solid var(--danger)' }}>
                      <td>Total Deductions</td><td className="text-right currency">{formatCurrency(detail.totalDeductions)}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>

            {/* Net Salary */}
            <div style={{
              marginTop: 24, padding: 20, background: 'var(--hex-blue-pale)', borderRadius: 12,
              display: 'flex', justifyContent: 'space-between', alignItems: 'center',
              border: '1px solid var(--hex-blue)',
            }}>
              <span style={{ fontSize: 14, fontWeight: 600, color: 'var(--hex-blue)', textTransform: 'uppercase', letterSpacing: '0.04em' }}>
                Net Salary
              </span>
              <span style={{ fontSize: 28, fontWeight: 800, fontFamily: 'var(--font-heading)', color: 'var(--hex-blue)' }}>
                {formatCurrency(detail.netSalary)}
              </span>
            </div>
          </div>
        </div>
      ) : (
        <div className="card">
          {loading ? (
            <div className="loading-spinner"><div className="spinner" /> Loading...</div>
          ) : payslips.length === 0 ? (
            <div className="empty-state">
              <div className="empty-icon">📄</div>
              <div className="empty-title">No payslips</div>
              <div className="empty-message">Your payslips will appear here once payroll is processed.</div>
            </div>
          ) : (
            <div style={{ overflowX: 'auto' }}>
              <table className="data-table">
                <thead><tr><th>Period</th><th>Gross</th><th>Deductions</th><th>Net Salary</th><th>Status</th><th className="text-right">Actions</th></tr></thead>
                <tbody>
                  {payslips.map(ps => (
                    <tr key={ps.payrollId}>
                      <td>{formatDate(ps.payPeriodStart)} — {formatDate(ps.payPeriodEnd)}</td>
                      <td className="currency">{formatCurrency(ps.grossEarnings)}</td>
                      <td className="currency">{formatCurrency(ps.totalDeductions)}</td>
                      <td className="currency" style={{ fontWeight: 700 }}>{formatCurrency(ps.netSalary)}</td>
                      <td><span className={`badge ${getStatusBadge(ps.status)}`}>{ps.status}</span></td>
                      <td className="text-right">
                        <button className="btn-ghost" onClick={() => viewDetail(ps.payrollId)}>View Details</button>
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
      )}
    </div>
  );
};

export default MyPayslips;
