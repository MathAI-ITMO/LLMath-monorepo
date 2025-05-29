<!-- AdminChatView.vue -->
<template>
  <div class="admin-chat-page">
    <v-card class="mx-auto chat-card" flat>
      <v-toolbar flat>
        <v-btn icon variant="text" color="primary" @click="goBack">
          <v-icon>mdi-arrow-left</v-icon>
        </v-btn>
        <v-toolbar-title>{{ chat?.name || 'Чат' }}</v-toolbar-title>
      </v-toolbar>
      
      <div class="messages-container">
        <v-list class="messages-list" :lines="false">
          <template v-if="messages.length === 0">
            <v-list-item>
              <v-list-item-title class="text-center">
                Загрузка сообщений...
              </v-list-item-title>
            </v-list-item>
          </template>
          <v-list-item
            v-for="message in messages"
            :key="message.id"
            class="message-item"
            :class="{
              'user-message': message.type === 'user',
              'bot-message': message.type === 'bot' && !message.text.includes('Условие задачи'),
              'problem-condition-message': message.type === 'bot' && message.text.includes('Условие задачи')
            }"
            density="compact"
            rounded="0"
          >
            <div class="message-content">
              <div v-html="formatMessage(message.text)" :class="{ 'text-left': message.type === 'user' }" class="message-text"></div>
              <small class="message-time">{{ moment(message?.time).fromNow() }}</small>
            </div>
          </v-list-item>
        </v-list>
      </div>
    </v-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import moment from 'moment';
import { useChat } from '@/composables/useChat';
import katex from 'katex';
import 'katex/dist/katex.min.css';
// @ts-ignore
import renderMathInElement from 'katex/dist/contrib/auto-render.mjs';
import type { Chat } from '@/models/Chat';
import type { Message } from '@/models/Message';

const route = useRoute();
const router = useRouter();
const { getChatById, getChatMessages } = useChat();

const chatId = ref<string | undefined>();
const chat = ref<Chat>();
const messages = ref<Message[]>([]);
const messagesCard = ref<HTMLElement | null>(null);

onMounted(async () => {
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
    
    const receivedMessages = await getChatMessages(chatId.value);
    messages.value = receivedMessages;
    
    scrollToBottom();
  } catch (error) {
    console.error('Error loading chat data:', error);
  }
}

function scrollToBottom() {
  // Прокрутка всей страницы вниз
  setTimeout(() => {
    window.scrollTo(0, document.body.scrollHeight);
  }, 100);
}

