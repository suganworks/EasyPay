import api from './axios';

export const employeeService = {
  getAll: (params) => api.get('/Employees', { params }),
  getById: (id) => api.get(`/Employees/${id}`),
  getByCode: (code) => api.get(`/Employees/code/${code}`),
  getByDepartment: (departmentId) => api.get(`/Employees/by-department/${departmentId}`),
  getMyTeam: () => api.get('/Employees/my-team'),
  create: (data) => api.post('/Employees', data),
  update: (id, data) => api.put(`/Employees/${id}`, data),
  delete: (id) => api.delete(`/Employees/${id}`),
  reactivate: (id) => api.patch(`/Employees/${id}/reactivate`),
};
