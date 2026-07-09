import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import { ToastProvider } from './context/ToastContext';
import ProtectedRoute from './components/ProtectedRoute';
import Layout from './components/Layout';
import Login from './pages/Auth/Login';
import Unauthorized from './pages/Auth/Unauthorized';
import ForgotPassword from './pages/Auth/ForgotPassword';
import ResetPassword from './pages/Auth/ResetPassword';

// Admin
import AdminDashboard from './pages/Admin/AdminDashboard';
import EmployeeList from './pages/Admin/EmployeeList';
import EmployeeForm from './pages/Admin/EmployeeForm';
import EmployeeDetail from './pages/Admin/EmployeeDetail';
import DepartmentList from './pages/Admin/DepartmentList';
import DesignationList from './pages/Admin/DesignationList';
import BenefitList from './pages/Admin/BenefitList';
import SalaryStructureForm from './pages/Admin/SalaryStructureForm';
import PayrollList from './pages/Admin/PayrollList';
import PayrollProcess from './pages/Admin/PayrollProcess';
import PayrollPolicyList from './pages/Admin/PayrollPolicyList';
import LeaveManagement from './pages/Admin/LeaveManagement';
import TimesheetManagement from './pages/Admin/TimesheetManagement';
import ReportsPage from './pages/Admin/ReportsPage';
import AuditLogs from './pages/Admin/AuditLogs';

// Employee
import EmployeeDashboard from './pages/Employee/EmployeeDashboard';
import MyLeaves from './pages/Employee/MyLeaves';
import MyTimesheets from './pages/Employee/MyTimesheets';
import MyBenefits from './pages/Employee/MyBenefits';
import MyPayslips from './pages/Employee/MyPayslips';
import MyProfile from './pages/Employee/MyProfile';

// Manager
import ManagerDashboard from './pages/Manager/ManagerDashboard';
import ManagerTeam from './pages/Manager/ManagerTeam';
import TeamLeaveApprovals from './pages/Manager/TeamLeaveApprovals';
import TeamTimesheetApprovals from './pages/Manager/TeamTimesheetApprovals';

// Processor
import ProcessorDashboard from './pages/Processor/ProcessorDashboard';
import ProcessPayroll from './pages/Processor/ProcessPayroll';
import PayrollQueue from './pages/Processor/PayrollQueue';

// Root Redirect component
const RootRedirect = () => {
  const { user } = useAuth();
  if (!user) return <Navigate to="/login" />;
  if (user.role === 'Admin' || user.role === 'HRManager') return <Navigate to="/admin" />;
  if (user.role === 'Manager') return <Navigate to="/manager" />;
  if (user.role === 'PayrollProcessor') return <Navigate to="/processor" />;
  return <Navigate to="/employee" />;
};

