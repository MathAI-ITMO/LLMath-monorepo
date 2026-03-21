import { ref, onMounted, computed } from 'vue';
import { backendApi } from '@/utils/apiClient';

export interface UserStats {
  userId: string;
  firstName: string;
  lastName: string;
  studentGroup: string;
  solvedCount: number;
  inProgressCount: number;
  normalChatsCount: number;
}

export function useUserStats() {
  const stats = ref<UserStats[]>([]);
  const loading = ref(false);
  const error = ref<string | null>(null);

  const groupedStats = computed(() => {
    const arr = Array.isArray(stats.value) ? stats.value : [];
    return arr.reduce((acc: Record<string, UserStats[]>, stat) => {
      const group = stat.studentGroup || 'Без группы';
      if (!acc[group]) acc[group] = [];
      acc[group].push(stat);
      return acc;
    }, {} as Record<string, UserStats[]>);
  });

  async function fetchUserStats() {
    loading.value = true;
    error.value = null;
    try {
      const response = await backendApi.get<UserStats[]>('/api/stats/user-stats');
      const data = response.data;
      stats.value = Array.isArray(data) ? data : [];
    } catch (e: any) {
      console.error('Error fetching user stats:', e);
      error.value = e instanceof Error ? e.message : 'Ошибка при загрузке статистики.';
    } finally {
      loading.value = false;
    }
  }

  onMounted(() => {
    fetchUserStats();
  });

  return {
    stats,
    loading,
    error,
    groupedStats,
    fetchUserStats,
  };
}
