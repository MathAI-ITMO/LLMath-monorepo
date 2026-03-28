import { ref, computed } from 'vue'
import { videoApi } from '@/api/video'
import type { VideoInfo } from '@/types/video'

export type ProcessingStatus = 'pending' | 'extract' | 'transcribe' | 'summarize' | 'done' | 'error'

function parseStatusFromLogs(entries: { type: string; content: string }[]): ProcessingStatus {
  if (!entries.length) return 'pending'
  let status: ProcessingStatus = 'pending'
  for (const e of entries) {
    const c = e.content
    if (
      c.startsWith('extract_audio_error') || c.startsWith('transcribe_error') ||
      c.startsWith('summarize_error') || c.startsWith('transcribe_empty')
    ) {
      status = 'error'
    } else if (c.startsWith('extract_audio') && !c.includes('done') && !c.includes('skip') && !c.includes('error')) {
      status = 'extract'
    } else if (c.startsWith('transcribe_start')) {
      status = 'transcribe'
    } else if (c.startsWith('summarize_start') || c.startsWith('suggestions_start')) {
      status = 'summarize'
    } else if (
      c.startsWith('summarize_done') || c.startsWith('suggestions_done') ||
      c.startsWith('worker_done') || c.startsWith('worker_finish')
    ) {
      status = 'done'
    }
  }
  return status
}

export type LogEntry = { type: string; content: string }

export function useVideoList() {
  const videos = ref<VideoInfo[]>([])
  const loading = ref(false)
  const filter = ref('')
  const processingStatus = ref<Record<string, ProcessingStatus>>({})
  const videoLogs = ref<Record<string, LogEntry[]>>({})

  const filteredVideos = computed(() =>
    filter.value
      ? videos.value.filter(v =>
          v.name.toLowerCase().includes(filter.value.toLowerCase())
        )
      : videos.value
  )

  async function fetchStatuses(names: string[]) {
    await Promise.all(names.map(async name => {
      try {
        const r = await videoApi.get<{ entries: LogEntry[] }>(
          `/logs/${encodeURIComponent(name)}`
        )
        const entries = r.data?.entries ?? []
        videoLogs.value[name] = entries
        processingStatus.value[name] = parseStatusFromLogs(entries)
      } catch {
        // leave status unchanged if logs unavailable
      }
    }))
  }

  async function fetchVideos() {
    loading.value = true
    try {
      const response = await videoApi.get<VideoInfo[]>('/videos')
      videos.value = response.data || []
      await fetchStatuses(videos.value.map(v => v.name))
    } catch (e) {
      console.error('Failed to fetch videos', e)
      videos.value = []
    } finally {
      loading.value = false
    }
  }

  async function deleteVideo(name: string) {
    await videoApi.delete(`/video/${encodeURIComponent(name)}`)
    delete processingStatus.value[name]
    delete videoLogs.value[name]
    await fetchVideos()
  }

  async function pollProcessingStatus(name: string) {
    let attempts = 0
    const maxAttempts = 60
    while (attempts < maxAttempts) {
      await new Promise(r => setTimeout(r, 3000))
      attempts++
      try {
        const r = await videoApi.get<{ entries: LogEntry[] }>(
          `/logs/${encodeURIComponent(name)}`
        )
        const entries = r.data?.entries ?? []
        videoLogs.value[name] = entries
        const status = parseStatusFromLogs(entries)
        processingStatus.value[name] = status
        if (status === 'done' || status === 'error') {
          await fetchVideos()
          return
        }
      } catch {
        // ignore, keep polling
      }
    }
  }

  async function uploadVideo(name: string, file: File): Promise<VideoInfo> {
    const formData = new FormData()
    formData.append('file', file)
    processingStatus.value[name] = 'pending'
    const response = await videoApi.post<VideoInfo>('/upload', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
    const uploaded = response.data
    processingStatus.value[uploaded.name] = 'extract'
    pollProcessingStatus(uploaded.name)
    await fetchVideos()
    return uploaded
  }

  return { videos, loading, filter, filteredVideos, processingStatus, videoLogs, fetchVideos, deleteVideo, uploadVideo }
}
