import { ref, computed } from 'vue'
import type { Ref } from 'vue'
import { videoApi } from '@/api/video'
import type { SuggestionItem } from '@/types/video'

function parseHHMMSS(s: string): number {
  const m = String(s).trim().match(/^(\d{1,2}):(\d{2}):(\d{2})$/)
  if (!m) return NaN
  return +m[1] * 3600 + +m[2] * 60 + +m[3]
}

export function useVideoSuggestions(currentTime: Ref<number>) {
  const allSuggestions = ref<SuggestionItem[]>([])

  const activeSuggestions = computed(() => {
    const t = currentTime.value
    return allSuggestions.value
      .filter(s => t >= s.startSec && t <= s.endSec)
      .slice(0, 6)
  })

  async function fetchSuggestions(name: string) {
    allSuggestions.value = []
    try {
      const r = await videoApi.get<{ items: { text: string; start: string; end: string }[] }>(
        `/suggestions/${encodeURIComponent(name)}`
      )
      const items = r.data?.items ?? []
      allSuggestions.value = items
        .map((it, idx) => ({
          key: `${idx}-${it.start}-${it.end}`,
          text: String(it.text || '').trim(),
          startSec: parseHHMMSS(it.start),
          endSec: parseHHMMSS(it.end),
        }))
        .filter(it => it.text && Number.isFinite(it.startSec) && Number.isFinite(it.endSec) && it.endSec > it.startSec)
    } catch {
      allSuggestions.value = []
    }
  }

  return { activeSuggestions, fetchSuggestions }
}
