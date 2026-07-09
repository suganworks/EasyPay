import React from 'react';
import { NavLink } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const Sidebar = ({ open, onClose }) => {
  const { user } = useAuth();

  if (!user) return null;

  const role = user.role;

  const getLinks = () => {
    switch (role) {
      case 'Admin':
      case 'HRManager':
        return [
          { group: 'Overview', links: [
            { to: '/admin', label: '🏠 Dashboard' }
          ]},
          { group: 'Organization', links: [
            { to: '/admin/employees', label: '👥 Employees' },
            { to: '/admin/departments', label: '🏢 Departments' },
            { to: '/admin/designations', label: '🎖️ Designations' },
            { to: '/admin/benefits', label: '🎁 Benefits' }
          ]},
          { group: 'Operations', links: [
            { to: '/admin/payroll', label: '💰 Payroll' },
            { to: '/admin/payroll/process', label: '⚙️ Process Payroll' },
            { to: '/admin/payroll-policies', label: '📋 Payroll Policies' },
            { to: '/admin/leaves', label: '🌴 Leave Requests' },
            { to: '/admin/timesheets', label: '🕐 Timesheets' }
          ]},
          { group: 'Insights', links: [
            { to: '/admin/reports', label: '📊 Reports' },
            { to: '/admin/audit-logs', label: '🔍 Audit Logs' }
          ]}
        ];
      case 'Manager':
        return [
          { group: 'Team Management', links: [
            { to: '/manager', label: '🏠 Team Dashboard' },
            { to: '/manager/team', label: '👥 My Team' },
            { to: '/manager/leave-approvals', label: '✅ Leave Approvals' },
            { to: '/manager/timesheet-approvals', label: '🕐 Timesheet Approvals' }
          ]},
          { group: 'Personal', links: [
            { to: '/employee', label: '👤 My Portal' }
          ]}
        ];
      case 'PayrollProcessor':
        return [
          { group: 'Payroll Processing', links: [
            { to: '/processor', label: '🏠 Dashboard' },
            { to: '/processor/process', label: '⚙️ Process Payroll' },
            { to: '/processor/queue', label: '📋 Payroll Queue' }
          ]},
          { group: 'Personal', links: [
            { to: '/employee', label: '👤 My Portal' }
          ]}
        ];
      case 'Employee':
      default:
        return [
          { group: 'Self Service', links: [
            { to: '/employee', label: '🏠 Dashboard' },
            { to: '/employee/leaves', label: '🌴 My Leaves' },
            { to: '/employee/timesheets', label: '🕐 My Timesheets' },
            { to: '/employee/payslips', label: '💰 My Payslips' },
            { to: '/employee/benefits', label: '🎁 My Benefits' }
          ]},
          { group: 'Settings', links: [
            { to: '/employee/profile', label: '👤 My Profile' }
          ]}
        ];
    }
  };

  const navGroups = getLinks();

  return (
    <aside className={`sidebar${open ? ' sidebar--open' : ''}`}>
      {/* Sidebar header with logo + close btn on mobile */}
      <div className="sidebar-header">
        <div className="sidebar-logo">
          <span className="sidebar-logo-text">EasyPay<span className="dot">.</span></span>
          <span className="sidebar-logo-sub">by Hexaware</span>
        </div>
        <button className="sidebar-close-btn" onClick={onClose} aria-label="Close sidebar">✕</button>
      </div>

      <nav className="sidebar-nav">
        {navGroups.map((group, i) => (
          <div key={i}>
            <div className="sidebar-section-label">{group.group}</div>
            <div style={{ display: 'flex', flexDirection: 'column', gap: '2px' }}>
              {group.links.map((link) => (
                <NavLink
                  key={link.to}
                  to={link.to}
                  end={link.to === '/admin' || link.to === '/employee' || link.to === '/manager' || link.to === '/processor'}
                  className={({ isActive }) => `sidebar-nav-item${isActive ? ' active' : ''}`}
                  onClick={onClose}
                >
                  {link.label}
                </NavLink>
              ))}
            </div>
          </div>
        ))}
      </nav>
    </aside>
  );
};

export default Sidebar;
