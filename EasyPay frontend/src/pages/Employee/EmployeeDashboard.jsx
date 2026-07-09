import React, { useState, useEffect } from 'react';
import { useAuth } from '../../context/AuthContext';
import { useToast } from '../../context/ToastContext';
import { leaveService } from '../../api/leaveService';
import { payrollService } from '../../api/payrollService';
import { salaryStructureService } from '../../api/services';

const formatCurrency = (val) => {
  if (val == null || isNaN(val)) return '—';
  return new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(val);
};

const EmployeeDashboard = () => {
  const { user } = useAuth();
  const toast = useToast();
  const [leaveBalance, setLeaveBalance] = useState([]);
  const [salary, setSalary] = useState(null);
  const [recentPayrolls, setRecentPayrolls] = useState([]);
  const [loading, setLoading] = useState(true);

  const hasProfile = !!user?.employeeId;

  useEffect(() => {
    if (hasProfile) loadData();
    else setLoading(false);
  }, []);

  const loadData = async () => {
    setLoading(true);
    try {
      const lbRes = await leaveService.getMyBalance(new Date().getFullYear());
      setLeaveBalance(lbRes.data?.data || lbRes.data || []);
    } catch (e) { /* ignore */ }
    try {
      const salRes = await salaryStructureService.getMySalary();
      setSalary(salRes.data?.data || salRes.data);
    } catch (e) { /* ignore */ }
    try {
      const prRes = await payrollService.getMyPayrolls({ PageNumber: 1, PageSize: 5 });
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

  if (!hasProfile) {
    return (
      <div>
        <div className="page-header">
          <h1 className="page-title">My Dashboard</h1>
          <div className="page-title-accent" />
        </div>
        <div className="no-profile-box">
          <div className="empty-icon">👤</div>
          <div className="empty-title">No Employee Profile</div>
          <div className="empty-message">Your account is not linked to an employee profile. Self-service features are unavailable.</div>
        </div>
      </div>
    );
  }

  return (
    <div>
      <div className="page-header">
        <h1 className="page-title">Welcome, {user?.fullName || user?.username || 'Employee'}</h1>
        <div className="page-title-accent" />
        <p className="page-subtitle">{new Date().toLocaleDateString('en-IN', { weekday: 'long', month: 'long', day: 'numeric', year: 'numeric' })}</p>
      </div>

      {loading ? (
        <div className="loading-spinner"><div className="spinner" /> Loading dashboard...</div>
      ) : (
        <>
          {/* Leave Balance */}
          {leaveBalance.length > 0 && (
            <div style={{ marginBottom: 28 }}>
              <h3 className="section-title">Leave Balance</h3>
              <div className="stat-grid">
                {leaveBalance.map(lb => (
                  <div key={lb.leaveTypeId} className="stat-card accent-blue">
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                      <div>
                        <div className="stat-card-value">{lb.remainingDays}</div>
                        <div className="stat-card-label">{lb.leaveTypeName}</div>
                      </div>
                      <div style={{ fontSize: 12, color: 'var(--text-secondary)', textAlign: 'right' }}>
                        <div>{lb.usedDays} used</div>
                        <div>{lb.maxDaysPerYear} total</div>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Salary Info */}
          {salary && (
            <div className="card" style={{ marginBottom: 28 }}>
              <div className="card-header">Current Salary</div>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 16 }}>
                <div>
                  <div style={{ fontSize: 13, color: 'var(--text-secondary)', textTransform: 'uppercase', letterSpacing: '0.04em' }}>Gross Monthly</div>
                  <div style={{ fontSize: 28, fontWeight: 800, fontFamily: 'var(--font-heading)', color: 'var(--hex-blue)' }}>{formatCurrency(salary.grossSalary)}</div>
                </div>
                <div className="detail-grid" style={{ flex: 1, maxWidth: 400 }}>
                  <div className="detail-item"><div className="detail-label">Basic</div><div className="detail-value currency">{formatCurrency(salary.basicSalary)}</div></div>
                  <div className="detail-item"><div className="detail-label">HRA</div><div className="detail-value currency">{formatCurrency(salary.hra)}</div></div>
                </div>
              </div>
            </div>
          )}

          {/* Recent Pay Stubs */}
          <div className="card">
            <div className="card-header">Recent Pay Stubs</div>
            {recentPayrolls.length === 0 ? (
              <div className="empty-state" style={{ padding: 24 }}>
                <div className="empty-message">No pay stubs available yet.</div>
              </div>
            ) : (
              <table className="data-table">
                <thead>
                  <tr><th>Period</th><th>Gross</th><th>Deductions</th><th>Net</th><th>Status</th></tr>
                </thead>
                <tbody>
                  {recentPayrolls.map(pr => (
                    <tr key={pr.payrollId}>
                      <td>{formatDate(pr.payPeriodStart)} — {formatDate(pr.payPeriodEnd)}</td>
                      <td className="currency">{formatCurrency(pr.grossEarnings)}</td>
                      <td className="currency">{formatCurrency(pr.totalDeductions)}</td>
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

export default EmployeeDashboard;
