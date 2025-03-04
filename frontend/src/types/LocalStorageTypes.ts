export interface UserInfo {
  id: number,
  email: string,
  firstName: string,
  lastName: string
}

export interface AuthTokenInfo {
  token: string,
  expirationDate: Date
}

