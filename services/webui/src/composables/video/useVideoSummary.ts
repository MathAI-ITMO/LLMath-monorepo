import { ref } from 'vue'
import { videoApi } from '@/api/video'

export function useVideoSummary() {
  const summary = ref('')
  const loading = ref(false)

  async function fetchSummary(name: string, maxAttempts = 6) {
    summary.value = ''
    loading.value = true
    try {
      for (let i = 0; i < maxAttempts; i++) {
        try {
          const r = await videoApi.get<{ text: string }>(`/summary/${encodeURIComponent(name)}`)
          const text = r.data?.text?.trim() ?? ''
          if (text) {
            summary.value = text
            return
          }
        } catch {}
        if (i < maxAttempts - 1) await new Promise(r => setTimeout(r, 3000))
      }
    } finally {
      loading.value = false
    }
  }

  return { summary, loading, fetchSummary }
}
