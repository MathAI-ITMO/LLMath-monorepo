<template>
  <div class="chat-container">
    <v-navigation-drawer
      v-model="sidebarOpen"
      permanent
      class="chat-sidebar"
    >
      <div class="sidebar-header">
        <v-btn
          block
          color="primary"
          prepend-icon="mdi-plus"
          @click="createNewChat"
          class="new-chat-button"
        >
          Новый чат
        </v-btn>
      </div>

      <v-list class="chat-list">
        <v-list-item
          v-for="chatItem in chats"
          :key="chatItem.id"
          link
          :active="chatId === chatItem.id"
          @click="onChatSelect(chatItem.id)"
          class="chat-list-item"
        >
          <template v-slot:prepend>
            <v-icon :icon="chatItem.type === 'ProblemSolver' ? 'mdi-function' : 'mdi-chat'" class="mr-2"></v-icon>
          </template>

          <v-list-item-title>
            {{ chatItem.name }}
          </v-list-item-title>

          <template v-slot:append>
            <v-btn
              variant="text"
              density="comfortable"
              icon="mdi-delete"
              color="error"
              @click.stop="onChatDelete(chatItem.id)"
              class="delete-btn"
            >
            </v-btn>
          </template>
        </v-list-item>
      </v-list>
    </v-navigation-drawer>

    <div class="top-panel">
      <v-app-bar flat class="px-4">
        <v-btn
          icon
          variant="text"
          @click="sidebarOpen = !sidebarOpen"
          class="sidebar-toggle"
        >
          <v-icon>mdi-message-outline</v-icon>
        </v-btn>

        <v-btn
          icon
          variant="text"
          :to="userTaskId ? `/select-task?taskType=${taskType}` : '/'"
          class="ml-2 nav-button"
        >
          <v-icon>{{ userTaskId ? 'mdi-view-list' : 'mdi-home' }}</v-icon>
        </v-btn>

        <v-tabs
          v-if="hasTheory"
          v-model="activeTab"
          color="primary"
          class="header-tabs"
        >
          <v-tab value="theory" @click="setTab('theory')">теория</v-tab>
          <v-tab value="practice" @click="setTab('practice')">практика</v-tab>
        </v-tabs>

        <v-app-bar-title class="ml-4">
          {{ chat?.name || 'Чат' }}
        </v-app-bar-title>
        <v-spacer></v-spacer>
        <v-btn
          v-if="userTaskId && taskStatus === UserTaskStatus.Solved"
          variant="elevated"
          color="success"
          class="solved-btn"
          density="comfortable"
          disabled
        >
          Задача отмечена решенной
        </v-btn>
        <v-btn
          v-else-if="userTaskId && taskStatus !== UserTaskStatus.Solved"
          variant="outlined"
          color="success"
          :loading="markingSolved"
          @click="markTaskSolved"
          class="mark-solved-btn"
          density="comfortable"
        >
          Отметить задачу решенной
        </v-btn>
      </v-app-bar>
    </div>

    <div class="chat-content">
      <template v-if="chatId">
        <div ref="messagesCard" class="messages-wrapper">
          <div class="messages-container">
            <div class="messages-list">
              <!-- блок 'пока нет сообщений' -->
              <template v-if="messages.length === 0">
                <div class="no-messages">
                  <div class="text-center">
                    Сообщений пока нет, напишите первое сообщение
                  </div>
                </div>
              </template>

              <!-- список сообщений -->
              <MessageItem
                v-for="m in messages"
                :key="m.id"
                :message="m"
              />
            </div>
          </div>
        </div>

        <!-- Full-width, full-height overlay from tabs to bottom -->
        <div
          v-if="hasTheory"
          class="theory-overlay"
          :class="{ open: panelOpen }"
          :style="{ top: panelTop + 'px', height: 'calc(100dvh - ' + panelTop + 'px)' }"
        >
          <!-- collapsed clickable strip -->
          <div v-if="!panelOpen" class="theory-strip" @click="setTab('theory')"></div>
          <!-- expanded content: always iframe (no native video substitution) -->
          <iframe
            v-show="panelOpen"
            class="theory-iframe"
            :src="theoryUrl"
            title="Theory Video"
            frameborder="0"
            allow="accelerometer; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
            allowfullscreen
          ></iframe>
        </div>

        <div ref="inputCard" class="input-container">
          <v-row no-gutters justify="center">
            <v-col cols="12" sm="10" md="8" lg="6">
              <v-text-field
                hide-details="auto"
                placeholder="Введите сообщение..."
                v-model="currentMessageText"
                variant="outlined"
                density="comfortable"
                class="message-input"
                @keyup.enter="sendMessage"
              >
                <template v-slot:append-inner>
                  <v-btn
                    @click="sendMessage"
                    :disabled="isSending"
                    :loading="isSending"
                    color="primary"
                    variant="elevated"
                    icon="mdi-send"
                    size="small"
                  >
                  </v-btn>
                </template>
              </v-text-field>
            </v-col>
          </v-row>
        </div>
      </template>

      <template v-else>
        <div class="no-chat-selected">
          <v-container class="fill-height">
            <v-row justify="center" align="center">
              <v-col cols="12" sm="8" md="6" class="text-center">
                <v-icon size="64" color="primary" class="mb-4">mdi-chat-processing-outline</v-icon>
                <h2 class="text-h4 mb-4">Выберите чат для начала общения</h2>
                <p class="text-body-1 mb-6 text-medium-emphasis">
                  Выберите существующий чат из списка ниже или создайте новый, чтобы начать обсуждение математических задач.
                </p>

                <v-card variant="outlined" class="pa-0 mb-6 chat-list-card">
                  <v-list class="text-left py-0">
                    <v-list-subheader>Ваши чаты</v-list-subheader>
                    <v-divider></v-divider>
                    <template v-if="chats.length > 0">
                      <v-list-item
                        v-for="chatItem in chats"
                        :key="chatItem.id"
                        link
                        @click="onChatSelect(chatItem.id)"
                        class="chat-list-item-main"
                      >
                        <template v-slot:prepend>
                          <v-icon :icon="chatItem.type === 'ProblemSolver' ? 'mdi-function' : 'mdi-chat'" class="mr-2"></v-icon>
                        </template>
                        <v-list-item-title>{{ chatItem.name }}</v-list-item-title>
                        <template v-slot:append>
                          <v-icon size="small">mdi-chevron-right</v-icon>
                        </template>
                      </v-list-item>
                    </template>
                    <v-list-item v-else>
                      <v-list-item-title class="text-center py-4 text-medium-emphasis">
                        У вас пока нет чатов
                      </v-list-item-title>
                    </v-list-item>
                  </v-list>
                </v-card>

                <v-btn
                  color="primary"
                  size="large"
                  prepend-icon="mdi-plus"
                  @click="createNewChat"
                >
                  Создать новый чат
                </v-btn>
              </v-col>
            </v-row>
          </v-container>
        </div>
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
import MessageItem from '../MessageItem.vue'
import { useChatView } from '@/composables/useChatView'

