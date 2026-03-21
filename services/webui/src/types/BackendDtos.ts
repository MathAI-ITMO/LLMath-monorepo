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
  type: string;
  taskType?: number;
  theoryLink?: string;
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
  theoryLink?: string;
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

// Соответствует enum TaskType на бэкенде
export enum TaskType {
  Default = 0,
  Learning = 1,
  Guided = 2,
  Exam = 3
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

// --- Problem DTOs ---

// Соответствует CreateProblemRequestDto на бэкенде
export interface CreateProblemRequestDto {
  title?: string | null;
  statement?: string | null;
  llmSolution?: string | null;
  theoryLink?: string | null;
  geolinHash?: string | null;
  geolinSeed?: number | null;
  types?: TaskType[] | null;
}

// Соответствует UpdateProblemRequestDto на бэкенде
export interface UpdateProblemRequestDto {
  title?: string | null;
  statement?: string | null;
  llmSolution?: string | null;
  theoryLink?: string | null;
  geolinHash?: string | null;
  geolinSeed?: number | null;
  types?: TaskType[] | null;
}
