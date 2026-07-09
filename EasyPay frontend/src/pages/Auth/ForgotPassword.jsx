import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { authService } from '../../api/authService';
import { useToast } from '../../context/ToastContext';

const ForgotPassword = () => {
  const [email, setEmail] = useState('');
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);
  const navigate = useNavigate();
  const { success: toastSuccess } = useToast();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      await authService.forgotPassword({ email });
      setSuccess(true);
      toastSuccess('If an account exists, a reset link was sent to your email.');
    } catch (err) {
      // Don't leak if email exists or not, always show success and navigate
      setSuccess(true);
      toastSuccess('If an account exists, a reset link was sent to your email.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-page">
      <div className="login-left">
        <div className="login-left-content">
          <div className="login-brand">
            EasyPay<span className="dot">.</span>
          </div>
          <div className="login-brand-sub">by Hexaware Technologies</div>
          <div style={{
            width: 40, height: 3,
            background: 'linear-gradient(90deg, #F5A800 0%, rgba(255,255,255,0.3) 100%)',
            margin: '0 auto 24px',
            borderRadius: 2,
          }} />
          <p className="login-tagline">
            Precision payroll management<br />for the modern enterprise
          </p>
        </div>
      </div>

      <div className="login-right">
        <div>
          <h2 className="login-heading">Reset Password</h2>
          
          {success ? (
            <div style={{ textAlign: 'center', marginTop: 24 }}>
              <div className="badge badge-success" style={{ padding: '12px 24px', fontSize: 14 }}>
                Check your email for the reset link.
              </div>
              <button 
                onClick={() => navigate('/login')} 
                className="btn btn-secondary" 
                style={{ marginTop: 32, width: '100%' }}
              >
                Return to Login
              </button>
            </div>
          ) : (
            <>
              <p className="login-subheading">Enter your email to receive a password reset link.</p>
              <form onSubmit={handleSubmit}>
                <div className="form-group">
                  <label className="form-label">Email Address</label>
                  <input
                    type="email"
                    className="form-input"
                    placeholder="you@company.com"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    required
                    disabled={loading}
                  />
                </div>
                <button
                  type="submit"
                  className="btn btn-primary"
                  style={{ width: '100%', marginTop: 8, padding: '12px' }}
                  disabled={loading}
                >
                  {loading ? 'Sending Request...' : 'Send Reset Link'}
                </button>
              </form>
              <div style={{ textAlign: 'center', marginTop: 24 }}>
                <a href="/login" style={{ fontSize: 13, color: 'var(--text-muted)', textDecoration: 'none' }}>
                  &larr; Back to Login
                </a>
              </div>
            </>
          )}
        </div>
      </div>
    </div>
  );
};

export default ForgotPassword;
