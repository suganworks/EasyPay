import React, { useState, useEffect } from 'react';
import { userService } from '../../api/userService';
import { useToast } from '../../context/ToastContext';
import ConfirmDialog from '../../components/ConfirmDialog';

const UserManagement = () => {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [confirmToggle, setConfirmToggle] = useState(null);
  const { success: toastSuccess, error: toastError } = useToast();

  useEffect(() => {
    fetchUsers();
  }, []);

  const fetchUsers = async () => {
    try {
      const res = await userService.getAll({ PageNumber: 1, PageSize: 100 });
      setUsers(res.data?.data?.data || []);
    } catch (e) {
      toastError('Failed to load users');
    } finally {
      setLoading(false);
    }
  };

  const handleToggleStatus = async () => {
    if (!confirmToggle) return;
    try {
      await userService.toggleStatus(confirmToggle.userId, !confirmToggle.isActive);
      toastSuccess(`User ${!confirmToggle.isActive ? 'activated' : 'deactivated'} successfully`);
      fetchUsers();
    } catch (e) {
      toastError('Failed to change status');
    } finally {
      setConfirmToggle(null);
    }
  };

  return (
    <div className="page-container animate-fade-in">
      <div className="page-header">
        <h1 className="page-title">User Management</h1>
        <p className="page-subtitle">Manage system access and roles</p>
      </div>

      <div className="glass-panel" style={{ padding: '24px' }}>
        {loading ? (
          <div>Loading users...</div>
        ) : (
          <div className="table-responsive">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Username</th>
                  <th>Email</th>
                  <th>Role ID</th>
                  <th>Status</th>
                  <th style={{ textAlign: 'right' }}>Actions</th>
                </tr>
              </thead>
              <tbody>
                {users.map(u => (
                  <tr key={u.userId}>
                    <td>{u.username}</td>
                    <td>{u.email}</td>
                    <td>{u.role}</td>
                    <td>
                      <span className={`badge ${u.isActive ? 'badge-success' : 'badge-danger'}`}>
                        {u.isActive ? 'Active' : 'Locked'}
                      </span>
                    </td>
                    <td style={{ textAlign: 'right' }}>
                      <button 
                        className="btn btn-secondary" 
                        style={{ padding: '4px 12px', fontSize: 13 }}
                        onClick={() => setConfirmToggle({ userId: u.userId, isActive: u.isActive })}
                      >
                        {u.isActive ? 'Lock Account' : 'Unlock Account'}
                      </button>
                    </td>
                  </tr>
                ))}
                {users.length === 0 && (
                  <tr>
                    <td colSpan="5" style={{ textAlign: 'center', padding: '24px' }}>No users found</td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {confirmToggle && (
        <ConfirmDialog
          title={`${confirmToggle.isActive ? 'Lock' : 'Unlock'} Account`}
          message={`Are you sure you want to ${confirmToggle.isActive ? 'lock' : 'unlock'} this user account?`}
          onConfirm={handleToggleStatus}
          onCancel={() => setConfirmToggle(null)}
          confirmText="Yes, Proceed"
          isDanger={confirmToggle.isActive}
        />
      )}
    </div>
  );
};

export default UserManagement;
