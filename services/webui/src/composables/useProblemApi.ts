import { reactive, ref } from 'vue';
import type { TaskType } from '@/api/generated/api';

export const LLMATH_PROBLEMS_API_URL = '/app';

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
  geolin_ans_key?: GeoilonAnsKey; // Legacy field for backward compatibility
  result?: string;
  solution?: Solution; // May not be present in backend response
  llmSolution?: string;
  llm_solution?: any; // Legacy field for backward compatibility
  theoryLink?: string;
  theory_link?: string; // Legacy field for backward compatibility
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

  async function makeApiCall(
    endpoint: string,
    method: string,
    body?: any,
    loadingKey?: string,
    responseKey?: string
  ) {
    if (loadingKey) apiCallLoading[loadingKey] = true;
    if (responseKey) apiResponse[responseKey] = null;

    try {
      const options: RequestInit = {
        method,
        headers: {
          'Content-Type': 'application/json',
        },
      };
      if (body && (method === 'POST' || method === 'PUT')) {
        options.body = JSON.stringify(body);
      }
      const response = await fetch(`${LLMATH_PROBLEMS_API_URL}${endpoint}`, {
        ...options,
        credentials: 'include'
      });

      let responseData;
      const contentType = response.headers.get("content-type");
      if (contentType && contentType.indexOf("application/json") !== -1) {
        responseData = await response.json();
      } else {
        responseData = await response.text();
      }

      if (!response.ok) {
        const errorDetail = typeof responseData === 'object' ? responseData : { message: responseData, status: response.status };
        throw errorDetail;
      }

      if (responseKey) {
        apiResponse[responseKey] = responseData;
      }
      return responseData;
    } catch (e: any) {
      console.error(`Ошибка при вызове ${method} ${LLMATH_PROBLEMS_API_URL}${endpoint}:`, e);
      const errorMsg = e.message || e.error || e;
      if (responseKey) {
        apiResponse[responseKey] = { error: true, message: errorMsg };
      }
      return { error: true, message: errorMsg };
    } finally {
      if (loadingKey) apiCallLoading[loadingKey] = false;
    }
  }

  // Get problems by TaskType enum
  async function fetchProblemsByType(type: TaskType) {
    return makeApiCall(`/api/Problems/type/${type}`, 'GET', undefined, 'fetchProblemsByType', 'fetchProblemsByType');
  }

  return {
    apiCallLoading,
    apiResponse,
    makeApiCall,
    fetchProblemsByType,
  };
}
