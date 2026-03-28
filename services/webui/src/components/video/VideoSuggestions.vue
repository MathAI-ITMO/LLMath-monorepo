<template>
  <div v-if="suggestions.length > 0" class="suggestions-row">
    <v-chip
      v-for="s in suggestions"
      :key="s.key"
      size="small"
      variant="tonal"
      color="primary"
      class="suggestion-chip"
      @click="$emit('select', s.text)"
    >
      {{ s.text }}
    </v-chip>
  </div>
</template>

<script setup lang="ts">
import type { SuggestionItem } from '@/types/video'

defineProps<{ suggestions: SuggestionItem[] }>()
defineEmits<{ select: [text: string] }>()
</script>

<style scoped>
.suggestions-row {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  padding: 6px 8px;
}
.suggestion-chip {
  cursor: pointer;
  max-width: 220px;
  white-space: normal;
  height: auto;
  padding: 4px 10px;
}
</style>
