import { ref, computed } from 'vue';
import axios from 'axios';
import { servicesConfig } from '@/config/services.config';

export function useVideoManagement() {
  const availableVideos = ref<string[]>([]);
  const loadingVideos = ref(false);

  const videoAppUrl = computed(() => servicesConfig.videoServiceUrl);

  const videoSelectItems = computed(() => [
    { title: '-- Не выбрано --', value: '' },
    ...availableVideos.value.map(v => ({ title: v, value: v }))
  ]);

  async function fetchAvailableVideos() {
    if (loadingVideos.value) return;
    
    loadingVideos.value = true;
    try {
      const response = await axios.get(`${servicesConfig.videoServiceUrl}/videos`);
      availableVideos.value = response.data.map((v: any) => v.name);
    } catch (error) {
      console.error('Error fetching videos:', error);
      availableVideos.value = [];
    } finally {
      loadingVideos.value = false;
    }
  }

  return {
    availableVideos,
    loadingVideos,
    videoAppUrl,
    videoSelectItems,
    fetchAvailableVideos
  };
}