<template>
  <div
    class="upload-zone"
    :class="{ 'dragging': dragging, 'uploading': uploading }"
    @dragover.prevent="dragging = true"
    @dragleave="dragging = false"
    @drop.prevent="onDrop"
    @click="!uploading && fileInput?.click()"
  >
    <template v-if="uploading">
      <v-progress-circular indeterminate color="primary" size="32" class="mb-2" />
      <p class="text-body-2 text-medium-emphasis">Загрузка видео…</p>
    </template>
    <template v-else>
      <v-icon size="36" color="primary" class="mb-2">mdi-cloud-upload</v-icon>
      <p class="text-body-2">
        Перетащите видео сюда или
        <span class="upload-link">выберите файл</span>
      </p>
      <p class="text-caption text-medium-emphasis mt-1">MP4, WebM, MKV, MOV до любого размера</p>
    </template>
    <input ref="fileInput" type="file" accept="video/*" style="display:none" @change="onFileChange" />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'

defineProps<{ uploading?: boolean }>()
const emit = defineEmits<{ upload: [file: File] }>()

const dragging = ref(false)
const fileInput = ref<HTMLInputElement | null>(null)

function onDrop(e: DragEvent) {
  dragging.value = false
  const file = e.dataTransfer?.files?.[0]
  if (file) emit('upload', file)
}

function onFileChange(e: Event) {
  const file = (e.target as HTMLInputElement).files?.[0]
  if (file) emit('upload', file)
}
</script>

<style scoped>
.upload-zone {
  border: 2px dashed rgba(var(--v-theme-primary), 0.4);
  border-radius: 8px;
  padding: 24px;
  text-align: center;
  cursor: pointer;
  transition: border-color 0.2s, background 0.2s;
}
.upload-zone:hover:not(.uploading) {
  border-color: rgb(var(--v-theme-primary));
  background: rgba(var(--v-theme-primary), 0.04);
}
.upload-zone.dragging {
  border-color: rgb(var(--v-theme-primary));
  background: rgba(var(--v-theme-primary), 0.08);
}
.upload-zone.uploading {
  cursor: default;
  opacity: 0.7;
}
.upload-link {
  color: rgb(var(--v-theme-primary));
  text-decoration: underline;
}
</style>
