<template>
  <div class="chat-panel">
    <div ref="messagesEl" class="chat-messages">
      <div
        v-for="(msg, idx) in messages"
        :key="idx"
        :class="['chat-msg', msg.role === 'student' ? 'msg-student' : 'msg-lecturer']"
      >
        <div class="msg-bubble" v-html="renderMessage(msg.text)" />
      </div>
      <div v-if="isBusy" class="chat-msg msg-lecturer">
        <div class="msg-bubble loader-dots">
          <span /><span /><span />
        </div>
      </div>
    </div>

    <VideoSuggestions :suggestions="activeSuggestions" @select="onSuggestion" />

    <div class="chat-input-row">
      <v-text-field
        v-model="inputText"
        density="compact"
        hide-details
        variant="outlined"
        placeholder="Задайте вопрос по лекции…"
        :disabled="isBusy"
        @keydown.enter.prevent="sendMessage()"
      />
      <v-btn
        icon="mdi-send"
        size="small"
        color="primary"
        :disabled="isBusy || !inputText.trim()"
        @click="sendMessage()"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, nextTick, toRef, computed } from 'vue'
import { renderMessage } from '@/utils/renderMessage'
import { useVideoChat } from '@/composables/video/useVideoChat'
import { useVideoSuggestions } from '@/composables/video/useVideoSuggestions'
import VideoSuggestions from './VideoSuggestions.vue'

const props = defineProps<{
  videoName: string
  currentTime: number
}>()

const videoNameRef = toRef(props, 'videoName')
const currentTimeRef = toRef(props, 'currentTime')

const { messages, isBusy, inputText, sendMessage, explainFrame } = useVideoChat(videoNameRef, currentTimeRef)
const { activeSuggestions, fetchSuggestions } = useVideoSuggestions(currentTimeRef)

fetchSuggestions(props.videoName)

const messagesEl = ref<HTMLElement | null>(null)

watch(messages, async () => {
  await nextTick()
  if (messagesEl.value) messagesEl.value.scrollTop = messagesEl.value.scrollHeight
}, { deep: true })

function onSuggestion(text: string) {
  sendMessage(text)
}

defineExpose({ explainFrame })
</script>

<style scoped>
.chat-panel {
  display: flex;
  flex-direction: column;
  height: 100%;
}
.chat-messages {
  flex: 1;
  overflow-y: auto;
  padding: 10px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}
.chat-msg {
  display: flex;
}
.msg-student {
  justify-content: flex-end;
}
.msg-lecturer {
  justify-content: flex-start;
}
.msg-bubble {
  max-width: 85%;
  padding: 8px 12px;
  border-radius: 12px;
  font-size: 13px;
  line-height: 1.5;
  white-space: pre-wrap;
  word-break: break-word;
}
.msg-student .msg-bubble {
  background: #1565c0;
  color: #fff;
  border-bottom-right-radius: 4px;
}
.msg-lecturer .msg-bubble {
  background: #2d2d2d;
  color: #e0e0e0;
  border-bottom-left-radius: 4px;
}
.loader-dots {
  display: flex;
  align-items: center;
  gap: 5px;
  padding: 10px 14px;
}
.loader-dots span {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: #888;
  animation: bounce 1.2s infinite;
}
.loader-dots span:nth-child(2) { animation-delay: 0.2s; }
.loader-dots span:nth-child(3) { animation-delay: 0.4s; }
@keyframes bounce {
  0%, 80%, 100% { transform: scale(0.8); opacity: 0.5; }
  40% { transform: scale(1.2); opacity: 1; }
}
.chat-input-row {
  display: flex;
  gap: 6px;
  padding: 8px;
  align-items: center;
  border-top: 1px solid #333;
}
</style>
