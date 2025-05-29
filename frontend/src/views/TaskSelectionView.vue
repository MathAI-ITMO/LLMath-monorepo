<template>
  <v-container>
    <v-card class="mx-auto pa-6" max-width="1000">
      <div class="d-flex align-center mb-6">
        <v-btn
          icon
          variant="text"
          color="primary"
          to="/"
          class="me-3 home-btn"
          size="large"
        >
          <v-icon size="x-large">mdi-home</v-icon>
        </v-btn>
        <h1 class="text-h4 text-left flex-grow-1" style="margin-bottom: 0;">{{ pageTitle }}</h1>
      </div>

      <v-progress-circular v-if="loading && !isStartingTask" indeterminate color="primary" class="d-block mx-auto mb-6"></v-progress-circular>
      
      <v-alert v-if="error" type="error" variant="tonal" class="mb-4">
        {{ error }}
      </v-alert>

      <template v-if="!loading && !error">
        <v-data-table
          :headers="headers"
          :items="tasks"
          item-value="id"
          class="elevation-1 task-table"
          hover
          :loading="isStartingTask"
          loading-text=""
          @click:row="handleRowClick"
          items-per-page="-1"
          hide-default-footer
        >
          <template v-slot:item.status="{ item }">
            <v-chip :color="getStatusColor(item.status)" size="small">
              {{ getStatusText(item.status) }}
            </v-chip>
          </template>
          <template v-slot:loading>
            <v-skeleton-loader type="table-row@5"></v-skeleton-loader>
          </template>
        </v-data-table>
      </template>
    </v-card>

    <v-overlay
      v-model="isStartingTask"
      class="align-center justify-center text-center"
      persistent
      scroll-strategy="block"
      content-class="elevation-4"
    >
      <v-card color="surface" class="pa-6" rounded="lg">
        <v-progress-circular indeterminate size="64" color="primary" class="mb-4"></v-progress-circular>
        <p class="text-h6">
          Готовлю задачу<br>
          <span class="font-weight-medium">"{{ startingTaskName }}"</span><br>
          и начинаю диалог...
        </p>
      </v-card>
    </v-overlay>

  </v-container>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { useUserTasks } from '@/composables/useUserTasks';
import { useChat } from '@/composables/useChat';
import type { UserTaskDto, CreateChatDto } from '@/types/BackendDtos';
import { UserTaskStatus } from '@/types/BackendDtos';

const router = useRouter();
const route = useRoute();
const { fetchUserTasks, startUserTask } = useUserTasks();
const { createChat } = useChat();
const loading = ref(true);
const isStartingTask = ref(false);
const startingTaskName = ref('');
const error = ref<string | null>(null);
const tasks = ref<UserTaskDto[]>([]);

// Получаем taskType из URL параметров
const taskType = computed(() => {
  const typeParam = route.query.taskType;
  if (typeParam) {
    const type = parseInt(typeParam as string);
    return isNaN(type) ? 0 : type;
  }
  return 0; // По умолчанию тип 0
});

// Заголовок страницы (можно расширить в будущем, если понадобится отображать названия режимов)
const pageTitle = 'Выберите задачу для решения';

// Заголовки для таблицы
const headers: readonly any[] = [
  { title: 'Название задачи', key: 'displayName', align: 'start', sortable: true },
  { title: 'Статус', key: 'status', align: 'center', sortable: true, width: '15%' },
];

const EMPTY_GUID = '00000000-0000-0000-0000-000000000000';

// Загрузка задач при инициализации
onMounted(async () => {
  loading.value = true;
  isStartingTask.value = false;
  error.value = null;
  tasks.value = [];

  try {
    // Загружаем задачи пользователя отфильтрованные по taskType на бэкенде
    const allTasks = await fetchUserTasks(taskType.value);
    tasks.value = allTasks;

    if (tasks.value.length === 0) {
      error.value = 'Список доступных задач пуст.';
    }
  } catch (err) {
    console.error('Ошибка при загрузке задач пользователя:', err);
    error.value = err instanceof Error ? err.message : 'Произошла ошибка при загрузке списка задач.';
  } finally {
    loading.value = false;
  }
});

