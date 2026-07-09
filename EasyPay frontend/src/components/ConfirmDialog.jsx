import React from 'react';

const ConfirmDialog = ({ isOpen, onClose, onConfirm, title, message, confirmText = 'Confirm', variant = 'danger', loading }) => {
  if (!isOpen) return null;

  const icons = { danger: '⚠', warning: '⚡' };
  const iconColor = variant === 'danger' ? 'var(--hex-red)' : 'var(--hex-yellow)';

  return (
    <div className="modal-overlay confirm-dialog" onClick={(e) => { if (e.target === e.currentTarget) onClose(); }}>
      <div className="modal" style={{ maxWidth: 400 }}>
        <div className="modal-body" style={{ paddingTop: '32px', textAlign: 'center' }}>
          <div style={{ fontSize: '32px', color: iconColor, marginBottom: '16px' }}>
            {icons[variant] || '⚠'}
          </div>
          <div className="modal-title" style={{ marginBottom: '8px' }}>{title}</div>
          <div style={{ fontSize: '14px', color: 'var(--text-secondary)' }}>{message}</div>
        </div>
        <div className="modal-footer" style={{ justifyContent: 'center' }}>
          <button className="btn btn-secondary" onClick={onClose} disabled={loading}>Cancel</button>
          <button className={`btn btn-${variant}`} onClick={onConfirm} disabled={loading}>
            {loading ? 'Processing...' : confirmText}
          </button>
        </div>
      </div>
    </div>
  );
};

export default ConfirmDialog;
