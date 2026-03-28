import axios from 'axios'
import { servicesConfig } from '@/config/services.config'

export const videoApi = axios.create({
  baseURL: servicesConfig.videoServiceUrl,
  withCredentials: true,
})
