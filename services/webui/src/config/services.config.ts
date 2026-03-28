import { getRuntimeConfig } from './runtime.config'

/**
 * Configuration for external services used by the application
 * Uses runtime configuration loaded from /config.js
 * This allows configuration changes without rebuilding the application
 */

export interface ServicesConfig {
  /** Base URL for the video service (VideoApp) */
  videoServiceUrl: string
}

/**
 * Service configuration
 * Reads from runtime config (window.APP_CONFIG) loaded from /config.js
 * 
 * Note: All backend API requests go through /app path
 * - In development: Vite proxies /app to localhost:5000
 * - In production: nginx proxies /app to the backend service
 */
export const servicesConfig: ServicesConfig = {
  videoServiceUrl: getRuntimeConfig().services.videoServiceUrl || '/video'
}

/**
 * Helper function to construct video URL from filename
 * @param filename - Video filename (e.g., "03.mp4")
 * @returns URL to the VideoApp page with the video loaded (relative or absolute depending on config)
 */
export function getVideoUrl(filename: string): string {
  if (!filename) return ''
  
  // If it's already a full URL (for backward compatibility), return as is
  if (filename.startsWith('http://') || filename.startsWith('https://')) {
    return filename
  }
  
  // Remove leading slash if present
  const cleanFilename = filename.startsWith('/') ? filename.slice(1) : filename
  
  // Normalize base URL - remove trailing slash if present to avoid double slashes
  const baseUrl = servicesConfig.videoServiceUrl.replace(/\/$/, '')
  
  // Construct URL to VideoApp page with video loaded
  // This opens the full VideoApp interface with chat, subtitles, etc.
  return `${baseUrl}/${cleanFilename}`
}
