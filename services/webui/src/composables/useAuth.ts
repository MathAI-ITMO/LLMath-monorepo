import { ref, shallowRef } from 'vue';
import Cookies from 'js-cookie';
import { AxiosError } from 'axios';
import type { UserModel } from '@/types/Models';
import { api } from '@/api';
import type { JwtUser, RegisterDto } from '@/api/generated/api';

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
  async function login(email: string, password: string): Promise<void> {
    await api.postLogin({ email, password }, { useCookies: true });
    isAuthenticatedState.value = true;
    localStorage.setItem('llmath_auth', 'true');
    await fetchCurrentUser();
  }

  async function register(
    email: string,
    password: string,
    firstName: string,
    lastName: string,
    studentGroup: string
  ): Promise<{ success: boolean; error?: RegisterErrorResponse }> {
    try {
      const dto: RegisterDto = { email, password, firstName, lastName, studentGroup };
      await api.postApiAuthRegister(dto);
      await login(email, password);
      return { success: true };
    } catch (error) {
      console.error('Ошибка регистрации:', error);

      if (error instanceof AxiosError && error.response?.data) {
        return { success: false, error: error.response.data };
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
      const response = await api.getApiUserMe();
      currentUser.value = response.data as unknown as UserModel;
      localStorage.setItem('llmath_auth', 'true');
      return currentUser.value;
    } catch (error) {
      console.error('Ошибка при получении данных пользователя:', error);

      if (error instanceof AxiosError && (error.response?.status === 401 || error.response?.status === 403)) {
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
