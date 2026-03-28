import axios, { type AxiosRequestConfig, type AxiosResponse } from 'axios'
import { servicesConfig } from '@/config/services.config'

export const videoApi = axios.create({
  baseURL: servicesConfig.videoServiceUrl,
  withCredentials: true,
})

export const videoApiInstance = <T>(config: AxiosRequestConfig): Promise<AxiosResponse<T>> => {
  return videoApi.request<T>(config)
}
