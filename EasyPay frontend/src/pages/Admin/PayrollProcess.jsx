import React, { useState, useEffect } from 'react';
import { payrollService } from '../../api/payrollService';
import { employeeService } from '../../api/employeeService';
import { departmentService, salaryStructureService } from '../../api/services';
import { useToast } from '../../context/ToastContext';

const PayrollProcess = () => {
  const [mode, setMode] = useState('individual'); // individual | bulk
  const [employees, setEmployees] = useState([]);
  const [departments, setDepartments] = useState([]);
  const [loading, setLoading] = useState(false);
  const [processing, setProcessing] = useState(false);
  const [result, setResult] = useState(null);

  const [individual, setIndividual] = useState({
    employeeId: '', payPeriodStart: '', payPeriodEnd: '', bonusAmount: 0, otherDeductions: 0, remarks: '',
  });
  const [bulk, setBulk] = useState({
    payPeriodStart: '', payPeriodEnd: '', departmentId: '', remarks: '',
  });
  const [salaryCheck, setSalaryCheck] = useState(null); // null=unchecked, true=exists, false=missing
  const toast = useToast();

  useEffect(() => {
    loadDropdowns();
  }, []);

  const loadDropdowns = async () => {
    try {
      const empRes = await employeeService.getAll({ PageNumber: 1, PageSize: 200 });
      setEmployees(empRes.data?.data || []);
    } catch (e) { /* ignore */ }
    try {
      const deptRes = await departmentService.getActive();
      setDepartments(deptRes.data?.data || deptRes.data || []);
    } catch (e) { /* ignore */ }
  };

  const checkSalaryStructure = async (empId) => {
    if (!empId) { setSalaryCheck(null); return; }
    try {
      const res = await salaryStructureService.getEmployeeCurrent(empId);
      setSalaryCheck(res.data?.data || res.data ? true : false);
    } catch (e) {
      setSalaryCheck(false);
    }
  };

  const handleEmployeeChange = (empId) => {
    setIndividual(prev => ({ ...prev, employeeId: empId }));
    checkSalaryStructure(empId);
  };

  const handleProcessIndividual = async (e) => {
    e.preventDefault();
    if (salaryCheck === false) {
      toast.error('This employee has no salary structure. Please create one first.');
      return;
    }
    setProcessing(true);
    try {
      const res = await payrollService.process({
        employeeId: Number(individual.employeeId),
        payPeriodStart: individual.payPeriodStart,
        payPeriodEnd: individual.payPeriodEnd,
        bonusAmount: Number(individual.bonusAmount) || undefined,
        otherDeductions: Number(individual.otherDeductions) || undefined,
        remarks: individual.remarks || undefined,
      });
      toast.success('Payroll processed successfully');
      setResult(res.data?.data || res.data);
    } catch (e) {
      const msg = e.response?.data?.message || 'Processing failed';
      if (e.response?.status === 409) {
        toast.error('Payroll already processed for this period');
      } else {
        toast.error(msg);
      }
    } finally { setProcessing(false); }
  };

  const handleProcessBulk = async (e) => {
    e.preventDefault();
    setProcessing(true);
    try {
      const payload = {
        payPeriodStart: bulk.payPeriodStart,
        payPeriodEnd: bulk.payPeriodEnd,
        remarks: bulk.remarks || undefined,
      };
      if (bulk.departmentId) {
        // Get employees from department and process them
        const empRes = await employeeService.getByDepartment(Number(bulk.departmentId));
        const empList = empRes.data?.data || empRes.data || [];
        payload.employeeIds = empList.map(e => e.employeeId);
      }
      const res = await payrollService.bulkProcess(payload);
      const results = res.data?.data || res.data || [];
      toast.success(`Bulk processing complete: ${Array.isArray(results) ? results.length : 1} payrolls processed`);
      setResult(results);
    } catch (e) {
      toast.error(e.response?.data?.message || 'Bulk processing failed');
    } finally { setProcessing(false); }
  };

  const formatCurrency = (val) => {
    if (val == null) return '—';
    return new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(val);
  };

  return (
    <div>
      <div className="page-header">
        <h1 className="page-title">Process Payroll</h1>
        <div className="page-title-accent" />
        <p className="page-subtitle">Process payroll for individual employees or in bulk</p>
      </div>

      {/* Mode Toggle */}
      <div className="tab-nav">
        <button className={`tab-btn ${mode === 'individual' ? 'active' : ''}`}
          onClick={() => { setMode('individual'); setResult(null); }}>Individual</button>
        <button className={`tab-btn ${mode === 'bulk' ? 'active' : ''}`}
          onClick={() => { setMode('bulk'); setResult(null); }}>Bulk Process</button>
      </div>

      {!result ? (
        <div className="card">
          {mode === 'individual' ? (
            <form onSubmit={handleProcessIndividual}>
              <div className="form-grid">
                <div className="form-group full-width">
                  <label className="form-label">Employee *</label>
                  <select className="form-input" value={individual.employeeId}
                    onChange={e => handleEmployeeChange(e.target.value)} required>
                    <option value="">Select Employee</option>
                    {employees.map(emp => (
                      <option key={emp.employeeId} value={emp.employeeId}>
                        {emp.fullName || `${emp.firstName} ${emp.lastName}`} ({emp.employeeCode})
                      </option>
                    ))}
                  </select>
                </div>
                {salaryCheck === false && (
                  <div className="full-width">
                    <div className="warning-box">
                      ⚠️ No salary structure found for this employee. Please create one before processing payroll.
                    </div>
                  </div>
                )}
                <div className="form-group">
                  <label className="form-label">Period Start *</label>
                  <input type="date" className="form-input" value={individual.payPeriodStart}
                    max={individual.payPeriodEnd || undefined}
                    onChange={e => setIndividual(p => ({ ...p, payPeriodStart: e.target.value }))} required />
                </div>
                <div className="form-group">
                  <label className="form-label">Period End *</label>
                  <input type="date" className="form-input" value={individual.payPeriodEnd}
                    min={individual.payPeriodStart || undefined}
                    onChange={e => setIndividual(p => ({ ...p, payPeriodEnd: e.target.value }))} required />
                </div>
                <div className="form-group">
                  <label className="form-label">Bonus Amount</label>
                  <input type="number" className="form-input" min="0" step="100"
                    value={individual.bonusAmount}
                    onChange={e => setIndividual(p => ({ ...p, bonusAmount: e.target.value }))} />
                </div>
                <div className="form-group">
                  <label className="form-label">Other Deductions</label>
                  <input type="number" className="form-input" min="0" step="100"
                    value={individual.otherDeductions}
                    onChange={e => setIndividual(p => ({ ...p, otherDeductions: e.target.value }))} />
                </div>
                <div className="form-group full-width">
                  <label className="form-label">Remarks</label>
                  <textarea className="form-input" rows={2} value={individual.remarks}
                    onChange={e => setIndividual(p => ({ ...p, remarks: e.target.value }))} placeholder="Optional remarks" />
                </div>
              </div>
              <div style={{ display: 'flex', justifyContent: 'flex-end', marginTop: 16 }}>
                <button type="submit" className="btn btn-primary" disabled={processing || salaryCheck === false}>
                  {processing ? 'Processing...' : 'Process Payroll'}
                </button>
              </div>
            </form>
          ) : (
            <form onSubmit={handleProcessBulk}>
              <div className="form-grid">
                <div className="form-group">
                  <label className="form-label">Period Start *</label>
                  <input type="date" className="form-input" value={bulk.payPeriodStart}
                    max={bulk.payPeriodEnd || undefined}
                    onChange={e => setBulk(p => ({ ...p, payPeriodStart: e.target.value }))} required />
                </div>
                <div className="form-group">
                  <label className="form-label">Period End *</label>
                  <input type="date" className="form-input" value={bulk.payPeriodEnd}
                    min={bulk.payPeriodStart || undefined}
                    onChange={e => setBulk(p => ({ ...p, payPeriodEnd: e.target.value }))} required />
                </div>
                <div className="form-group full-width">
                  <label className="form-label">Department (leave empty for all)</label>
                  <select className="form-input" value={bulk.departmentId}
                    onChange={e => setBulk(p => ({ ...p, departmentId: e.target.value }))}>
                    <option value="">All Departments</option>
                    {departments.map(d => <option key={d.departmentId} value={d.departmentId}>{d.departmentName}</option>)}
                  </select>
                </div>
                <div className="form-group full-width">
                  <label className="form-label">Remarks</label>
                  <textarea className="form-input" rows={2} value={bulk.remarks}
                    onChange={e => setBulk(p => ({ ...p, remarks: e.target.value }))} />
                </div>
              </div>
              <div style={{ display: 'flex', justifyContent: 'flex-end', marginTop: 16 }}>
                <button type="submit" className="btn btn-primary" disabled={processing}>
                  {processing ? 'Processing...' : 'Process Bulk Payroll'}
                </button>
              </div>
            </form>
          )}
        </div>
      ) : (
        <div className="card">
          <div className="card-header">Processing Complete ✅</div>
          {Array.isArray(result) ? (
            <div>
              <p style={{ marginBottom: 16 }}>{result.length} payroll(s) processed successfully.</p>
              <table className="data-table">
                <thead>
                  <tr><th>Employee</th><th>Gross</th><th>Deductions</th><th>Net Salary</th></tr>
                </thead>
                <tbody>
                  {result.map(r => (
                    <tr key={r.payrollId}>
                      <td>{r.employeeName} ({r.employeeCode})</td>
                      <td className="currency">{formatCurrency(r.grossEarnings)}</td>
                      <td className="currency">{formatCurrency(r.totalDeductions)}</td>
                      <td className="currency" style={{ fontWeight: 700 }}>{formatCurrency(r.netSalary)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : (
            <div className="detail-grid">
              <div className="detail-item"><div className="detail-label">Employee</div><div className="detail-value">{result.employeeName}</div></div>
              <div className="detail-item"><div className="detail-label">Net Salary</div><div className="detail-value currency" style={{ fontWeight: 700, fontSize: 18 }}>{formatCurrency(result.netSalary)}</div></div>
              <div className="detail-item"><div className="detail-label">Gross Earnings</div><div className="detail-value currency">{formatCurrency(result.grossEarnings)}</div></div>
              <div className="detail-item"><div className="detail-label">Total Deductions</div><div className="detail-value currency">{formatCurrency(result.totalDeductions)}</div></div>
            </div>
          )}
          <div style={{ marginTop: 20 }}>
            <button className="btn btn-secondary" onClick={() => setResult(null)}>Process Another</button>
          </div>
        </div>
      )}
    </div>
  );
};

export default PayrollProcess;
