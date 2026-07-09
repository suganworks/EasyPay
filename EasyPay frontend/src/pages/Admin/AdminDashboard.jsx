import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { reportService } from '../../api/services';
import { employeeService } from '../../api/employeeService';
import { leaveService } from '../../api/leaveService';
import { payrollService } from '../../api/payrollService';
import { useAuth } from '../../context/AuthContext';
import { useToast } from '../../context/ToastContext';

const formatCurrency = (val) => {
  if (val == null || isNaN(val)) return '—';
  return new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(val);
};

const getStatusBadge = (status) => {
  const map = {
    Approved: 'badge-success', Pending: 'badge-warning', Paid: 'badge-success',
    Rejected: 'badge-danger', Cancelled: 'badge-danger', Processing: 'badge-info',
  };
  return map[status] || 'badge-info';
};

const AdminDashboard = () => {
  const { user } = useAuth();
  const toast = useToast();
  const [stats, setStats] = useState({ totalEmployees: '—', pendingLeaves: '—', payrollCount: '—' });
  const [recentPayrolls, setRecentPayrolls] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => { loadDashboardData(); }, []);

  const loadDashboardData = async () => {
    setLoading(true);
    try {
      const empRes = await employeeService.getAll({ PageNumber: 1, PageSize: 1 });
      setStats(prev => ({ ...prev, totalEmployees: empRes.data?.totalCount ?? '—' }));
    } catch (e) { /* optional */ }
    try {
      const leaveRes = await leaveService.getAll({ status: 'Pending', PageNumber: 1, PageSize: 1 });
      setStats(prev => ({ ...prev, pendingLeaves: leaveRes.data?.totalCount ?? '—' }));
    } catch (e) { /* optional */ }
    try {
      const now = new Date();
      const summaryRes = await reportService.payrollStatusDashboard({ year: now.getFullYear(), month: now.getMonth() + 1 });
      const summary = summaryRes.data?.data || summaryRes.data;
      setStats(prev => ({ ...prev, payrollCount: summary?.totalCount ?? '—' }));
    } catch (e) { /* optional */ }
    try {
      const prRes = await payrollService.getAll({ PageNumber: 1, PageSize: 5 });
      setRecentPayrolls(prRes.data?.data || []);
    } catch (e) { /* optional */ }
    setLoading(false);
  };

  const formatDate = (dateStr) => {
    if (!dateStr) return '—';
    const d = new Date(dateStr + 'T00:00:00');
    return d.toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
  };

  return (
    <div>
      {/* Page Header */}
      <div className="page-header">
        <h1 className="page-title">
          Welcome back, {user?.fullName || user?.username || 'Admin'}
        </h1>
        <div className="page-title-accent" />
        <p className="page-subtitle">
          {new Date().toLocaleDateString('en-IN', { weekday: 'long', month: 'long', day: 'numeric', year: 'numeric' })}
        </p>
      </div>

      {/* Stat Cards */}
      <div className="stat-grid">
        <div className="stat-card accent-blue">
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
            <div>
              <div className="stat-card-value">{loading ? '...' : stats.totalEmployees}</div>
              <div className="stat-card-label">Total Employees</div>
            </div>
            <div className="stat-card-icon">👥</div>
          </div>
        </div>
        <div className="stat-card accent-yellow">
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
            <div>
              <div className="stat-card-value">{loading ? '...' : stats.pendingLeaves}</div>
              <div className="stat-card-label">Pending Leave Requests</div>
            </div>
            <div className="stat-card-icon">📋</div>
          </div>
        </div>
        <div className="stat-card accent-green">
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
            <div>
              <div className="stat-card-value">{loading ? '...' : stats.payrollCount}</div>
              <div className="stat-card-label">Payrolls This Month</div>
            </div>
            <div className="stat-card-icon">💰</div>
          </div>
        </div>
      </div>

      {/* Quick Actions */}
      <div style={{ display: 'flex', gap: 12, marginBottom: 28 }}>
        <Link to="/admin/payroll/process" className="btn btn-primary">Process Payroll</Link>
        <Link to="/admin/employees/new" className="btn btn-secondary">Add Employee</Link>
        <Link to="/admin/reports" className="btn btn-secondary">View Reports</Link>
      </div>

      {/* Recent Payrolls */}
      <div className="card">
        <div className="card-header">Recent Payroll Records</div>
        {loading ? (
          <div className="loading-spinner">
            <div className="spinner" />
            Loading...
          </div>
        ) : recentPayrolls.length === 0 ? (
          <div className="empty-state">
            <div className="empty-icon">📊</div>
            <div className="empty-title">No payroll records yet</div>
            <div className="empty-message">Process your first payroll to see data here.</div>
          </div>
        ) : (
          <div style={{ overflowX: 'auto' }}>
            <table className="data-table">
              <thead>
                <tr>
                  <th>Employee</th>
                  <th>Department</th>
                  <th>Period</th>
                  <th>Net Salary</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {recentPayrolls.map(pr => (
                  <tr key={pr.payrollId}>
                    <td>
                      <strong>{pr.employeeName || '—'}</strong>
                      <div className="sub-text">{pr.employeeCode || ''}</div>
                    </td>
                    <td>{pr.departmentName || '—'}</td>
                    <td>
                      {pr.payPeriodStart && pr.payPeriodEnd
                        ? `${formatDate(pr.payPeriodStart)} — ${formatDate(pr.payPeriodEnd)}`
                        : '—'}
                    </td>
                    <td className="currency">{formatCurrency(pr.netSalary)}</td>
                    <td>
                      <span className={`badge ${getStatusBadge(pr.status)}`}>
                        {pr.status || '—'}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
};

export default AdminDashboard;
