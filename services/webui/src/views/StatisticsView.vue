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
import { useRouter } from 'vue-router';
import { useUserStats, type UserStats } from '@/composables/useUserStats';

const { loading, error, groupedStats } = useUserStats();

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

// Роутер для навигации
const router = useRouter();

// Обработка клика по строке: перенаправление на страницу деталей пользователя
function onRowClick(_event: any, payload: { item: UserStats }) {
  const row = payload.item;
  if (row.userId) {
    router.push({ name: 'user-details', params: { userId: row.userId } });
  }
}
</script>

<style scoped>
/* Стили для заголовков таблицы статистики */
.statistics-table :deep(thead tr th) {
  background-color: rgba(var(--v-theme-primary), 0.4) !important;
  color: rgba(var(--v-theme-on-surface), 0.87) !important;
  font-weight: bold !important;
}
</style>
