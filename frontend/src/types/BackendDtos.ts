export interface LoginRequestDto {
  email: string;
  password: string;
}

export interface RegisterRequestDto {
  firstName: string;
  lastName: string;
  studentGroup: string;
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

// --- Добавлено для UserTasks ---

// Соответствует enum UserTaskStatus на бэкенде
export enum UserTaskStatus {
  NotStarted = 0,
  InProgress = 1,
  Solved = 2,    
  Attempted = 3 
}

// Соответствует UserTaskDto на бэкенде
export interface UserTaskDto {
  id: number;
  problemId: string;
  problemHash: string;
  displayName: string;
  taskType: number;
  status: UserTaskStatus;
  associatedChatId: string | null; // Guid? преобразуется в string? 
}

// Соответствует StartUserTaskRequestDto на бэкенде
export interface StartUserTaskRequestDto {
  chatId: string; // Guid преобразуется в string
}
