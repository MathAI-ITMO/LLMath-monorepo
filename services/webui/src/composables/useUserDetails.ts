import { ref, onMounted } from 'vue';
import { backendApi } from '@/utils/apiClient';

export interface TaskItemDto {
  UserTaskId: string;
  DisplayName: string;
  ChatId?: string;
  TaskType?: number;
  userTaskId?: string;
  displayName?: string;
  chatId?: string;
  taskType?: number;
}

export interface ChatItemDto {
  ChatId: string;
  Name: string;
  chatId?: string;
  name?: string;
}

export interface UserDetailDto {
  solvedTasks: TaskItemDto[];
  inProgressTasks: TaskItemDto[];
  chats: ChatItemDto[];
}

export interface UserInfo {
  firstName: string;
  lastName: string;
  email: string;
  studentGroup: string;
}

export function useUserDetails(userId: string) {
  const details = ref<UserDetailDto>({ solvedTasks: [], inProgressTasks: [], chats: [] });
  const loading = ref(false);
  const error = ref<string | null>(null);
  const userInfo = ref<UserInfo | null>(null);
  const taskModeTitles = ref<Record<string, string>>({});

  async function fetchTaskModeTitles() {
    const titlesResponse = await backendApi.get<Record<string, string>>('/api/stats/task-mode-titles');
    taskModeTitles.value = titlesResponse.data;
  }

  async function fetchUserInfo() {
    try {
      const statsResp = await backendApi.get('/api/stats/user-stats');
      const userDataFromStats = statsResp.data.find((user: any) => user.userId === userId);

      if (userDataFromStats) {
        userInfo.value = {
          firstName: userDataFromStats.firstName,
          lastName: userDataFromStats.lastName,
          email: userDataFromStats.email || 'Не указан',
          studentGroup: userDataFromStats.studentGroup || 'Не указана'
        };
      } else {
        error.value = 'Не удалось загрузить информацию о пользователе.';
      }
    } catch (e) {
      console.error('Failed to load user info from stats API:', e);
      error.value = 'Ошибка при загрузке информации о пользователе.';
    }
  }

  async function fetchUserDetails() {
    loading.value = true;
    error.value = null;
    try {
      const resp = await backendApi.get<UserDetailDto>(`/api/stats/user-details/${userId}`);
      details.value = resp.data;
    } catch (e: any) {
      console.error('Error fetching user details:', e);
      error.value = e instanceof Error ? e.message : 'Ошибка при загрузке деталей пользователя.';
    } finally {
      loading.value = false;
    }
  }

  const formatTaskType = (type: number | undefined): string => {
    if (type === undefined) return 'Тип не указан';
    const typeStr = type.toString();
    if (taskModeTitles.value && taskModeTitles.value[typeStr]) {
      return taskModeTitles.value[typeStr];
    }
    if (type === 0) return 'Упражнение (из списка)';
    return `Неизвестный тип (${type})`;
  };

  onMounted(async () => {
    await fetchTaskModeTitles();
    await fetchUserInfo();
    await fetchUserDetails();
  });

  return {
    details,
    loading,
    error,
    userInfo,
    taskModeTitles,
    formatTaskType,
    fetchUserDetails,
  };
}
