import React, { useState, useEffect } from 'react';
import { useAuth } from '../../context/AuthContext';
import { useToast } from '../../context/ToastContext';
import { employeeService } from '../../api/employeeService';

const formatDate = (dateStr) => {
  if (!dateStr) return '—';
  const d = new Date(dateStr + (dateStr.includes('T') ? '' : 'T00:00:00'));
  return d.toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
};

const MyProfile = () => {
  const { user } = useAuth();
  const toast = useToast();
  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState(false);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState({});

  const hasProfile = !!user?.employeeId;

  useEffect(() => { if (hasProfile) loadProfile(); else setLoading(false); }, []);

  const loadProfile = async () => {
    setLoading(true);
    try {
      const res = await employeeService.getById(user.employeeId);
      const p = res.data?.data || res.data;
      setProfile(p);
    } catch (e) {
      toast.error('Failed to load profile');
    } finally { setLoading(false); }
  };

  const startEdit = () => {
    setForm({
      phone: profile.phone || '', personalEmail: profile.personalEmail || '',
      address: profile.address || '', city: profile.city || '',
      state: profile.state || '', postalCode: profile.postalCode || '',
      bankName: profile.bankName || '', bankAccountNo: profile.bankAccountNo || '',
      bankIFSC: profile.bankIFSC || '',
    });
    setEditing(true);
  };

  const handleSave = async () => {
    setSaving(true);
    try {
      await employeeService.update(user.employeeId, {
        firstName: profile.firstName, lastName: profile.lastName,
        departmentId: profile.departmentId, designationId: profile.designationId,
        employmentType: profile.employmentType,
        phone: form.phone || undefined,
        personalEmail: form.personalEmail || undefined,
        address: form.address || undefined, city: form.city || undefined,
        state: form.state || undefined, postalCode: form.postalCode || undefined,
        bankName: form.bankName || undefined,
        bankAccountNo: form.bankAccountNo || undefined,
        bankIFSC: form.bankIFSC || undefined,
      });
      toast.success('Profile updated');
      setEditing(false);
      loadProfile();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Failed to update profile');
    } finally { setSaving(false); }
  };

  if (!hasProfile) {
    return (
      <div>
        <div className="page-header"><h1 className="page-title">My Profile</h1><div className="page-title-accent" /></div>
        <div className="no-profile-box">
          <div className="empty-icon">👤</div><div className="empty-title">No Employee Profile</div>
          <div className="empty-message">Your account is not linked to an employee profile. Contact HR for assistance.</div>
        </div>
      </div>
    );
  }

  if (loading) return <div className="loading-spinner"><div className="spinner" /> Loading profile...</div>;
  if (!profile) return <div className="empty-state"><div className="empty-title">Profile not found</div></div>;

  return (
    <div>
      <div className="page-actions">
        <div className="page-header" style={{ marginBottom: 0 }}>
          <h1 className="page-title">My Profile</h1>
          <div className="page-title-accent" />
        </div>
        {!editing && <button className="btn btn-secondary" onClick={startEdit}>Edit Personal Info</button>}
      </div>

      {/* Employee Card Header */}
      <div className="card" style={{ marginBottom: 20 }}>
        <div style={{ display: 'flex', gap: 24, alignItems: 'center' }}>
          <div style={{
            width: 64, height: 64, borderRadius: 16,
            background: 'linear-gradient(135deg, var(--hex-blue) 0%, var(--hex-blue-light) 100%)',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            fontSize: 24, fontWeight: 800, color: '#FFFFFF',
          }}>
            {(profile.firstName?.[0] || '') + (profile.lastName?.[0] || '')}
          </div>
          <div>
            <div style={{ fontFamily: 'var(--font-heading)', fontSize: 22, fontWeight: 800, color: 'var(--text-primary)' }}>
              {profile.fullName || `${profile.firstName} ${profile.lastName}`}
            </div>
            <div style={{ display: 'flex', gap: 12, marginTop: 4 }}>
              <span className="badge badge-info">{profile.employeeCode}</span>
              <span className="badge badge-success">{profile.designationName}</span>
              <span className="badge badge-neutral">{profile.departmentName}</span>
            </div>
          </div>
        </div>
      </div>

      {/* Personal Info */}
      <div className="card" style={{ marginBottom: 20 }}>
        <div className="card-header">{editing ? 'Edit Personal Information' : 'Personal Information'}</div>
        {editing ? (
          <div className="form-grid">
            <div className="form-group">
              <label className="form-label">Phone</label>
              <input type="text" className="form-input" value={form.phone}
                onChange={e => setForm(f => ({ ...f, phone: e.target.value }))} />
            </div>
            <div className="form-group">
              <label className="form-label">Personal Email</label>
              <input type="email" className="form-input" value={form.personalEmail}
                onChange={e => setForm(f => ({ ...f, personalEmail: e.target.value }))} />
            </div>
            <div className="form-group full-width">
              <label className="form-label">Address</label>
              <input type="text" className="form-input" value={form.address}
                onChange={e => setForm(f => ({ ...f, address: e.target.value }))} />
            </div>
            <div className="form-group">
              <label className="form-label">City</label>
              <input type="text" className="form-input" value={form.city}
                onChange={e => setForm(f => ({ ...f, city: e.target.value }))} />
            </div>
            <div className="form-group">
              <label className="form-label">State</label>
              <input type="text" className="form-input" value={form.state}
                onChange={e => setForm(f => ({ ...f, state: e.target.value }))} />
            </div>
            <div className="form-group">
              <label className="form-label">Postal Code</label>
              <input type="text" className="form-input" value={form.postalCode}
                onChange={e => setForm(f => ({ ...f, postalCode: e.target.value }))} />
            </div>
            <div className="form-group">
              <label className="form-label">Bank Name</label>
              <input type="text" className="form-input" value={form.bankName}
                onChange={e => setForm(f => ({ ...f, bankName: e.target.value }))} />
            </div>
            <div className="form-group">
              <label className="form-label">Account Number</label>
              <input type="text" className="form-input" value={form.bankAccountNo}
                onChange={e => setForm(f => ({ ...f, bankAccountNo: e.target.value }))} />
            </div>
            <div className="form-group">
              <label className="form-label">IFSC Code</label>
              <input type="text" className="form-input" value={form.bankIFSC}
                onChange={e => setForm(f => ({ ...f, bankIFSC: e.target.value }))} />
            </div>
            <div className="full-width" style={{ display: 'flex', gap: 12, justifyContent: 'flex-end', paddingTop: 8 }}>
              <button className="btn btn-secondary" onClick={() => setEditing(false)} disabled={saving}>Cancel</button>
              <button className="btn btn-primary" onClick={handleSave} disabled={saving}>
                {saving ? 'Saving...' : 'Save Changes'}
              </button>
            </div>
          </div>
        ) : (
          <div className="detail-grid">
            <div className="detail-item"><div className="detail-label">Work Email</div><div className="detail-value">{profile.workEmail || '—'}</div></div>
            <div className="detail-item"><div className="detail-label">Personal Email</div><div className="detail-value">{profile.personalEmail || '—'}</div></div>
            <div className="detail-item"><div className="detail-label">Phone</div><div className="detail-value">{profile.phone || '—'}</div></div>
            <div className="detail-item"><div className="detail-label">Gender</div><div className="detail-value">{profile.gender || '—'}</div></div>
            <div className="detail-item"><div className="detail-label">Date of Birth</div><div className="detail-value">{formatDate(profile.dateOfBirth)}</div></div>
            <div className="detail-item"><div className="detail-label">Joining Date</div><div className="detail-value">{formatDate(profile.joiningDate)}</div></div>
            <div className="detail-item"><div className="detail-label">Manager</div><div className="detail-value">{profile.managerName || 'None'}</div></div>
            <div className="detail-item"><div className="detail-label">Employment Type</div><div className="detail-value">{profile.employmentType}</div></div>
            <div className="detail-item"><div className="detail-label">Address</div><div className="detail-value">{[profile.address, profile.city, profile.state].filter(Boolean).join(', ') || '—'}</div></div>
            <div className="detail-item"><div className="detail-label">Bank Name</div><div className="detail-value">{profile.bankName || '—'}</div></div>
          </div>
        )}
      </div>
    </div>
  );
};

export default MyProfile;
