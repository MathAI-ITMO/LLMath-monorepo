<template>
  <div class="pa-4">
    <!-- Upload card (admins only) -->
    <v-card v-if="isAdmin" variant="outlined" class="mb-4">
      <v-card-text>
        <VideoUpload @upload="onUpload" :uploading="uploading" />
      </v-card-text>
    </v-card>

    <!-- Video table -->
    <v-card variant="outlined">
      <v-card-title class="d-flex align-center pa-4 pb-2">
        <v-icon start color="primary">mdi-video-box</v-icon>
        Видеолекции
        <v-spacer />
        <v-text-field
          v-model="filter"
          density="compact"
          hide-details
          variant="outlined"
          placeholder="Поиск…"
          prepend-inner-icon="mdi-magnify"
          style="max-width: 240px"
        />
      </v-card-title>

      <v-divider />

      <v-data-table
        :headers="headers"
        :items="filteredVideos"
        :loading="loading"
        item-value="name"
        density="comfortable"
        no-data-text="Нет загруженных видео"
      >
        <template #item.name="{ item }">
          <div class="d-flex align-center gap-2">
            <v-icon color="primary" size="20">mdi-play-circle-outline</v-icon>
            <span class="text-body-2 font-weight-medium">{{ item.name }}</span>
          </div>
        </template>

        <template #item.status="{ item }">
          <v-menu open-on-hover location="end" :open-delay="150" :close-delay="100">
            <template #activator="{ props: menuProps }">
              <v-chip
                v-bind="menuProps"
                v-if="processingStatus[item.name]"
                :color="statusColor(processingStatus[item.name])"
                size="small"
                :prepend-icon="statusIcon(processingStatus[item.name])"
              >
                {{ statusLabel(processingStatus[item.name]) }}
              </v-chip>
              <v-chip v-bind="menuProps" v-else color="success" size="small" prepend-icon="mdi-check-circle">
                Готово
              </v-chip>
            </template>
            <v-card min-width="400" max-width="600" elevation="8">
              <v-card-title class="text-body-2 pa-3 pb-1 d-flex align-center ga-2">
                <v-icon size="16" color="primary">mdi-text-box-outline</v-icon>
                Логи обработки
              </v-card-title>
              <v-divider />
              <v-card-text class="pa-0">
                <div class="log-list" v-if="videoLogs[item.name]?.length">
                  <div
                    v-for="(entry, i) in videoLogs[item.name]"
                    :key="i"
                    class="log-list__row"
                    :class="entry.type === 'error' ? 'log-list__row--error' : ''"
                  >
                    <v-chip
                      :color="entry.type === 'error' ? 'error' : 'default'"
                      size="x-small"
                      variant="tonal"
                      class="log-list__badge"
                    >{{ entry.type }}</v-chip>
                    <span class="log-list__content">{{ entry.content }}</span>
                  </div>
                </div>
                <div v-else class="pa-3 text-medium-emphasis text-body-2">Нет логов</div>
              </v-card-text>
            </v-card>
          </v-menu>
        </template>

        <template #item.actions="{ item }">
          <v-btn
            size="small"
            variant="text"
            color="primary"
            prepend-icon="mdi-play"
            @click="$emit('select', item.name)"
          >
            Открыть
          </v-btn>
          <v-btn
            v-if="isAdmin"
            size="small"
            variant="text"
            color="error"
            icon="mdi-delete"
            @click="onDelete(item.name)"
          />
        </template>
      </v-data-table>
    </v-card>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useVideoList, type ProcessingStatus } from '@/composables/video/useVideoList'
import { useAuth } from '@/composables/useAuth'
import { USER_ROLES } from '@/config/roles.constants'
import VideoUpload from './VideoUpload.vue'

const emit = defineEmits<{ select: [name: string] }>()

const { videos, loading, filter, filteredVideos, processingStatus, videoLogs, fetchVideos, deleteVideo, uploadVideo } = useVideoList()
const { currentUser } = useAuth()
const isAdmin = computed(() => currentUser.value?.role === USER_ROLES.ADMIN)
const uploading = ref(false)

const headers = [
  { title: 'Название', key: 'name', sortable: true },
  { title: 'Статус обработки', key: 'status', sortable: false, width: '200px' },
  { title: '', key: 'actions', sortable: false, align: 'end' as const },
]

onMounted(fetchVideos)

async function onUpload(file: File) {
  uploading.value = true
  try {
    await uploadVideo(file.name, file)
  } catch (e) {
    console.error('Upload failed', e)
  } finally {
    uploading.value = false
  }
}

async function onDelete(name: string) {
  await deleteVideo(name)
}

function statusLabel(s: ProcessingStatus): string {
  switch (s) {
    case 'pending':   return 'Ожидание…'
    case 'extract':   return 'Извлечение звука'
    case 'transcribe': return 'Распознавание речи'
    case 'summarize': return 'Саммаризация'
    case 'done':      return 'Готово'
    case 'error':     return 'Ошибка'
  }
}

function statusColor(s: ProcessingStatus): string {
  switch (s) {
    case 'done':  return 'success'
    case 'error': return 'error'
    default:      return 'warning'
  }
}

function statusIcon(s: ProcessingStatus): string {
  switch (s) {
    case 'done':  return 'mdi-check-circle'
    case 'error': return 'mdi-alert-circle'
    default:      return 'mdi-progress-clock'
  }
}
</script>

<style scoped>
.log-list {
  max-height: 320px;
  overflow-y: auto;
  font-family: monospace;
  font-size: 12px;
}
.log-list__row {
  display: flex;
  align-items: baseline;
  gap: 10px;
  padding: 5px 12px;
  border-bottom: 1px solid rgba(255,255,255,0.04);
}
.log-list__row--error {
  background: rgba(244, 67, 54, 0.08);
}
.log-list__badge {
  flex-shrink: 0;
}
.log-list__content {
  word-break: break-all;
  line-height: 1.4;
}
</style>
