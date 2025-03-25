import { ref } from 'vue';
import Cookies from 'js-cookie';
import axios, { type AxiosInstance } from 'axios';
import type { LoginRequestDto } from '@/types/BackendDtos';

const baseUrl = import.meta.env.VITE_MATHLLM_BACKEND_ADDRESS;

// Create a reactive auth state
const isAuthenticatedState = ref(!!Cookies.get('.AspNetCore.Identity.Application'));

export function useAuth() {
  const client: AxiosInstance = axios.create({
    baseURL: baseUrl,
  });

  async function login(email: string, password: string): Promise<void> {
    const dto: LoginRequestDto = { email, password };
    await client.post('/login?useCookies=true', dto, { withCredentials: true });
    isAuthenticatedState.value = true;
  }

  async function logout(): Promise<void> {
    Cookies.remove('.AspNetCore.Identity.Application');
    isAuthenticatedState.value = false;
  }

  return {
    login,
    logout,
    isAuthenticated: isAuthenticatedState
  };
}
