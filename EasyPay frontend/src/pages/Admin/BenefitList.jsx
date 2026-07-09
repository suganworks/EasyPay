import React, { useState, useEffect } from 'react';
import { benefitService } from '../../api/services';
import Modal from '../../components/Modal';
import ConfirmDialog from '../../components/ConfirmDialog';
import { useToast } from '../../context/ToastContext';

const formatCurrency = (val) => {
  if (val == null || isNaN(val)) return '—';
  return new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(val);
};

const BenefitList = () => {
  const [benefits, setBenefits] = useState([]);
  const [loading, setLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState(null);
  const [formData, setFormData] = useState({
    benefitName: '', benefitCode: '', benefitType: 'Allowance', amount: '', isPercentage: false, description: ''
  });
  const [saving, setSaving] = useState(false);
  const toast = useToast();

  useEffect(() => { fetchBenefits(); }, []);

  const fetchBenefits = async () => {
    setLoading(true);
    try {
      const res = await benefitService.getAll();
      setBenefits(res.data?.data || res.data || []);
    } catch (e) {
      toast.error('Failed to load benefits');
    } finally { setLoading(false); }
  };

  const openCreate = () => {
    setEditing(null);
    setFormData({ benefitName: '', benefitCode: '', benefitType: 'Allowance', amount: '', isPercentage: false, description: '' });
    setModalOpen(true);
  };

  const openEdit = (b) => {
    setEditing(b);
    setFormData({
      benefitName: b.benefitName, benefitCode: b.benefitCode,
      benefitType: b.benefitType, amount: b.amount,
      isPercentage: b.isPercentage, description: b.description || '',
    });
    setModalOpen(true);
  };

  const handleSave = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      const payload = {
        benefitName: formData.benefitName,
        benefitCode: formData.benefitCode,
        benefitType: formData.benefitType,
        amount: Number(formData.amount),
        isPercentage: formData.isPercentage,
        description: formData.description || undefined,
      };
      if (editing) {
        await benefitService.update(editing.benefitId, payload);
        toast.success('Benefit updated');
      } else {
        await benefitService.create(payload);
        toast.success('Benefit created');
      }
      setModalOpen(false);
      fetchBenefits();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Operation failed');
    } finally { setSaving(false); }
  };

  return (
    <div>
      <div className="page-actions">
        <div className="page-header" style={{ marginBottom: 0 }}>
          <h1 className="page-title">Benefits</h1>
          <div className="page-title-accent" />
          <p className="page-subtitle">{benefits.length} benefit definitions</p>
        </div>
        <button className="btn btn-primary" onClick={openCreate}>+ New Benefit</button>
      </div>

      <div className="card">
        {loading ? (
          <div className="loading-spinner"><div className="spinner" /> Loading...</div>
        ) : benefits.length === 0 ? (
          <div className="empty-state">
            <div className="empty-icon">🎁</div>
            <div className="empty-title">No benefits defined</div>
            <div className="empty-message">Create benefit definitions to assign to employees.</div>
          </div>
        ) : (
          <div style={{ overflowX: 'auto' }}>
            <table className="data-table">
              <thead>
                <tr>
                  <th>Code</th>
                  <th>Name</th>
                  <th>Type</th>
                  <th>Amount</th>
                  <th>Description</th>
                  <th>Status</th>
                  <th className="text-right">Actions</th>
                </tr>
              </thead>
              <tbody>
                {benefits.map(b => (
                  <tr key={b.benefitId}>
                    <td><span className="badge badge-info">{b.benefitCode}</span></td>
                    <td><strong>{b.benefitName}</strong></td>
                    <td>{b.benefitType}</td>
                    <td className="currency">
                      {b.isPercentage ? `${b.amount}%` : formatCurrency(b.amount)}
                    </td>
                    <td className="text-secondary">{b.description || '—'}</td>
                    <td>
                      <span className={`badge ${b.isActive !== false ? 'badge-success' : 'badge-danger'}`}>
                        {b.isActive !== false ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                    <td className="text-right">
                      <button className="btn-ghost" onClick={() => openEdit(b)}>Edit</button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      <Modal isOpen={modalOpen} onClose={() => setModalOpen(false)}
        title={editing ? 'Edit Benefit' : 'New Benefit'}
        footer={<>
          <button className="btn btn-secondary" onClick={() => setModalOpen(false)} disabled={saving}>Cancel</button>
          <button className="btn btn-primary" onClick={handleSave} disabled={saving}>
            {saving ? 'Saving...' : (editing ? 'Update' : 'Create')}
          </button>
        </>}
      >
        <form onSubmit={handleSave}>
          <div className="form-grid">
            <div className="form-group">
              <label className="form-label">Benefit Code</label>
              <input type="text" className="form-input" placeholder="e.g. HI001"
                value={formData.benefitCode}
                onChange={e => setFormData({ ...formData, benefitCode: e.target.value })} required />
            </div>
            <div className="form-group">
              <label className="form-label">Benefit Name</label>
              <input type="text" className="form-input" placeholder="e.g. Health Insurance"
                value={formData.benefitName}
                onChange={e => setFormData({ ...formData, benefitName: e.target.value })} required />
            </div>
            <div className="form-group">
              <label className="form-label">Type</label>
              <select className="form-input" value={formData.benefitType}
                onChange={e => setFormData({ ...formData, benefitType: e.target.value })}>
                <option value="Allowance">Allowance</option>
                <option value="Insurance">Insurance</option>
                <option value="Bonus">Bonus</option>
                <option value="Other">Other</option>
              </select>
            </div>
            <div className="form-group">
              <label className="form-label">Amount {formData.isPercentage ? '(%)' : '(₹)'}</label>
              <input type="number" className="form-input" step="0.01" min="0"
                value={formData.amount}
                onChange={e => setFormData({ ...formData, amount: e.target.value })} required />
            </div>
            <div className="form-group full-width">
              <label className="checkbox-label">
                <input type="checkbox" checked={formData.isPercentage}
                  onChange={e => setFormData({ ...formData, isPercentage: e.target.checked })} />
                Amount is a percentage of basic salary
              </label>
            </div>
            <div className="form-group full-width">
              <label className="form-label">Description</label>
              <textarea className="form-input" placeholder="Optional description"
                value={formData.description}
                onChange={e => setFormData({ ...formData, description: e.target.value })} rows={2} />
            </div>
          </div>
        </form>
      </Modal>
    </div>
  );
};

export default BenefitList;
