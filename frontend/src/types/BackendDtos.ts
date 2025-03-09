export interface LoginRequestDto {
  email: string;
  password: string;
}

export interface ChatDto {
  id: string;
  userId: string;
  name: string;
}

export interface CreateChatDto {
  name: string
}

export interface MessageDto {
  id: string;
  chatId: string;
  text: string;
  type: string;
  creationTime: Date
}

export interface SendMessageRequestDto {
  chatId: string;
  text: string;
}
