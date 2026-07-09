import React, { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { notificationService } from '../api/services';

const Navbar = ({ onMenuClick }) => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [unreadCount, setUnreadCount] = useState(0);
  const [showNotif, setShowNotif] = useState(false);
  const [notifications, setNotifications] = useState([]);
  const notifRef = useRef(null);

  useEffect(() => {
    fetchUnreadCount();
    const interval = setInterval(fetchUnreadCount, 30000);
    return () => clearInterval(interval);
  }, []);

  useEffect(() => {
    const handleClickOutside = (e) => {
      if (notifRef.current && !notifRef.current.contains(e.target)) setShowNotif(false);
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const fetchUnreadCount = async () => {
    try {
      const res = await notificationService.getUnreadCount();
      setUnreadCount(res.data?.data?.unreadCount ?? res.data?.data ?? 0);
    } catch (e) { /* ignore */ }
  };

  const fetchNotifications = async () => {
    try {
      const res = await notificationService.getAll({ PageSize: 10, PageNumber: 1 });
      setNotifications(res.data?.data || []);
    } catch (e) { /* ignore */ }
  };

  const handleBellClick = () => {
    if (!showNotif) fetchNotifications();
    setShowNotif(!showNotif);
  };

  const handleMarkAllRead = async () => {
    try {
      await notificationService.markAllRead();
      setUnreadCount(0);
      setNotifications(prev => prev.map(n => ({ ...n, isRead: true })));
    } catch (e) { /* ignore */ }
  };

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  const initials = (user?.fullName || user?.username || 'U')
    .split(' ')
    .map(n => n[0])
    .join('')
    .slice(0, 2)
    .toUpperCase();

  return (
    <header className="navbar">
      {/* Hamburger menu button — visible only on mobile/tablet */}
      <button
        className="navbar-hamburger"
        onClick={onMenuClick}
        aria-label="Toggle navigation menu"
      >
        <span /><span /><span />
      </button>

      {/* Logo */}
      <div className="navbar-logo">
        <span className="navbar-logo-text">
          EasyPay<span className="dot">.</span>
        </span>
        <span className="navbar-logo-subtitle">by Hexaware</span>
      </div>

      {/* Right side controls */}
      <div className="navbar-actions">
        {/* Notification bell */}
        <div ref={notifRef} style={{ position: 'relative' }}>
          <button
            className="navbar-icon-btn"
            onClick={handleBellClick}
            title="Notifications"
          >
            🔔
            {unreadCount > 0 && (
              <span className="navbar-badge-count">
                {unreadCount > 9 ? '9+' : unreadCount}
              </span>
            )}
          </button>

          {showNotif && (
            <div className="notif-dropdown">
              <div className="notif-dropdown-header">
                <span style={{ fontFamily: 'var(--font-heading)', fontSize: '15px', fontWeight: 700 }}>
                  Notifications
                </span>
                {unreadCount > 0 && (
                  <button onClick={handleMarkAllRead} className="btn-ghost" style={{ fontSize: '11px', padding: '4px 8px' }}>
                    Mark all read
                  </button>
                )}
              </div>
              {notifications.length === 0 ? (
                <div className="notif-empty">No notifications yet</div>
              ) : (
                notifications.map((n, i) => (
                  <div
                    key={n.notificationId || i}
                    className={`notif-item ${n.isRead ? 'read' : ''}`}
                  >
                    <div className="notif-title">{n.title || 'Notification'}</div>
                    <div className="notif-message">{n.message || ''}</div>
                  </div>
                ))
              )}
            </div>
          )}
        </div>

        {/* Divider — hidden on small screens */}
        <div className="navbar-divider navbar-divider--desktop" />

        {/* User info — name hidden on very small screens */}
        <div className="navbar-user">
          <div className="navbar-avatar">{initials}</div>
          <div className="navbar-user-info">
            <div className="navbar-username">{user?.fullName || user?.username || 'User'}</div>
            <div className="navbar-role">{user?.role || 'User'}</div>
          </div>
        </div>

        {/* Logout */}
        <button onClick={handleLogout} className="btn btn-secondary navbar-signout" style={{ padding: '6px 14px', fontSize: '13px' }}>
          Sign Out
        </button>
      </div>
    </header>
  );
};

export default Navbar;
