import { reactive } from 'vue';
import type { TaskType } from '@/api/generated/api';
import { api } from '@/api';

export interface GeoilonAnsKey {
  hash: string;
  seed: number;
}

export interface Step {
  order: number;
  prerequisites?: Record<string, any>;
  transition?: Record<string, any>;
  outcomes?: Record<string, any>;
}

export interface Solution {
  steps: Step[];
}

export interface Problem {
  id?: string;
  title?: string;
  statement: string;
  geolinHash?: string;
  geolinSeed?: number;
  result?: string;
  solution?: Solution; // May not be present in backend response
  llmSolution?: string;
  theoryLink?: string;
  types?: TaskType[]; // Array of task types assigned to this problem
}

export interface GeolinProblemData {
  name?: string;
  hash?: string;
  condition?: string;
  seed?: number;
  error?: string;
  problemParams?: string;
}

export function useProblemApi() {
  const apiCallLoading = reactive<Record<string, boolean>>({
    createProblem: false,
    fetchProblemById: false,
    updateProblem: false,
    deleteProblem: false,
    fetchProblemsByType: false,
    loadProblemForUpdate: false,
    managementAddProblem: false,
    managementUpdateProblem: false,
    loadFromGeolin: false,
    getLlmSolution: false,
    checkSolution: false,
  });

  const apiResponse = reactive<Record<string, any>>({
    createProblem: null,
    fetchProblemById: null,
    updateProblem: null,
    deleteProblem: null,
    fetchProblemsByType: null,
    managementAddProblem: null,
    managementUpdateProblem: null,
    loadFromGeolin: null,
    checkSolution: null,
  });

  // Get problems by TaskType enum
  async function fetchProblemsByType(type: TaskType) {
    apiCallLoading.fetchProblemsByType = true;
    apiResponse.fetchProblemsByType = null;
    try {
      const response = await api.getApiProblemsTypeType(type);
      apiResponse.fetchProblemsByType = response.data;
      return response.data;
    } catch (e: any) {
      console.error(`Ошибка при вызове fetchProblemsByType:`, e);
      const errorMsg = e.message || e.error || e;
      apiResponse.fetchProblemsByType = { error: true, message: errorMsg };
      return { error: true, message: errorMsg };
    } finally {
      apiCallLoading.fetchProblemsByType = false;
    }
  }

  return {
    apiCallLoading,
    apiResponse,
    fetchProblemsByType,
  };
}
