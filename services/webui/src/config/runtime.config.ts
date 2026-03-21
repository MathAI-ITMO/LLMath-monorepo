/**
 * Runtime Configuration Access
 * 
 * This module provides type-safe access to the runtime configuration
 * loaded from /config.js. The configuration can be modified at runtime
 * without rebuilding the application.
 */

export interface AppConfig {
  baseUrl: string
  services: {
    videoServiceUrl: string
  }
  features: Record<string, boolean | string | number>
  meta: {
    version: string
    environment: string
  }
}

declare global {
  interface Window {
    APP_CONFIG?: AppConfig
  }
}

/**
 * Default configuration fallback
 * Used if config.js fails to load or is not available
 */
const DEFAULT_CONFIG: AppConfig = {
  baseUrl: '/',
  services: {
    videoServiceUrl: '/video',
  },
  features: {},
  meta: {
    version: '1.0.0',
    environment: 'development',
  },
}

/**
 * Get the runtime configuration
 * Falls back to default config if window.APP_CONFIG is not available
 */
export function getRuntimeConfig(): AppConfig {
  return window.APP_CONFIG || DEFAULT_CONFIG
}

/**
 * Get a specific service URL from the runtime configuration
 */
export function getServiceUrl(service: keyof AppConfig['services']): string {
  const config = getRuntimeConfig()
  return config.services[service]
}

/**
 * Get the base URL for the application
 */
export function getBaseUrl(): string {
  const config = getRuntimeConfig()
  return config.baseUrl
}

/**
 * Check if a feature is enabled
 */
export function isFeatureEnabled(featureName: string): boolean {
  const config = getRuntimeConfig()
  return Boolean(config.features[featureName])
}

/**
 * Get application metadata
 */
export function getAppMeta(): AppConfig['meta'] {
  const config = getRuntimeConfig()
  return config.meta
}

// Export the config object for direct access
export const runtimeConfig = getRuntimeConfig()
