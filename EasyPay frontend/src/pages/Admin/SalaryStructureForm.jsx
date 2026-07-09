import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { salaryStructureService, payrollPolicyService } from '../../api/services';
import { employeeService } from '../../api/employeeService';
import { useToast } from '../../context/ToastContext';

const formatCurrency = (val) => {
  if (val == null || isNaN(val)) return '₹0';
  return new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(val);
};

const SalaryStructureForm = () => {
  const { id } = useParams(); // employeeId
  const navigate = useNavigate();
  const toast = useToast();

  const [employee, setEmployee] = useState(null);
  const [policies, setPolicies] = useState([]);
  const [existing, setExisting] = useState(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  const [form, setForm] = useState({
    policyId: '', effectiveFrom: new Date().toISOString().split('T')[0],
    basicSalary: 0, hra: 0, conveyanceAllowance: 0, medicalAllowance: 0,
    specialAllowance: 0, lta: 0, otherAllowances: 0,
  });

  useEffect(() => { loadData(); }, [id]);

  const loadData = async () => {
    setLoading(true);
    try {
      const empRes = await employeeService.getById(id);
      setEmployee(empRes.data?.data || empRes.data);
    } catch (e) {
      toast.error('Failed to load employee');
    }
    try {
      const polRes = await payrollPolicyService.getAll();
      const pols = polRes.data?.data || polRes.data || [];
      setPolicies(pols);
      // Default to first active policy
      const active = pols.find(p => p.isActive);
      if (active) setForm(prev => ({ ...prev, policyId: active.policyId }));
    } catch (e) { /* ignore */ }
    try {
      const salRes = await salaryStructureService.getEmployeeCurrent(id);
      const sal = salRes.data?.data || salRes.data;
      if (sal) {
        setExisting(sal);
        setForm({
          policyId: sal.policyId || '',
          effectiveFrom: sal.effectiveFrom || new Date().toISOString().split('T')[0],
          basicSalary: sal.basicSalary || 0, hra: sal.hra || 0,
          conveyanceAllowance: sal.conveyanceAllowance || 0,
          medicalAllowance: sal.medicalAllowance || 0,
          specialAllowance: sal.specialAllowance || 0,
          lta: sal.lta || 0, otherAllowances: sal.otherAllowances || 0,
        });
      }
    } catch (e) { /* no existing salary structure */ }
    setLoading(false);
  };

  const gross = Number(form.basicSalary) + Number(form.hra) + Number(form.conveyanceAllowance) +
    Number(form.medicalAllowance) + Number(form.specialAllowance) + Number(form.lta) + Number(form.otherAllowances);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      await salaryStructureService.create({
        employeeId: Number(id),
        policyId: Number(form.policyId),
        effectiveFrom: form.effectiveFrom,
        basicSalary: Number(form.basicSalary),
        hra: Number(form.hra),
        conveyanceAllowance: Number(form.conveyanceAllowance),
        medicalAllowance: Number(form.medicalAllowance),
        specialAllowance: Number(form.specialAllowance),
        lta: Number(form.lta),
        otherAllowances: Number(form.otherAllowances),
      });
      toast.success('Salary structure saved successfully');
      navigate(`/admin/employees/${id}`);
    } catch (e) {
      toast.error(e.response?.data?.message || 'Failed to save salary structure');
    } finally { setSaving(false); }
  };

  const updateField = (field, value) => setForm(prev => ({ ...prev, [field]: value }));

  if (loading) return <div className="loading-spinner"><div className="spinner" /> Loading...</div>;

  return (
    <div>
      <div className="page-header">
        <h1 className="page-title">
          {existing ? 'Update' : 'Create'} Salary Structure
        </h1>
        <div className="page-title-accent" />
        <p className="page-subtitle">
          {employee ? `${employee.fullName || employee.firstName + ' ' + employee.lastName} (${employee.employeeCode})` : 'Employee'}
        </p>
      </div>

      <form onSubmit={handleSubmit}>
        <div className="card" style={{ marginBottom: 20 }}>
          <div className="card-header">Configuration</div>
          <div className="form-grid">
            <div className="form-group">
              <label className="form-label">Payroll Policy *</label>
              <select className="form-input" value={form.policyId}
                onChange={e => updateField('policyId', e.target.value)} required>
                <option value="">Select Policy</option>
                {policies.map(p => (
                  <option key={p.policyId} value={p.policyId}>
                    {p.policyName} {p.isActive ? '(Active)' : ''}
                  </option>
                ))}
              </select>
            </div>
            <div className="form-group">
              <label className="form-label">Effective From *</label>
              <input type="date" className="form-input" value={form.effectiveFrom}
                onChange={e => updateField('effectiveFrom', e.target.value)} required />
            </div>
          </div>
        </div>

        <div className="card" style={{ marginBottom: 20 }}>
          <div className="card-header">Earnings Components</div>
          <div className="form-grid">
            <div className="form-group">
              <label className="form-label">Basic Salary *</label>
              <input type="number" className="form-input" min="0" step="100"
                value={form.basicSalary} onChange={e => updateField('basicSalary', e.target.value)} required />
            </div>
            <div className="form-group">
              <label className="form-label">HRA</label>
              <input type="number" className="form-input" min="0" step="100"
                value={form.hra} onChange={e => updateField('hra', e.target.value)} />
            </div>
            <div className="form-group">
              <label className="form-label">Conveyance Allowance</label>
              <input type="number" className="form-input" min="0" step="100"
                value={form.conveyanceAllowance} onChange={e => updateField('conveyanceAllowance', e.target.value)} />
            </div>
            <div className="form-group">
              <label className="form-label">Medical Allowance</label>
              <input type="number" className="form-input" min="0" step="100"
                value={form.medicalAllowance} onChange={e => updateField('medicalAllowance', e.target.value)} />
            </div>
            <div className="form-group">
              <label className="form-label">Special Allowance</label>
              <input type="number" className="form-input" min="0" step="100"
                value={form.specialAllowance} onChange={e => updateField('specialAllowance', e.target.value)} />
            </div>
            <div className="form-group">
              <label className="form-label">LTA</label>
              <input type="number" className="form-input" min="0" step="100"
                value={form.lta} onChange={e => updateField('lta', e.target.value)} />
            </div>
            <div className="form-group">
              <label className="form-label">Other Allowances</label>
              <input type="number" className="form-input" min="0" step="100"
                value={form.otherAllowances} onChange={e => updateField('otherAllowances', e.target.value)} />
            </div>
          </div>
        </div>

        {/* Live Summary */}
        <div className="card" style={{ marginBottom: 20, background: 'var(--hex-blue-pale)', border: '1px solid var(--hex-blue)' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <div>
              <div style={{ fontSize: 13, fontWeight: 600, color: 'var(--hex-blue)', textTransform: 'uppercase', letterSpacing: '0.04em' }}>
                Gross Salary (Monthly)
              </div>
              <div style={{ fontSize: 28, fontWeight: 800, fontFamily: 'var(--font-heading)', color: 'var(--hex-blue)', marginTop: 4 }}>
                {formatCurrency(gross)}
              </div>
            </div>
            <div style={{ fontSize: 13, color: 'var(--text-secondary)' }}>
              Annual: {formatCurrency(gross * 12)}
            </div>
          </div>
        </div>

        <div style={{ display: 'flex', gap: 12, justifyContent: 'flex-end' }}>
          <button type="button" className="btn btn-secondary" onClick={() => navigate(`/admin/employees/${id}`)}>Cancel</button>
          <button type="submit" className="btn btn-primary" disabled={saving}>
            {saving ? 'Saving...' : 'Save Salary Structure'}
          </button>
        </div>
      </form>
    </div>
  );
};

export default SalaryStructureForm;
