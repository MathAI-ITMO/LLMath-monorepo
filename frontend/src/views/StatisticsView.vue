<template>
  <v-container>
    <v-card class="mx-auto pa-6" max-width="1200">
      <div class="d-flex align-center mb-6">
        <h1 class="text-h4 text-left flex-grow-1" style="margin-bottom: 0;">Статистика пользователей</h1>
      </div>

      <v-progress-circular v-if="loading" indeterminate color="primary" class="d-block mx-auto mb-6" />
      <v-alert v-if="error" type="error" variant="tonal" class="mb-4">
        {{ error }}
      </v-alert>

      <template v-if="!loading && !error">
        <div v-for="(items, group) in groupedStats" :key="group" class="mb-8">
          <h2 class="text-h5 mb-4">Группа: {{ group }}</h2>
          <v-data-table
            :headers="tableHeaders"
            :items="items"
            item-value="userId"
            class="elevation-1 statistics-table"
            hover
            hide-default-footer
            @click:row="onRowClick"
            @row-click="onRowClick"
          />
        </div>
      </template>

    </v-card>
  </v-container>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import axios from 'axios';

interface UserStats {
  userId: string;
  firstName: string;
  lastName: string;
  studentGroup: string;
  solvedCount: number;
  inProgressCount: number;
  normalChatsCount: number;
}

const stats = ref<UserStats[]>([]);
const loading = ref(false);
const error = ref<string | null>(null);

const headers = [
  { title: 'Имя', key: 'firstName', sortable: true },
  { title: 'Фамилия', key: 'lastName', sortable: true },
  { title: 'Группа', key: 'studentGroup', sortable: true },
  { title: 'Решено задач', key: 'solvedCount', sortable: true },
  { title: 'В процессе', key: 'inProgressCount', sortable: true },
  { title: 'Обычных чатов', key: 'normalChatsCount', sortable: true },
];

// Убираем колонку Группа из таблицы, так как будем группировать вручную
const tableHeaders = headers.filter(h => h.key !== 'studentGroup');

// Группировка по полю studentGroup
const groupedStats = computed(() => {
  const arr = Array.isArray(stats.value) ? stats.value : [];
  return arr.reduce((acc: Record<string, UserStats[]>, stat) => {
    const group = stat.studentGroup || 'Без группы';
    if (!acc[group]) acc[group] = [];
    acc[group].push(stat);
    return acc;
  }, {} as Record<string, UserStats[]>);
});

// Роутер для навигации
const router = useRouter();

// Обработка клика по строке: перенаправление на страницу деталей пользователя
function onRowClick(_event: any, payload: { item: UserStats }) {
  const row = payload.item;
  if (row.userId) {
    router.push({ name: 'user-details', params: { userId: row.userId } });
  }
}

// Базовый адрес бэкенда из переменных окружения
const baseUrl = import.meta.env.VITE_MATHLLM_BACKEND_ADDRESS;

onMounted(async () => {
  loading.value = true;
  error.value = null;
  try {
    // Запрашиваем статистику у бэкенда
    const response = await axios.get<UserStats[]>(`${baseUrl}/api/stats/user-stats`, { withCredentials: true });
    const data = response.data;
    stats.value = Array.isArray(data) ? data : [];
  } catch (e: any) {
    console.error('Error fetching user stats:', e);
    error.value = e instanceof Error ? e.message : 'Ошибка при загрузке статистики.';
  } finally {
    loading.value = false;
  }
});
</script>

<style scoped>
/* Стили для заголовков таблицы статистики */
.statistics-table :deep(thead tr th) {
  background-color: rgba(var(--v-theme-primary), 0.4) !important;
  color: rgba(var(--v-theme-on-surface), 0.87) !important;
  font-weight: bold !important;
}
</style> 