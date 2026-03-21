import { api } from '@/api';
import type { UserTaskDto } from '@/api/generated/api';

export function useUserTasks() {
  async function fetchUserTasks(taskType: number): Promise<UserTaskDto[]> {
    const response = await api.getApiUserTasks({ taskType });
    return response.data;
  }

  async function startUserTask(userTaskId: string, _chatId?: string): Promise<UserTaskDto> {
    const response = await api.postApiUserTasksUserTaskIdStart(userTaskId);
    return response.data;
  }

  async function completeUserTask(taskId: string): Promise<UserTaskDto | null> {
    try {
      const response = await api.postApiUserTasksUserTaskIdComplete(taskId);
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
