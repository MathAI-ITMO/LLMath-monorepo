import { ref } from 'vue';
import Cookies from 'js-cookie';
import axios, { type AxiosInstance, AxiosError } from 'axios';
import type { LoginRequestDto } from '@/types/BackendDtos';

interface RegisterErrorResponse {
  type: string;
  title: string;
  status: number;
  detail: string;
  instance: string;
  errors?: {
    [key: string]: string[];
  };
  [key: string]: any;
}

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

  async function register(email: string, password: string): Promise<{ success: boolean; error?: RegisterErrorResponse }> {
    try {
      const dto: LoginRequestDto = { email, password };
      await client.post('/register', dto);
      await login(email, password);
      return { success: true };
    } catch (error) {
      if (axios.isAxiosError(error)) {
        const axiosError = error as AxiosError<RegisterErrorResponse>;
        if (axiosError.response?.data) {
          return {
            success: false,
            error: axiosError.response.data
          };
        }
      }
      throw error;
    }
  }

  async function logout(): Promise<void> {
    Cookies.remove('.AspNetCore.Identity.Application');
    isAuthenticatedState.value = false;
  }

  return {
    login,
    logout,
    register,
    isAuthenticated: isAuthenticatedState
  };
}
