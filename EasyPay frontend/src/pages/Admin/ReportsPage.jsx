import React, { useState, useEffect } from 'react';
import { reportService, departmentService } from '../../api/services';
import { useToast } from '../../context/ToastContext';

const formatCurrency = (val) => {
  if (val == null || isNaN(val)) return '—';
  return new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(val);
};

const ReportsPage = () => {
  const [activeTab, setActiveTab] = useState('payroll-register');
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(false);
  const [filters, setFilters] = useState({
    periodStart: '', periodEnd: '', year: new Date().getFullYear(), month: new Date().getMonth() + 1,
    status: '', departmentId: '',
  });
  const [departments, setDepartments] = useState([]);
  const toast = useToast();

  useEffect(() => {
    const fetchDepartments = async () => {
      try {
        const res = await departmentService.getActive();
        setDepartments(res.data?.data || res.data || []);
      } catch (err) {
        toast.error('Failed to load departments');
      }
    };
    fetchDepartments();
  }, []);

  const tabs = [
    { key: 'payroll-register', label: 'Payroll Register' },
    { key: 'headcount', label: 'Headcount' },
    { key: 'leave-utilisation', label: 'Leave Utilisation' },
    { key: 'payroll-status', label: 'Payroll Status' },
  ];

  const loadReport = async () => {
    setLoading(true);
    setData(null);
    try {
      let res;
      switch (activeTab) {
        case 'payroll-register':
          res = await reportService.payrollRegister({ periodStart: filters.periodStart, periodEnd: filters.periodEnd, status: filters.status || undefined });
          break;
        case 'headcount':
          res = await reportService.headcount();
          break;
        case 'leave-utilisation':
          res = await reportService.leaveUtilisation({ year: filters.year, departmentId: filters.departmentId || undefined });
          break;
        case 'payroll-status':
          res = await reportService.payrollStatusDashboard({ year: filters.year, month: filters.month });
          break;
      }
      setData(res.data?.data || res.data);
    } catch (e) {
      toast.error(e.response?.data?.message || 'Failed to load report');
    } finally { setLoading(false); }
  };

  const renderFilters = () => {
    switch (activeTab) {
      case 'payroll-register':
        return (
          <div className="filter-bar">
            <div className="form-group" style={{ marginBottom: 0 }}>
              <label className="form-label">Period Start</label>
              <input type="date" className="form-input" style={{ minWidth: 150 }}
                value={filters.periodStart} onChange={e => setFilters(f => ({ ...f, periodStart: e.target.value }))}
                max={filters.periodEnd || undefined} />
            </div>
            <div className="form-group" style={{ marginBottom: 0 }}>
              <label className="form-label">Period End</label>
              <input type="date" className="form-input" style={{ minWidth: 150 }}
                value={filters.periodEnd} onChange={e => setFilters(f => ({ ...f, periodEnd: e.target.value }))}
                min={filters.periodStart || undefined} />
            </div>
            <button className="btn btn-primary" onClick={loadReport} disabled={loading || !filters.periodStart || !filters.periodEnd}>
              {loading ? 'Loading...' : 'Generate'}
            </button>
          </div>
        );
      case 'headcount':
        return (
          <div className="filter-bar">
            <button className="btn btn-primary" onClick={loadReport} disabled={loading}>
              {loading ? 'Loading...' : 'Generate Report'}
            </button>
          </div>
        );
      case 'leave-utilisation':
        return (
          <div className="filter-bar">
            <div className="form-group" style={{ marginBottom: 0 }}>
              <label className="form-label">Year</label>
              <input type="number" className="form-input" style={{ minWidth: 100 }}
                value={filters.year} onChange={e => setFilters(f => ({ ...f, year: e.target.value }))} />
            </div>
            <div className="form-group" style={{ marginBottom: 0 }}>
              <label className="form-label">Department</label>
              <select className="form-select" style={{ minWidth: 180 }}
                value={filters.departmentId} onChange={e => setFilters(f => ({ ...f, departmentId: e.target.value }))}>
                <option value="">All Departments</option>
                {departments.map(d => (
                  <option key={d.departmentId} value={d.departmentId}>{d.departmentName}</option>
                ))}
              </select>
            </div>
            <button className="btn btn-primary" onClick={loadReport} disabled={loading}>
              {loading ? 'Loading...' : 'Generate'}
            </button>
          </div>
        );
      case 'payroll-status':
        return (
          <div className="filter-bar">
            <div className="form-group" style={{ marginBottom: 0 }}>
              <label className="form-label">Year</label>
              <input type="number" className="form-input" style={{ minWidth: 100 }}
                value={filters.year} onChange={e => setFilters(f => ({ ...f, year: e.target.value }))} />
            </div>
            <div className="form-group" style={{ marginBottom: 0 }}>
              <label className="form-label">Month</label>
              <input type="number" className="form-input" style={{ minWidth: 80 }} min="1" max="12"
                value={filters.month} onChange={e => setFilters(f => ({ ...f, month: e.target.value }))} />
            </div>
            <button className="btn btn-primary" onClick={loadReport} disabled={loading}>
              {loading ? 'Loading...' : 'Generate'}
            </button>
          </div>
        );
      default: return null;
    }
  };

  const renderData = () => {
    if (!data) return null;

    switch (activeTab) {
      case 'payroll-register':
        return (
          <div>
            <div className="stat-grid">
              <div className="stat-card accent-blue">
                <div className="stat-card-value">{data.totalRecords ?? 0}</div>
                <div className="stat-card-label">Total Records</div>
              </div>
              <div className="stat-card accent-green">
                <div className="stat-card-value">{formatCurrency(data.totalGross)}</div>
                <div className="stat-card-label">Total Gross</div>
              </div>
              <div className="stat-card accent-red">
                <div className="stat-card-value">{formatCurrency(data.totalDeductions)}</div>
                <div className="stat-card-label">Total Deductions</div>
              </div>
              <div className="stat-card accent-yellow">
                <div className="stat-card-value">{formatCurrency(data.totalNet)}</div>
                <div className="stat-card-label">Total Net</div>
              </div>
            </div>
            {data.records && data.records.length > 0 && (
              <div style={{ overflowX: 'auto' }}>
                <table className="data-table">
                  <thead><tr><th>Employee</th><th>Gross</th><th>PF</th><th>ESI</th><th>Tax</th><th>Net</th></tr></thead>
                  <tbody>
                    {data.records.map((r, i) => (
                      <tr key={i}>
                        <td><strong>{r.employeeName}</strong><div className="sub-text">{r.employeeCode}</div></td>
                        <td className="currency">{formatCurrency(r.grossEarnings)}</td>
                        <td className="currency">{formatCurrency(r.pfEmployee)}</td>
                        <td className="currency">{formatCurrency(r.esiEmployee)}</td>
                        <td className="currency">{formatCurrency(r.incomeTax)}</td>
                        <td className="currency" style={{ fontWeight: 700 }}>{formatCurrency(r.netSalary)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        );

      case 'headcount':
        return (
          <div>
            <div className="stat-grid">
              <div className="stat-card accent-blue">
                <div className="stat-card-value">{data.totalActive ?? 0}</div>
                <div className="stat-card-label">Total Active Employees</div>
              </div>
            </div>
            {data.departmentBreakdown && (
              <div style={{ overflowX: 'auto' }}>
                <table className="data-table">
                  <thead><tr><th>Department</th><th>Total</th><th>Full-Time</th><th>Part-Time</th><th>Contract</th><th>Intern</th></tr></thead>
                  <tbody>
                    {data.departmentBreakdown.map((d, i) => (
                      <tr key={i}>
                        <td><strong>{d.departmentName}</strong></td>
                        <td>{d.totalActive}</td><td>{d.fullTime}</td><td>{d.partTime}</td><td>{d.contract}</td><td>{d.intern}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        );

      case 'leave-utilisation':
        return (
          <div>
            <div className="stat-grid">
              <div className="stat-card accent-blue">
                <div className="stat-card-value">{data.totalRequests ?? 0}</div>
                <div className="stat-card-label">Total Requests</div>
              </div>
              <div className="stat-card accent-yellow">
                <div className="stat-card-value">{data.totalDays ?? 0}</div>
                <div className="stat-card-label">Total Days</div>
              </div>
            </div>
            {data.byLeaveType && (
              <table className="data-table">
                <thead><tr><th>Leave Type</th><th>Requests</th><th>Total Days</th></tr></thead>
                <tbody>
                  {data.byLeaveType.map((lt, i) => (
                    <tr key={i}><td><strong>{lt.leaveType}</strong></td><td>{lt.totalRequests}</td><td>{lt.totalDays}</td></tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        );

      case 'payroll-status':
        return (
          <div>
            <div className="stat-grid">
              <div className="stat-card accent-blue">
                <div className="stat-card-value">{data.totalCount ?? 0}</div>
                <div className="stat-card-label">Total Payrolls</div>
              </div>
            </div>
            {data.byStatus && (
              <table className="data-table">
                <thead><tr><th>Status</th><th>Count</th><th>Net Total</th></tr></thead>
                <tbody>
                  {data.byStatus.map((s, i) => (
                    <tr key={i}>
                      <td><span className={`badge ${s.status === 'Paid' || s.status === 'Approved' ? 'badge-success' : s.status === 'Pending' ? 'badge-warning' : 'badge-danger'}`}>{s.status}</span></td>
                      <td>{s.count}</td>
                      <td className="currency">{formatCurrency(s.netTotal)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        );

      default: return null;
    }
  };

  return (
    <div>
      <div className="page-header">
        <h1 className="page-title">Reports</h1>
        <div className="page-title-accent" />
        <p className="page-subtitle">Generate and view organizational reports</p>
      </div>

      <div className="tab-nav">
        {tabs.map(t => (
          <button key={t.key} className={`tab-btn ${activeTab === t.key ? 'active' : ''}`}
            onClick={() => { setActiveTab(t.key); setData(null); }}>
            {t.label}
          </button>
        ))}
      </div>

      {renderFilters()}

      <div className="card">
        {loading ? (
          <div className="loading-spinner"><div className="spinner" /> Generating report...</div>
        ) : data ? (
          renderData()
        ) : (
          <div className="empty-state">
            <div className="empty-icon">📊</div>
            <div className="empty-title">Select filters and generate</div>
            <div className="empty-message">Choose your report parameters and click Generate to see results.</div>
          </div>
        )}
      </div>
    </div>
  );
};

export default ReportsPage;