defineOptions({
  name: 'ChatMessages'
})

// Note: controlling playback inside iframe across origins is not reliable,
// so no autoplay/pause is attempted here.

const emit = defineEmits(['chatSelected', 'chatDeleted', 'update:chatId'])

const props = defineProps({
  chatId: String,
})

const {
  chatId,
  messagesCard,
  sidebarOpen,
  chats,
  chat,
  messages,
  currentMessageText,
  isSending,
  userTaskId,
  taskStatus,
  taskType,
  markingSolved,
  sendMessage,
  onChatSelect,
  onChatDelete,
  createNewChat,
  markTaskSolved,
  UserTaskStatus
} = useChatView(props, emit)

// --- Theory integration (VideoApp) ---
import { getVideoUrl } from '@/config/services.config'
import { ref, computed, onMounted, onBeforeUnmount, nextTick, watch } from 'vue'

// Legacy codai.ru regex for backward compatibility
const codaiRe = /(?:https?:\/\/)?codai\.ru\/[\w\-\/.%?#=&]+\.mp4\b/i
const codaiReAll = new RegExp(codaiRe.source, 'gi')

// Get theory URL from chat.theoryLink using VideoApp
const theoryUrl = computed(() => {
  // First priority: use theoryLink from chat if available
  if (chat.value?.theoryLink) {
    // Convert theory link to VideoApp URL
    return getVideoUrl(chat.value.theoryLink)
  }

  // Fallback: extract from messages (for backward compatibility with old codai.ru links)
  for (const m of messages.value ?? []) {
    const match = m.text?.match(codaiRe)
    if (match) {
      // Extract filename from codai.ru URL and use VideoApp
      const url = match[0]
      const filename = url.split('/').pop() || ''
      return getVideoUrl(filename)
    }
  }
  return ''
})
const hasTheory = computed(() => !!theoryUrl.value)

// Tabs state and overlay panel behavior
const activeTab = ref<'practice' | 'theory'>('practice')
const panelOpen = computed(() => activeTab.value === 'theory')
const tabsRef = ref<HTMLElement | null>(null)
const panelTop = ref(0)

function measurePanelTop() {
  const el = tabsRef.value
  if (!el) return
  const rect = el.getBoundingClientRect()
  // For position: fixed overlay, top is relative to viewport
  panelTop.value = Math.max(0, Math.round(rect.top + rect.height))
}

function setTab(tab: 'practice' | 'theory') {
  activeTab.value = tab
}

function onResize() {
  measurePanelTop()
}

onMounted(async () => {
  await nextTick()
  measurePanelTop()
  window.addEventListener('resize', onResize)
  window.addEventListener('scroll', onResize, { passive: true })
})

onBeforeUnmount(() => {
  window.removeEventListener('resize', onResize)
  window.removeEventListener('scroll', onResize)
})

watch([hasTheory, () => messages.value?.length, activeTab], async () => {
  await nextTick()
  measurePanelTop()
})
</script>

<style lang="css" scoped>
.chat-container {
  height: 100dvh;
  display: flex;
  flex-direction: column;
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
}

.top-panel {
  flex: 0 0 auto;
}

.chat-content {
  flex: 1 1 auto;
  display: flex;
  flex-direction: column;
  min-height: 0;
  overflow: hidden;
  padding-top: var(--v-layout-top);
  background-color: var(--v-theme-background);
}

.messages-wrapper {
  flex: 1 1 auto;
  overflow-y: auto;
  padding: 1rem;
  min-height: 0;
}

.messages-container {
  width: 90%;
  max-width: 75rem;
  margin: 0 auto;
}

.messages-list {
  padding: 1rem;
  --v-border-opacity: 0;
  background-color: transparent !important;
}

.no-messages {
  padding: 2rem;
  text-align: center;
  color: rgba(var(--v-theme-on-surface), 0.6);
}

.input-container {
  flex: 0 0 auto;
  padding: 1rem;
  background: var(--v-theme-background);
  position: sticky;
  bottom: 0;
  z-index: 10;
}

.message-input {
  flex: 1;
}

.send-button {
  margin-left: 0.5rem;
  border-radius: 0.75rem;
  transition: all 0.3s ease;
}

.send-button:hover {
  background-color: white !important;
  color: rgba(var(--v-theme-primary), 1) !important;
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
}

.sidebar-toggle {
  color: rgba(var(--v-theme-primary), 0.8);
}

.nav-button {
  color: rgba(var(--v-theme-primary), 0.8);
}

.v-btn {
  border-radius: 0.75rem;
}

.mark-solved-btn {
  transition: all 0.3s ease;
  border-color: rgba(var(--v-theme-success), 0.7) !important;
  color: rgba(var(--v-theme-success), 0.9) !important;
  font-size: 0.875rem;
  text-transform: none;
}

.mark-solved-btn:hover {
  background-color: rgba(var(--v-theme-success), 0.9) !important;
  color: white !important;
  border-color: rgba(var(--v-theme-success), 0.9) !important;
}

.solved-btn {
  background-color: rgb(0 0 0) !important;
  color: #89986D !important;
  font-size: 0.875rem;
  text-transform: none;
  font-weight: 500;
  letter-spacing: 0.5px;
  opacity: 1 !important;
}

.no-chat-selected {
  height: 100%;
  overflow-y: auto;
}

.chat-list-card {
  border-radius: 1rem !important;
  background-color: rgba(var(--v-theme-surface), 0.5) !important;
  max-height: 400px;
  overflow-y: auto;
}

.chat-list-item-main {
  border-bottom: 1px solid rgba(var(--v-theme-on-surface), 0.05);
}

.chat-list-item-main:last-child {
  border-bottom: none;
}

.chat-sidebar {
  display: flex;
  flex-direction: column;
  height: 100vh;
}

.sidebar-header {
  padding: 1rem;
  border-bottom: 1px solid rgba(var(--v-theme-primary), 0.1);
}

.new-chat-button {
  text-transform: none;
  font-weight: 600;
  letter-spacing: 0.5px;
}

.chat-list {
  flex-grow: 1;
  overflow-y: auto;
  overflow-x: hidden;
}

.chat-list-item {
  margin: 0.25rem 0.5rem;
  border-radius: 1rem;
  transition: all 0.2s ease;
}

.chat-list-item:hover {
  background-color: rgba(var(--v-theme-primary), 0.1);
}

.chat-list-item .delete-btn {
  opacity: 0;
  transition: opacity 0.2s ease;
}

.chat-list-item:hover .delete-btn {
  opacity: 1;
}

.v-list-item--active {
  background-color: rgba(var(--v-theme-primary), 0.15);
}

/* Стили для правильного отображения матриц KaTeX */
:deep(.katex) {
  display: inline-block;
}

:deep(.katex .base) {
  display: inline-block;
}


.theory-overlay {
  position: fixed;
  left: 0;
  top: 0; /* overridden via inline style to var(--v-layout-top) */
  width: 100vw; /* full width */
  background: #0f172a; /* dark background under iframe */
  transform: translateX(calc(-100% + 18px));
  transition: transform 260ms ease;
  z-index: 1000; /* above content, below app bars */
  /* no border on top to avoid any perceived cropping */
}

.theory-overlay.open {
  transform: translateX(0);
}

.theory-strip {
  position: absolute;
  left: 0;
  top: 0;
  bottom: 0;
  width: 18px; /* 15-20px */
  background: #ffffff; /* white strip */
  cursor: pointer;
}

.theory-iframe {
  width: 100%;
  height: 100%;
  border: 0;
}
</style>
