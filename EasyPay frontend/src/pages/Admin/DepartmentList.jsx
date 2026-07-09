import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { departmentService, reportService } from '../../api/services';
import Modal from '../../components/Modal';
import ConfirmDialog from '../../components/ConfirmDialog';
import { useToast } from '../../context/ToastContext';

const DepartmentList = () => {
  const [departments, setDepartments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState(null);
  const [formData, setFormData] = useState({ departmentCode: '', departmentName: '', description: '' });
  const [saving, setSaving] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [deleting, setDeleting] = useState(false);
  const toast = useToast();

  useEffect(() => { fetchDepartments(); }, []);

  const fetchDepartments = async () => {
    setLoading(true);
    try {
      const [departmentRes, headcountRes] = await Promise.allSettled([
        departmentService.getAll(),
        reportService.headcount(),
      ]);

      const baseDepartments = departmentRes.status === 'fulfilled'
        ? (departmentRes.value.data?.data || departmentRes.value.data || [])
        : [];

      const breakdown = headcountRes.status === 'fulfilled'
        ? (headcountRes.value.data?.data?.departmentBreakdown || headcountRes.value.data?.departmentBreakdown || [])
        : [];

      const countsById = new Map(breakdown.map(item => [String(item.departmentId), item.totalActive ?? 0]));

      setDepartments(baseDepartments.map(dept => ({
        ...dept,
        employeeCount: countsById.has(String(dept.departmentId))
          ? countsById.get(String(dept.departmentId))
          : (dept.employeeCount ?? 0),
      })));
    } catch (e) {
      toast.error('Failed to load departments');
    } finally { setLoading(false); }
  };

  const openCreate = () => {
    setEditing(null);
    setFormData({ departmentCode: '', departmentName: '', description: '' });
    setModalOpen(true);
  };

  const openEdit = (dept) => {
    setEditing(dept);
    setFormData({
      departmentCode: dept.departmentCode || '',
      departmentName: dept.departmentName || '',
      description: dept.description || '',
    });
    setModalOpen(true);
  };

  const handleSave = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      if (editing) {
        await departmentService.update(editing.departmentId, {
          departmentName: formData.departmentName,
          description: formData.description,
          isActive: editing.isActive,
        });
        toast.success('Department updated successfully');
      } else {
        await departmentService.create({
          departmentCode: formData.departmentCode,
          departmentName: formData.departmentName,
          description: formData.description,
        });
        toast.success('Department created successfully');
      }
      setModalOpen(false);
      fetchDepartments();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Operation failed');
    } finally { setSaving(false); }
  };

  const handleDelete = async () => {
    setDeleting(true);
    try {
      await departmentService.delete(deleteTarget.departmentId);
      toast.success('Department deleted');
      setDeleteTarget(null);
      fetchDepartments();
    } catch (e) {
      const msg = e.response?.data?.message || 'Delete failed';
      toast.error(msg);
    } finally { setDeleting(false); }
  };

  return (
    <div>
      <div className="page-actions">
        <div className="page-header" style={{ marginBottom: 0 }}>
          <h1 className="page-title">Departments</h1>
          <div className="page-title-accent" />
          <p className="page-subtitle">{departments.length} departments · Organizational structure</p>
        </div>
        <button className="btn btn-primary" onClick={openCreate}>+ New Department</button>
      </div>

      <div className="card">
        {loading ? (
          <div className="loading-spinner"><div className="spinner" /> Loading departments...</div>
        ) : departments.length === 0 ? (
          <div className="empty-state">
            <div className="empty-icon">🏢</div>
            <div className="empty-title">No departments yet</div>
            <div className="empty-message">Create your first department to organize your workforce.</div>
            <button className="btn btn-primary" onClick={openCreate} style={{ marginTop: 16 }}>+ Create Department</button>
          </div>
        ) : (
          <div style={{ overflowX: 'auto' }}>
            <table className="data-table">
              <thead>
                <tr>
                  <th>Code</th>
                  <th>Name</th>
                  <th>Description</th>
                  <th>Employees</th>
                  <th>Status</th>
                  <th className="text-right">Actions</th>
                </tr>
              </thead>
              <tbody>
                {departments.map(dept => (
                  <tr key={dept.departmentId}>
                    <td><span className="badge badge-info">{dept.departmentCode}</span></td>
                    <td>
                      <Link to={`/admin/employees?departmentId=${dept.departmentId}`}>
                        <strong>{dept.departmentName}</strong>
                      </Link>
                    </td>
                    <td className="text-secondary">{dept.description || '—'}</td>
                    <td>{dept.employeeCount ?? 0}</td>
                    <td>
                      <span className={`badge ${dept.isActive !== false ? 'badge-success' : 'badge-danger'}`}>
                        {dept.isActive !== false ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                    <td className="text-right">
                      <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end' }}>
                        <button className="btn-ghost" onClick={() => openEdit(dept)}>Edit</button>
                        <button className="btn-ghost danger" onClick={() => setDeleteTarget(dept)}>Delete</button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      <Modal
        isOpen={modalOpen}
        onClose={() => setModalOpen(false)}
        title={editing ? 'Edit Department' : 'New Department'}
        footer={
          <>
            <button className="btn btn-secondary" onClick={() => setModalOpen(false)} disabled={saving}>Cancel</button>
            <button className="btn btn-primary" onClick={handleSave} disabled={saving}>
              {saving ? 'Saving...' : (editing ? 'Update' : 'Create')}
            </button>
          </>
        }
      >
        <form onSubmit={handleSave}>
          {!editing && (
            <div className="form-group">
              <label className="form-label">Department Code</label>
              <input type="text" className="form-input" placeholder="e.g. IT, HR, FIN"
                value={formData.departmentCode}
                onChange={(e) => setFormData({ ...formData, departmentCode: e.target.value })}
                required />
            </div>
          )}
          <div className="form-group">
            <label className="form-label">Department Name</label>
            <input type="text" className="form-input" placeholder="e.g. Information Technology"
              value={formData.departmentName}
              onChange={(e) => setFormData({ ...formData, departmentName: e.target.value })}
              required />
          </div>
          <div className="form-group">
            <label className="form-label">Description</label>
            <textarea className="form-input" placeholder="Brief description of the department"
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              rows={3} />
          </div>
        </form>
      </Modal>

      <ConfirmDialog
        isOpen={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleDelete}
        title="Delete Department"
        message={
          deleteTarget?.employeeCount > 0
            ? `"${deleteTarget?.departmentName}" has ${deleteTarget?.employeeCount} active employee(s). Deletion will fail if active employees exist.`
            : `Are you sure you want to delete "${deleteTarget?.departmentName}"? This action cannot be undone.`
        }
        confirmText="Delete"
        variant="danger"
        loading={deleting}
      />
    </div>
  );
};

export default DepartmentList;
