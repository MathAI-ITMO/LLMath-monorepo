<template>
  <div class="video-page">
    <!-- Player section -->
    <div class="player-section">
      <div class="video-wrapper">
        <video
          ref="videoEl"
          :src="videoSrc"
          class="video-el"
          playsinline
          @click="onVideoClick"
        />
        <!-- Explain tooltip -->
        <div
          v-if="tooltipVisible"
          class="explain-tooltip"
          :style="{ left: tooltipStyle.left, top: tooltipStyle.top }"
          @click.stop
        >
          <span class="explain-label">Анализ кадра</span>
          <v-btn size="x-small" color="primary" @click="onExplainClick">Поясни</v-btn>
        </div>
      </div>

      <VideoControls
        :playing="playing"
        :current-time="currentTime"
        :duration="duration"
        :volume="volume"
        :speed="speed"
        @toggle-play="togglePlay"
        @seek="seek"
        @volume="setVolume"
        @speed="setSpeed"
      />
    </div>

    <!-- Sidebar -->
    <VideoSidebar
      ref="sidebarRef"
      :video-name="filename"
      :current-time="currentTime"
      class="sidebar-section"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { videoApi } from '@/api/video'
import { useVideoPlayer } from '@/composables/video/useVideoPlayer'
import VideoControls from './VideoControls.vue'
import VideoSidebar from './VideoSidebar.vue'

const props = defineProps<{ filename: string }>()

const videoEl = ref<HTMLVideoElement | null>(null)
const sidebarRef = ref<InstanceType<typeof VideoSidebar> | null>(null)

const {
  currentTime, duration, playing, volume, speed,
  togglePlay, seek, setVolume, setSpeed, captureFrameWithMarker,
} = useVideoPlayer(videoEl)

const videoSrc = computed(
  () => `${videoApi.defaults.baseURL}/video/${encodeURIComponent(props.filename)}`
)

// Explain tooltip state
const tooltipVisible = ref(false)
const tooltipStyle = ref({ left: '0px', top: '0px' })
const lastClickRel = ref({ x: 0.5, y: 0.5 })
let tooltipTimeout: ReturnType<typeof setTimeout> | null = null

function getVideoBounds(el: HTMLVideoElement): { left: number; top: number; width: number; height: number } {
  const rect = el.getBoundingClientRect()
  const elW = rect.width
  const elH = rect.height
  const vidW = el.videoWidth || elW
  const vidH = el.videoHeight || elH
  const scale = Math.min(elW / vidW, elH / vidH)
  const renderW = vidW * scale
  const renderH = vidH * scale
  return {
    left: rect.left + (elW - renderW) / 2,
    top: rect.top + (elH - renderH) / 2,
    width: renderW,
    height: renderH,
  }
}

function onVideoClick(e: MouseEvent) {
  const el = videoEl.value
  if (!el) return
  const bounds = getVideoBounds(el)
  const cx = e.clientX
  const cy = e.clientY

  // Ignore clicks in the letterbox area
  if (cx < bounds.left || cx > bounds.left + bounds.width ||
      cy < bounds.top  || cy > bounds.top  + bounds.height) return

  const x = (cx - bounds.left) / bounds.width
  const y = (cy - bounds.top)  / bounds.height
  lastClickRel.value = { x, y }

  // Position tooltip relative to the video element's top-left corner
  const elRect = el.getBoundingClientRect()
  const tipW = 140, tipH = 60
  let left = cx - elRect.left - tipW / 2
  let top  = cy - elRect.top  - tipH - 10
  left = Math.max(0, Math.min(left, elRect.width  - tipW))
  top  = Math.max(0, Math.min(top,  elRect.height - tipH))
  tooltipStyle.value = { left: `${left}px`, top: `${top}px` }
  tooltipVisible.value = true

  if (tooltipTimeout) clearTimeout(tooltipTimeout)
  tooltipTimeout = setTimeout(() => { tooltipVisible.value = false }, 5000)
}

function onExplainClick() {
  tooltipVisible.value = false
  if (tooltipTimeout) clearTimeout(tooltipTimeout)
  const dataUrl = captureFrameWithMarker(lastClickRel.value.x, lastClickRel.value.y)
  sidebarRef.value?.explainFrame(lastClickRel.value.x, lastClickRel.value.y, dataUrl)
}
</script>

<style scoped>
.video-page {
  display: flex;
  height: 100%;
  background: #121212;
  color: #fff;
}
.player-section {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-width: 0;
}
.video-wrapper {
  flex: 1;
  position: relative;
  background: #000;
  cursor: crosshair;
  overflow: hidden;
}
.video-el {
  width: 100%;
  height: 100%;
  object-fit: contain;
  display: block;
}
.sidebar-section {
  width: 28%;
  min-width: 260px;
  max-width: 380px;
  flex-shrink: 0;
}
.explain-tooltip {
  position: absolute;
  background: rgba(30, 30, 30, 0.9);
  border: 1px solid rgba(255,255,255,0.2);
  border-radius: 8px;
  padding: 6px 10px;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
  z-index: 20;
  pointer-events: all;
  backdrop-filter: blur(6px);
}
.explain-label {
  font-size: 11px;
  color: #ccc;
}
</style>
