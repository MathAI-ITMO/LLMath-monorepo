import { ref, computed } from 'vue'
import type { Ref } from 'vue'
import { videoApi } from '@/api/video'
import type { SubtitleSegment } from '@/types/video'

export function useSubtitles(currentTime: Ref<number>) {
  const subtitles = ref<SubtitleSegment[]>([])

  const activeIndex = computed(() => {
    const t = currentTime.value
    return subtitles.value.findIndex(s => t >= s.start && t <= s.end)
  })

  async function fetchSubtitles(name: string) {
    subtitles.value = []
    try {
      const r = await videoApi.get<{ segments: SubtitleSegment[] }>(
        `/subtitles/${encodeURIComponent(name)}.json`
      )
      subtitles.value = r.data?.segments ?? []
    } catch {
      subtitles.value = []
    }
  }

  async function ensureProcessed(name: string) {
    try {
      await videoApi.post('/api/ensure_processed', { name })
    } catch {}
  }

  async function pollSubtitles(name: string, maxAttempts = 20) {
    await ensureProcessed(name)
    for (let i = 0; i < maxAttempts; i++) {
      await fetchSubtitles(name)
      if (subtitles.value.length > 0) return
      await new Promise(r => setTimeout(r, 3000))
    }
  }

  return { subtitles, activeIndex, fetchSubtitles, ensureProcessed, pollSubtitles }
}
