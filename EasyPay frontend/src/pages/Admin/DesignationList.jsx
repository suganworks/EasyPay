import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { designationService, departmentService } from '../../api/services';
import Modal from '../../components/Modal';
import ConfirmDialog from '../../components/ConfirmDialog';
import { useToast } from '../../context/ToastContext';

const DesignationList = () => {
  const [designations, setDesignations] = useState([]);
  const [departments, setDepartments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [filterDept, setFilterDept] = useState('');
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState(null);
  const [formData, setFormData] = useState({ designationCode: '', designationName: '', departmentId: '', gradeLevel: '' });
  const [saving, setSaving] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [deleting, setDeleting] = useState(false);
  const toast = useToast();

  useEffect(() => {
    fetchDesignations();
    fetchDepartments();
  }, []);

  const fetchDepartments = async () => {
    try {
      const res = await departmentService.getActive();
      setDepartments(res.data?.data || res.data || []);
    } catch (e) { /* ignore */ }
  };

  const fetchDesignations = async () => {
    setLoading(true);
    try {
      const res = await designationService.getAll();
      setDesignations(res.data?.data || res.data || []);
    } catch (e) {
      toast.error('Failed to load designations');
    } finally { setLoading(false); }
  };

  const filtered = filterDept
    ? designations.filter(d => d.departmentId === Number(filterDept))
    : designations;

  const openCreate = () => {
    setEditing(null);
    setFormData({ designationCode: '', designationName: '', departmentId: '', gradeLevel: '' });
    setModalOpen(true);
  };

  const openEdit = (desig) => {
    setEditing(desig);
    setFormData({
      designationCode: desig.designationCode || '',
      designationName: desig.designationName || '',
      departmentId: desig.departmentId || '',
      gradeLevel: desig.gradeLevel || '',
    });
    setModalOpen(true);
  };

  const handleSave = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      const payload = {
        designationName: formData.designationName,
        departmentId: Number(formData.departmentId),
        gradeLevel: formData.gradeLevel || undefined,
      };
      if (editing) {
        await designationService.update(editing.designationId, { ...payload, isActive: editing.isActive });
        toast.success('Designation updated');
      } else {
        await designationService.create({ ...payload, designationCode: formData.designationCode });
        toast.success('Designation created');
      }
      setModalOpen(false);
      fetchDesignations();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Operation failed');
    } finally { setSaving(false); }
  };

  const handleDelete = async () => {
    setDeleting(true);
    try {
      await designationService.delete(deleteTarget.designationId);
      toast.success('Designation deleted');
      setDeleteTarget(null);
      fetchDesignations();
    } catch (e) {
      toast.error(e.response?.data?.message || 'Delete failed');
    } finally { setDeleting(false); }
  };

  return (
    <div>
      <div className="page-actions">
        <div className="page-header" style={{ marginBottom: 0 }}>
          <h1 className="page-title">Designations</h1>
          <div className="page-title-accent" />
          <p className="page-subtitle">{filtered.length} designations</p>
        </div>
        <button className="btn btn-primary" onClick={openCreate}>+ New Designation</button>
      </div>

      <div className="filter-bar">
        <div className="form-group" style={{ marginBottom: 0, minWidth: 200 }}>
          <label className="form-label">Filter by Department</label>
          <select className="form-input" value={filterDept} onChange={e => setFilterDept(e.target.value)}>
            <option value="">All Departments</option>
            {departments.map(d => (
              <option key={d.departmentId} value={d.departmentId}>{d.departmentName}</option>
            ))}
          </select>
        </div>
      </div>

      <div className="card">
        {loading ? (
          <div className="loading-spinner"><div className="spinner" /> Loading...</div>
        ) : filtered.length === 0 ? (
          <div className="empty-state">
            <div className="empty-icon">📋</div>
            <div className="empty-title">No designations found</div>
            <div className="empty-message">Create designations to define job roles within departments.</div>
          </div>
        ) : (
          <div style={{ overflowX: 'auto' }}>
            <table className="data-table">
              <thead>
                <tr>
                  <th>Code</th>
                  <th>Title</th>
                  <th>Department</th>
                  <th>Grade</th>
                  <th>Status</th>
                  <th className="text-right">Actions</th>
                </tr>
              </thead>
              <tbody>
                {filtered.map(desig => (
                  <tr key={desig.designationId}>
                    <td><span className="badge badge-info">{desig.designationCode}</span></td>
                    <td>
                      <Link to={`/admin/employees?designationId=${desig.designationId}`}>
                        <strong>{desig.designationName}</strong>
                      </Link>
                    </td>
                    <td>{desig.departmentName || '—'}</td>
                    <td>{desig.gradeLevel || '—'}</td>
                    <td>
                      <span className={`badge ${desig.isActive !== false ? 'badge-success' : 'badge-danger'}`}>
                        {desig.isActive !== false ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                    <td className="text-right">
                      <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end' }}>
                        <button className="btn-ghost" onClick={() => openEdit(desig)}>Edit</button>
                        <button className="btn-ghost danger" onClick={() => setDeleteTarget(desig)}>Delete</button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      <Modal isOpen={modalOpen} onClose={() => setModalOpen(false)}
        title={editing ? 'Edit Designation' : 'New Designation'}
        footer={<>
          <button className="btn btn-secondary" onClick={() => setModalOpen(false)} disabled={saving}>Cancel</button>
          <button className="btn btn-primary" onClick={handleSave} disabled={saving}>
            {saving ? 'Saving...' : (editing ? 'Update' : 'Create')}
          </button>
        </>}
      >
        <form onSubmit={handleSave}>
          {!editing && (
            <div className="form-group">
              <label className="form-label">Designation Code</label>
              <input type="text" className="form-input" placeholder="e.g. SE, SSE, TL"
                value={formData.designationCode}
                onChange={e => setFormData({ ...formData, designationCode: e.target.value })} required />
            </div>
          )}
          <div className="form-group">
            <label className="form-label">Designation Name</label>
            <input type="text" className="form-input" placeholder="e.g. Software Engineer"
              value={formData.designationName}
              onChange={e => setFormData({ ...formData, designationName: e.target.value })} required />
          </div>
          <div className="form-group">
            <label className="form-label">Department</label>
            <select className="form-input" value={formData.departmentId}
              onChange={e => setFormData({ ...formData, departmentId: e.target.value })} required>
              <option value="">Select Department</option>
              {departments.map(d => (
                <option key={d.departmentId} value={d.departmentId}>{d.departmentName}</option>
              ))}
            </select>
          </div>
          <div className="form-group">
            <label className="form-label">Grade Level (optional)</label>
            <input type="text" className="form-input" placeholder="e.g. L1, L2, Senior"
              value={formData.gradeLevel}
              onChange={e => setFormData({ ...formData, gradeLevel: e.target.value })} />
          </div>
        </form>
      </Modal>

      <ConfirmDialog isOpen={!!deleteTarget} onClose={() => setDeleteTarget(null)}
        onConfirm={handleDelete} title="Delete Designation"
        message={`Are you sure you want to delete "${deleteTarget?.designationName}"?`}
        confirmText="Delete" variant="danger" loading={deleting} />
    </div>
  );
};

export default DesignationList;
