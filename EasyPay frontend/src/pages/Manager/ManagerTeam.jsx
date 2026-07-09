import React, { useState, useEffect } from 'react';
import { useAuth } from '../../context/AuthContext';
import { useToast } from '../../context/ToastContext';
import { employeeService } from '../../api/employeeService';
import { leaveService } from '../../api/leaveService';

const formatDate = (dateStr) => {
  if (!dateStr) return '—';
  const d = new Date(dateStr + (dateStr.includes('T') ? '' : 'T00:00:00'));
  return d.toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
};

const ManagerTeam = () => {
  const { user } = useAuth();
  const toast = useToast();
  const [team, setTeam] = useState([]);
  const [loading, setLoading] = useState(true);
  const [selectedMember, setSelectedMember] = useState(null);
  const [memberLeaves, setMemberLeaves] = useState([]);
  const [leaveLoading, setLeaveLoading] = useState(false);

  const hasProfile = !!user?.employeeId;

  useEffect(() => { if (hasProfile) loadTeam(); else setLoading(false); }, []);

  const loadTeam = async () => {
    setLoading(true);
    try {
      const res = await employeeService.getMyTeam();
      setTeam(res.data?.data || res.data || []);
    } catch (e) {
      toast.error('Failed to load team');
    } finally { setLoading(false); }
  };

  const viewMemberLeaves = async (member) => {
    setSelectedMember(member);
    setLeaveLoading(true);
    try {
      const res = await leaveService.getEmployeeBalance(member.employeeId, new Date().getFullYear());
      setMemberLeaves(res.data?.data || res.data || []);
    } catch (e) {
      setMemberLeaves([]);
    } finally { setLeaveLoading(false); }
  };

  if (!hasProfile) {
    return (
      <div>
        <div className="page-header"><h1 className="page-title">My Team</h1><div className="page-title-accent" /></div>
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
        <h1 className="page-title">My Team</h1>
        <div className="page-title-accent" />
        <p className="page-subtitle">{team.length} team member{team.length !== 1 ? 's' : ''}</p>
      </div>

      {loading ? (
        <div className="loading-spinner"><div className="spinner" /> Loading...</div>
      ) : team.length === 0 ? (
        <div className="card">
          <div className="empty-state">
            <div className="empty-icon">👥</div>
            <div className="empty-title">No team members</div>
            <div className="empty-message">You have no direct reports assigned.</div>
          </div>
        </div>
      ) : (
        <div className="card-grid">
          {team.map(m => (
            <div key={m.employeeId} className="card" style={{ cursor: 'pointer' }}
              onClick={() => viewMemberLeaves(m)}>
              <div style={{ display: 'flex', gap: 14, alignItems: 'center', marginBottom: 14 }}>
                <div style={{
                  width: 44, height: 44, borderRadius: 12,
                  background: 'linear-gradient(135deg, var(--hex-blue), var(--hex-blue-light))',
                  display: 'flex', alignItems: 'center', justifyContent: 'center',
                  fontSize: 14, fontWeight: 800, color: '#FFFFFF',
                }}>
                  {(m.firstName?.[0] || '') + (m.lastName?.[0] || '')}
                </div>
                <div>
                  <div style={{ fontWeight: 700, fontSize: 15 }}>
                    {m.fullName || `${m.firstName} ${m.lastName}`}
                  </div>
                  <div className="sub-text">{m.employeeCode}</div>
                </div>
              </div>
              <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
                <span className="badge badge-info">{m.designationName || '—'}</span>
                <span className={`badge ${m.employmentStatus === 'Active' ? 'badge-success' : 'badge-danger'}`}>
                  {m.employmentStatus || 'Active'}
                </span>
              </div>
              <div className="sub-text" style={{ marginTop: 8 }}>
                Joined: {formatDate(m.joiningDate)}
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Selected member leave balance */}
      {selectedMember && (
        <div className="card" style={{ marginTop: 28 }}>
          <div className="card-header">
            Leave Balance — {selectedMember.fullName || selectedMember.firstName}
            <button className="btn-ghost" style={{ float: 'right' }} onClick={() => setSelectedMember(null)}>✕</button>
          </div>
          {leaveLoading ? (
            <div className="loading-spinner"><div className="spinner" /> Loading...</div>
          ) : memberLeaves.length === 0 ? (
            <div className="empty-state" style={{ padding: 20 }}><div className="empty-message">No leave balance data.</div></div>
          ) : (
            <div className="stat-grid">
              {memberLeaves.map(lb => (
                <div key={lb.leaveTypeId} className="stat-card accent-blue">
                  <div className="stat-card-value">{lb.remainingDays}</div>
                  <div className="stat-card-label">{lb.leaveTypeName}</div>
                  <div className="sub-text">{lb.usedDays}/{lb.maxDaysPerYear} used</div>
                </div>
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  );
};

export default ManagerTeam;
