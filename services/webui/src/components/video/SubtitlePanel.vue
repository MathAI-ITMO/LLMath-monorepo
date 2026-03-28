<template>
  <div class="subtitle-panel">
    <v-list density="compact" class="subtitle-list">
      <v-list-item
        v-for="(seg, idx) in subtitles"
        :key="idx"
        :ref="el => { if (el) itemRefs[idx] = el as any }"
        :class="{ 'active-subtitle': idx === activeIndex }"
        class="subtitle-item"
        @click="$emit('seek', seg.start)"
      >
        <template #prepend>
          <span class="subtitle-time">{{ formatTime(seg.start) }}</span>
        </template>
        <v-list-item-title class="subtitle-text">{{ seg.text }}</v-list-item-title>
      </v-list-item>
    </v-list>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import type { SubtitleSegment } from '@/types/video'

const props = defineProps<{
  subtitles: SubtitleSegment[]
  activeIndex: number
}>()
defineEmits<{ seek: [t: number] }>()

const itemRefs = ref<Record<number, any>>({})

watch(() => props.activeIndex, (idx) => {
  if (idx >= 0) {
    const el = itemRefs.value[idx]
    const native = el?.$el ?? el
    native?.scrollIntoView({ block: 'nearest', behavior: 'smooth' })
  }
})

function formatTime(secs: number): string {
  const s = Math.floor(secs % 60).toString().padStart(2, '0')
  const m = Math.floor((secs / 60) % 60).toString().padStart(2, '0')
  return `${m}:${s}`
}
</script>

<style scoped>
.subtitle-panel {
  height: 100%;
  overflow-y: auto;
}
.subtitle-list {
  background: transparent;
}
.subtitle-item {
  cursor: pointer;
  border-radius: 4px;
}
.active-subtitle {
  background: rgba(25, 118, 210, 0.15);
}
.subtitle-time {
  font-size: 11px;
  color: #888;
  min-width: 36px;
  margin-right: 8px;
}
.subtitle-text {
  font-size: 13px;
  white-space: normal;
}
</style>
