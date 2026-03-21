import type { UserTaskDto, StartUserTaskRequestDto } from '@/types/BackendDtos';
import { createBackendApiClient } from '@/utils/apiClient';

export function useUserTasks() {
  const client = createBackendApiClient();

  async function fetchUserTasks(taskType: number): Promise<UserTaskDto[]> {
    const response = await client.get<UserTaskDto[]>('/api/usertasks', {
      params: { taskType },
      withCredentials: true,
    });
    return response.data;
  }

  async function startUserTask(userTaskId: string, chatId: string): Promise<UserTaskDto> {
    const requestData: StartUserTaskRequestDto = { chatId };
    const response = await client.post<UserTaskDto>(
      `/api/usertasks/${userTaskId}/start`,
      requestData,
      { withCredentials: true },
    );
    return response.data;
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