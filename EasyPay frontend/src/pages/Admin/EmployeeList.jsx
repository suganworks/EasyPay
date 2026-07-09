import React, { useState, useEffect } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { employeeService } from '../../api/employeeService';
import { departmentService, designationService } from '../../api/services';
import { useToast } from '../../context/ToastContext';

const getStatusBadge = (status) => {
  if (status === 'Active') return 'badge-success';
  if (status === 'Terminated') return 'badge-danger';
  return 'badge-warning';
};
const getTypeBadge = (type) => {
  if (type === 'FullTime') return 'badge-info';
  return 'badge-neutral';
};

const EmployeeList = () => {
  const [employees, setEmployees] = useState([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const toast = useToast();

  const [searchParams] = useSearchParams();
  const [searchTerm, setSearchTerm] = useState('');
  const [departmentId, setDepartmentId] = useState(searchParams.get('departmentId') || '');
  const [designationId, setDesignationId] = useState(searchParams.get('designationId') || '');
  const [departments, setDepartments] = useState([]);
  const [designations, setDesignations] = useState([]);

  useEffect(() => {
    departmentService.getActive().then(res => setDepartments(res.data?.data || res.data || []));
  }, []);

  useEffect(() => {
    if (departmentId) {
      designationService.getByDepartment(departmentId).then(res => setDesignations(res.data?.data || res.data || []));
    } else {
      setDesignations([]);
    }
  }, [departmentId]);

  useEffect(() => {
    const delayDebounceFn = setTimeout(() => {
      fetchEmployees();
    }, 300);
    return () => clearTimeout(delayDebounceFn);
  }, [page, departmentId, designationId, searchTerm]);

  const fetchEmployees = async () => {
    setLoading(true);
    try {
      const params = { PageNumber: page, PageSize: 10 };
      if (departmentId) params.departmentId = departmentId;
      if (designationId) params.designationId = designationId;
      if (searchTerm) params.searchTerm = searchTerm;
      const res = await employeeService.getAll(params);
      setEmployees(res.data?.data || []);
      setTotalPages(res.data?.totalPages || 1);
      setTotalCount(res.data?.totalCount || 0);
    } catch (e) {
      toast.error('Failed to load employees');
    } finally { setLoading(false); }
  };

  return (
    <div>
      <div className="page-actions">
        <div className="page-header" style={{ marginBottom: 0 }}>
          <h1 className="page-title">Employees</h1>
          <div className="page-title-accent" />
          <p className="page-subtitle">{totalCount} employees</p>
        </div>
        <Link to="/admin/employees/new" className="btn btn-primary">+ Add Employee</Link>
      </div>

      <div className="filter-bar">
        <div className="form-group" style={{ marginBottom: 0 }}>
          <label className="form-label">Search</label>
          <input type="text" className="form-input" style={{ minWidth: 200 }} placeholder="Search name, code, email..."
            value={searchTerm} onChange={e => { setSearchTerm(e.target.value); setPage(1); }} />
        </div>
        <div className="form-group" style={{ marginBottom: 0 }}>
          <label className="form-label">Department</label>
          <select className="form-select" style={{ minWidth: 160 }}
            value={departmentId} onChange={e => { setDepartmentId(e.target.value); setDesignationId(''); setPage(1); }}>
            <option value="">All Departments</option>
            {departments.map(d => <option key={d.departmentId} value={d.departmentId}>{d.departmentName}</option>)}
          </select>
        </div>
        <div className="form-group" style={{ marginBottom: 0 }}>
          <label className="form-label">Designation</label>
          <select className="form-select" style={{ minWidth: 160 }}
            value={designationId} onChange={e => { setDesignationId(e.target.value); setPage(1); }}>
            <option value="">All Designations</option>
            {designations.map(d => <option key={d.designationId} value={d.designationId}>{d.designationName}</option>)}
          </select>
        </div>
        <button className="btn btn-secondary" onClick={() => { setSearchTerm(''); setDepartmentId(''); setDesignationId(''); setPage(1); }}>
          Clear
        </button>
      </div>

      <div className="card">
        {loading ? (
          <div className="loading-spinner"><div className="spinner" /> Loading...</div>
        ) : employees.length === 0 ? (
          <div className="empty-state">
            <div className="empty-icon">👥</div>
            <div className="empty-title">No employees found</div>
            <div className="empty-message">Add your first employee to get started.</div>
          </div>
        ) : (
          <div style={{ overflowX: 'auto' }}>
            <table className="data-table">
              <thead>
                <tr>
                  <th>Code</th>
                  <th>Name</th>
                  <th>Department</th>
                  <th>Designation</th>
                  <th>Type</th>
                  <th>Status</th>
                  <th className="text-right">Actions</th>
                </tr>
              </thead>
              <tbody>
                {employees.map(emp => (
                  <tr key={emp.employeeId}>
                    <td><span className="badge badge-info">{emp.employeeCode}</span></td>
                    <td>
                      <strong>{emp.fullName || `${emp.firstName} ${emp.lastName}`}</strong>
                      <div className="sub-text">{emp.workEmail}</div>
                    </td>
                    <td>{emp.departmentName || '—'}</td>
                    <td>{emp.designationName || '—'}</td>
                    <td><span className={`badge ${getTypeBadge(emp.employmentType)}`}>{emp.employmentType}</span></td>
                    <td>
                      <span className={`badge ${getStatusBadge(emp.employmentStatus)}`}>
                        {emp.employmentStatus || (emp.isActive ? 'Active' : 'Inactive')}
                      </span>
                    </td>
                    <td className="text-right">
                      <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end' }}>
                        <Link to={`/admin/employees/${emp.employeeId}`} className="btn-ghost">View</Link>
                        <Link to={`/admin/employees/${emp.employeeId}/edit`} className="btn-ghost">Edit</Link>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        {totalPages > 1 && (
          <div className="pagination">
            <button className="pagination-btn" disabled={page <= 1} onClick={() => setPage(p => p - 1)}>← Prev</button>
            <span className="pagination-info">Page {page} of {totalPages}</span>
            <button className="pagination-btn" disabled={page >= totalPages} onClick={() => setPage(p => p + 1)}>Next →</button>
          </div>
        )}
      </div>
    </div>
  );
};

export default EmployeeList;
