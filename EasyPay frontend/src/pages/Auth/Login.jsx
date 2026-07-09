import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';

const Login = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const { login, loading, error, setError, isAuthenticated, getRolePath } = useAuth();
  const navigate = useNavigate();

  React.useEffect(() => {
    if (isAuthenticated) navigate(getRolePath(), { replace: true });
  }, [isAuthenticated]);

  const handleLogin = async (e) => {
    e.preventDefault();
    setError(null);
    try {
      const userData = await login(email, password);
      const role = userData?.role?.toLowerCase();
      switch (role) {
        case 'admin':
        case 'hrmanager':
          navigate('/admin'); break;
        case 'manager': navigate('/manager'); break;
        case 'payrollprocessor': navigate('/processor'); break;
        default: navigate('/employee'); break;
      }
    } catch (err) { /* handled in context */ }
  };

  return (
    <div className="login-page">
      {/* Left decorative panel */}
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

      {/* Right login form */}
      <div className="login-right">
        <div>
          <h2 className="login-heading">Welcome back</h2>
          <p className="login-subheading">Sign in to continue to your dashboard</p>

          {error && <div className="error-box">{error}</div>}

          <form onSubmit={handleLogin}>
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
            <div className="form-group">
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <label className="form-label">Password</label>
                <a href="/forgot-password" style={{ fontSize: 13, color: 'var(--accent-color)', textDecoration: 'none' }}>
                  Forgot Password?
                </a>
              </div>
              <div style={{ position: 'relative' }}>
                <input
                  type={showPassword ? "text" : "password"}
                  className="form-input"
                  placeholder="Enter your password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  required
                  disabled={loading}
                  style={{ paddingRight: '40px' }}
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  style={{
                    position: 'absolute',
                    right: '10px',
                    top: '50%',
                    transform: 'translateY(-50%)',
                    background: 'none',
                    border: 'none',
                    cursor: 'pointer',
                    color: 'var(--text-muted)',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    padding: 4
                  }}
                  title={showPassword ? "Hide password" : "Show password"}
                >
                  {showPassword ? (
                    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                      <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"></path>
                      <line x1="1" y1="1" x2="23" y2="23"></line>
                    </svg>
                  ) : (
                    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                      <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path>
                      <circle cx="12" cy="12" r="3"></circle>
                    </svg>
                  )}
                </button>
              </div>
            </div>

            <button
              type="submit"
              className="btn btn-primary"
              style={{ width: '100%', marginTop: 8, padding: '12px' }}
              disabled={loading}
            >
              {loading ? 'Authenticating...' : 'Sign In'}
            </button>
          </form>

          <div style={{
            textAlign: 'center',
            marginTop: 32,
            fontSize: 13,
            color: 'var(--text-muted)',
          }}>
            Please contact your HR administrator if you need an account.
          </div>
        </div>
      </div>
    </div>
  );
};

export default Login;
