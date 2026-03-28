import { ref, computed } from 'vue';
import { servicesConfig } from '@/config/services.config';
import { getFastAPI } from '@/api/generated/videoApi';

export function useVideoManagement() {
  const availableVideos = ref<string[]>([]);
  const loadingVideos = ref(false);

  const videoAppUrl = computed(() => servicesConfig.videoServiceUrl);

  const videoSelectItems = computed(() => [
    { title: '-- Не выбрано --', value: '' },
    ...availableVideos.value.map(v => ({ title: v, value: v }))
  ]);

  const { listVideosVideosGet } = getFastAPI()

  async function fetchAvailableVideos() {
    if (loadingVideos.value) return;

    loadingVideos.value = true;
    try {
      const response = await listVideosVideosGet();
      availableVideos.value = (response.data as { name: string }[]).map((v) => v.name);
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