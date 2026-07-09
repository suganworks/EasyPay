import React, { useState, useEffect } from 'react';
import { useAuth } from '../../context/AuthContext';
import { useToast } from '../../context/ToastContext';
import { benefitService } from '../../api/services';

const formatCurrency = (val) => {
  if (val == null || isNaN(val)) return '—';
  return new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(val);
};
const formatDate = (dateStr) => {
  if (!dateStr) return '—';
  const d = new Date(dateStr + (dateStr.includes('T') ? '' : 'T00:00:00'));
  return d.toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
};

const MyBenefits = () => {
  const { user } = useAuth();
  const toast = useToast();
  const [benefits, setBenefits] = useState([]);
  const [loading, setLoading] = useState(true);

  const hasProfile = !!user?.employeeId;

  useEffect(() => { if (hasProfile) loadBenefits(); else setLoading(false); }, []);

  const loadBenefits = async () => {
    setLoading(true);
    try {
      const res = await benefitService.getMyBenefits();
      setBenefits(res.data?.data || res.data || []);
    } catch (e) { toast.error('Failed to load benefits'); }
    setLoading(false);
  };

  if (!hasProfile) {
    return (
      <div>
        <div className="page-header"><h1 className="page-title">My Benefits</h1><div className="page-title-accent" /></div>
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
        <h1 className="page-title">My Benefits</h1>
        <div className="page-title-accent" />
        <p className="page-subtitle">View your assigned benefits and perks</p>
      </div>

      <div className="card">
        {loading ? (
          <div className="loading-spinner"><div className="spinner" /> Loading...</div>
        ) : benefits.length === 0 ? (
          <div className="empty-state">
            <div className="empty-icon">🎁</div>
            <div className="empty-title">No benefits assigned</div>
            <div className="empty-message">Contact HR for information about available benefits.</div>
          </div>
        ) : (
          <div className="card-grid">
            {benefits.map(b => (
              <div key={b.employeeBenefitId} className="card" style={{ border: b.isActive ? '1px solid var(--success)' : '1px solid var(--border)' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 12 }}>
                  <div>
                    <div style={{ fontWeight: 700, fontSize: 15 }}>{b.benefitName}</div>
                    <div className="sub-text">{b.benefitType}</div>
                  </div>
                  <span className={`badge ${b.isActive ? 'badge-success' : 'badge-danger'}`}>
                    {b.isActive ? 'Active' : 'Ended'}
                  </span>
                </div>
                <div className="detail-grid">
                  <div className="detail-item">
                    <div className="detail-label">Amount</div>
                    <div className="detail-value currency">{formatCurrency(b.effectiveAmount)}</div>
                  </div>
                  <div className="detail-item">
                    <div className="detail-label">Effective From</div>
                    <div className="detail-value">{formatDate(b.effectiveFrom)}</div>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default MyBenefits;
