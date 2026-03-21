import { ref } from 'vue'
import { api } from '@/api'

export function useLlmSolution() {
  const loading = ref(false)
  const activeForm = ref<'management' | 'database' | 'update' | 'edit' | null>(null)

  async function getLlmSolution(problemStatement: string) {
    if (!problemStatement) {
      throw new Error('Поле "Условие" не может быть пустым для получения решения LLM')
    }

    loading.value = true

    try {
      const response = await api.postApiV1LlmSolveProblem({ problemDescription: problemStatement })
      const data = response.data as unknown as { solution?: string; error?: string }

      if (data?.error) {
        throw new Error(data.error)
      }

      if (!data.solution) throw new Error('Бэкенд не вернул решение')
      return data.solution
    } catch (error: any) {
      console.error('Ошибка при получении решения от LLM:', error)
      const data = error.response?.data
      const errorMessage = data?.error || data?.message || data || error.message || error
      throw new Error(errorMessage)
    } finally {
      loading.value = false
    }
  }

  return {
    loading,
    activeForm,
    getLlmSolution,
  }
}
