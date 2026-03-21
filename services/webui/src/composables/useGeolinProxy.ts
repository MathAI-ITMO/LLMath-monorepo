import { ref, reactive } from 'vue';
import { api } from '@/api';
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
      const resp = await api.getApiTasksProblemPrefixName(encodeURIComponent(prefix));
      const data = resp.data as unknown as GeolinProblemData;
      console.log("Response from GeoLin API:", data);
      response.loadFromGeolin = data;
      return data;
    } catch (e: any) {
      console.error("Ошибка при загрузке из GeoLin прокси:", e);
      const msg = e.response?.data?.error || e.response?.data?.message || e.message || 'Неизвестная ошибка при запросе к GeoLin прокси';
      response.loadFromGeolin = { error: msg };
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
      const extractResponse = await api.postApiV1LlmExtractAnswer({
        problemStatement: statement,
        solution: solution
      });
      const extractedAnswer = (extractResponse.data as unknown as { extractedAnswer: string }).extractedAnswer;

      if (!extractedAnswer) {
        throw new Error('LLM не смог извлечь ответ из решения - получен пустой ответ');
      }

      const checkResponse = await api.postApiV1GeolinProxyCheckAnswerDirect({
        hash,
        answerAttempt: extractedAnswer,
        seed,
        problemParams: '',
      });
      const checkResult = checkResponse.data;

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
