import { LocalStorageHandler } from '@/utils/LocalStorageHandler'
import axios, { type AxiosInstance } from 'axios';
import type { LoginRequestDto, LoginResponseDto, TokenDto } from '@/types/BackendDtos';
import type { UserInfo, AuthTokenInfo } from '@/types/LocalStorageTypes';
import type { UserModel } from '@/types/Models';

export class AuthService
{
  client: AxiosInstance;

  constructor(baseUrl: string)
  {
    this.client = axios.create({
      baseURL: baseUrl,
    });
  }

  async login(email: string, password: string) : Promise<UserModel | null> {
    LocalStorageHandler.clear()
    const dto: LoginRequestDto = { email: email, password: password }
    try {
    const response = await this.client.post<LoginResponseDto>(
      '/api/auth/login',
      dto)
        const user: UserInfo = {
          id: response.data.user.id,
          email: response.data.user.email,
          firstName: response.data.user.firstName,
          lastName: response.data.user.firstName
        }
        const token:  AuthTokenInfo = {
          token: response.data.token.token,
          expirationDate: response.data.token.validUntill
        }

        LocalStorageHandler.setAuthToken(token)
        LocalStorageHandler.setUser(user)

        const res: UserModel =
        {
          id: user.id,
          email: user.email,
          firstName: user.firstName,
          lastName: user.lastName
        }

        return res;
      }
    catch (error)
    {
      localStorage.clear()
      console.log(error)
      return null;
    }
  }

  async getCurrentUser() : Promise<UserModel | null>
  {
    try {
      const token = LocalStorageHandler.getAuthToken();
      if (token == null)
      {
        return null
      }

      const now = new Date();
      const yesterday = new Date();
      yesterday.setDate(now.getDate() - 2);

      if (token.expirationDate >= yesterday)
      {
        await this.refreshToken()
        console.log('refreshed old token')
      }

      const user = LocalStorageHandler.getUser()
      const model: UserModel =
      {
        id: user!.id,
        email: user!.email,
        firstName: user!.firstName,
        lastName: user!.lastName
      }

      return model;

    }
    catch (error)
    {
      localStorage.clear()
      console.log(error)
      return null;
    }
  }

  async refreshToken() : Promise<void>
  {
    const token = LocalStorageHandler.getAuthToken();
    if (token == null)
    {
      throw new Error("No token found in local storage");
    }

    const response = await this.client.post<TokenDto>("/api/auth/refresn", await this.getAuthOptions())

    const newToken: AuthTokenInfo = {
      token: response.data.token,
      expirationDate: response.data.validUntill
    }

    LocalStorageHandler.setAuthToken(newToken);
  }

  async logout() : Promise<void>
  {
    try
    {
      await await this.client.post('/api/auth/logout', {
          Headers : { "Authorization" : await this.getAuthOptions() }
        })
    }
    finally
    {
      LocalStorageHandler.clear();
    }
  }

  async getAuthHeaders() : Promise<object>
  {
    try {
      let token = LocalStorageHandler.getAuthToken();
      if (token == null)
      {
        return {}
      }

      const now = new Date();
      const yesterday = new Date();
      yesterday.setDate(now.getDate() - 2);

      if (token.expirationDate >= yesterday)
      {
        await this.refreshToken()
        console.log('refreshed old token')
      }

      token = LocalStorageHandler.getAuthToken();

      return {"Authorization" : `Bearer ${token?.token}`}

    }
    catch
    {
      return {}
    }
  }

  async getAuthOptions() : Promise<object>
  {
    const headers = await this.getAuthHeaders()
    if (Object.keys(headers).length === 0)
    {
      return {}
    }

    return { headers: headers }
  }
}
