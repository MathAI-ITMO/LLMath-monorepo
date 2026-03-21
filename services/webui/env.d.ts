/// <reference types="vite/client" />


interface AppConfig {
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

interface Window {
  APP_CONFIG?: AppConfig
}
