import api from './axios';

export const userService = {
  getAll: (params) => api.get('/Users', { params }),
  getById: (id) => api.get(`/Users/${id}`),
  updateRole: (id, roleId) => api.patch(`/Users/${id}/role`, roleId, { headers: { 'Content-Type': 'application/json' } }),
  toggleStatus: (id, isActive) => api.patch(`/Users/${id}/status`, isActive, { headers: { 'Content-Type': 'application/json' } }),
};
