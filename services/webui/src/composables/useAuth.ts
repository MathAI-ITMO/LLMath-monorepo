import { ref, shallowRef } from 'vue';
import Cookies from 'js-cookie';
import axios, { AxiosError } from 'axios';
import type { LoginRequestDto, RegisterRequestDto } from '@/types/BackendDtos';
import type { UserModel } from '@/types/Models';
import { createBackendApiClient } from '@/utils/apiClient';

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

const checkAuthState = () => {
  const hasCookie = !!Cookies.get('.AspNetCore.Identity.Application');
  const hasLocalStorage = localStorage.getItem('llmath_auth') === 'true';
  
  return hasCookie || hasLocalStorage;
};

const isAuthenticatedState = ref(checkAuthState());
const currentUser = shallowRef<UserModel | null>(null);

export function useAuth() {
  const client = createBackendApiClient();

  async function login(email: string, password: string): Promise<void> {
    const dto: LoginRequestDto = { email, password };
    await client.post('/login?useCookies=true', dto, { withCredentials: true });
    isAuthenticatedState.value = true;
    // Сохраняем состояние аутентификации в localStorage
    localStorage.setItem('llmath_auth', 'true');
    await fetchCurrentUser(); // Получаем информацию о пользователе после входа
  }

  async function register(
    email: string, 
    password: string, 
    firstName: string, 
    lastName: string, 
    studentGroup: string
  ): Promise<{ success: boolean; error?: RegisterErrorResponse }> {
    try {
      const dto: RegisterRequestDto = { 
        email, 
        password, 
        firstName, 
        lastName, 
        studentGroup 
      };
      await client.post('/api/auth/register', dto);
      await login(email, password);
      return { success: true };
    } catch (error) {
      console.error('Ошибка регистрации:', error);
      
      if (axios.isAxiosError(error)) {
        const axiosError = error as AxiosError<RegisterErrorResponse>;
        if (axiosError.response?.data) {
          return {
            success: false,
            error: axiosError.response.data
          };
        }
      }
      
      return { 
        success: false, 
        error: { 
          detail: "Ошибка при регистрации. Пожалуйста, попробуйте позже.",
          status: 500,
          title: "Ошибка регистрации",
          type: "https://tools.ietf.org/html/rfc7231#section-6.5.1",
          instance: ""
        } 
      };
    }
  }

  async function logout(): Promise<void> {
    Cookies.remove('.AspNetCore.Identity.Application');
    localStorage.removeItem('llmath_auth');
    isAuthenticatedState.value = false;
    currentUser.value = null;
  }

  async function fetchCurrentUser(): Promise<UserModel | null> {
    if (!isAuthenticatedState.value) {
      return null;
    }
    
    try {
      const response = await client.get('/api/user/me');
      currentUser.value = response.data;
      // Если запрос успешен, значит пользователь аутентифицирован
      localStorage.setItem('llmath_auth', 'true');
      return currentUser.value;
    } catch (error) {
      console.error('Ошибка при получении данных пользователя:', error);
      
      // Если при получении данных произошла ошибка аутентификации, очищаем состояние
      if (axios.isAxiosError(error) && (error.response?.status === 401 || error.response?.status === 403)) {
        isAuthenticatedState.value = false;
        localStorage.removeItem('llmath_auth');
      }
      
      return null;
    }
  }

  return {
    login,
    logout,
    register,
    isAuthenticated: isAuthenticatedState,
    currentUser,
    fetchCurrentUser
  };
}
