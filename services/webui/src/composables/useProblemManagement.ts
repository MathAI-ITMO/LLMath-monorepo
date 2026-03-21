import { ref, reactive } from 'vue';
import { useProblemApi, type Problem, LLMATH_PROBLEMS_API_URL } from './useProblemApi';
import { TaskType, type CreateProblemRequestDto, type UpdateProblemRequestDto } from '@/types/BackendDtos';

export function useProblemManagement() {
  const { makeApiCall, apiCallLoading, apiResponse } = useProblemApi();

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

  // Helper function to convert string type to TaskType enum
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

  // Helper function to convert TaskType enum to display string
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
      const data = await makeApiCall('/api/Problems', 'GET');
      if (data && data.error) {
        throw new Error(data.message || 'Error fetching problems');
      }
      problems.value = (Array.isArray(data) ? data : []) || [];
      
      // Populate types map from problems
      const tempMap: Record<string, TaskType[]> = {};
      for (const problem of problems.value) {
        const problemId = problem.id || (problem as any)._id; // Support legacy _id field
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
    const result = await makeApiCall(`/api/Problems/${id}`, 'DELETE', undefined, 'deleteProblem', 'deleteProblem');
    if (result && !result.error) {
      await fetchAllProblems();
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
        ? (tryParseJson(problemData.llmSolutionJson, problemData.llmSolutionJson))
        : problemData.llmSolutionJson;
      
      // Convert llmSolution to string if it's an object
      const llmSolutionString = typeof llmSolution === 'string' 
        ? llmSolution 
        : (llmSolution ? JSON.stringify(llmSolution) : null);

      // Convert type string to TaskType array
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
        types: types.length > 0 ? types : null,
      };

      const createdProblemResponse = await makeApiCall('/api/Problems', 'POST', createPayload, 'managementAddProblem', 'managementAddProblem');

      if (createdProblemResponse && !createdProblemResponse.error && (createdProblemResponse.id || (createdProblemResponse as any)._id)) {
        apiResponse.managementAddProblem = { success: true, createdProblem: createdProblemResponse };
        await fetchAllProblems();
        return { success: true, createdProblem: createdProblemResponse };
      } else {
        console.error("Ошибка при создании задачи (management tab):", createdProblemResponse?.details);
        return { error: true, details: createdProblemResponse?.details };
      }
    } catch (e: any) {
      console.error("Ошибка при добавлении задачи (management tab):", e);
      apiResponse.managementAddProblem = { error: true, message: e.message || "Ошибка при добавлении задачи", details: e };
      return { error: true, details: e };
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
        ? (tryParseJson(problemData.llmSolutionJson, problemData.llmSolutionJson))
        : problemData.llmSolutionJson;
      
      // Convert llmSolution to string if it's an object
      const llmSolutionString = typeof llmSolution === 'string' 
        ? llmSolution 
        : (llmSolution ? JSON.stringify(llmSolution) : null);

      // Convert type string to TaskType array
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
        types: types.length > 0 ? types : null,
      };

      const updateResponse = await makeApiCall(`/api/Problems/${problemId}`, 'PUT', updatePayload, 'managementUpdateProblem', 'managementUpdateProblem');

      if (updateResponse && !updateResponse.error) {
        await fetchAllProblems();
        return { success: true };
      } else {
        return { error: true, details: updateResponse?.details };
      }
    } catch (e: any) {
      console.error("Ошибка при обновлении задачи (management tab):", e);
      apiResponse.managementUpdateProblem = { error: true, message: e.message || "Ошибка при обновлении задачи", details: e };
      return { error: true, details: e };
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
    makeApiCall,
    tryParseJson,
    taskTypeToString,
  };
}
