<template>
  <v-container>
    <v-card class="mx-auto pa-6" max-width="800">
      <v-btn icon variant="text" color="primary" @click="$router.back()" class="mb-4">
        <v-icon>mdi-arrow-left</v-icon>
      </v-btn>
      <v-card flat class="mb-6 user-profile-card">
        <v-card-text>
          <div v-if="userInfo" class="user-info">
            <div class="user-info-compact">
              <div class="user-name-block">
                <h2 class="user-name primary--text">{{ userInfo.firstName }} {{ userInfo.lastName }}</h2>
              </div>
              <div class="user-details-block">
                <div class="info-item">
                  <v-icon size="small" color="primary" class="mr-1">mdi-email</v-icon>
                  <span class="info-label">Email:</span>
                  <span class="info-value-small">{{ userInfo.email }}</span>
                </div>
                <div class="info-item">
                  <v-icon size="small" color="primary" class="mr-1">mdi-account-group</v-icon>
                  <span class="info-label">Группа:</span>
                  <span class="info-value-small">{{ userInfo.studentGroup || 'Не указана' }}</span>
                </div>
              </div>
            </div>
          </div>
          <v-skeleton-loader v-else type="list-item-avatar-three-line" />
        </v-card-text>
        <v-divider></v-divider>
      </v-card>
      <v-progress-circular v-if="loading" indeterminate color="primary" class="d-block mx-auto mb-6" />
      <v-alert v-if="error" type="error" variant="tonal" class="mb-4">{{ error }}</v-alert>
      <template v-if="!loading && !error">
        <h2>Решённые задачи</h2>
        <v-list dense class="task-list">
          <v-list-item 
            v-for="(task, index) in details.solvedTasks" 
            :key="task.UserTaskId || task.userTaskId" 
            @click="goToChat(task.ChatId || task.chatId)"
            :class="{'list-item-odd': index % 2 !== 0, 'task-list-item': true}"
          >
            <v-list-item-title>{{ task.DisplayName || task.displayName }}</v-list-item-title>
          </v-list-item>
          <div v-if="details.solvedTasks.length === 0" class="text-caption mb-4">Нет решённых задач.</div>
        </v-list>
        <h2 class="mt-6">Задачи в процессе</h2>
        <v-list dense class="task-list">
          <v-list-item 
            v-for="(task, index) in details.inProgressTasks" 
            :key="task.UserTaskId || task.userTaskId" 
            @click="goToChat(task.ChatId || task.chatId)"
            :class="{'list-item-odd': index % 2 !== 0, 'task-list-item': true}"
          >
            <v-list-item-title>{{ task.DisplayName || task.displayName }}</v-list-item-title>
          </v-list-item>
          <div v-if="details.inProgressTasks.length === 0" class="text-caption mb-4">Нет задач в процессе.</div>
        </v-list>
        <h2 class="mt-6">Обычные чаты</h2>
        <v-list dense class="task-list">
          <v-list-item 
            v-for="(chat, index) in details.chats" 
            :key="chat.ChatId || chat.chatId" 
            @click="goToChat(chat.ChatId || chat.chatId)"
            :class="{'list-item-odd': index % 2 !== 0, 'task-list-item': true}"
          >
            <v-list-item-title>{{ chat.Name || chat.name }}</v-list-item-title>
          </v-list-item>
          <div v-if="details.chats.length === 0" class="text-caption mb-4">Нет обычных чатов.</div>
        </v-list>
      </template>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import axios from 'axios';
import { VContainer, VCard, VBtn, VIcon, VList, VListItem, VListItemTitle, VListItemAction, VProgressCircular, VAlert } from 'vuetify/components';

interface TaskItemDto { 
  UserTaskId: string; 
  DisplayName: string; 
  ChatId?: string;
  userTaskId?: string;
  displayName?: string;
  chatId?: string;
}

interface ChatItemDto { 
  ChatId: string; 
  Name: string;
  chatId?: string;
  name?: string;
}

interface UserDetailDto { 
  solvedTasks: TaskItemDto[]; 
  inProgressTasks: TaskItemDto[]; 
  chats: ChatItemDto[]; 
}

interface UserInfo {
  firstName: string;
  lastName: string;
  email: string;
  studentGroup: string;
}

const route = useRoute();
const router = useRouter();
const userId = route.params.userId as string;

const details = ref<UserDetailDto>({ solvedTasks: [], inProgressTasks: [], chats: [] });
const loading = ref(false);
const error = ref<string | null>(null);
const userInfo = ref<UserInfo | null>(null);

