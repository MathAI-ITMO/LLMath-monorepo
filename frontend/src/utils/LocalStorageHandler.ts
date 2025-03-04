import type { UserInfo, AuthTokenInfo } from '@/types/LocalStorageTypes';

export class LocalStorageHandler {
  static getUser(): UserInfo | null {
    try {
      const item = localStorage.getItem('currentUser');
      return item ? JSON.parse(item) as UserInfo : null;
    } catch (error) {
      console.error(`Error retrieving item from localStorage: ${error}`);
      return null;
    }
  }

  static setUser(user: UserInfo): void {
    try {
      const item = JSON.stringify(user);
      localStorage.setItem('currentUser', item);
    } catch (error) {
      console.error(`Error setting item in localStorage: ${error}`);
    }
  }

  static getAuthToken(): AuthTokenInfo | null {
    try {
      const item = localStorage.getItem('authToken');
      return item ? JSON.parse(item) as AuthTokenInfo : null;
    } catch (error) {
      console.error(`Error retrieving item from localStorage: ${error}`);
      return null;
    }
  }

  static setAuthToken(user: AuthTokenInfo): void {
    try {
      const item = JSON.stringify(user);
      localStorage.setItem('authToken', item);
    } catch (error) {
      console.error(`Error setting item in localStorage: ${error}`);
    }
  }


  static clear(): void {
    localStorage.clear();
  }
}
