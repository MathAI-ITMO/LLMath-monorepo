<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import axios from 'axios'
import { servicesConfig } from '@/config/services.config'

interface VideoInfo {
  name: string
  url: string
}

const videos = ref<VideoInfo[]>([])
const loading = ref(true)
const selectedVideo = ref<string | null>(null)
const showVideoModal = ref(false)
const showAllVideos = ref(false)

// Градиенты для карточек
const gradients = [
  'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
  'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
  'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
  'linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)',
  'linear-gradient(135deg, #fa709a 0%, #fee140 100%)',
  'linear-gradient(135deg, #ff9a56 0%, #ff6a88 100%)',
]

onMounted(async () => {
  await fetchVideos()
})

async function fetchVideos() {
  loading.value = true
  try {
    const response = await axios.get(`${servicesConfig.videoServiceUrl}/videos`)
    videos.value = response.data || []
  } catch (error) {
    console.error('Ошибка загрузки видео:', error)
    videos.value = []
  } finally {
    loading.value = false
  }
}

const topVideos = computed(() => videos.value.slice(0, 6))
const totalVideos = computed(() => videos.value.length)

function getGradient(index: number): string {
  return gradients[index % gradients.length]
}

function openVideo(videoName: string) {
  selectedVideo.value = videoName
  showVideoModal.value = true
  showAllVideos.value = false
}

function closeVideoModal() {
  showVideoModal.value = false
  selectedVideo.value = null
}

function openAllVideos() {
  showAllVideos.value = true
  showVideoModal.value = true
}

function backToHome() {
  showVideoModal.value = false
  showAllVideos.value = false
  selectedVideo.value = null
}

const videoAppUrl = computed(() => {
  if (selectedVideo.value) {
    return `${servicesConfig.videoServiceUrl}/${selectedVideo.value}`
  }
  return servicesConfig.videoServiceUrl
})

// Форматирование имени видео для отображения
function formatVideoName(name: string): string {
  return name.replace(/\.(mp4|avi|mkv|webm)$/i, '').replace(/_/g, ' ')
}
</script>

<template>
  <div class="video-lectures-section">
    <v-card-text class="text-body-1 mb-0">
      <p class="mb-4 text-h6">
        <v-icon class="me-2" color="primary">mdi-play-circle</v-icon>
        Видеолекции по теории:
      </p>
    </v-card-text>

    <div v-if="loading" class="d-flex justify-center my-8">
      <v-progress-circular indeterminate color="primary" size="64"></v-progress-circular>
    </div>

    <div v-else-if="videos.length === 0" class="text-center my-8">
      <v-icon size="64" color="grey">mdi-video-off</v-icon>
      <p class="text-grey mt-2">Видеолекции пока не загружены</p>
    </div>

    <div v-else class="lectures-container px-4">
      <v-row>
        <!-- Слайдер с топ-6 лекциями -->
        <v-col cols="12" md="9">
          <div class="lectures-slider">
            <v-row>
              <v-col 
                v-for="(video, index) in topVideos" 
                :key="video.name"
                cols="12" 
                sm="6" 
                md="4"
              >
                <v-card 
                  class="video-card"
                  :style="{ background: getGradient(index) }"
                  elevation="4"
                  @click="openVideo(video.name)"
                >
                  <div class="video-card-content">
                    <div class="video-number">{{ index + 1 }}</div>
                    <v-icon class="video-play-icon" size="48" color="white">mdi-play-circle-outline</v-icon>
                    <h3 class="video-title">{{ formatVideoName(video.name) }}</h3>
                  </div>
                </v-card>
              </v-col>
            </v-row>
          </div>
        </v-col>

        <!-- Статистика и кнопка "Все лекции" -->
        <v-col cols="12" md="3">
          <v-card 
            class="stats-card" 
            elevation="4"
            @click="openAllVideos"
          >
            <v-card-item>
              <div class="stats-content">
                <v-icon size="64" color="white" class="mb-3">mdi-video-box</v-icon>
                <div class="stats-number">{{ totalVideos }}</div>
                <div class="stats-label">Лекций доступно</div>
                <div class="stats-button">Все лекции</div>
              </div>
            </v-card-item>
          </v-card>
        </v-col>
      </v-row>
    </div>

    <!-- Модальное окно с видео -->
    <v-dialog 
      v-model="showVideoModal" 
      fullscreen
      transition="dialog-bottom-transition"
    >
      <v-card class="video-modal">
        <v-toolbar dark color="primary">
          <v-btn icon @click="backToHome">
            <v-icon>mdi-arrow-left</v-icon>
          </v-btn>
          <v-toolbar-title v-if="!showAllVideos && selectedVideo">
            {{ formatVideoName(selectedVideo) }}
          </v-toolbar-title>
          <v-toolbar-title v-else>
            Все видеолекции
          </v-toolbar-title>
          <v-spacer></v-spacer>
          <v-btn 
            v-if="showAllVideos" 
            icon 
            @click="closeVideoModal"
          >
            <v-icon>mdi-close</v-icon>
          </v-btn>
        </v-toolbar>

        <!-- Сетка всех видео -->
        <v-card-text v-if="showAllVideos" class="pa-6">
          <v-row>
            <v-col 
              v-for="(video, index) in videos" 
              :key="video.name"
              cols="12" 
              sm="6" 
              md="4" 
              lg="3"
            >
              <v-card 
                class="video-card"
                :style="{ background: getGradient(index) }"
                elevation="4"
                @click="openVideo(video.name)"
              >
                <div class="video-card-content">
                  <div class="video-number">{{ index + 1 }}</div>
                  <v-icon class="video-play-icon" size="48" color="white">mdi-play-circle-outline</v-icon>
                  <h3 class="video-title">{{ formatVideoName(video.name) }}</h3>
                </div>
              </v-card>
            </v-col>
          </v-row>
        </v-card-text>

        <!-- Iframe с видео -->
        <div v-else class="video-iframe-wrapper">
          <iframe 
            :src="videoAppUrl" 
            class="video-iframe"
            frameborder="0"
            allow="accelerometer; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
            allowfullscreen
          ></iframe>
        </div>
      </v-card>
    </v-dialog>
  </div>
