<script setup lang="ts">
import WelcomeItem from './WelcomeItem.vue'
import ToolingIcon from './icons/IconTooling.vue'
import EcosystemIcon from './icons/IconEcosystem.vue'
import { useRouter } from 'vue-router'
import { useAuth } from '@/composables/useAuth'
import { onMounted, ref, computed } from 'vue'
import { useChat } from '@/composables/useChat.ts'
import type { CreateChatDto } from '@/types/BackendDtos'

const router = useRouter()
const { isAuthenticated, fetchCurrentUser, currentUser } = useAuth()
const { createChat } = useChat()
const loading = ref(true)

onMounted(async () => {
  if (isAuthenticated.value) {
    await fetchCurrentUser()
  }
  loading.value = false
})

function goToAuth(isRegistration = false) {
  router.push({ 
    path: '/auth',
    query: isRegistration ? { register: 'true' } : undefined 
  })
}

async function goToChat(mode = 'question') {
  try {
    // Формируем имя чата из текущей даты и времени
    const now = new Date()
    const formattedDate = now.toLocaleDateString('ru-RU', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric'
    })
    const formattedTime = now.toLocaleTimeString('ru-RU', {
      hour: '2-digit',
      minute: '2-digit'
    })
    
    let chatType: 'Chat' | 'ProblemSolver' = 'Chat'
    let chatName = ''
    let problemHash: string | undefined = undefined

    switch(mode) {
      case 'question':
      default:
        chatName = `Вопрос ${formattedDate} ${formattedTime}`
        break
    }
    
    const dto: CreateChatDto = {
      name: chatName,
      type: chatType,
      problemHash,
    }
    const chatId = await createChat(dto)
    router.push({ path: `/chat/${chatId}` })
  } catch (error) {
    console.error('Ошибка при создании чата:', error)
    alert('Не удалось создать чат. Пожалуйста, попробуйте еще раз.')
  }
}

// Вычисляемое свойство для отображения полного имени пользователя
const fullName = computed(() => {
  if (!currentUser.value) return ''
  return `${currentUser.value.firstName} ${currentUser.value.lastName}`
})
</script>

