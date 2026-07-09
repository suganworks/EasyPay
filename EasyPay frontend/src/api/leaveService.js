import api from './axios';

export const leaveService = {
  getAll: (params) => api.get('/Leave', { params }),
  getMyLeaves: (params) => api.get('/Leave/my-leaves', { params }),
  getPendingForMe: () => api.get('/Leave/pending-for-me'),
  getById: (id) => api.get(`/Leave/${id}`),
  getMyBalance: (year) => api.get('/Leave/my-balance', { params: { year } }),
  getEmployeeBalance: (employeeId, year) => api.get(`/Leave/${employeeId}/balance`, { params: { year } }),
  create: (data) => api.post('/Leave', data),
  action: (id, data) => api.patch(`/Leave/${id}/action`, data),
  cancel: (id) => api.patch(`/Leave/${id}/cancel`),
  carryForward: (employeeId, fromYear) => api.post(`/Leave/carry-forward/${employeeId}`, null, { params: { fromYear } }),
  bulkCarryForward: (fromYear) => api.post('/Leave/carry-forward/bulk', null, { params: { fromYear } }),
};
