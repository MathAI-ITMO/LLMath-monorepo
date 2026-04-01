import { ref, reactive } from 'vue';
import { useProblemApi, type Problem } from './useProblemApi';
import { TaskType } from '@/api/generated/api';
import { api } from '@/api';
import type { CreateProblemRequestDto, UpdateProblemRequestDto } from '@/api/generated/api';

export function useProblemManagement() {
  const { apiCallLoading, apiResponse } = useProblemApi();

  const problems = ref<Problem[]>([]);
  const loading = ref(false);
  const error = ref<any>(null);
  const attemptedLoad = ref(false);
  const problemTypesMap = ref<Record<string, TaskType[]>>({});

  function tryParseJson(jsonString: string, defaultValue: any = null) {
    if (!jsonString || jsonString.trim() === '') return defaultValue;
    try {
      return JSON.parse(jsonString);
    } catch (e) {
      console.warn("Failed to parse JSON, returning as text: ", jsonString, e);
      return jsonString;
    }
  }

  function stringToTaskType(typeStr: string): TaskType | null {
    const normalized = typeStr.trim().toLowerCase();
    switch (normalized) {
      case '0':
      case 'default':
        return TaskType.Default;
      case '1':
      case 'learning':
        return TaskType.Learning;
      case '2':
      case 'guided':
        return TaskType.Guided;
      case '3':
      case 'exam':
        return TaskType.Exam;
      default:
        return null;
    }
  }

  function taskTypeToString(type: TaskType): string {
    switch (type) {
      case TaskType.Default:
        return 'Default';
      case TaskType.Learning:
        return 'Learning';
      case TaskType.Guided:
        return 'Guided';
      case TaskType.Exam:
        return 'Exam';
      default:
        return String(type);
    }
  }

  async function fetchAllProblems() {
    loading.value = true;
    error.value = null;
    attemptedLoad.value = true;
    try {
      const response = await api.getApiProblems();
      const data = response.data as unknown as Problem[];
      problems.value = Array.isArray(data) ? data : [];

      const tempMap: Record<string, TaskType[]> = {};
      for (const problem of problems.value) {
        const problemId = problem.id || (problem as any)._id;
        if (problemId && problem.types && Array.isArray(problem.types)) {
          tempMap[problemId] = problem.types;
        }
      }
      problemTypesMap.value = tempMap;
    } catch (e: any) {
      console.error('Error fetching all problems:', e);
      error.value = e;
    } finally {
      loading.value = false;
    }
  }

  function getProblemAssignedTypes(problemId: string | undefined): string {
    if (!problemId) return '';
    const types = problemTypesMap.value[problemId];
    if (!types || types.length === 0) return '';
    return types.map(taskTypeToString).join(', ');
  }

  async function deleteProblemByIdAndRefresh(id: string | undefined) {
    if (!id) {
      console.warn("ID для удаления не предоставлен");
      apiResponse.deleteProblem = { error: true, message: "ID для удаления не предоставлен" };
      return;
    }
    try {
      apiCallLoading.deleteProblem = true;
      await api.deleteApiProblemsId(id);
      apiResponse.deleteProblem = { success: true };
      await fetchAllProblems();
    } catch (e: any) {
      console.error('Error deleting problem:', e);
      apiResponse.deleteProblem = { error: true, message: e.message };
    } finally {
      apiCallLoading.deleteProblem = false;
    }
  }

  async function createProblem(problemData: {
    title: string;
    statement: string;
    geolin_ans_key: { hash: string; seed: number };
    llmSolutionJson: string;
    theory_link?: string;
    type?: string;
  }) {
    apiResponse.managementAddProblem = null;
    try {
      const llmSolution = typeof problemData.llmSolutionJson === 'string'
        ? tryParseJson(problemData.llmSolutionJson, problemData.llmSolutionJson)
        : problemData.llmSolutionJson;

      const llmSolutionString = typeof llmSolution === 'string'
        ? llmSolution
        : (llmSolution ? JSON.stringify(llmSolution) : null);

      const types: TaskType[] = [];
      if (problemData.type && problemData.type.trim() !== '') {
        const taskType = stringToTaskType(problemData.type);
        if (taskType !== null) {
          types.push(taskType);
        }
      }

      const createPayload: CreateProblemRequestDto = {
        title: problemData.title || null,
        statement: problemData.statement || null,
        llmSolution: llmSolutionString || null,
        theoryLink: problemData.theory_link || null,
        geolinHash: problemData.geolin_ans_key?.hash || null,
        geolinSeed: problemData.geolin_ans_key?.seed ? Number(problemData.geolin_ans_key.seed) : null,
        types: types.length > 0 ? types : [],
      } as CreateProblemRequestDto;

      apiCallLoading.managementAddProblem = true;
      const response = await api.postApiProblems(createPayload);
      const createdProblem = response.data as unknown as Problem;

      if (createdProblem && (createdProblem.id || (createdProblem as any)._id)) {
        apiResponse.managementAddProblem = { success: true, createdProblem };
        await fetchAllProblems();
        return { success: true, createdProblem };
      } else {
        return { error: true, details: 'Unexpected response from server' };
      }
    } catch (e: any) {
      console.error("Ошибка при добавлении задачи:", e);
      apiResponse.managementAddProblem = { error: true, message: e.message || "Ошибка при добавлении задачи", details: e };
      return { error: true, details: e };
    } finally {
      apiCallLoading.managementAddProblem = false;
    }
  }

  async function updateProblem(problemId: string, problemData: {
    title: string;
    statement: string;
    geolin_ans_key: { hash: string; seed: number };
    llmSolutionJson: string;
    theory_link?: string;
    type?: string;
  }) {
    if (!problemId) {
      apiResponse.managementUpdateProblem = { error: true, message: "ID редактируемой задачи не найден." };
      return { error: true, message: "ID редактируемой задачи не найден." };
    }

    try {
      const llmSolution = typeof problemData.llmSolutionJson === 'string'
        ? tryParseJson(problemData.llmSolutionJson, problemData.llmSolutionJson)
        : problemData.llmSolutionJson;

      const llmSolutionString = typeof llmSolution === 'string'
        ? llmSolution
        : (llmSolution ? JSON.stringify(llmSolution) : null);

      const types: TaskType[] = [];
      if (problemData.type && problemData.type.trim() !== '') {
        const taskType = stringToTaskType(problemData.type);
        if (taskType !== null) {
          types.push(taskType);
        }
      }

      const updatePayload: UpdateProblemRequestDto = {
        title: problemData.title || null,
        statement: problemData.statement || null,
        llmSolution: llmSolutionString || null,
        theoryLink: problemData.theory_link || null,
        geolinHash: problemData.geolin_ans_key?.hash || null,
        geolinSeed: problemData.geolin_ans_key?.seed ? Number(problemData.geolin_ans_key.seed) : null,
        types: types.length > 0 ? types : [],
      } as UpdateProblemRequestDto;

      apiCallLoading.managementUpdateProblem = true;
      await api.putApiProblemsId(problemId, updatePayload);
      apiResponse.managementUpdateProblem = { success: true };
      await fetchAllProblems();
      return { success: true };
    } catch (e: any) {
      console.error("Ошибка при обновлении задачи:", e);
      apiResponse.managementUpdateProblem = { error: true, message: e.message || "Ошибка при обновлении задачи", details: e };
      return { error: true, details: e };
    } finally {
      apiCallLoading.managementUpdateProblem = false;
    }
  }

  return {
    problems,
    loading,
    error,
    attemptedLoad,
    problemTypesMap,
    apiCallLoading,
    apiResponse,
    fetchAllProblems,
    getProblemAssignedTypes,
    deleteProblemByIdAndRefresh,
    createProblem,
    updateProblem,
    tryParseJson,
    taskTypeToString,
  };
}
