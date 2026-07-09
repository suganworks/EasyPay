import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { employeeService } from '../../api/employeeService';
import { salaryStructureService, benefitService } from '../../api/services';
import { leaveService } from '../../api/leaveService';
import Modal from '../../components/Modal';
import ConfirmDialog from '../../components/ConfirmDialog';
import { useToast } from '../../context/ToastContext';

const formatCurrency = (val) => {
  if (val == null || isNaN(val)) return '—';
  return new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(val);
};

const formatDate = (dateStr) => {
  if (!dateStr) return '—';
  const d = new Date(dateStr + (dateStr.includes('T') ? '' : 'T00:00:00'));
  return d.toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
};

const formatDateInput = (date = new Date()) => {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
};

const EmployeeDetail = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const toast = useToast();
  const [employee, setEmployee] = useState(null);
  const [salary, setSalary] = useState(null);
  const [benefits, setBenefits] = useState([]);
  const [benefitOptions, setBenefitOptions] = useState([]);
  const [leaveBalance, setLeaveBalance] = useState([]);
  const [loading, setLoading] = useState(true);
  const [confirmAction, setConfirmAction] = useState(null);
  const [actionLoading, setActionLoading] = useState(false);
  const [benefitModalOpen, setBenefitModalOpen] = useState(false);
  const [benefitSaving, setBenefitSaving] = useState(false);
  const [benefitForm, setBenefitForm] = useState({
    benefitId: '',
    effectiveFrom: formatDateInput(),
    effectiveTo: '',
    overrideAmount: '',
  });
  const [benefitToRemove, setBenefitToRemove] = useState(null);

  useEffect(() => { loadEmployee(); }, [id]);

  const loadEmployee = async () => {
    setLoading(true);
    try {
      const res = await employeeService.getById(id);
      setEmployee(res.data?.data || res.data);
    } catch (e) {
      toast.error('Failed to load employee');
    }
    try {
      const salRes = await salaryStructureService.getEmployeeCurrent(id);
      setSalary(salRes.data?.data || salRes.data);
    } catch (e) { /* may not exist */ }
    try {
      const benRes = await benefitService.getByEmployee(id);
      setBenefits(benRes.data?.data || benRes.data || []);
    } catch (e) { /* ignore */ }
    try {
      const allBenefitsRes = await benefitService.getAll();
      const allBenefits = allBenefitsRes.data?.data || allBenefitsRes.data || [];
      setBenefitOptions(allBenefits.filter(b => b.isActive !== false));
    } catch (e) { /* ignore */ }
    try {
      const lbRes = await leaveService.getEmployeeBalance(id);
      setLeaveBalance(lbRes.data?.data || lbRes.data || []);
    } catch (e) { /* ignore */ }
    setLoading(false);
  };

  const handleDeactivate = async () => {
    setActionLoading(true);
    try {
      await employeeService.delete(id);
      toast.success('Employee deactivated');
      loadEmployee();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Failed to deactivate');
    } finally { setActionLoading(false); setConfirmAction(null); }
  };

  const handleReactivate = async () => {
    setActionLoading(true);
    try {
      await employeeService.reactivate(id);
      toast.success('Employee reactivated');
      loadEmployee();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Failed to reactivate');
    } finally { setActionLoading(false); setConfirmAction(null); }
  };

  const openBenefitAssignModal = () => {
    setBenefitForm({
      benefitId: benefitOptions[0]?.benefitId ? String(benefitOptions[0].benefitId) : '',
      effectiveFrom: formatDateInput(),
      effectiveTo: '',
      overrideAmount: '',
    });
    setBenefitModalOpen(true);
  };

  const handleAssignBenefit = async (e) => {
    e.preventDefault();
    if (!benefitForm.benefitId) {
      toast.error('Select a benefit to assign');
      return;
    }

    setBenefitSaving(true);
    try {
      const payload = {
        benefitId: Number(benefitForm.benefitId),
        effectiveFrom: benefitForm.effectiveFrom,
        effectiveTo: benefitForm.effectiveTo || undefined,
        overrideAmount: benefitForm.overrideAmount === '' ? undefined : Number(benefitForm.overrideAmount),
      };
      await benefitService.assign(id, payload);
      toast.success('Benefit assigned to employee');
      setBenefitModalOpen(false);
      await loadEmployee();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Failed to assign benefit');
    } finally {
      setBenefitSaving(false);
    }
  };

  const handleRemoveBenefit = async () => {
    if (!benefitToRemove) return;

    setActionLoading(true);
    try {
      await benefitService.removeEmployeeBenefit(benefitToRemove.employeeBenefitId);
      toast.success('Benefit removed from employee');
      setBenefitToRemove(null);
      await loadEmployee();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Failed to remove benefit');
    } finally {
      setActionLoading(false);
    }
  };

  if (loading) return <div className="loading-spinner"><div className="spinner" /> Loading employee...</div>;
  if (!employee) return <div className="empty-state"><div className="empty-title">Employee not found</div></div>;

  const isActive = employee.isActive !== false && employee.employmentStatus !== 'Terminated';

  return (
    <div>
      <div className="page-actions">
        <div className="page-header" style={{ marginBottom: 0 }}>
          <h1 className="page-title">{employee.fullName || `${employee.firstName} ${employee.lastName}`}</h1>
          <div className="page-title-accent" />
          <p className="page-subtitle">{employee.employeeCode} · {employee.designationName} · {employee.departmentName}</p>
        </div>
        <div style={{ display: 'flex', gap: 8 }}>
          <Link to={`/admin/employees/${id}/edit`} className="btn btn-secondary">Edit</Link>
          <Link to={`/admin/employees/${id}/salary`} className="btn btn-primary">
            {salary ? 'Update Salary' : 'Set Salary'}
          </Link>
          {isActive ? (
            <button className="btn btn-danger" onClick={() => setConfirmAction('deactivate')}>Deactivate</button>
          ) : (
            <button className="btn btn-success" onClick={() => setConfirmAction('reactivate')}>Reactivate</button>
          )}
        </div>
      </div>

      {/* Profile Info */}
      <div className="card" style={{ marginBottom: 20 }}>
        <div className="card-header">Profile Information</div>
        <div className="detail-grid">
          <div className="detail-item">
            <div className="detail-label">Full Name</div>
            <div className="detail-value">{employee.fullName || `${employee.firstName} ${employee.lastName}`}</div>
          </div>
          <div className="detail-item">
            <div className="detail-label">Work Email</div>
            <div className="detail-value">{employee.workEmail || '—'}</div>
          </div>
          <div className="detail-item">
            <div className="detail-label">Phone</div>
            <div className="detail-value">{employee.phone || '—'}</div>
          </div>
          <div className="detail-item">
            <div className="detail-label">Date of Birth</div>
            <div className="detail-value">{formatDate(employee.dateOfBirth)}</div>
          </div>
          <div className="detail-item">
            <div className="detail-label">Gender</div>
            <div className="detail-value">{employee.gender || '—'}</div>
          </div>
          <div className="detail-item">
            <div className="detail-label">Joining Date</div>
            <div className="detail-value">{formatDate(employee.joiningDate)}</div>
          </div>
          <div className="detail-item">
            <div className="detail-label">Employment Type</div>
            <div className="detail-value">
              <span className={`badge ${employee.employmentType === 'FullTime' ? 'badge-info' : 'badge-neutral'}`}>
                {employee.employmentType}
              </span>
            </div>
          </div>
          <div className="detail-item">
            <div className="detail-label">Status</div>
            <div className="detail-value">
              <span className={`badge ${isActive ? 'badge-success' : 'badge-danger'}`}>
                {employee.employmentStatus || (isActive ? 'Active' : 'Inactive')}
              </span>
            </div>
          </div>
          <div className="detail-item">
            <div className="detail-label">Manager</div>
            <div className="detail-value">{employee.managerName || 'None'}</div>
          </div>
          <div className="detail-item">
            <div className="detail-label">Location</div>
            <div className="detail-value">{[employee.city, employee.state, employee.country].filter(Boolean).join(', ') || '—'}</div>
          </div>
        </div>
      </div>

      {/* Salary Structure */}
      <div className="card" style={{ marginBottom: 20 }}>
        <div className="card-header">Current Salary Structure</div>
        {salary ? (
          <div className="detail-grid">
            <div className="detail-item">
              <div className="detail-label">Basic Salary</div>
              <div className="detail-value currency">{formatCurrency(salary.basicSalary)}</div>
            </div>
            <div className="detail-item">
              <div className="detail-label">HRA</div>
              <div className="detail-value currency">{formatCurrency(salary.hra)}</div>
            </div>
            <div className="detail-item">
              <div className="detail-label">Conveyance</div>
              <div className="detail-value currency">{formatCurrency(salary.conveyanceAllowance)}</div>
            </div>
            <div className="detail-item">
              <div className="detail-label">Medical</div>
              <div className="detail-value currency">{formatCurrency(salary.medicalAllowance)}</div>
            </div>
            <div className="detail-item">
              <div className="detail-label">Special Allowance</div>
              <div className="detail-value currency">{formatCurrency(salary.specialAllowance)}</div>
            </div>
            <div className="detail-item">
              <div className="detail-label">Gross Salary</div>
              <div className="detail-value currency" style={{ color: 'var(--success)', fontWeight: 700, fontSize: 16 }}>
                {formatCurrency(salary.grossSalary)}
              </div>
            </div>
            <div className="detail-item">
              <div className="detail-label">Effective From</div>
              <div className="detail-value">{formatDate(salary.effectiveFrom)}</div>
            </div>
            <div className="detail-item">
              <div className="detail-label">Policy</div>
              <div className="detail-value">{salary.policyName || '—'}</div>
            </div>
          </div>
        ) : (
          <div className="warning-box">
            ⚠️ No salary structure assigned. <Link to={`/admin/employees/${id}/salary`} style={{ fontWeight: 600, marginLeft: 4 }}>Create one now</Link>
          </div>
        )}
      </div>

      {/* Benefits */}
      <div className="card" style={{ marginBottom: 20 }}>
        <div className="card-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: 12 }}>
          <span>Assigned Benefits</span>
          <button className="btn btn-primary" onClick={openBenefitAssignModal} disabled={benefitOptions.length === 0}>
            + Assign Benefit
          </button>
        </div>
        {benefits.length === 0 ? (
          <div className="empty-state" style={{ padding: 24 }}>
            <div className="empty-message">No benefits assigned to this employee.</div>
          </div>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th>Benefit</th>
                <th>Type</th>
                <th>Amount</th>
                <th>Effective From</th>
                <th>Status</th>
                <th className="text-right">Actions</th>
              </tr>
            </thead>
            <tbody>
              {benefits.map(b => (
                <tr key={b.employeeBenefitId}>
                  <td><strong>{b.benefitName}</strong></td>
                  <td>{b.benefitType}</td>
                  <td className="currency">{formatCurrency(b.effectiveAmount)}</td>
                  <td>{formatDate(b.effectiveFrom)}</td>
                  <td><span className={`badge ${b.isActive ? 'badge-success' : 'badge-danger'}`}>{b.isActive ? 'Active' : 'Ended'}</span></td>
                  <td className="text-right">
                    {b.isActive && (
                      <button className="btn-ghost" onClick={() => setBenefitToRemove(b)}>
                        Remove
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      <Modal
        isOpen={benefitModalOpen}
        onClose={() => setBenefitModalOpen(false)}
        title="Assign Benefit"
        wide
        footer={
          <>
            <button className="btn btn-secondary" onClick={() => setBenefitModalOpen(false)} disabled={benefitSaving}>
              Cancel
            </button>
            <button className="btn btn-primary" onClick={handleAssignBenefit} disabled={benefitSaving || benefitOptions.length === 0}>
              {benefitSaving ? 'Assigning...' : 'Assign'}
            </button>
          </>
        }
      >
        <form onSubmit={handleAssignBenefit}>
          <div className="form-grid">
            <div className="form-group full-width">
              <label className="form-label">Benefit</label>
              <select
                className="form-input"
                value={benefitForm.benefitId}
                onChange={e => setBenefitForm({ ...benefitForm, benefitId: e.target.value })}
                required
              >
                <option value="">Select benefit</option>
                {benefitOptions.map(b => (
                  <option key={b.benefitId} value={b.benefitId}>
                    {b.benefitName} ({b.benefitCode})
                  </option>
                ))}
              </select>
            </div>
            <div className="form-group">
              <label className="form-label">Effective From</label>
              <input
                type="date"
                className="form-input"
                value={benefitForm.effectiveFrom}
                onChange={e => setBenefitForm({ ...benefitForm, effectiveFrom: e.target.value })}
                required
              />
            </div>
            <div className="form-group">
              <label className="form-label">Effective To</label>
              <input
                type="date"
                className="form-input"
                value={benefitForm.effectiveTo}
                onChange={e => setBenefitForm({ ...benefitForm, effectiveTo: e.target.value })}
              />
            </div>
            <div className="form-group full-width">
              <label className="form-label">Override Amount</label>
              <input
                type="number"
                className="form-input"
                min="0"
                step="0.01"
                value={benefitForm.overrideAmount}
                onChange={e => setBenefitForm({ ...benefitForm, overrideAmount: e.target.value })}
                placeholder="Leave blank to use benefit default"
              />
            </div>
          </div>
        </form>
      </Modal>

      {/* Leave Balance */}
      <div className="card">
        <div className="card-header">Leave Balance ({new Date().getFullYear()})</div>
        {leaveBalance.length === 0 ? (
          <div className="empty-state" style={{ padding: 24 }}>
            <div className="empty-message">No leave balance data available.</div>
          </div>
        ) : (
          <div className="card-grid">
            {leaveBalance.map(lb => (
              <div key={lb.leaveTypeId} className="stat-card accent-blue">
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                  <div>
                    <div className="stat-card-value">{lb.remainingDays}</div>
                    <div className="stat-card-label">{lb.leaveTypeName}</div>
                  </div>
                  <div style={{ fontSize: 12, color: 'var(--text-secondary)' }}>
                    {lb.usedDays}/{lb.maxDaysPerYear} used
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      <ConfirmDialog
        isOpen={!!confirmAction}
        onClose={() => setConfirmAction(null)}
        onConfirm={confirmAction === 'deactivate' ? handleDeactivate : handleReactivate}
        title={confirmAction === 'deactivate' ? 'Deactivate Employee' : 'Reactivate Employee'}
        message={confirmAction === 'deactivate'
          ? `Are you sure you want to deactivate ${employee.fullName || employee.firstName}? They will lose access to the system.`
          : `Reactivate ${employee.fullName || employee.firstName}? They will regain system access.`}
        confirmText={confirmAction === 'deactivate' ? 'Deactivate' : 'Reactivate'}
        variant={confirmAction === 'deactivate' ? 'danger' : 'warning'}
        loading={actionLoading}
      />

      <ConfirmDialog
        isOpen={!!benefitToRemove}
        onClose={() => setBenefitToRemove(null)}
        onConfirm={handleRemoveBenefit}
        title="Remove Benefit"
        message={benefitToRemove ? `Remove ${benefitToRemove.benefitName} from this employee?` : ''}
        confirmText="Remove"
        variant="danger"
        loading={actionLoading}
      />
    </div>
  );
};

export default EmployeeDetail;
