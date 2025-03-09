import { computed } from 'vue';
import Cookies from 'js-cookie';
import axios, { type AxiosInstance } from 'axios';
import type { LoginRequestDto } from '@/types/BackendDtos';

const baseUrl = import.meta.env.VITE_MATHLLM_BACKEND_ADDRESS;

export function useAuth() {
  const client: AxiosInstance = axios.create({
    baseURL: baseUrl,
  });

  const isAuthenticated =  computed(() =>
  {
    const cookies = Cookies.get('.AspNetCore.Identity.Application')
    return !!cookies;
  });

  async function login(email: string, password: string): Promise<void> {
    const dto: LoginRequestDto = { email, password };
    await client.post('/login?useCookies=true', dto, { withCredentials: true });
  }

  async function logout(): Promise<void> {
      Cookies.remove('.AspNetCore.Identity.Application');
  }

  return {
    login,
    logout,
    isAuthenticated
  };
}
