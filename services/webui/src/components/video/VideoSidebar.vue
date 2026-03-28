<template>
  <div class="sidebar">
    <v-tabs v-model="activeTab" density="compact" class="sidebar-tabs">
      <v-tab value="chat">Чат</v-tab>
      <v-tab value="about">О лекции</v-tab>
    </v-tabs>
    <div class="sidebar-content">
      <VideoChat
        v-if="activeTab === 'chat'"
        ref="chatRef"
        :video-name="videoName"
        :current-time="currentTime"
      />
      <VideoSummary
        v-else-if="activeTab === 'about'"
        :video-name="videoName"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import VideoChat from './VideoChat.vue'
import VideoSummary from './VideoSummary.vue'

defineProps<{ videoName: string; currentTime: number }>()

const activeTab = ref<'chat' | 'about'>('chat')
const chatRef = ref<InstanceType<typeof VideoChat> | null>(null)

function explainFrame(normX: number, normY: number, dataUrl: string) {
  activeTab.value = 'chat'
  chatRef.value?.explainFrame(normX, normY, dataUrl)
}

defineExpose({ explainFrame })
</script>

<style scoped>
.sidebar {
  display: flex;
  flex-direction: column;
  height: 100%;
  background: #1a1a1a;
  border-left: 1px solid #333;
}
.sidebar-tabs {
  flex-shrink: 0;
  border-bottom: 1px solid #333;
}
.sidebar-content {
  flex: 1;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}
</style>
