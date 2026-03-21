/**
 * Centralized API Client Configuration
 * 
 * All backend API requests use /app path which is proxied to the backend:
 * - In development: Vite dev server proxies /app to localhost:5000
 * - In production: nginx proxies /app to the backend service
 */

import axios, { type AxiosInstance } from 'axios'

/**
 * Create an axios instance for the backend API
 * Always uses /app path which is proxied to the backend:
 * - In development: Vite dev server proxies /app to localhost:5000
 * - In production: nginx proxies /app to the backend service
 */
export function createBackendApiClient(): AxiosInstance {
  return axios.create({
    baseURL: '/app',
    withCredentials: true,
    headers: {
      'Content-Type': 'application/json',
    },
  })
}

/**
 * Singleton instance for backend API access
 * All API requests (including problems, auth, chats, etc.) go through this
 */
export const backendApi = createBackendApiClient()
