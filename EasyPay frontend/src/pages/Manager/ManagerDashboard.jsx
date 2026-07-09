import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { useToast } from '../../context/ToastContext';
import { employeeService } from '../../api/employeeService';
import { leaveService } from '../../api/leaveService';

const ManagerDashboard = () => {
  const { user } = useAuth();
  const toast = useToast();
  const [team, setTeam] = useState([]);
  const [pendingLeaves, setPendingLeaves] = useState([]);
  const [loading, setLoading] = useState(true);

  const hasProfile = !!user?.employeeId;

  useEffect(() => { if (hasProfile) loadData(); else setLoading(false); }, []);

  const loadData = async () => {
    setLoading(true);
    try {
      const teamRes = await employeeService.getMyTeam();
      setTeam(teamRes.data?.data || teamRes.data || []);
    } catch (e) { /* ignore */ }
    try {
      const leaveRes = await leaveService.getPendingForMe();
      setPendingLeaves(leaveRes.data?.data || leaveRes.data || []);
    } catch (e) { /* ignore */ }
    setLoading(false);
  };

  const formatDate = (dateStr) => {
    if (!dateStr) return '—';
    const d = new Date(dateStr + (dateStr.includes('T') ? '' : 'T00:00:00'));
    return d.toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
  };

  const handleLeaveAction = async (id, action, rejectionReason) => {
    try {
      await leaveService.action(id, { action, rejectionReason: action === 'Rejected' ? rejectionReason : undefined });
      toast.success(`Leave ${action.toLowerCase()}`);
      loadData();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Action failed');
    }
  };

  if (!hasProfile) {
    return (
      <div>
        <div className="page-header"><h1 className="page-title">Manager Dashboard</h1><div className="page-title-accent" /></div>
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
        <h1 className="page-title">Welcome, {user?.fullName || 'Manager'}</h1>
        <div className="page-title-accent" />
        <p className="page-subtitle">{new Date().toLocaleDateString('en-IN', { weekday: 'long', month: 'long', day: 'numeric', year: 'numeric' })}</p>
      </div>

      {loading ? (
        <div className="loading-spinner"><div className="spinner" /> Loading...</div>
      ) : (
        <>
          {/* Quick Stats */}
          <div className="stat-grid">
            <div className="stat-card accent-blue">
              <div className="stat-card-value">{team.length}</div>
              <div className="stat-card-label">Team Members</div>
            </div>
            <div className="stat-card accent-yellow">
              <div className="stat-card-value">{pendingLeaves.length}</div>
              <div className="stat-card-label">Pending Leave Requests</div>
            </div>
          </div>

          {/* Pending Leave Requests */}
          {pendingLeaves.length > 0 && (
            <div className="card" style={{ marginBottom: 28 }}>
              <div className="card-header">Pending Leave Requests</div>
              <table className="data-table">
                <thead><tr><th>Employee</th><th>Type</th><th>From</th><th>To</th><th>Days</th><th>Reason</th><th className="text-right">Actions</th></tr></thead>
                <tbody>
                  {pendingLeaves.map(l => (
                    <tr key={l.leaveRequestId}>
                      <td><strong>{l.employeeName}</strong></td>
                      <td><span className="badge badge-info">{l.leaveTypeName}</span></td>
                      <td>{formatDate(l.fromDate)}</td>
                      <td>{formatDate(l.toDate)}</td>
                      <td>{l.totalDays}{l.isHalfDay ? ' (½)' : ''}</td>
                      <td className="text-secondary">{l.reason || '—'}</td>
                      <td className="text-right">
                        <div style={{ display: 'flex', gap: 6, justifyContent: 'flex-end' }}>
                          <button className="btn-ghost" onClick={() => handleLeaveAction(l.leaveRequestId, 'Approved')}>Approve</button>
                          <button className="btn-ghost danger" onClick={() => {
                            const reason = prompt('Rejection reason:');
                            if (reason) handleLeaveAction(l.leaveRequestId, 'Rejected', reason);
                          }}>Reject</button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          {/* Team Members */}
          <div className="card">
            <div className="card-header">My Team</div>
            {team.length === 0 ? (
              <div className="empty-state" style={{ padding: 24 }}>
                <div className="empty-message">No team members found.</div>
              </div>
            ) : (
              <table className="data-table">
                <thead><tr><th>Name</th><th>Code</th><th>Department</th><th>Designation</th><th>Status</th></tr></thead>
                <tbody>
                  {team.map(m => (
                    <tr key={m.employeeId}>
                      <td><strong>{m.fullName || `${m.firstName} ${m.lastName}`}</strong><div className="sub-text">{m.workEmail}</div></td>
                      <td><span className="badge badge-info">{m.employeeCode}</span></td>
                      <td>{m.departmentName || '—'}</td>
                      <td>{m.designationName || '—'}</td>
                      <td><span className={`badge ${m.employmentStatus === 'Active' ? 'badge-success' : 'badge-danger'}`}>{m.employmentStatus || 'Active'}</span></td>
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

export default ManagerDashboard;
