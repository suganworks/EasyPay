import React, { useState, useEffect } from 'react';
import { payrollPolicyService } from '../../api/services';
import Modal from '../../components/Modal';
import ConfirmDialog from '../../components/ConfirmDialog';
import { useToast } from '../../context/ToastContext';

const PayrollPolicyList = () => {
  const [policies, setPolicies] = useState([]);
  const [loading, setLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState(null);
  const [saving, setSaving] = useState(false);
  const [deactivateTarget, setDeactivateTarget] = useState(null);
  const [deactivating, setDeactivating] = useState(false);
  const toast = useToast();

  const [form, setForm] = useState({
    policyName: '', effectiveFrom: '', effectiveTo: '', payFrequency: 'Monthly',
    overtimeRate: 1.5, pfEmployeeRate: 12, pfEmployerRate: 12,
    esiEmployeeRate: 0.75, esiEmployerRate: 3.25, professionalTax: 200,
    gratuityRate: 4.81, workingDaysMonth: 26, workingHoursDay: 8, description: '',
  });

  useEffect(() => { fetchPolicies(); }, []);

  const fetchPolicies = async () => {
    setLoading(true);
    try {
      const res = await payrollPolicyService.getAll();
      setPolicies(res.data?.data || res.data || []);
    } catch (e) {
      toast.error('Failed to load policies');
    } finally { setLoading(false); }
  };

  const openCreate = () => {
    setEditing(null);
    setForm({
      policyName: '', effectiveFrom: new Date().toISOString().split('T')[0], effectiveTo: '',
      payFrequency: 'Monthly', overtimeRate: 1.5, pfEmployeeRate: 12, pfEmployerRate: 12,
      esiEmployeeRate: 0.75, esiEmployerRate: 3.25, professionalTax: 200,
      gratuityRate: 4.81, workingDaysMonth: 26, workingHoursDay: 8, description: '',
    });
    setModalOpen(true);
  };

  const openEdit = (p) => {
    setEditing(p);
    setForm({
      policyName: p.policyName, effectiveFrom: p.effectiveFrom || '',
      effectiveTo: p.effectiveTo || '', payFrequency: p.payFrequency || 'Monthly',
      overtimeRate: p.overtimeRate, pfEmployeeRate: p.pfEmployeeRate,
      pfEmployerRate: p.pfEmployerRate, esiEmployeeRate: p.esiEmployeeRate,
      esiEmployerRate: p.esiEmployerRate, professionalTax: p.professionalTax,
      gratuityRate: p.gratuityRate, workingDaysMonth: p.workingDaysMonth,
      workingHoursDay: p.workingHoursDay, description: p.description || '',
    });
    setModalOpen(true);
  };

  const handleSave = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      const payload = {
        policyName: form.policyName, effectiveFrom: form.effectiveFrom,
        effectiveTo: form.effectiveTo || undefined, payFrequency: form.payFrequency,
        overtimeRate: Number(form.overtimeRate), pfEmployeeRate: Number(form.pfEmployeeRate),
        pfEmployerRate: Number(form.pfEmployerRate), esiEmployeeRate: Number(form.esiEmployeeRate),
        esiEmployerRate: Number(form.esiEmployerRate), professionalTax: Number(form.professionalTax),
        gratuityRate: Number(form.gratuityRate), workingDaysMonth: Number(form.workingDaysMonth),
        workingHoursDay: Number(form.workingHoursDay), description: form.description || undefined,
      };
      if (editing) {
        await payrollPolicyService.update(editing.policyId, payload);
        toast.success('Policy updated');
      } else {
        await payrollPolicyService.create(payload);
        toast.success('Policy created');
      }
      setModalOpen(false);
      fetchPolicies();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Operation failed');
    } finally { setSaving(false); }
  };

  const handleDeactivate = async () => {
    setDeactivating(true);
    try {
      await payrollPolicyService.deactivate(deactivateTarget.policyId);
      toast.success('Policy deactivated');
      setDeactivateTarget(null);
      fetchPolicies();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Failed to deactivate');
    } finally { setDeactivating(false); }
  };

  const updateField = (field, value) => setForm(prev => ({ ...prev, [field]: value }));

  return (
    <div>
      <div className="page-actions">
        <div className="page-header" style={{ marginBottom: 0 }}>
          <h1 className="page-title">Payroll Policies</h1>
          <div className="page-title-accent" />
          <p className="page-subtitle">Configure payroll calculation rules</p>
        </div>
        <button className="btn btn-primary" onClick={openCreate}>+ New Policy</button>
      </div>

      <div className="card">
        {loading ? (
          <div className="loading-spinner"><div className="spinner" /> Loading...</div>
        ) : policies.length === 0 ? (
          <div className="empty-state">
            <div className="empty-icon">📜</div>
            <div className="empty-title">No policies defined</div>
            <div className="empty-message">Create a payroll policy to configure calculation rules.</div>
          </div>
        ) : (
          <div style={{ overflowX: 'auto' }}>
            <table className="data-table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Effective From</th>
                  <th>PF (Emp/Er)</th>
                  <th>ESI (Emp/Er)</th>
                  <th>Prof Tax</th>
                  <th>Status</th>
                  <th className="text-right">Actions</th>
                </tr>
              </thead>
              <tbody>
                {policies.map(p => (
                  <tr key={p.policyId}>
                    <td>
                      <strong>{p.policyName}</strong>
                      {p.description && <div className="sub-text">{p.description}</div>}
                    </td>
                    <td>{p.effectiveFrom || '—'}</td>
                    <td>{p.pfEmployeeRate}% / {p.pfEmployerRate}%</td>
                    <td>{p.esiEmployeeRate}% / {p.esiEmployerRate}%</td>
                    <td>₹{p.professionalTax}</td>
                    <td>
                      <span className={`badge ${p.isActive ? 'badge-success' : 'badge-danger'}`}>
                        {p.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                    <td className="text-right">
                      <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end' }}>
                        <button className="btn-ghost" onClick={() => openEdit(p)}>Edit</button>
                        {p.isActive && (
                          <button className="btn-ghost danger" onClick={() => setDeactivateTarget(p)}>Deactivate</button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      <Modal isOpen={modalOpen} onClose={() => setModalOpen(false)} wide
        title={editing ? 'Edit Policy' : 'New Policy'}
        footer={<>
          <button className="btn btn-secondary" onClick={() => setModalOpen(false)} disabled={saving}>Cancel</button>
          <button className="btn btn-primary" onClick={handleSave} disabled={saving}>
            {saving ? 'Saving...' : (editing ? 'Update' : 'Create')}
          </button>
        </>}>
        <form onSubmit={handleSave}>
          <div className="form-grid">
            <div className="form-group full-width">
              <label className="form-label">Policy Name *</label>
              <input type="text" className="form-input" value={form.policyName}
                onChange={e => updateField('policyName', e.target.value)} required />
            </div>
            <div className="form-group">
              <label className="form-label">Effective From *</label>
              <input type="date" className="form-input" value={form.effectiveFrom}
                max={form.effectiveTo || undefined}
                onChange={e => updateField('effectiveFrom', e.target.value)} required />
            </div>
            <div className="form-group">
              <label className="form-label">Effective To</label>
              <input type="date" className="form-input" value={form.effectiveTo}
                min={form.effectiveFrom || undefined}
                onChange={e => updateField('effectiveTo', e.target.value)} />
            </div>
            <div className="form-group">
              <label className="form-label">PF Employee Rate (%)</label>
              <input type="number" className="form-input" step="0.01" value={form.pfEmployeeRate}
                onChange={e => updateField('pfEmployeeRate', e.target.value)} />
            </div>
            <div className="form-group">
              <label className="form-label">PF Employer Rate (%)</label>
              <input type="number" className="form-input" step="0.01" value={form.pfEmployerRate}
                onChange={e => updateField('pfEmployerRate', e.target.value)} />
            </div>
            <div className="form-group">
              <label className="form-label">ESI Employee Rate (%)</label>
              <input type="number" className="form-input" step="0.01" value={form.esiEmployeeRate}
                onChange={e => updateField('esiEmployeeRate', e.target.value)} />
            </div>
            <div className="form-group">
              <label className="form-label">ESI Employer Rate (%)</label>
              <input type="number" className="form-input" step="0.01" value={form.esiEmployerRate}
                onChange={e => updateField('esiEmployerRate', e.target.value)} />
            </div>
            <div className="form-group">
              <label className="form-label">Professional Tax (₹)</label>
              <input type="number" className="form-input" step="1" value={form.professionalTax}
                onChange={e => updateField('professionalTax', e.target.value)} />
            </div>
            <div className="form-group">
              <label className="form-label">Overtime Rate (x)</label>
              <input type="number" className="form-input" step="0.1" value={form.overtimeRate}
                onChange={e => updateField('overtimeRate', e.target.value)} />
            </div>
            <div className="form-group">
              <label className="form-label">Gratuity Rate (%)</label>
              <input type="number" className="form-input" step="0.01" value={form.gratuityRate}
                onChange={e => updateField('gratuityRate', e.target.value)} />
            </div>
            <div className="form-group">
              <label className="form-label">Working Days/Month</label>
              <input type="number" className="form-input" value={form.workingDaysMonth}
                onChange={e => updateField('workingDaysMonth', e.target.value)} />
            </div>
            <div className="form-group">
              <label className="form-label">Working Hours/Day</label>
              <input type="number" className="form-input" value={form.workingHoursDay}
                onChange={e => updateField('workingHoursDay', e.target.value)} />
            </div>
            <div className="form-group full-width">
              <label className="form-label">Description</label>
              <textarea className="form-input" rows={2} value={form.description}
                onChange={e => updateField('description', e.target.value)} />
            </div>
          </div>
        </form>
      </Modal>

      <ConfirmDialog isOpen={!!deactivateTarget} onClose={() => setDeactivateTarget(null)}
        onConfirm={handleDeactivate} title="Deactivate Policy"
        message={`Deactivate "${deactivateTarget?.policyName}"? This will prevent it from being used in new salary structures.`}
        confirmText="Deactivate" variant="warning" loading={deactivating} />
    </div>
  );
};

export default PayrollPolicyList;
