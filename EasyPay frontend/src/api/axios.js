import axios from 'axios';

const API_BASE_URL = '/api/v1';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor — attach JWT token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('accessToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

let isRefreshing = false;
let failedQueue = [];

const processQueue = (error, token = null) => {
  failedQueue.forEach(prom => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token);
    }
  });
  failedQueue = [];
};

// Response interceptor — handle 401 and token refresh
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    
    // Only attempt token refresh for authenticated requests.
    // Anonymous endpoints (login, forgot-password, reset-password, etc.) should
    // never trigger a refresh — their 401s are legitimate business errors.
    const ANON_ENDPOINTS = ['/Auth/login', '/Auth/register', '/Auth/refresh-token',
      '/Auth/forgot-password', '/Auth/reset-password'];
    const isAnonEndpoint = ANON_ENDPOINTS.some(ep => originalRequest.url?.includes(ep));

    // Trigger refresh on any 401 from authenticated endpoints.
    // The backend does not send a 'token-expired' header, so we rely on status code alone.
    if (error.response?.status === 401 && !originalRequest._retry && !isAnonEndpoint) {
      if (isRefreshing) {
        return new Promise(function(resolve, reject) {
          failedQueue.push({ resolve, reject });
        }).then(token => {
          originalRequest.headers.Authorization = 'Bearer ' + token;
          return api(originalRequest);
        }).catch(err => {
          return Promise.reject(err);
        });
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        const accessToken = localStorage.getItem('accessToken');
        const refreshToken = localStorage.getItem('refreshToken');

        if (!refreshToken) {
          throw new Error('No refresh token');
        }

        // Must use base axios to avoid interceptor loop
        const res = await axios.post(`${API_BASE_URL}/Auth/refresh-token`, {
          accessToken,
          refreshToken,
        });

        if (res.data?.success && res.data?.data) {
          const { accessToken: newAccess, refreshToken: newRefresh } = res.data.data;
          localStorage.setItem('accessToken', newAccess);
          localStorage.setItem('refreshToken', newRefresh);
          
          api.defaults.headers.common['Authorization'] = `Bearer ${newAccess}`;
          originalRequest.headers.Authorization = `Bearer ${newAccess}`;
          
          processQueue(null, newAccess);
          
          return api(originalRequest);
        }
      } catch (refreshError) {
        processQueue(refreshError, null);
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('user');
        window.location.href = '/login';
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);

export default api;