<template>
  <div class="welcome-container">
    <!-- Для неавторизованных пользователей -->
    <v-card v-if="!isAuthenticated" class="mx-auto pa-6" max-width="800">
      <h1 class="text-h3 text-center mb-6">Тест‑площадка «ИИ‑лектор»</h1>
      
      <v-card-text class="text-body-1 text-center mb-6">
        <p class="mb-3">Перед вами — первое тестирование системы «ИИ лектор».</p>
        <p class="mb-3">LLM‑ассистент помогает разбирать материал, решать задачи и отслеживать прогресс.</p>
        <p>Диалоги могут быть анонимно проанализированы для оптимизации работы и дообучения модели.</p>
      </v-card-text>
      
      <div class="d-flex justify-center gap-4 mt-6">
        <v-btn 
          color="primary" 
          variant="elevated" 
          size="large" 
          @click="goToAuth(true)"
        >
          Регистрация
        </v-btn>
        <v-btn 
          color="secondary" 
          variant="outlined" 
          size="large" 
          @click="goToAuth(false)"
        >
          Вход
        </v-btn>
      </div>
    </v-card>

    <!-- Для авторизованных пользователей -->
    <v-card v-else class="mx-auto pa-6" max-width="900">
      <h1 class="text-h3 text-center mb-2">«ИИ-лектор»</h1>
      
      <div v-if="loading" class="d-flex justify-center my-4">
        <v-progress-circular indeterminate color="primary"></v-progress-circular>
      </div>
      
      <div v-else-if="currentUser" class="user-info-container">
        <v-card class="user-card mx-n6" variant="flat" color="#3e3e3e" rounded="0">
          <div class="user-info-content">
            <div class="user-avatar">
              <v-avatar color="primary" size="64">
                <span class="text-h5 text-white">{{ currentUser.firstName.charAt(0) }}{{ currentUser.lastName.charAt(0) }}</span>
              </v-avatar>
            </div>
            <div class="user-details">
              <h3 class="user-name">{{ fullName }}</h3>
              <div class="user-meta">
                <div class="user-meta-item">
                  <v-icon size="small" class="me-1" color="white">mdi-email</v-icon>
                  <span>{{ currentUser.email }}</span>
                </div>
                <div class="user-meta-item">
                  <v-icon size="small" class="me-1" color="white">mdi-account-group</v-icon>
                  <span>Группа: {{ currentUser.studentGroup }}</span>
                </div>
              </div>
            </div>
            <div class="logout-btn">
              <v-btn 
                color="error" 
                variant="text" 
                size="small"
                to="/logout"
                class="white-hover"
              >
                <v-icon class="me-1">mdi-logout</v-icon>
                Выйти
              </v-btn>
            </div>
          </div>
        </v-card>
      </div>
      
      <v-card-text class="text-body-1 mt-6 mb-0">
        <p class="mb-4 text-h6">Выберите режим работы:</p>
      </v-card-text>

      <!-- Карточное представление режимов обучения -->
      <v-row class="px-4">
        <v-col cols="12" sm="4">
          <v-card class="mode-card" height="100%" elevation="3" @click="router.push('/select-task?taskType=1')">
            <v-card-item>
              <v-avatar class="mode-icon" color="primary" size="56">
                <v-icon size="x-large" color="white">mdi-school</v-icon>
              </v-avatar>
              <v-card-title class="mt-2">Учебный режим</v-card-title>
            </v-card-item>
            <v-card-text>
              <p>Система пошагово показывает процесс решения, вы следите и подтверждаете шаги.</p>
            </v-card-text>
            <v-card-actions>
              <v-btn variant="tonal" color="primary" block class="white-hover">Перейти</v-btn>
            </v-card-actions>
          </v-card>
        </v-col>

        <v-col cols="12" sm="4">
          <v-card class="mode-card" height="100%" elevation="3" @click="router.push('/select-task?taskType=2')">
            <v-card-item>
              <v-avatar class="mode-icon" color="amber-darken-2" size="56">
                <v-icon size="x-large" color="white">mdi-lightbulb</v-icon>
              </v-avatar>
              <v-card-title class="mt-2">Наведение на мысль</v-card-title>
            </v-card-item>
            <v-card-text>
              <p>ИИ поможет вам подойти к решению самостоятельно, задавая уточняющие вопросы.</p>
            </v-card-text>
            <v-card-actions>
              <v-btn variant="tonal" color="amber-darken-2" block class="white-hover">Перейти</v-btn>
            </v-card-actions>
          </v-card>
        </v-col>

        <v-col cols="12" sm="4">
          <v-card class="mode-card" height="100%" elevation="3" @click="router.push('/select-task?taskType=3')">
            <v-card-item>
              <v-avatar class="mode-icon" color="success" size="56">
                <v-icon size="x-large" color="white">mdi-clipboard-check</v-icon>
              </v-avatar>
              <v-card-title class="mt-2">Контрольная работа</v-card-title>
            </v-card-item>
            <v-card-text>
              <p>Самостоятельное решение задач без помощи системы для проверки знаний.</p>
            </v-card-text>
            <v-card-actions>
              <v-btn variant="tonal" color="success" block class="white-hover">Перейти</v-btn>
            </v-card-actions>
          </v-card>
        </v-col>
      </v-row>

      <v-card-text class="mt-4">        
        <p class="my-4 text-h6">Также вы можете задать свой вопрос системе:</p>
        
        <v-card class="question-card mb-4" elevation="2">
          <v-card-item>
            <v-row align="center">
              <v-col cols="12" sm="9">
                <div class="d-flex align-center">
                  <v-avatar class="me-3" color="secondary" size="48">
                    <v-icon size="large" color="white">mdi-help-circle</v-icon>
                  </v-avatar>
                  <div>
                    <v-card-title>Задать свой вопрос</v-card-title>
                    <v-card-subtitle>Начните новый чат и спросите что угодно</v-card-subtitle>
                  </div>
                </div>
              </v-col>
              <v-col cols="12" sm="3" class="text-center text-sm-end">
                <v-btn 
                  color="secondary" 
                  variant="elevated" 
                  size="large"
                  @click="goToChat('question')"
                  class="white-hover"
                >
                  Задать вопрос
                </v-btn>
              </v-col>
            </v-row>
          </v-card-item>
        </v-card>

        <p class="mt-6 text-caption text-grey text-center">Ваши диалоги с системой сохраняются и помогают нам делать «ИИ-лектора» умнее и полезнее.</p>
      </v-card-text>
    </v-card>
  </div>
</template>

<style scoped>
.welcome-container {
  padding: 2rem;
  max-width: 1200px;
  margin: 0 auto;
  max-height: 100dvh;
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 80vh;
}

.mode-card {
  transition: all 0.3s ease;
  cursor: pointer;
  border-radius: 12px;
}

.mode-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 8px 16px rgba(0,0,0,0.2);
}

.mode-card:hover .white-hover {
  color: rgba(0,0,0,0.87) !important;
  background-color: white !important;
}

.mode-icon {
  margin-bottom: 10px;
}

.question-card {
  border-radius: 12px;
  transition: all 0.3s ease;
}

.question-card:hover {
  box-shadow: 0 4px 12px rgba(0,0,0,0.15);
}

/* Стили для блока с информацией о пользователе */
.user-info-container {
  margin-top: 1rem;
}

.user-card {
  transition: all 0.3s ease;
}

.user-card:hover {
  box-shadow: 0 4px 12px rgba(0,0,0,0.15);
}

.user-info-content {
  padding: 1rem 2rem;
  display: flex;
  align-items: center;
  position: relative;
}

.user-avatar {
  margin-right: 1rem;
}

.user-details {
  flex: 1;
}

.logout-btn {
  position: absolute;
  top: 10px;
  right: 16px;
}

.white-hover:hover {
  color: rgba(0,0,0,0.87) !important;
  background-color: white !important;
}

.user-name {
  font-size: 1.4rem;
  font-weight: 600;
  margin-bottom: 0.5rem;
  color: white;
}

.user-meta {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.user-meta-item {
  display: flex;
  align-items: center;
  font-size: 0.9rem;
  color: rgba(255, 255, 255, 0.85);
}

@media (min-width: 600px) {
  .user-meta {
    flex-direction: row;
    gap: 1.5rem;
  }
}

@media (max-width: 599px) {
  .mode-card {
    margin-bottom: 16px;
  }
}
</style>
