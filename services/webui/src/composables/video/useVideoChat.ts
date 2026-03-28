import { ref } from 'vue'
import type { Ref } from 'vue'
import { videoApi } from '@/api/video'
import type { ChatMessage } from '@/types/video'

interface DialogEntry {
  role: 'student' | 'lecturer'
  text: string
}

export function useVideoChat(videoName: Ref<string | null>, currentTime: Ref<number>) {
  const messages = ref<ChatMessage[]>([])
  const isBusy = ref(false)
  const inputText = ref('')

  const dialog: DialogEntry[] = []

  async function sendMessage(text?: string) {
    const question = (text ?? inputText.value).trim()
    if (!question || isBusy.value || !videoName.value) return
    inputText.value = ''
    messages.value.push({ role: 'student', text: question })
    dialog.push({ role: 'student', text: question })
    isBusy.value = true
    try {
      const r = await videoApi.post<{ answer: string }>('/api/chat', {
        name: videoName.value,
        time: currentTime.value,
        dialog: dialog.filter(d => d.role !== 'student' || true),
        question,
      })
      const answer = r.data?.answer ?? ''
      messages.value.push({ role: 'lecturer', text: answer })
      dialog.push({ role: 'lecturer', text: answer })
    } catch {
      messages.value.push({ role: 'lecturer', text: 'Ошибка при отправке запроса.' })
    } finally {
      isBusy.value = false
    }
  }

  async function explainFrame(normX: number, normY: number, dataUrl: string) {
    if (isBusy.value || !videoName.value) return
    isBusy.value = true
    messages.value.push({ role: 'student', text: 'Поясни этот кадр', kind: 'frame', normX, normY })
    try {
      const r = await videoApi.post<{ answer: string }>('/api/explain_frame', {
        name: videoName.value,
        currentTime: currentTime.value,
        image: dataUrl,
        norm_x: normX,
        norm_y: normY,
      })
      const explanation = r.data?.answer ?? ''
      messages.value.push({ role: 'lecturer', text: explanation, kind: 'frame' })
    } catch {
      messages.value.push({ role: 'lecturer', text: 'Не удалось получить пояснение кадра.' })
    } finally {
      isBusy.value = false
    }
  }

  return { messages, isBusy, inputText, sendMessage, explainFrame }
}
