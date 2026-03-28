<template>
  <div class="video-controls">
    <v-btn
      :icon="playing ? 'mdi-pause' : 'mdi-play'"
      variant="text"
      size="small"
      @click="$emit('toggle-play')"
    />
    <span class="time-display">{{ formatTime(currentTime) }} / {{ formatTime(duration) }}</span>
    <v-slider
      :model-value="currentTime"
      :max="duration || 1"
      :min="0"
      step="0.1"
      hide-details
      density="compact"
      class="progress-slider"
      @update:model-value="$emit('seek', $event)"
    />
    <v-icon size="18" class="ml-2">mdi-volume-high</v-icon>
    <v-slider
      :model-value="volume"
      :max="1"
      :min="0"
      step="0.01"
      hide-details
      density="compact"
      class="volume-slider"
      @update:model-value="$emit('volume', $event)"
    />
    <v-select
      :model-value="speed"
      :items="speedOptions"
      density="compact"
      hide-details
      variant="outlined"
      class="speed-select"
      @update:model-value="$emit('speed', $event)"
    />
  </div>
</template>

<script setup lang="ts">
defineProps<{
  playing: boolean
  currentTime: number
  duration: number
  volume: number
  speed: number
}>()

defineEmits<{
  'toggle-play': []
  seek: [t: number]
  volume: [v: number]
  speed: [r: number]
}>()

const speedOptions = [
  { title: '0.5×', value: 0.5 },
  { title: '0.75×', value: 0.75 },
  { title: '1×', value: 1 },
  { title: '1.25×', value: 1.25 },
  { title: '1.5×', value: 1.5 },
  { title: '2×', value: 2 },
]

function formatTime(secs: number): string {
  if (!secs || !Number.isFinite(secs)) return '0:00'
  const s = Math.floor(secs % 60).toString().padStart(2, '0')
  const m = Math.floor((secs / 60) % 60).toString().padStart(2, '0')
  const h = Math.floor(secs / 3600)
  return h > 0 ? `${h}:${m}:${s}` : `${m}:${s}`
}
</script>

<style scoped>
.video-controls {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 4px 8px;
  background: #1e1e1e;
}
.time-display {
  font-size: 12px;
  color: #ccc;
  white-space: nowrap;
  min-width: 90px;
}
.progress-slider {
  flex: 1;
  min-width: 60px;
}
.volume-slider {
  width: 80px;
  flex-shrink: 0;
}
.speed-select {
  width: 72px;
  flex-shrink: 0;
  font-size: 12px;
}
</style>