function formatMessage(message: string): string {
  if (!message) return '';
  
  let buff = message;
  
  // Прямая замена \textbf{...} на <b>...</b> перед обработкой формул
  buff = buff.replace(/\\textbf\{([^{}]*(?:\{[^{}]*\}[^{}]*)*)\}/g, '<b>$1</b>');
  
  // Сначала создаем временный элемент для обработки формул
  const tempDiv = document.createElement('div');
  tempDiv.innerHTML = buff;
  
  // Применяем KaTeX к временному элементу с базовыми настройками
  renderMathInElement(tempDiv, {
    delimiters: [
      {left: "$$", right: "$$", display: true},
      {left: "$", right: "$", display: false},
      {left: "\\(", right: "\\)", display: false},
      {left: "\\[", right: "\\]", display: true},
      {left: "\\begin{equation}", right: "\\end{equation}", display: true},
      {left: "\\begin{equation*}", right: "\\end{equation*}", display: true},
      {left: "\\begin{align}", right: "\\end{align}", display: true},
      {left: "\\begin{alignat}", right: "\\end{alignat}", display: true},
      {left: "\\begin{gather}", right: "\\end{gather}", display: true},
      {left: "\\begin{CD}", right: "\\end{CD}", display: true},
      {left: "\\begin{pmatrix}", right: "\\end{pmatrix}", display: true},
      {left: "\\begin{bmatrix}", right: "\\end{bmatrix}", display: true},
      {left: "\\begin{vmatrix}", right: "\\end{vmatrix}", display: true},
      {left: "\\begin{Vmatrix}", right: "\\end{Vmatrix}", display: true},
      {left: "\\begin{cases}", right: "\\end{cases}", display: true}
    ],
    throwOnError: false,
    errorColor: '#cc0000',
    strict: false,
    trust: true,
    macros: {
      // Только самые необходимые и безопасные макросы
      "\\R": "\\mathbb{R}",
      "\\C": "\\mathbb{C}",
      "\\N": "\\mathbb{N}",
      "\\Z": "\\mathbb{Z}"
    },
    ignoredTags: ["script", "noscript", "style", "textarea", "pre", "code", "option"]
  });
  
  // Получаем текст с обработанным LaTeX
  buff = tempDiv.innerHTML;
  
  // Заголовки
  buff = buff.replace(/(?:^|\n)##### (.*?)(?:\n|$)/g, '<h5>$1</h5>');
  buff = buff.replace(/(?:^|\n)#### (.*?)(?:\n|$)/g, '<h4>$1</h4>');
  buff = buff.replace(/(?:^|\n)### (.*?)(?:\n|$)/g, '<h3>$1</h3>');
  buff = buff.replace(/(?:^|\n)## (.*?)(?:\n|$)/g, '<h2>$1</h2>');
  buff = buff.replace(/(?:^|\n)# (.*?)(?:\n|$)/g, '<h1>$1</h1>');
  
  // Жирный текст
  buff = buff.replace(/\*\*(.*?)\*\*/g, '<b>$1</b>');
  
  // Курсив
  buff = buff.replace(/\*(.*?)\*/g, '<i>$1</i>');
  
  // Подчеркивание
  buff = buff.replace(/__(.*?)__/g, '<u>$1</u>');
  
  // Обработка двойных обратных слешей - преобразуем \\ в перенос строки
  buff = buff.replace(/\\\\(?![^<>]*>)/g, '<br/>');
  
  // Перенос строки
  buff = buff.replace(/\n/g, '<br/>');
  
  return buff;
}

function goBack() {
  router.go(-1);
}
</script>

<style scoped>
.admin-chat-page {
  width: 100%;
  height: 100%;
  overflow-x: hidden;
}

.chat-card {
  /* Убираем фиксированную высоту, чтобы контент мог растягиваться */
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
  /* Убираем лишние свойства, чтобы избежать двойной полосы прокрутки */
}

.messages-list {
  padding: 0;
  display: flex;
  flex-direction: column;
  background-color: transparent;
}

.message-item {
  position: relative;
  padding: 1rem;
  margin-bottom: 1rem;
  border-radius: 1rem;
  max-width: 85%;
  white-space: pre-wrap;
  box-shadow: none;
  line-height: 1.5;
}

.user-message {
  align-self: flex-start;
  margin-left: 2rem; /* Отступ от левого края */
  min-width: 30%; /* Минимальная ширина 30% от диалога */
  background-color: rgba(var(--v-theme-primary), 0.15) !important;
  border-bottom-right-radius: 0.25rem;
}

.bot-message {
  align-self: flex-start;
  background-color: transparent !important;
  border-bottom-left-radius: 0.25rem;
  margin-right: auto;
}

.message-content {
  position: relative;
  width: 100%;
  padding-bottom: 1.2rem; /* Место для метки времени */
}

.message-text {
  margin-bottom: 0.5rem;
}

.message-time {
  position: absolute;
  bottom: 0;
  right: 0;
  font-size: 0.7rem;
  opacity: 0.7;
}

.problem-condition-message {
  background-color: rgba(var(--v-theme-primary), 0.15) !important;
  color: rgb(240, 244, 248);
  align-self: flex-start;
  margin-right: auto;
  border-bottom-left-radius: 0.25rem;
  margin-bottom: 1.5rem;
  max-width: 95% !important;
}

.problem-condition-message :deep(h1),
.problem-condition-message :deep(h2),
.problem-condition-message :deep(h3),
.problem-condition-message :deep(h4),
.problem-condition-message :deep(h5),
.problem-condition-message :deep(h6) {
  color: #50E3C2;
}

.problem-condition-message :deep(.katex) {
  color: #F0F4F8;
}

.problem-condition-message :deep(b) {
  color: #50E3C2;
  font-size: 1.1em;
}
</style> 