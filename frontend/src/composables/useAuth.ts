import { ref, shallowRef } from 'vue';
import Cookies from 'js-cookie';
import axios, { type AxiosInstance, AxiosError } from 'axios';
import type { LoginRequestDto, RegisterRequestDto } from '@/types/BackendDtos';
import type { UserModel } from '@/types/Models';

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

// Проверяем состояние аутентификации не только по кукам, но и по localStorage
const checkAuthState = () => {
  const hasCookie = !!Cookies.get('.AspNetCore.Identity.Application');
  const hasLocalStorage = localStorage.getItem('llmath_auth') === 'true';
  
  // Возвращаем true, если есть хотя бы один из признаков аутентификации
  return hasCookie || hasLocalStorage;
};

// Create a reactive auth state
const isAuthenticatedState = ref(checkAuthState());
const currentUser = shallowRef<UserModel | null>(null);

export function useAuth() {
  const client: AxiosInstance = axios.create({
    baseURL: baseUrl,
    withCredentials: true // Важно для отправки cookies с запросами
  });

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
      if (axios.isAxiosError(error)) {
        const axiosError = error as AxiosError<RegisterErrorResponse>;
        console.log('Ошибка регистрации:', axiosError.response?.data);
        
        if (axiosError.response?.data) {
          const errorData = axiosError.response.data;
          
          // Проверяем наличие конкретной ошибки о дубликате email
          if (typeof errorData === 'object') {
            // Проверяем все возможные форматы ошибок
            if (errorData.errors) {
              // Проверяем ошибки в формате ModelState
              const allErrorMessages = Object.values(errorData.errors).flat();
              const hasEmailDuplicate = allErrorMessages.some(
                (msg: string) => 
                  msg.includes('уже существует') || 
                  msg.includes('already exists') ||
                  msg.includes('DuplicateEmail') ||
                  msg.includes('DuplicateUserName')
              );
              
              if (hasEmailDuplicate) {
                return {
                  success: false,
                  error: {
                    ...errorData,
                    detail: "Пользователь с таким email уже существует"
                  }
                };
              }
            } else if (errorData.detail && (
                errorData.detail.includes('уже существует') || 
                errorData.detail.includes('already exists')
              )) {
              // Ошибка уже содержит правильное сообщение
              return {
                success: false,
                error: errorData
              };
            }
            
            // Если дошли сюда, просто возвращаем данные ошибки как есть
          return {
            success: false,
              error: errorData
          };
        }
      }
      }
      
      console.error('Непредвиденная ошибка при регистрации:', error);
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
