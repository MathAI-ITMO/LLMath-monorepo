<template>
  <div class="summary-panel">
    <v-skeleton-loader v-if="loading" type="paragraph" />
    <div
      v-else-if="renderedSummary"
      class="summary-content"
      v-html="renderedSummary"
    />
    <p v-else class="text-grey text-center mt-4">Саммари ещё не готово…</p>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { renderMessage } from '@/utils/renderMessage'
import { useVideoSummary } from '@/composables/video/useVideoSummary'

const props = defineProps<{ videoName: string }>()
const { summary, loading, fetchSummary } = useVideoSummary()

fetchSummary(props.videoName)

const renderedSummary = computed(() => summary.value ? renderMessage(summary.value) : '')
</script>

<style scoped>
.summary-panel {
  padding: 12px;
  overflow-y: auto;
  height: 100%;
}
.summary-content {
  font-size: 14px;
  line-height: 1.6;
  color: #e0e0e0;
}
</style>
