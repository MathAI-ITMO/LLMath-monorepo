import { ref } from 'vue';
import { createBackendApiClient } from '@/utils/apiClient';

export function useLlmSolution() {
  const loading = ref(false);
  const activeForm = ref<'management' | 'database' | 'update' | 'edit' | null>(null);

  async function getLlmSolution(problemStatement: string) {
    if (!problemStatement) {
      throw new Error('Поле "Условие" не может быть пустым для получения решения LLM');
    }

    loading.value = true;

    try {
      const client = createBackendApiClient();

      const response = await client.post('/api/v1/llm/solve-problem', {
        problemDescription: problemStatement
      });

      if (response.data && response.data.error) {
        throw new Error(response.data.error);
      }

      return response.data.solution;
    } catch (error: any) {
      console.error('Ошибка при получении решения от LLM:', error);
      const data = error.response?.data;
      const errorMessage = data?.error || data?.message || data || error.message || error;
      throw new Error(errorMessage);
    } finally {
      loading.value = false;
    }
  }

  return {
    loading,
    activeForm,
    getLlmSolution
  };
}