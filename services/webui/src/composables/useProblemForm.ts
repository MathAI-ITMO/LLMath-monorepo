import { ref, reactive } from 'vue';
import type { Problem } from './useProblemApi';
import { TaskType } from '@/api/generated/api';

export function useProblemForm() {
  // Create form state
  const managementNewProblem = reactive<Omit<Problem, 'id' | 'result'>>({
    title: '',
    statement: '',
    geolin_ans_key: { hash: '', seed: 0 }, // Legacy format for form, converted to geolinHash/geolinSeed in API call
    solution: { steps: [] },
    llm_solution: null, // Legacy format for form, converted to llmSolution string in API call
    theory_link: '', // Legacy format for form, converted to theoryLink in API call
  });

  const managementNewProblemLlmSolutionJson = ref('');
  const managementNewProblemType = ref('0');

  // Edit form state
  const editingProblem = ref<Problem | null>(null);
  const currentEditProblem = reactive<Omit<Problem, 'id' | 'geolin_ans_key' | 'geolinHash' | 'geolinSeed' | 'result'>>({
    title: '',
    statement: '',
    solution: { steps: [] },
    llm_solution: null, // Legacy format for form, converted to llmSolution string in API call
    theory_link: '', // Legacy format for form, converted to theoryLink in API call
  });

  const currentEditProblemType = ref('0');
  const currentEditProblemLlmSolutionJson = ref('');

  function resetCreateForm() {
    managementNewProblem.title = '';
    managementNewProblem.statement = '';
    managementNewProblem.geolin_ans_key = { hash: '', seed: 0 };
    managementNewProblemLlmSolutionJson.value = '';
    managementNewProblem.llm_solution = null;
    managementNewProblem.theory_link = '';
    managementNewProblemType.value = '0';
  }

  function resetEditForm() {
    editingProblem.value = null;
    currentEditProblem.title = '';
    currentEditProblem.statement = '';
    currentEditProblem.solution = { steps: [] };
    currentEditProblem.llm_solution = null;
    currentEditProblem.theory_link = '';
    currentEditProblemType.value = '0';
    currentEditProblemLlmSolutionJson.value = '';
  }

  function populateEditForm(problem: Problem, assignedTypes: TaskType[] = []) {
    editingProblem.value = JSON.parse(JSON.stringify(problem)); // Deep copy
    if (editingProblem.value) {
      currentEditProblem.title = editingProblem.value.title || '';
      currentEditProblem.statement = editingProblem.value.statement;
      currentEditProblem.solution = { ...(editingProblem.value.solution || { steps: [] }) };
      
      // Handle llm_solution - could be from llmSolution or llm_solution (legacy)
      const llmSolution = editingProblem.value.llmSolution || editingProblem.value.llm_solution;
      currentEditProblem.llm_solution = llmSolution !== undefined ? llmSolution : null;
      
      // Handle theory_link - could be from theoryLink or theory_link (legacy)
      currentEditProblem.theory_link = editingProblem.value.theoryLink || editingProblem.value.theory_link || '';

      currentEditProblemLlmSolutionJson.value = currentEditProblem.llm_solution
        ? (typeof currentEditProblem.llm_solution === 'string' ? currentEditProblem.llm_solution : JSON.stringify(currentEditProblem.llm_solution, null, 2))
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
    
    // Hash handling
    const hash = data.hash || data.geolinHash || (data.geolin_ans_key?.hash);
    if (managementNewProblem.geolin_ans_key) {
      managementNewProblem.geolin_ans_key.hash = hash || '';
    }

    // Seed handling
    const seed = data.seed !== undefined ? data.seed : (data.geolinSeed !== undefined ? data.geolinSeed : (data.geolin_ans_key?.seed));
    if (managementNewProblem.geolin_ans_key) {
      if (seed !== undefined && seed !== null) {
        managementNewProblem.geolin_ans_key.seed = Number(seed);
      } else {
        managementNewProblem.geolin_ans_key.seed = 0;
      }
    }

    // Clear solution fields
    managementNewProblem.solution = { steps: [] };
    managementNewProblemLlmSolutionJson.value = '';
    managementNewProblem.llm_solution = null;
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