function App() {
  return (
    <ToastProvider>
      <AuthProvider>
        <Router>
          <Routes>
            <Route path="/login" element={<Login />} />
            <Route path="/unauthorized" element={<Unauthorized />} />
            <Route path="/forgot-password" element={<ForgotPassword />} />
            <Route path="/reset-password" element={<ResetPassword />} />

            <Route path="/" element={<RootRedirect />} />

            <Route element={<Layout />}>
              {/* Admin Routes */}
              <Route path="/admin" element={<ProtectedRoute allowedRoles={['Admin', 'HRManager']}><AdminDashboard /></ProtectedRoute>} />
              <Route path="/admin/employees" element={<ProtectedRoute allowedRoles={['Admin', 'HRManager']}><EmployeeList /></ProtectedRoute>} />
              <Route path="/admin/employees/new" element={<ProtectedRoute allowedRoles={['Admin', 'HRManager']}><EmployeeForm /></ProtectedRoute>} />
              <Route path="/admin/employees/:id" element={<ProtectedRoute allowedRoles={['Admin', 'HRManager']}><EmployeeDetail /></ProtectedRoute>} />
              <Route path="/admin/employees/:id/edit" element={<ProtectedRoute allowedRoles={['Admin', 'HRManager']}><EmployeeForm /></ProtectedRoute>} />
              <Route path="/admin/employees/:id/salary" element={<ProtectedRoute allowedRoles={['Admin', 'HRManager']}><SalaryStructureForm /></ProtectedRoute>} />
              <Route path="/admin/departments" element={<ProtectedRoute allowedRoles={['Admin', 'HRManager']}><DepartmentList /></ProtectedRoute>} />
              <Route path="/admin/designations" element={<ProtectedRoute allowedRoles={['Admin', 'HRManager']}><DesignationList /></ProtectedRoute>} />
              <Route path="/admin/benefits" element={<ProtectedRoute allowedRoles={['Admin', 'HRManager']}><BenefitList /></ProtectedRoute>} />
              <Route path="/admin/payroll" element={<ProtectedRoute allowedRoles={['Admin', 'HRManager']}><PayrollList /></ProtectedRoute>} />
              <Route path="/admin/payroll/process" element={<ProtectedRoute allowedRoles={['Admin', 'HRManager']}><PayrollProcess /></ProtectedRoute>} />
              <Route path="/admin/payroll-policies" element={<ProtectedRoute allowedRoles={['Admin', 'HRManager']}><PayrollPolicyList /></ProtectedRoute>} />
              <Route path="/admin/leaves" element={<ProtectedRoute allowedRoles={['Admin', 'HRManager']}><LeaveManagement /></ProtectedRoute>} />
              <Route path="/admin/timesheets" element={<ProtectedRoute allowedRoles={['Admin', 'HRManager']}><TimesheetManagement /></ProtectedRoute>} />
              <Route path="/admin/reports" element={<ProtectedRoute allowedRoles={['Admin', 'HRManager']}><ReportsPage /></ProtectedRoute>} />
              <Route path="/admin/audit-logs" element={<ProtectedRoute allowedRoles={['Admin']}><AuditLogs /></ProtectedRoute>} />

              {/* Employee Routes */}
              <Route path="/employee" element={<ProtectedRoute allowedRoles={['Employee', 'Manager', 'PayrollProcessor', 'Admin', 'HRManager']}><EmployeeDashboard /></ProtectedRoute>} />
              <Route path="/employee/leaves" element={<ProtectedRoute allowedRoles={['Employee', 'Manager', 'PayrollProcessor', 'Admin', 'HRManager']}><MyLeaves /></ProtectedRoute>} />
              <Route path="/employee/timesheets" element={<ProtectedRoute allowedRoles={['Employee', 'Manager', 'PayrollProcessor', 'Admin', 'HRManager']}><MyTimesheets /></ProtectedRoute>} />
              <Route path="/employee/benefits" element={<ProtectedRoute allowedRoles={['Employee', 'Manager', 'PayrollProcessor', 'Admin', 'HRManager']}><MyBenefits /></ProtectedRoute>} />
              <Route path="/employee/payslips" element={<ProtectedRoute allowedRoles={['Employee', 'Manager', 'PayrollProcessor', 'Admin', 'HRManager']}><MyPayslips /></ProtectedRoute>} />
              <Route path="/employee/profile" element={<ProtectedRoute allowedRoles={['Employee', 'Manager', 'PayrollProcessor', 'Admin', 'HRManager']}><MyProfile /></ProtectedRoute>} />

              {/* Manager Routes */}
              <Route path="/manager" element={<ProtectedRoute allowedRoles={['Manager']}><ManagerDashboard /></ProtectedRoute>} />
              <Route path="/manager/team" element={<ProtectedRoute allowedRoles={['Manager']}><ManagerTeam /></ProtectedRoute>} />
              <Route path="/manager/leave-approvals" element={<ProtectedRoute allowedRoles={['Manager']}><TeamLeaveApprovals /></ProtectedRoute>} />
              <Route path="/manager/timesheet-approvals" element={<ProtectedRoute allowedRoles={['Manager']}><TeamTimesheetApprovals /></ProtectedRoute>} />

              {/* Processor Routes */}
              <Route path="/processor" element={<ProtectedRoute allowedRoles={['PayrollProcessor']}><ProcessorDashboard /></ProtectedRoute>} />
              <Route path="/processor/process" element={<ProtectedRoute allowedRoles={['PayrollProcessor']}><ProcessPayroll /></ProtectedRoute>} />
              <Route path="/processor/queue" element={<ProtectedRoute allowedRoles={['PayrollProcessor']}><PayrollQueue /></ProtectedRoute>} />
            </Route>

            {/* Catch-all */}
            <Route path="*" element={<Navigate to="/login" replace />} />
          </Routes>
        </Router>
      </AuthProvider>
    </ToastProvider>
  );
}

export default App;
