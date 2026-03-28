<template>
  <v-list-item
    :title="video.name"
    class="video-list-item"
    @click="$emit('select', video.name)"
  >
    <template #append>
      <v-btn
        icon="mdi-delete"
        size="small"
        variant="text"
        color="error"
        @click.stop="$emit('delete', video.name)"
      />
    </template>
  </v-list-item>
</template>

<script setup lang="ts">
import type { VideoInfo } from '@/types/video'

defineProps<{ video: VideoInfo }>()
defineEmits<{ select: [name: string]; delete: [name: string] }>()
</script>
