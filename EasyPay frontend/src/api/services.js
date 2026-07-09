import api from './axios';

export const departmentService = {
  getAll: () => api.get('/Departments'),
  getActive: () => api.get('/Departments/active'),
  getById: (id) => api.get(`/Departments/${id}`),
  create: (data) => api.post('/Departments', data),
  update: (id, data) => api.put(`/Departments/${id}`, data),
  delete: (id) => api.delete(`/Departments/${id}`),
};

export const designationService = {
  getAll: () => api.get('/Designations'),
  getByDepartment: (departmentId) => api.get(`/Designations/by-department/${departmentId}`),
  getById: (id) => api.get(`/Designations/${id}`),
  create: (data) => api.post('/Designations', data),
  update: (id, data) => api.put(`/Designations/${id}`, data),
  delete: (id) => api.delete(`/Designations/${id}`),
};

export const benefitService = {
  getAll: () => api.get('/Benefits'),
  getById: (id) => api.get(`/Benefits/${id}`),
  create: (data) => api.post('/Benefits', data),
  update: (id, data) => api.put(`/Benefits/${id}`, data),
  assign: (employeeId, data) => api.post(`/Benefits/assign/${employeeId}`, data),
  getByEmployee: (employeeId) => api.get(`/Benefits/employee/${employeeId}`),
  getMyBenefits: () => api.get('/Benefits/my-benefits'),
  removeEmployeeBenefit: (employeeBenefitId) => api.delete(`/Benefits/employee-benefit/${employeeBenefitId}`),
};

export const notificationService = {
  getAll: (params) => api.get('/Notifications', { params }),
  getUnreadCount: () => api.get('/Notifications/unread-count'),
  markAsRead: (id) => api.patch(`/Notifications/${id}/read`),
  markAllRead: () => api.patch('/Notifications/mark-all-read'),
};

export const reportService = {
  payrollRegister: (params) => api.get('/Reports/payroll-register', { params }),
  headcount: () => api.get('/Reports/headcount'),
  leaveUtilisation: (params) => api.get('/Reports/leave-utilisation', { params }),
  ctcSummary: (params) => api.get('/Reports/ctc-summary', { params }),
  payrollStatusDashboard: (params) => api.get('/Reports/payroll-status-dashboard', { params }),
};

export const salaryStructureService = {
  create: (data) => api.post('/salary-structures', data),
  getEmployeeCurrent: (employeeId) => api.get(`/salary-structures/employee/${employeeId}/current`),
  getEmployeeHistory: (employeeId) => api.get(`/salary-structures/employee/${employeeId}/history`),
  getMySalary: () => api.get('/salary-structures/my-salary'),
};

export const timesheetService = {
  getAll: (params) => api.get('/Timesheets', { params }),
  getMyTimesheets: (params) => api.get('/Timesheets/my-timesheets', { params }),
  getById: (id) => api.get(`/Timesheets/${id}`),
  create: (data) => api.post('/Timesheets', data),
  update: (id, data) => api.put(`/Timesheets/${id}`, data),
  action: (id, data) => api.patch(`/Timesheets/${id}/action`, data),
  bulkApprove: (data) => api.post('/Timesheets/bulk-approve', data),
  monthlySummary: (employeeId, params) => api.get(`/Timesheets/monthly-summary/${employeeId}`, { params }),
  myMonthlySummary: (params) => api.get('/Timesheets/my-monthly-summary', { params }),
};

export const payrollPolicyService = {
  getAll: () => api.get('/payroll-policies'),
  getActive: () => api.get('/payroll-policies/active'),
  getById: (id) => api.get(`/payroll-policies/${id}`),
  create: (data) => api.post('/payroll-policies', data),
  update: (id, data) => api.put(`/payroll-policies/${id}`, data),
  deactivate: (id) => api.patch(`/payroll-policies/${id}/deactivate`),
};

export const auditLogService = {
  getAll: (params) => api.get('/audit-logs', { params }),
};
