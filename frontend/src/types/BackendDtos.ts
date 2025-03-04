export interface LoginRequestDto {
  email: string;
  password: string;
}

export interface LoginResponseDto {
  token: TokenDto;
  user: UserInfoDto;
}

export interface TokenDto {
  token: string;
  validUntill: Date;
}

export interface UserInfoDto {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
}

export interface ChatDto {
  id: number;
  userId: number;
  name: string;
}

export interface CreateChatDto {
  name: string
}

export interface MessageDto {
  id: number;
  chatId: number;
  text: string;
  type: string;
  creationTime: Date
}

export interface SendMessageRequestDto {
  chatId: number;
  text: string;
}