onMounted(async () => {
  loading.value = true;
  error.value = null;
  
  await fetchUserInfo();
  
  try {
    const baseUrl = import.meta.env.VITE_MATHLLM_BACKEND_ADDRESS;
    const resp = await axios.get<UserDetailDto>(`${baseUrl}/api/stats/user-details/${userId}`, { withCredentials: true });
    details.value = resp.data;
    console.log('Fetched user details:', resp.data);
    
    if (resp.data.solvedTasks && resp.data.solvedTasks.length > 0) {
      console.log('Sample solved task:', resp.data.solvedTasks[0]);
    }
    if (resp.data.inProgressTasks && resp.data.inProgressTasks.length > 0) {
      console.log('Sample in progress task:', resp.data.inProgressTasks[0]);
    }
    if (resp.data.chats && resp.data.chats.length > 0) {
      console.log('Sample chat:', resp.data.chats[0]);
    }
  } catch (e: any) {
    console.error('Error fetching user details:', e);
    error.value = e instanceof Error ? e.message : 'Ошибка при загрузке деталей пользователя.';
  } finally {
    loading.value = false;
  }
});

async function fetchUserInfo() {
  try {
    const baseUrl = import.meta.env.VITE_MATHLLM_BACKEND_ADDRESS;
    
    // Вместо использования /api/stats/user-stats + /api/user/profile
    // Получаем информацию напрямую через /api/user/me
    try {
      const userResponse = await axios.get(`${baseUrl}/api/user/me`, { withCredentials: true });
      if (userResponse.data) {
        userInfo.value = {
          firstName: userResponse.data.firstName,
          lastName: userResponse.data.lastName,
          email: userResponse.data.email || 'Не указан',
          studentGroup: userResponse.data.studentGroup || 'Не указана'
        };
        console.log('User info loaded from /api/user/me:', userInfo.value);
        return;
      }
    } catch (userError) {
      console.warn('Could not fetch user via /api/user/me:', userError);
      // Продолжаем выполнение если не удалось получить через /api/user/me
    }
    
    // Запасной вариант через /api/stats/user-stats
    const resp = await axios.get(`${baseUrl}/api/stats/user-stats`, { withCredentials: true });
    const userData = resp.data.find((user: any) => user.userId === userId);
    if (userData) {
      userInfo.value = {
        firstName: userData.firstName,
        lastName: userData.lastName,
        email: 'Не указан', // Предполагаем, что email отсутствует
        studentGroup: userData.studentGroup || 'Не указана'
      };
      console.log('User info loaded from stats API:', userInfo.value);
    } else {
      console.warn('User not found in stats');
    }
  } catch (error) {
    console.error('Failed to load user info:', error);
  }
}

function goToChat(chatId?: string) {
  console.log('goToChat called with:', chatId);
  if (chatId) router.push(`/admin-chat/${chatId}`);
}
</script>

<style scoped>
h2 { font-weight: 500; margin-bottom: 0.5rem; }

.user-info-compact {
  display: flex;
  flex-direction: column;
  padding: 8px 0;
  margin-top: 0;
}

.user-name-block {
  margin-bottom: 12px;
}

.user-name {
  font-size: 1.8rem;
  font-weight: 700;
  margin: 0;
  line-height: 1.2;
}

.user-details-block {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
}

.info-item {
  display: flex;
  align-items: center;
  padding: 0;
  border-radius: 4px;
}

.info-label {
  font-size: 0.85rem;
  font-weight: 500;
  color: rgba(var(--v-theme-on-surface), 0.8);
  margin-right: 4px;
}

.info-value-small {
  font-size: 0.85rem;
}

.task-list {
  border: 1px solid rgba(var(--v-theme-on-surface), 0.1);
  border-radius: 4px;
  overflow: hidden;
}

.task-list-item {
  border-bottom: 1px solid rgba(var(--v-theme-on-surface), 0.05);
  transition: background-color 0.4s;
}

.task-list-item {
  border-bottom: 1px solid rgba(var(--v-theme-on-surface), 0.2);;
  background-color: rgba(var(--v-theme-primary), 0.2);
}

.list-item-odd {
  background-color: rgba(var(--v-theme-primary), 0.17);
}

.task-list-item:hover {
  background-color: rgba(var(--v-theme-primary), 0.8) !important;
  cursor: pointer;
}
</style> 