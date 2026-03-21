<template>
  <v-container fluid class="pa-4">
    <v-card class="mx-auto" max-width="1400">
      <!-- Header with Home Button -->
      <div class="d-flex align-center justify-space-between pa-4">
        <h1 class="text-h5">Админ-панель</h1>
        <v-btn
          color="primary"
          variant="outlined"
          prepend-icon="mdi-home"
          @click="router.push('/')"
        >
          На главную
        </v-btn>
      </div>

      <!-- Tabs Navigation -->
      <v-tabs v-model="activeTab" bg-color="primary" class="mb-4">
        <v-tab value="statistics" @click="navigateToTab('statistics')">
          <v-icon start>mdi-chart-box</v-icon>
          Статистика
        </v-tab>
        <v-tab value="llmath-problems" @click="navigateToTab('llmath-problems')">
          <v-icon start>mdi-file-document-edit</v-icon>
          LLMath Задачи
        </v-tab>
      </v-tabs>

      <v-window v-model="activeTab">
        <v-window-item value="statistics">
          <StatisticsView />
        </v-window-item>
        <v-window-item value="llmath-problems">
          <TestLLMathProblemsView />
        </v-window-item>
      </v-window>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import StatisticsView from './StatisticsView.vue';
import TestLLMathProblemsView from './TestLLMathProblemsView.vue';

const route = useRoute();
const router = useRouter();

const getTabFromRoute = () => {
  const path = route.path;
  if (path.includes('llmath-problems')) {
    return 'llmath-problems';
  }
  return 'statistics'; // default
};

const activeTab = ref(getTabFromRoute());

// Watch for route changes
watch(() => route.path, () => {
  activeTab.value = getTabFromRoute();
});

// Navigate when tab is clicked
function navigateToTab(tab: string) {
  if (tab === 'statistics') {
    router.push('/admin/statistics');
  } else if (tab === 'llmath-problems') {
    router.push('/admin/llmath-problems');
  }
}

onMounted(() => {
  // If we're at /admin, redirect to /admin/statistics
  if (route.path === '/admin') {
    router.replace('/admin/statistics');
  }
});
</script>

<style scoped>
/* Additional styles if needed */
</style>
