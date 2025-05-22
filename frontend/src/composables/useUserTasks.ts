import axios, { type AxiosInstance } from 'axios';
import type { UserTaskDto, StartUserTaskRequestDto } from '@/types/BackendDtos';

const baseUrl = import.meta.env.VITE_MATHLLM_BACKEND_ADDRESS;

export function useUserTasks() {
  const client: AxiosInstance = axios.create({
    baseURL: baseUrl,
  });

  /**
   * Получает список задач пользователя для указанного типа.
   * Если на бэкенде задач нет, они будут созданы по умолчанию.
   * @param taskType Тип задач (например, 0 для "Выбрать задачу")
   * @returns Промис со списком задач.
   */
  async function fetchUserTasks(taskType: number): Promise<UserTaskDto[]> {
    try {
      const response = await client.get<UserTaskDto[]>('/api/usertasks', {
        params: { taskType },
        withCredentials: true,
      });
      return response.data;
    } catch (error) {
      console.error(`Failed to fetch user tasks for type ${taskType}:`, error);
      // Можно выбросить ошибку или вернуть пустой массив, в зависимости от желаемой обработки в UI
      throw new Error('Failed to load tasks. Please try again later.'); 
    }
  }

  /**
   * Помечает задачу как начатую (InProgress) и связывает ее с чатом.
   * @param userTaskId ID задачи пользователя (из UserTaskDto.id)
   * @param chatId ID созданного чата
   * @returns Промис с обновленной задачей.
   */
  async function startUserTask(userTaskId: string, chatId: string): Promise<UserTaskDto> {
    const requestData: StartUserTaskRequestDto = { chatId };
    try {
      const response = await client.post<UserTaskDto>(
        `/api/usertasks/${userTaskId}/start`,
        requestData,
        { withCredentials: true },
      );
      return response.data;
    } catch (error) {
      console.error(`Failed to start user task ${userTaskId} with chat ${chatId}:`, error);
      // Можно выбросить конкретную ошибку или общую
      throw new Error('Failed to update task status. Please try again.');
    }
  }

  async function completeUserTask(taskId: number): Promise<UserTaskDto | null> {
    try {
      const response = await client.post<UserTaskDto>(`/api/usertasks/${taskId}/complete`, {}, { withCredentials: true });
      return response.data;
    } catch (error) {
      console.error(`Failed to mark task ${taskId} as solved:`, error);
      return null;
    }
  }

  return {
    fetchUserTasks,
    startUserTask,
    completeUserTask,
  };
} 