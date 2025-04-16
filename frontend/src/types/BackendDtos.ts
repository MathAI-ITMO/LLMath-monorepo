export interface LoginRequestDto {
  email: string;
  password: string;
}

export interface ChatDto {
  id: string;
  userId: string;
  name: string;
  type: 'ProblemSolver' | 'Chat';
}

export interface CreateChatDto {
  name: string;
  problemHash?: string;
  type: 'ProblemSolver' | 'Chat';
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

export interface ProblemDto {
  hash: string;
  name: string;
  description: string;
  condition: string;
}

export interface ProblemsResponseDto {
  problems: ProblemDto[];
  number: number;
}
