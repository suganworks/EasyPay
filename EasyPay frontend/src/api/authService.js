import api from './axios';

export const authService = {
  login: (data) => api.post('/Auth/login', data),
  register: (data) => api.post('/Auth/register', data),
  logout: (data) => api.post('/Auth/logout', data),
  refreshToken: (data) => api.post('/Auth/refresh-token', data),
  forgotPassword: (data) => api.post('/Auth/forgot-password', data),
  resetPassword: (data) => api.post('/Auth/reset-password', data),
  changePassword: (data) => api.post('/Auth/change-password', data),
  getMe: () => api.get('/Auth/me'),
};
