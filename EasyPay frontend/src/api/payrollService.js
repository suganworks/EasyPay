import api from './axios';

export const payrollService = {
  getAll: (params) => api.get('/Payroll', { params }),
  getMyPayrolls: (params) => api.get('/Payroll/my-payrolls', { params }),
  getById: (id) => api.get(`/Payroll/${id}`),
  getSummary: (params) => api.get('/Payroll/summary', { params }),
  process: (data) => api.post('/Payroll/process', data),
  bulkProcess: (data) => api.post('/Payroll/bulk-process', data),
  approve: (id) => api.patch(`/Payroll/${id}/approve`),
  reject: (id, data) => api.patch(`/Payroll/${id}/reject`, data),
  markAsPaid: (id, data) => api.patch(`/Payroll/${id}/mark-as-paid`, data),
};
