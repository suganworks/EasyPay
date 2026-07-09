import React, { useState, useEffect } from 'react';
import { useAuth } from '../../context/AuthContext';
import { useToast } from '../../context/ToastContext';
import { payrollService } from '../../api/payrollService';

const formatCurrency = (val) => {
  if (val == null || isNaN(val)) return '—';
  return new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(val);
};

const ProcessorDashboard = () => {
  const { user } = useAuth();
  const toast = useToast();
  const [stats, setStats] = useState({ pending: 0, approved: 0, paid: 0 });
  const [recentPayrolls, setRecentPayrolls] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => { loadData(); }, []);

  const loadData = async () => {
    setLoading(true);
    try {
      const pendingRes = await payrollService.getAll({ status: 'Pending', PageNumber: 1, PageSize: 1 });
      const approvedRes = await payrollService.getAll({ status: 'Approved', PageNumber: 1, PageSize: 1 });
      const paidRes = await payrollService.getAll({ status: 'Paid', PageNumber: 1, PageSize: 1 });
      setStats({
        pending: pendingRes.data?.totalCount ?? 0,
        approved: approvedRes.data?.totalCount ?? 0,
        paid: paidRes.data?.totalCount ?? 0,
      });
    } catch (e) { /* ignore */ }
    try {
      const prRes = await payrollService.getAll({ PageNumber: 1, PageSize: 5 });
      setRecentPayrolls(prRes.data?.data || []);
    } catch (e) { /* ignore */ }
    setLoading(false);
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

  return (
    <div>
      <div className="page-header">
        <h1 className="page-title">Welcome, {user?.fullName || 'Processor'}</h1>
        <div className="page-title-accent" />
        <p className="page-subtitle">Payroll processing dashboard</p>
      </div>

      {loading ? (
        <div className="loading-spinner"><div className="spinner" /> Loading...</div>
      ) : (
        <>
          <div className="stat-grid">
            <div className="stat-card accent-yellow">
              <div className="stat-card-value">{stats.pending}</div>
              <div className="stat-card-label">Pending Payrolls</div>
            </div>
            <div className="stat-card accent-green">
              <div className="stat-card-value">{stats.approved}</div>
              <div className="stat-card-label">Approved</div>
            </div>
            <div className="stat-card accent-blue">
              <div className="stat-card-value">{stats.paid}</div>
              <div className="stat-card-label">Paid</div>
            </div>
          </div>

          <div className="card">
            <div className="card-header">Recent Payroll Records</div>
            {recentPayrolls.length === 0 ? (
              <div className="empty-state" style={{ padding: 24 }}>
                <div className="empty-message">No payroll records found.</div>
              </div>
            ) : (
              <table className="data-table">
                <thead><tr><th>Employee</th><th>Period</th><th>Net Salary</th><th>Status</th></tr></thead>
                <tbody>
                  {recentPayrolls.map(pr => (
                    <tr key={pr.payrollId}>
                      <td><strong>{pr.employeeName}</strong><div className="sub-text">{pr.employeeCode}</div></td>
                      <td>{formatDate(pr.payPeriodStart)} — {formatDate(pr.payPeriodEnd)}</td>
                      <td className="currency" style={{ fontWeight: 700 }}>{formatCurrency(pr.netSalary)}</td>
                      <td><span className={`badge ${getStatusBadge(pr.status)}`}>{pr.status}</span></td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        </>
      )}
    </div>
  );
};

export default ProcessorDashboard;
