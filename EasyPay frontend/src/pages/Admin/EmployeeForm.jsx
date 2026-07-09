import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { employeeService } from '../../api/employeeService';
import { departmentService, designationService } from '../../api/services';
import { useToast } from '../../context/ToastContext';

const EmployeeForm = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const toast = useToast();
  const isEdit = !!id;

  const [departments, setDepartments] = useState([]);
  const [designations, setDesignations] = useState([]);
  const [employees, setEmployees] = useState([]);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);

  const [form, setForm] = useState({
    firstName: '', lastName: '', dateOfBirth: '', gender: 'Male',
    phone: '', personalEmail: '', address: '', city: '', state: '', postalCode: '', country: 'India',
    departmentId: '', designationId: '', managerId: '',
    joiningDate: '', employmentType: 'FullTime',
    workEmail: '', password: '',
    bankName: '', bankAccountNo: '', bankIFSC: '',
    panNumber: '', pfNumber: '', esiNumber: '', taxWithholding: 0,
    employmentStatus: 'Active',
  });

  useEffect(() => { loadDropdowns(); }, []);
  useEffect(() => { if (isEdit) loadEmployee(); }, [id]);

  const loadDropdowns = async () => {
    try {
      const deptRes = await departmentService.getActive();
      setDepartments(deptRes.data?.data || deptRes.data || []);
    } catch (e) {
      console.error('Failed to load departments:', e);
      toast.error('Failed to load departments. Please refresh.');
    }
    try {
      const empRes = await employeeService.getAll({ PageNumber: 1, PageSize: 200 });
      setEmployees(empRes.data?.data || []);
    } catch (e) {
      console.error('Failed to load employees:', e);
    }
  };

  const loadDesignations = async (deptId) => {
    if (!deptId) { setDesignations([]); return; }
    try {
      const res = await designationService.getByDepartment(deptId);
      setDesignations(res.data?.data || res.data || []);
    } catch (e) { setDesignations([]); }
  };

  const loadEmployee = async () => {
    setLoading(true);
    try {
      const res = await employeeService.getById(id);
      const emp = res.data?.data || res.data;
      if (emp) {
        setForm({
          firstName: emp.firstName || '', lastName: emp.lastName || '',
          dateOfBirth: emp.dateOfBirth || '', gender: emp.gender || 'Male',
          phone: emp.phone || '', personalEmail: emp.personalEmail || '',
          address: emp.address || '', city: emp.city || '', state: emp.state || '',
          postalCode: emp.postalCode || '', country: emp.country || 'India',
          departmentId: emp.departmentId || '', designationId: emp.designationId || '',
          managerId: emp.managerId || '',
          joiningDate: emp.joiningDate || '', employmentType: emp.employmentType || 'FullTime',
          workEmail: emp.workEmail || '', password: '',
          bankName: emp.bankName || '', bankAccountNo: emp.bankAccountNo || '',
          bankIFSC: emp.bankIFSC || '',
          panNumber: emp.panNumber || '', pfNumber: emp.pfNumber || '',
          esiNumber: emp.esiNumber || '', taxWithholding: emp.taxWithholding || 0,
          employmentStatus: emp.employmentStatus || 'Active',
        });
        if (emp.departmentId) loadDesignations(emp.departmentId);
      }
    } catch (e) {
      toast.error('Failed to load employee');
    } finally { setLoading(false); }
  };

  const handleDeptChange = (deptId) => {
    setForm(prev => ({ ...prev, departmentId: deptId, designationId: '' }));
    loadDesignations(deptId);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      if (isEdit) {
        await employeeService.update(id, {
          firstName: form.firstName, lastName: form.lastName,
          phone: form.phone || undefined, personalEmail: form.personalEmail || undefined,
          address: form.address || undefined, city: form.city || undefined,
          state: form.state || undefined, postalCode: form.postalCode || undefined,
          country: form.country || undefined,
          departmentId: Number(form.departmentId), designationId: Number(form.designationId),
          managerId: form.managerId ? Number(form.managerId) : undefined,
          employmentType: form.employmentType,
          employmentStatus: form.employmentStatus,
          bankName: form.bankName || undefined, bankAccountNo: form.bankAccountNo || undefined,
          bankIFSC: form.bankIFSC || undefined,
          panNumber: form.panNumber || undefined, pfNumber: form.pfNumber || undefined,
          esiNumber: form.esiNumber || undefined,
          taxWithholding: Number(form.taxWithholding) || 0,
        });
        toast.success('Employee updated successfully');
      } else {
        await employeeService.create({
          firstName: form.firstName, lastName: form.lastName,
          dateOfBirth: form.dateOfBirth, gender: form.gender,
          phone: form.phone || undefined, personalEmail: form.personalEmail || undefined,
          address: form.address || undefined, city: form.city || undefined,
          state: form.state || undefined, postalCode: form.postalCode || undefined,
          country: form.country || undefined,
          departmentId: Number(form.departmentId), designationId: Number(form.designationId),
          managerId: form.managerId ? Number(form.managerId) : undefined,
          joiningDate: form.joiningDate, employmentType: form.employmentType,
          workEmail: form.workEmail, password: form.password,
          bankName: form.bankName || undefined, bankAccountNo: form.bankAccountNo || undefined,
          bankIFSC: form.bankIFSC || undefined,
          panNumber: form.panNumber || undefined, pfNumber: form.pfNumber || undefined,
          esiNumber: form.esiNumber || undefined,
          taxWithholding: Number(form.taxWithholding) || 0,
        });
        toast.success('Employee created successfully');
      }
      navigate('/admin/employees');
    } catch (e) {
      toast.error(e.response?.data?.message || 'Failed to save employee');
    } finally { setSaving(false); }
  };

  const updateField = (field, value) => setForm(prev => ({ ...prev, [field]: value }));

  if (loading) return <div className="loading-spinner"><div className="spinner" /> Loading employee...</div>;

  return (
    <div>
      <div className="page-header">
        <h1 className="page-title">{isEdit ? 'Edit Employee' : 'Add New Employee'}</h1>
        <div className="page-title-accent" />
        <p className="page-subtitle">{isEdit ? 'Update employee information' : 'Fill in details to add a new employee'}</p>
      </div>

      <form onSubmit={handleSubmit}>
        {/* Personal Information */}
        <div className="card" style={{ marginBottom: 20 }}>
          <div className="card-header">Personal Information</div>
          <div className="form-grid">
            <div className="form-group">
              <label className="form-label">First Name *</label>
              <input type="text" className="form-input" value={form.firstName}
                onChange={e => updateField('firstName', e.target.value)} required />
            </div>
            <div className="form-group">
              <label className="form-label">Last Name *</label>
              <input type="text" className="form-input" value={form.lastName}
                onChange={e => updateField('lastName', e.target.value)} required />
            </div>
            <div className="form-group">
              <label className="form-label">Date of Birth *</label>
              <input type="date" className="form-input" value={form.dateOfBirth}
                onChange={e => updateField('dateOfBirth', e.target.value)} required={!isEdit} />
            </div>
            <div className="form-group">
              <label className="form-label">Gender *</label>
              <select className="form-input" value={form.gender}
                onChange={e => updateField('gender', e.target.value)}>
                <option value="Male">Male</option>
                <option value="Female">Female</option>
                <option value="Other">Other</option>
              </select>
            </div>
            <div className="form-group">
              <label className="form-label">Phone</label>
              <input type="text" className="form-input" value={form.phone}
                onChange={e => updateField('phone', e.target.value)} />
            </div>
            <div className="form-group">
              <label className="form-label">Personal Email</label>
              <input type="email" className="form-input" value={form.personalEmail}
                onChange={e => updateField('personalEmail', e.target.value)} />
            </div>
          </div>
        </div>

        {/* Address */}
        <div className="card" style={{ marginBottom: 20 }}>
          <div className="card-header">Address</div>
          <div className="form-grid">
            <div className="form-group full-width">
              <label className="form-label">Address</label>
              <input type="text" className="form-input" value={form.address}
                onChange={e => updateField('address', e.target.value)} />
            </div>
            <div className="form-group">
              <label className="form-label">City</label>
              <input type="text" className="form-input" value={form.city}
                onChange={e => updateField('city', e.target.value)} />
            </div>
            <div className="form-group">
              <label className="form-label">State</label>
              <input type="text" className="form-input" value={form.state}
                onChange={e => updateField('state', e.target.value)} />
            </div>
            <div className="form-group">
              <label className="form-label">Postal Code</label>
              <input type="text" className="form-input" value={form.postalCode}
                onChange={e => updateField('postalCode', e.target.value)} />
            </div>
            <div className="form-group">
              <label className="form-label">Country</label>
              <input type="text" className="form-input" value={form.country}
                onChange={e => updateField('country', e.target.value)} />
            </div>
          </div>
        </div>

        {/* Employment Details */}
        <div className="card" style={{ marginBottom: 20 }}>
          <div className="card-header">Employment Details</div>
          <div className="form-grid">
            <div className="form-group">
              <label className="form-label">Department *</label>
              <select className="form-input" value={form.departmentId}
                onChange={e => handleDeptChange(e.target.value)} required>
                <option value="">Select Department</option>
                {departments.map(d => <option key={d.departmentId} value={d.departmentId}>{d.departmentName}</option>)}
              </select>
            </div>
            <div className="form-group">
              <label className="form-label">Designation *</label>
              <select className="form-input" value={form.designationId}
                onChange={e => updateField('designationId', e.target.value)} required>
                <option value="">Select Designation</option>
                {designations.map(d => <option key={d.designationId} value={d.designationId}>{d.designationName}</option>)}
              </select>
            </div>
            <div className="form-group">
              <label className="form-label">Manager</label>
              <select className="form-input" value={form.managerId}
                onChange={e => updateField('managerId', e.target.value)}>
                <option value="">No Manager</option>
                {employees.filter(e => String(e.employeeId) !== String(id)).map(e => (
                  <option key={e.employeeId} value={e.employeeId}>{e.fullName || `${e.firstName} ${e.lastName}`} ({e.employeeCode})</option>
                ))}
              </select>
            </div>
            {!isEdit && (
              <div className="form-group">
                <label className="form-label">Joining Date *</label>
                <input type="date" className="form-input" value={form.joiningDate}
                  onChange={e => updateField('joiningDate', e.target.value)} required />
              </div>
            )}
            <div className="form-group">
              <label className="form-label">Employment Type *</label>
              <select className="form-input" value={form.employmentType}
                onChange={e => updateField('employmentType', e.target.value)}>
                <option value="FullTime">Full Time</option>
                <option value="PartTime">Part Time</option>
                <option value="Contract">Contract</option>
                <option value="Intern">Intern</option>
              </select>
            </div>
            {isEdit && (
              <div className="form-group">
                <label className="form-label">Employment Status</label>
                <select className="form-input" value={form.employmentStatus}
                  onChange={e => updateField('employmentStatus', e.target.value)}>
                  <option value="Active">Active</option>
                  <option value="Terminated">Terminated</option>
                  <option value="OnLeave">On Leave</option>
                </select>
              </div>
            )}
          </div>
        </div>

        {/* Login Credentials (Create only) */}
        {!isEdit && (
          <div className="card" style={{ marginBottom: 20 }}>
            <div className="card-header">Login Credentials</div>
            <div className="info-box">ℹ️ An employee account will be created with these credentials.</div>
            <div className="form-grid">
              <div className="form-group">
                <label className="form-label">Work Email *</label>
                <input type="email" className="form-input" value={form.workEmail}
                  onChange={e => updateField('workEmail', e.target.value)} required />
              </div>
              <div className="form-group">
                <label className="form-label">Password *</label>
                <input type="password" className="form-input" value={form.password}
                  onChange={e => updateField('password', e.target.value)} required placeholder="Min 8 chars, 1 uppercase, 1 number, 1 special" />
              </div>
            </div>
          </div>
        )}

        {/* Bank & Compliance */}
        <div className="card" style={{ marginBottom: 20 }}>
          <div className="card-header">Bank & Compliance</div>
          <div className="form-grid">
            <div className="form-group">
              <label className="form-label">Bank Name</label>
              <input type="text" className="form-input" value={form.bankName}
                onChange={e => updateField('bankName', e.target.value)} />
            </div>
            <div className="form-group">
              <label className="form-label">Account Number</label>
              <input type="text" className="form-input" value={form.bankAccountNo}
                onChange={e => updateField('bankAccountNo', e.target.value)} />
            </div>
            <div className="form-group">
              <label className="form-label">IFSC Code</label>
              <input type="text" className="form-input" value={form.bankIFSC}
                onChange={e => updateField('bankIFSC', e.target.value)} />
            </div>
            <div className="form-group">
              <label className="form-label">PAN Number</label>
              <input type="text" className="form-input" value={form.panNumber}
                onChange={e => updateField('panNumber', e.target.value)} />
            </div>
            <div className="form-group">
              <label className="form-label">PF Number</label>
              <input type="text" className="form-input" value={form.pfNumber}
                onChange={e => updateField('pfNumber', e.target.value)} />
            </div>
            <div className="form-group">
              <label className="form-label">ESI Number</label>
              <input type="text" className="form-input" value={form.esiNumber}
                onChange={e => updateField('esiNumber', e.target.value)} />
            </div>
            <div className="form-group">
              <label className="form-label">Tax Withholding (%)</label>
              <input type="number" className="form-input" value={form.taxWithholding} min="0" max="100" step="0.1"
                onChange={e => updateField('taxWithholding', e.target.value)} />
            </div>
          </div>
        </div>

        {/* Actions */}
        <div style={{ display: 'flex', gap: 12, justifyContent: 'flex-end' }}>
          <button type="button" className="btn btn-secondary" onClick={() => navigate('/admin/employees')}>Cancel</button>
          <button type="submit" className="btn btn-primary" disabled={saving}>
            {saving ? 'Saving...' : (isEdit ? 'Update Employee' : 'Create Employee')}
          </button>
        </div>
      </form>
    </div>
  );
};

export default EmployeeForm;
