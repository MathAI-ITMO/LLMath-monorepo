import { ref, reactive } from 'vue';
import { createBackendApiClient } from '@/utils/apiClient';
import { type GeolinProblemData } from './useProblemApi';

export function useGeolinProxy() {
  const loading = ref(false);
  const response = reactive<{ loadFromGeolin: GeolinProblemData | null }>({
    loadFromGeolin: null
  });

  const checkResultModal = reactive({
    show: false,
    problemStatement: '',
    solution: '',
    extractedAnswer: '',
    checkResult: null as any,
    hash: '',
    seed: undefined as number | undefined
  });

  async function loadFromGeolin(prefix: string) {
    if (!prefix) {
      response.loadFromGeolin = { error: "Префикс GeoLin не может быть пустым." };
      return response.loadFromGeolin;
    }

    loading.value = true;
    response.loadFromGeolin = null;

    try {
      const url = `/app/api/tasks/problem/${encodeURIComponent(prefix)}`
      const fetchResponse = await fetch(url, {
        credentials: 'include'
      });
      const data = await fetchResponse.json();
      console.log("Response from GeoLin API:", data);

      if (!fetchResponse.ok) {
        const errorMsg = data.error || data.message || `HTTP error! status: ${fetchResponse.status}`;
        console.error("Ошибка от GeoLin API:", data);
        throw new Error(typeof errorMsg === 'object' ? JSON.stringify(errorMsg) : String(errorMsg));
      }

      response.loadFromGeolin = data;
      return data;
    } catch (e: any) {
      console.error("Ошибка при загрузке из GeoLin прокси:", e);
      response.loadFromGeolin = { error: e.message || 'Неизвестная ошибка при запросе к GeoLin прокси' };
      return response.loadFromGeolin;
    } finally {
      loading.value = false;
    }
  }

  async function checkSolution(statement: string, solution: string, hash: string, seed?: number) {
    if (!statement) {
      throw new Error('Поле "Условие" не может быть пустым для проверки решения');
    }

    if (!solution) {
      throw new Error('Поле "Решение LLM" не может быть пустым для проверки');
    }

    loading.value = true;

    try {
      const client = createBackendApiClient();

      // Шаг 1: Извлекаем ответ из решения с помощью LLM
      const extractRequestData = {
        problemStatement: statement,
        solution: solution
      };

      const extractResponse = await client.post('/api/v1/llm/extract-answer', extractRequestData);
      const extractedAnswer = extractResponse.data.extractedAnswer;

      if (!extractedAnswer) {
        throw new Error('LLM не смог извлечь ответ из решения - получен пустой ответ');
      }

      const checkRequestData = {
        hash: hash,
        answerAttempt: extractedAnswer,
        seed: seed
      };

      const checkResponse = await client.post('/api/v1/geolin-proxy/check-answer-direct', checkRequestData);
      const checkResult = checkResponse.data;

      // Обновляем модальное окно результатов
      checkResultModal.show = true;
      checkResultModal.problemStatement = statement;
      checkResultModal.solution = solution;
      checkResultModal.extractedAnswer = extractedAnswer;
      checkResultModal.checkResult = checkResult;
      checkResultModal.hash = hash;
      checkResultModal.seed = seed;

      return checkResult;
    } catch (error: any) {
      console.error('Ошибка при проверке решения:', error);
      const data = error.response?.data;
      const errorMessage = data?.error || data?.message || data || error.message || error;
      throw new Error(errorMessage);
    } finally {
      loading.value = false;
    }
  }

  function closeCheckResultModal() {
    checkResultModal.show = false;
  }

  return {
    loading,
    response,
    checkResultModal,
    loadFromGeolin,
    checkSolution,
    closeCheckResultModal,
  };
}
