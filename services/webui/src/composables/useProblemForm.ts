import { ref, reactive } from 'vue';
import type { Problem } from './useProblemApi';
import { TaskType } from '@/api/generated/api';

export function useProblemForm() {
  // Create form state
  const managementNewProblem = reactive<Omit<Problem, 'id' | 'result'>>({
    title: '',
    statement: '',
    geolinHash: '',
    geolinSeed: 0,
    solution: { steps: [] },
    llmSolution: '',
    theoryLink: '',
  });

  const managementNewProblemLlmSolutionJson = ref('');
  const managementNewProblemType = ref('0');

  // Edit form state
  const editingProblem = ref<Problem | null>(null);
  const currentEditProblem = reactive<Omit<Problem, 'id' | 'geolinHash' | 'geolinSeed' | 'result'>>({
    title: '',
    statement: '',
    solution: { steps: [] },
    llmSolution: '',
    theoryLink: '',
  });

  const currentEditProblemType = ref('0');
  const currentEditProblemLlmSolutionJson = ref('');

  function resetCreateForm() {
    managementNewProblem.title = '';
    managementNewProblem.statement = '';
    managementNewProblem.geolinHash = '';
    managementNewProblem.geolinSeed = 0;
    managementNewProblemLlmSolutionJson.value = '';
    managementNewProblem.llmSolution = '';
    managementNewProblem.theoryLink = '';
    managementNewProblemType.value = '0';
  }

  function resetEditForm() {
    editingProblem.value = null;
    currentEditProblem.title = '';
    currentEditProblem.statement = '';
    currentEditProblem.solution = { steps: [] };
    currentEditProblem.llmSolution = '';
    currentEditProblem.theoryLink = '';
    currentEditProblemType.value = '0';
    currentEditProblemLlmSolutionJson.value = '';
  }

  function populateEditForm(problem: Problem, assignedTypes: TaskType[] = []) {
    editingProblem.value = JSON.parse(JSON.stringify(problem)); // Deep copy
    if (editingProblem.value) {
      currentEditProblem.title = editingProblem.value.title || '';
      currentEditProblem.statement = editingProblem.value.statement;
      currentEditProblem.solution = { ...(editingProblem.value.solution || { steps: [] }) };
      currentEditProblem.llmSolution = editingProblem.value.llmSolution ?? '';

      currentEditProblem.theoryLink = editingProblem.value.theoryLink || '';

      currentEditProblemLlmSolutionJson.value = currentEditProblem.llmSolution
        ? (typeof currentEditProblem.llmSolution === 'string' ? currentEditProblem.llmSolution : JSON.stringify(currentEditProblem.llmSolution, null, 2))
        : '';

      // Convert first TaskType to string for the select dropdown, default to '0'
      currentEditProblemType.value = assignedTypes.length > 0 ? String(assignedTypes[0]) : '0';
    }
  }

  function populateFromGeolin(rawData: any) {
    console.log("Populating from Geolin data:", rawData);

    // Handle different response formats
    let data = rawData;

    // 1. If it's an array, take the first element
    if (Array.isArray(rawData) && rawData.length > 0) {
      data = rawData[0];
    }
    // 2. If it's wrapped in a "problems" array (ProblemsResponseDto)
    else if (rawData && rawData.problems && Array.isArray(rawData.problems) && rawData.problems.length > 0) {
      data = rawData.problems[0];
    }
    // 3. If it's wrapped in a "data" property
    else if (rawData && rawData.data && !Array.isArray(rawData)) {
      data = rawData.data;
    }

    if (!data) return;

    // Mapping fields with fallbacks for different naming conventions
    managementNewProblem.title = data.name || data.title || data.displayName || '';
    managementNewProblem.statement = data.condition || data.statement || data.description || '';

    managementNewProblem.geolinHash = data.hash || data.geolinHash || '';

    const seed = data.seed !== undefined ? data.seed : (data.geolinSeed !== undefined ? data.geolinSeed : undefined);
    managementNewProblem.geolinSeed = seed !== undefined && seed !== null ? Number(seed) : 0;

    // Clear solution fields
    managementNewProblem.solution = { steps: [] };
    managementNewProblemLlmSolutionJson.value = '';
    managementNewProblem.llmSolution = '';
  }

  return {
    // Create form
    managementNewProblem,
    managementNewProblemLlmSolutionJson,
    managementNewProblemType,

    // Edit form
    editingProblem,
    currentEditProblem,
    currentEditProblemType,
    currentEditProblemLlmSolutionJson,

    // Functions
    resetCreateForm,
    resetEditForm,
    populateEditForm,
    populateFromGeolin,
  };
}
