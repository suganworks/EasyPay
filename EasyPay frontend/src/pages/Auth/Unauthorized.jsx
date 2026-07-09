import React from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';

const Unauthorized = () => {
  const { user } = useAuth();
  return (
    <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '100vh', background: 'var(--off-white)' }}>
      <div className="card" style={{ maxWidth: 480, textAlign: 'center', padding: 48 }}>
        <div className="empty-icon">🚫</div>
        <h1 className="page-title" style={{ marginBottom: 8 }}>Access Denied</h1>
        <p className="page-subtitle" style={{ marginBottom: 24 }}>
          You do not have permission to access this page.
        </p>
        {user ? (
          <Link to="/" className="btn btn-primary">Go to Dashboard</Link>
        ) : (
          <Link to="/login" className="btn btn-primary">Sign In</Link>
        )}
      </div>
    </div>
  );
};

export default Unauthorized;
