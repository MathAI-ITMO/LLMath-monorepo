import { ref, computed } from 'vue';
import axios, { type AxiosInstance } from 'axios';
import type { LoginRequestDto, LoginResponseDto, TokenDto } from '@/types/BackendDtos';
import type { UserInfo, AuthTokenInfo } from '@/types/LocalStorageTypes';
import type { UserModel } from '@/types/Models';
import { LocalStorageHandler } from '@/utils/LocalStorageHandler';

const baseUrl = import.meta.env.VITE_MATHLLM_BACKEND_ADDRES;

export function useAuth() {
  const client: AxiosInstance = axios.create({
    baseURL: baseUrl,
  });

  const currentUser = ref<UserModel | null>(LocalStorageHandler.getUser());
  const authToken = ref<AuthTokenInfo | null>(LocalStorageHandler.getAuthToken());

  const isAuthenticated = computed(() => authToken.value !== null);

  async function login(email: string, password: string): Promise<UserModel | null> {
    LocalStorageHandler.clear();
    const dto: LoginRequestDto = { email, password };

    try {
      const response = await client.post<LoginResponseDto>('/api/auth/login', dto);

      const user: UserInfo = {
        id: response.data.user.id,
        email: response.data.user.email,
        firstName: response.data.user.firstName,
        lastName: response.data.user.lastName,
      };

      const token: AuthTokenInfo = {
        token: response.data.token.token,
        expirationDate: response.data.token.validUntill,
      };

      LocalStorageHandler.setAuthToken(token);
      LocalStorageHandler.setUser(user);

      authToken.value = token;
      currentUser.value = {
        id: user.id,
        email: user.email,
        firstName: user.firstName,
        lastName: user.lastName,
      };

      return currentUser.value;
    } catch (error) {
      localStorage.clear();
      console.error(error);
      return null;
    }
  }

  async function getCurrentUser(): Promise<UserModel | null> {
    try {
      const token = LocalStorageHandler.getAuthToken();
      if (!token) {
        return null;
      }

      const now = new Date();
      const twoDaysAgo = new Date();
      twoDaysAgo.setDate(now.getDate() - 2);

      if (token.expirationDate >= twoDaysAgo) {
        await refreshToken();
        console.log('Refreshed old token');
      }

      const storedUser = LocalStorageHandler.getUser();
      if (!storedUser) {
        return null;
      }

      // Update reactive state.
      currentUser.value = {
        id: storedUser.id,
        email: storedUser.email,
        firstName: storedUser.firstName,
        lastName: storedUser.lastName,
      };

      return currentUser.value;
    } catch (error) {
      localStorage.clear();
      console.error(error);
      return null;
    }
  }

  async function refreshToken(): Promise<void> {
    const token = LocalStorageHandler.getAuthToken();
    if (!token) {
      throw new Error('No token found in local storage');
    }

    const authOptions = await getAuthOptions();
    const response = await client.post<TokenDto>('/api/auth/refresn', authOptions);

    const newToken: AuthTokenInfo = {
      token: response.data.token,
      expirationDate: response.data.validUntill,
    };

    LocalStorageHandler.setAuthToken(newToken);
    authToken.value = newToken;
  }

  async function logout(): Promise<void> {
    try {
      const authOptions = await getAuthOptions();
      await client.post('/api/auth/logout', {
        Headers: { Authorization: authOptions },
      });
    } finally {
      LocalStorageHandler.clear();
      currentUser.value = null;
      authToken.value = null;
    }
  }

  async function getAuthHeaders(): Promise<object> {
    try {
      let token = LocalStorageHandler.getAuthToken();
      if (!token) {
        return {};
      }

      const now = new Date();
      const twoDaysAgo = new Date();
      twoDaysAgo.setDate(now.getDate() - 2);

      if (token.expirationDate >= twoDaysAgo) {
        await refreshToken();
        console.log('Refreshed old token');
      }

      token = LocalStorageHandler.getAuthToken();
      return { Authorization: `Bearer ${token?.token}` };
    } catch {
      return {};
    }
  }

  async function getAuthOptions(): Promise<object> {
    const headers = await getAuthHeaders();
    if (Object.keys(headers).length === 0) {
      return {};
    }
    return { headers };
  }

  return {
    currentUser,
    authToken,
    login,
    getCurrentUser,
    refreshToken,
    logout,
    getAuthHeaders,
    getAuthOptions,
    isAuthenticated
  };
}
