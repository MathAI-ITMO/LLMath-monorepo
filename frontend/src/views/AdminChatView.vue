<!-- AdminChatView.vue -->
<template>
  <div class="admin-chat-page">
    <v-card class="mx-auto chat-card" flat>
      <v-toolbar flat>
        <v-btn icon variant="text" color="primary" @click="goBack">
          <v-icon>mdi-arrow-left</v-icon>
        </v-btn>
        <v-toolbar-title>
          {{ chat?.name || 'Чат' }}
          <v-chip v-if="chat?.type === 'ProblemSolver' && chat.taskType !== undefined && taskModeTitlesReady" size="small" class="ml-2" color="secondary" label>{{ formatTaskTypeForChat(chat.taskType) }}</v-chip>
          <v-chip v-else-if="chat?.type" size="small" class="ml-2" color="primary" label>{{ chat.type }}</v-chip>
        </v-toolbar-title>
      </v-toolbar>
      
      <div ref="messagesCard" class="messages-container">
        <div class="messages-list">
          <template v-if="messages.length === 0">
            <div class="no-messages">
              <div class="text-center">
                Загрузка сообщений...
              </div>
            </div>
          </template>
          
          <!-- Используем MessageItem компонент вместо прямого вывода -->
          <MessageItem
            v-for="message in messages"
            :key="message.id"
            :message="message"
          />
        </div>
      </div>
    </v-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import moment from 'moment';
import { useChat } from '@/composables/useChat';
import MessageItem from '@/components/MessageItem.vue';
import type { Chat } from '@/models/Chat';
import type { Message } from '@/models/Message';
import axios from 'axios';
import 'katex/dist/katex.min.css';

const route = useRoute();
const router = useRouter();
const { getChatById, getChatMessages } = useChat();

const chatId = ref<string | undefined>();
const chat = ref<Chat>();
const messages = ref<Message[]>([]);
const messagesCard = ref<HTMLElement | null>(null);
const taskModeTitles = ref<Record<string, string>>({});
const taskModeTitlesReady = ref(false);

onMounted(async () => {
  // Загрузка названий типов задач
  try {
    const baseUrl = import.meta.env.VITE_MATHLLM_BACKEND_ADDRESS;
    const titlesResponse = await axios.get<Record<string, string>>(`${baseUrl}/api/stats/task-mode-titles`, { withCredentials: true });
    taskModeTitles.value = titlesResponse.data;
    taskModeTitlesReady.value = true;
    console.log('AdminChatView: Loaded task mode titles:', taskModeTitles.value);
  } catch (e) {
    console.error('AdminChatView: Failed to load task mode titles:', e);
  }

  chatId.value = route.params.chatId as string;
  if (chatId.value) {
    await loadChatData();
  }
});

watch(() => route.params.chatId, async (newChatId) => {
  chatId.value = newChatId as string;
  if (chatId.value) {
    await loadChatData();
  }
});

async function loadChatData() {
  if (!chatId.value) return;
  
  try {
    const receivedChat = await getChatById(chatId.value);
    chat.value = receivedChat;
    console.log('Loaded chat details:', receivedChat);
    
    const receivedMessages = await getChatMessages(chatId.value);
    messages.value = receivedMessages;
    
    scrollToBottom();
  } catch (error) {
    console.error('Error loading chat data:', error);
  }
}

function scrollToBottom() {
  setTimeout(() => {
    window.scrollTo(0, document.body.scrollHeight);
  }, 100);
}

function goBack() {
  router.go(-1);
}

const formatTaskTypeForChat = (type: number | undefined): string => {
  if (type === undefined) return 'Тип задачи не определен';
  const typeStr = type.toString();
  if (taskModeTitles.value && taskModeTitles.value[typeStr]) {
    return taskModeTitles.value[typeStr];
  }
  if (type === 0) return 'Упражнение (из списка)';
  return `Тип задачи (${type})`;
};
</script>

<style scoped>
.admin-chat-page {
  width: 100%;
  height: 100%;
  overflow-x: hidden;
}

.chat-card {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  margin: 0;
  padding: 0;
  border-radius: 0;
  background-color: transparent;
  overflow-y: visible;
}

.messages-container {
  max-width: 75rem;
  margin: 0 auto;
  padding: 1rem;
}

.messages-list {
  padding: 1rem;
  display: flex;
  flex-direction: column;
  background-color: transparent;
}

.no-messages {
  padding: 2rem;
  text-align: center;
  color: rgba(var(--v-theme-on-surface), 0.6);
}

/* Переопределяем стили MessageItem для AdminChatView */
.messages-list :deep(.message-item) {
  position: relative;
  padding: 1rem;
  margin-bottom: 1rem;
  border-radius: 1rem;
  max-width: 85%;
  white-space: pre-wrap;
  box-shadow: none;
  line-height: 1.5;
}

.messages-list :deep(.user-message) {
  align-self: flex-start;
  margin-left: 2rem;
  min-width: 30%;
  background-color: rgba(var(--v-theme-primary), 0.15) !important;
  border-bottom-right-radius: 0.25rem;
}

.messages-list :deep(.bot-message) {
  align-self: flex-start;
  background-color: transparent !important;
  border-bottom-left-radius: 0.25rem;
  margin-right: auto;
}

.messages-list :deep(.message-time) {
  position: absolute;
  bottom: 0;
  right: 0;
  font-size: 0.7rem;
  opacity: 0.7;
}

.messages-list :deep(.problem-condition-message) {
  background-color: rgba(var(--v-theme-primary), 0.15) !important;
  color: rgb(240, 244, 248);
  align-self: flex-start;
  margin-right: auto;
  border-bottom-left-radius: 0.25rem;
  margin-bottom: 1.5rem;
  max-width: 95% !important;
}

.messages-list :deep(.problem-condition-message h1),
.messages-list :deep(.problem-condition-message h2),
.messages-list :deep(.problem-condition-message h3),
.messages-list :deep(.problem-condition-message h4),
.messages-list :deep(.problem-condition-message h5),
.messages-list :deep(.problem-condition-message h6) {
  color: #50E3C2;
}

.messages-list :deep(.problem-condition-message .katex) {
  color: #F0F4F8;
}

.messages-list :deep(.problem-condition-message b) {
  color: #50E3C2;
  font-size: 1.1em;
}

/* Стили для правильного отображения матриц KaTeX */
:deep(.katex) {
  display: inline-block;
}

:deep(.katex .base) {
  display: inline-block;
}
</style> 