// Обновляем задачи при изменении taskType в URL
watch(taskType, async (newType) => {
  if (!loading.value && !isStartingTask.value) {
    loading.value = true;
    error.value = null;
    tasks.value = [];

    try {
      const allTasks = await fetchUserTasks(newType);
      tasks.value = allTasks;

      if (tasks.value.length === 0) {
        error.value = 'Список доступных задач пуст.';
      }
    } catch (err) {
      console.error(`Ошибка при загрузке задач пользователя типа ${newType}:`, err);
      error.value = err instanceof Error ? err.message : 'Произошла ошибка при загрузке списка задач.';
    } finally {
      loading.value = false;
    }
  }
});

// Определяем цвет для статуса
function getStatusColor(status: UserTaskStatus): string {
  switch (status) {
    case UserTaskStatus.Solved: return 'success';
    case UserTaskStatus.InProgress: return 'warning';
    case UserTaskStatus.Attempted: return 'info';
    case UserTaskStatus.NotStarted:
    default: return 'grey';
  }
}

// Определяем текст для статуса
function getStatusText(status: UserTaskStatus): string {
  switch (status) {
    case UserTaskStatus.Solved: return 'Решена';
    case UserTaskStatus.InProgress: return 'В процессе';
    case UserTaskStatus.Attempted: return 'Была попытка';
    case UserTaskStatus.NotStarted:
    default: return 'Не начата';
  }
}

// Вспомогательная функция для получения короткого имени задачи
function extractTaskShortName(task: UserTaskDto): string {
  return task.displayName || task.problemId;
}

// Обработчик клика по строке таблицы
async function handleRowClick(event: any, { item }: { item: UserTaskDto }) {
  console.log('Clicked task:', item);
  // Сначала проверим, есть ли уже связанный чат и он не является пустым GUID
  if (item.associatedChatId && item.associatedChatId !== EMPTY_GUID) {
    console.log(`Task ${item.problemId} has associated chat ${item.associatedChatId}, navigating directly.`);
    router.push({ name: 'chat', params: { chatId: item.associatedChatId } });
    return;
  }

  // Если чата нет или associatedChatId был пустым GUID, стартуем/перезапускаем задачу на бэкенде
  startingTaskName.value = extractTaskShortName(item);
  isStartingTask.value = true;
  error.value = null;

  try {
    // Т.к. бэкенд сам создаст чат, мы не передаем chatId (второй параметр)
    const updatedTask = await startUserTask(String(item.id), '');

    if (updatedTask && updatedTask.associatedChatId && updatedTask.associatedChatId !== EMPTY_GUID) {
      // Бэкенд вернул валидный ID чата, переходим в него
      console.log(`Task ${item.problemId} started/updated, navigating to chat ${updatedTask.associatedChatId}`);
      router.push({ name: 'chat', params: { chatId: updatedTask.associatedChatId } });
    } else {
      let errorMsg = 'Не удалось получить валидный ID чата от сервера после старта задачи.';
      if (updatedTask && updatedTask.associatedChatId === EMPTY_GUID) {
        errorMsg = 'Задача обработана, но связанный чат имеет некорректный (пустой) ID. Пожалуйста, сообщите администратору или попробуйте позже.';
      } else if (!updatedTask) {
        errorMsg = 'Не удалось обработать запуск задачи на сервере. Ответ от сервера был пустым.';
      }
      console.error('Failed to start task or get a valid associated chat ID from backend.', updatedTask);
      error.value = errorMsg;
    }
  } catch (err) {
    console.error('Error handling task start or chat navigation:', err);
    error.value = err instanceof Error ? err.message : 'Произошла ошибка при попытке открыть задачу.';
  } finally {
    isStartingTask.value = false;
    startingTaskName.value = '';
  }
}
</script>

<style scoped>
.task-table :deep(tbody tr) {
  cursor: pointer;
}

.task-table :deep(tbody tr:hover) {
  background-color: rgba(128, 128, 128, 0.2) !important;
  color: rgb(var(--v-theme-primary)) !important;
}

/* Стилизация заголовков таблицы */
.task-table :deep(thead tr th) {
  background-color: rgba(var(--v-theme-primary), 0.4) !important;
  font-weight: bold !important;
  color: rgba(var(--v-theme-on-surface), 0.87) !important;
  font-size: 1rem !important;
}

.v-container {
  padding-top: 2rem;
}

.v-overlay .v-card p {
  line-height: 1.5;
  margin-bottom: 0;
}

.home-btn {
  transition: all 0.3s ease;
}

.home-btn:hover {
  transform: translateY(-2px);
  color: white !important;
  background-color: rgba(var(--v-theme-primary), 0.15) !important;
}
</style> 