<template>
  <div class="video-app" :class="{ 'video-app--embedded': embedded }">
    <template v-if="embedded">
      <div v-if="selectedVideo" class="video-app__player">
        <div class="video-app__player-bar">
          <v-btn
            variant="text"
            prepend-icon="mdi-arrow-left"
            size="small"
            @click="selectedVideo = null"
          >
            К списку
          </v-btn>
          <span class="text-body-2 ml-2 text-medium-emphasis">{{ selectedVideo }}</span>
        </div>
        <VideoPlayer :filename="selectedVideo" class="video-app__player-content" />
      </div>
      <VideoList v-else @select="selectedVideo = $event" />
    </template>
    <template v-else>
      <VideoList v-if="!filename" @select="onSelect" />
      <VideoPlayer v-else :filename="filename" />
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import VideoList from './VideoList.vue'
import VideoPlayer from './VideoPlayer.vue'

const props = defineProps<{ filename?: string; embedded?: boolean }>()
const router = useRouter()
const selectedVideo = ref<string | null>(null)

function onSelect(name: string) {
  router.push({ name: 'video-player', params: { filename: name } })
}
</script>

<style scoped>
.video-app {
  width: 100%;
  height: 100vh;
  overflow: hidden;
}
.video-app--embedded {
  height: 100%;
  display: flex;
  flex-direction: column;
}
.video-app__player {
  display: flex;
  flex-direction: column;
  height: 100%;
}
.video-app__player-bar {
  display: flex;
  align-items: center;
  padding: 4px 8px;
  border-bottom: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
  flex-shrink: 0;
}
.video-app__player-content {
  flex: 1;
  min-height: 0;
}
</style>