</template>

<style scoped>
.video-lectures-section {
  margin: 2rem 0;
}

.lectures-container {
  margin-bottom: 1rem;
}

.lectures-slider {
  width: 100%;
}

.video-card {
  height: 180px;
  border-radius: 16px;
  cursor: pointer;
  transition: all 0.3s ease;
  overflow: hidden;
  position: relative;
}

.video-card:hover {
  transform: translateY(-8px) scale(1.02);
  box-shadow: 0 12px 24px rgba(0,0,0,0.3) !important;
}

.video-card-content {
  height: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 1.5rem;
  position: relative;
  color: white;
  text-align: center;
}

.video-number {
  position: absolute;
  top: 12px;
  right: 12px;
  background: rgba(0, 0, 0, 0.3);
  backdrop-filter: blur(10px);
  border-radius: 50%;
  width: 36px;
  height: 36px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.1rem;
  font-weight: 700;
  color: white;
}

.video-play-icon {
  margin-bottom: 0.5rem;
  opacity: 0.9;
  transition: all 0.3s ease;
}

.video-card:hover .video-play-icon {
  opacity: 1;
  transform: scale(1.2);
}

.video-title {
  font-size: 1rem;
  font-weight: 600;
  color: white;
  text-shadow: 0 2px 4px rgba(0,0,0,0.2);
  line-height: 1.3;
  max-width: 100%;
  overflow: hidden;
  text-overflow: ellipsis;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
}

.stats-card {
  height: 100%;
  min-height: 180px;
  border-radius: 16px;
  background: linear-gradient(135deg, #1e3c72 0%, #2a5298 100%);
  color: white;
  cursor: pointer;
  transition: all 0.3s ease;
}

.stats-card:hover {
  background: linear-gradient(135deg, #2a4d8f 0%, #3567b8 100%);
  transform: translateY(-4px);
  box-shadow: 0 8px 16px rgba(0,0,0,0.3) !important;
}

.stats-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
  padding: 1.5rem 1rem;
  height: 100%;
}

.stats-number {
  font-size: 3rem;
  font-weight: 700;
  color: white;
  line-height: 1;
  margin-bottom: 0.5rem;
}

.stats-label {
  font-size: 1rem;
  color: rgba(255, 255, 255, 0.9);
  font-weight: 500;
  margin-bottom: 0.5rem;
}

.stats-button {
  margin-top: 0.5rem;
  padding: 0.75rem 1.5rem;
  background: rgba(255, 255, 255, 0.2);
  border-radius: 8px;
  font-weight: 600;
  font-size: 1rem;
  color: white;
  transition: all 0.3s ease;
}

.stats-card:hover .stats-button {
  background: rgba(255, 255, 255, 0.3);
}

.video-modal {
  background: #1e1e1e;
}

.video-iframe-wrapper {
  width: 100%;
  height: calc(100vh - 64px);
  background: #000;
}

.video-iframe {
  width: 100%;
  height: 100%;
  border: none;
}

@media (max-width: 960px) {
  .stats-card {
    min-height: auto;
    margin-top: 1rem;
  }
  
  .video-card {
    height: 160px;
  }
}

@media (max-width: 600px) {
  .video-card {
    height: 140px;
  }
  
  .video-title {
    font-size: 0.9rem;
  }
  
  .stats-number {
    font-size: 2.5rem;
  }
}
</style>
