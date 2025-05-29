<template>
  <div class="chat-container">
    <v-navigation-drawer
      v-model="sidebarOpen"
      permanent
      class="chat-sidebar"
    >
      <div class="sidebar-header">
        <v-divider></v-divider>
        <v-card-actions class="justify-center pa-4">
          <v-btn
            class="new-chat-button"
            variant="tonal"
            block
            :disabled="!chatId"
            @click="createNewChat()"
          >
            <v-icon icon="mdi-plus" class="mr-2" />
            Новый чат
          </v-btn>
        </v-card-actions>
        <v-divider></v-divider>
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

        <v-app-bar-title class="ml-4">
          {{ chat?.name || 'Новый чат' }}
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
      <template v-if="!chatId">
        <v-card class="new-chat-card">
          <v-card-title class="pb-4">Создание нового чата</v-card-title>
          <v-radio-group
            v-model="chatMode"
            inline
            density="comfortable"
            class="mb-4"
            :disabled="isCreatingChat"
          >
            <v-radio
              label="Чат"
              value="chat"
            ></v-radio>
            <v-radio
              label="Решение задач"
              value="problem-solving"
            ></v-radio>
          </v-radio-group>
          <v-row align="center" no-gutters>
            <v-col>
              <v-text-field
                hide-details="auto"
                label="Название чата"
                variant="solo"
                density="comfortable"
                bg-color="surface"
                v-model="chatName"
                class="chat-input"
                :disabled="isCreatingChat"
                :error="hasDuplicateName"
              ></v-text-field>
            </v-col>
            <v-col cols="auto" class="pl-2">
              <v-btn
                variant="tonal"
                :disabled="isCreateDisabled"
                @click="onChatCreate"
                class="action-button"
                :loading="isCreatingChat"
                :color="needsConfirmation ? 'error' : undefined"
              >{{ needsConfirmation ? 'Подтвердить' : 'Создать' }}</v-btn>
            </v-col>
          </v-row>
          <v-alert
            v-if="hasDuplicateName && needsConfirmation"
            type="error"
            density="compact"
            class="mt-2"
            variant="tonal"
          >
            Чат с таким именем существует. Нажмите "Подтвердить" для подтверждения.
          </v-alert>
          <template v-if="chatMode === 'problem-solving'">
            <v-row align="center" no-gutters class="mt-4">
              <v-col>
                <v-text-field
                  v-model="searchQuery"
                  label="Поиск задач"
                  variant="solo"
                  density="comfortable"
                  bg-color="surface"
                  prepend-inner-icon="mdi-magnify"
                  clearable
                  class="chat-input"
                  hide-details="auto"
                  @keyup.enter="onSearch"
                  :disabled="isCreatingChat"
                ></v-text-field>
              </v-col>
              <v-col cols="auto" class="pl-2">
                <v-btn
                  variant="tonal"
                  @click="onSearch"
                  class="action-button"
                  :loading="isLoading"
                  :disabled="isCreatingChat"
                >Поиск</v-btn>
              </v-col>
            </v-row>
            <v-card class="mt-4 problems-card">
              <v-list lines="one" class="problems-list">
                <template v-if="isLoading">
                  <v-list-item v-for="n in 5" :key="n">
                    <v-skeleton-loader type="list-item"></v-skeleton-loader>
                  </v-list-item>
                </template>
                <template v-else>
                  <v-list-item
                    v-for="problem in problems"
                    :key="problem.hash"
                    :value="problem.name"
                    @click="() => {
                      selectedProblem = problem.name;
                      selectedProblemHash = problem.hash;
                    }"
                    :active="selectedProblem === problem.name"
                    class="problem-item"
                    :disabled="isCreatingChat"
                  >
                    <v-list-item-title>{{ problem.name }}</v-list-item-title>
                  </v-list-item>
                </template>
              </v-list>
              <v-card-actions class="justify-center pa-2">
                <v-pagination
                  v-model="page"
                  :length="pageCount"
                  :total-visible="5"
                  density="comfortable"
                  :disabled="isLoading || isCreatingChat"
                ></v-pagination>
              </v-card-actions>
            </v-card>
          </template>
        </v-card>
      </template>
      <template v-else>
        <div ref="messagesCard" class="messages-wrapper">
          <div class="messages-container">
            <v-list class="messages-list" :lines="false">
              <template v-if="messages.length === 0">
                <v-list-item>
                  <v-list-item-title class="text-center">
                    Сообщений пока нет, напишите первое сообщение
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
                <div v-html="formatMessage(message.text)" :class="{ 'text-left': message.type === 'user' }"></div>
                <small class="message-time">{{ moment(message?.time).fromNow() }}</small>
              </v-list-item>
            </v-list>
          </div>
        </div>

        <div ref="inputCard" class="input-container">
          <div class="input-panel">
            <v-text-field
              hide-details="auto"
              placeholder="Введите сообщение..."
              v-model="currentMessageText"
              auto-grow
              rows="1"
              max-rows="1"
              variant="outlined"
              density="comfortable"
              class="message-input"
              @keyup.enter="sendMessage"
            ></v-text-field>
            <v-btn
              @click="sendMessage"
              :disabled="isSending"
              :loading="isSending"
              color="primary"
              variant="elevated"
              class="send-button"
            >
              <v-icon icon="mdi-send" class="mr-1"></v-icon>
              Отправить
            </v-btn>
          </div>
        </div>
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
defineOptions({
  name: 'ChatMessages'
})

const emit = defineEmits(['chatSelected', 'chatDeleted', 'update:chatId'])

import { ref, watch, onMounted, computed, nextTick } from 'vue'
import type { Chat } from '@/models/Chat'
import type { Message } from '@/models/Message'
import moment from 'moment'
import { useChat } from '@/composables/useChat.ts'
import { useRoute, useRouter } from 'vue-router'
import type { ProblemDto, CreateChatDto } from '@/types/BackendDtos'
import { useUserTasks } from '@/composables/useUserTasks'
import { UserTaskStatus } from '@/types/BackendDtos'

import 'katex/dist/katex.min.css'
import katex from 'katex'
// @ts-ignore
import renderMathInElement from 'katex/dist/contrib/auto-render.mjs'

const route = useRoute()
const router = useRouter()

const { getChatById, getChatMessages, getNextMessage, createChat, getChats, deleteChat, getProblems } = useChat()
const { completeUserTask, fetchUserTasks } = useUserTasks()

const props = defineProps({
  chatId: String,
})

const chatId = ref<string | undefined>(props.chatId)

const inputCard = ref<HTMLElement | null>(null)
const messagesCard = ref<HTMLElement | null>(null)
const chatName = ref<string>('')
const chatMode = ref<string>('chat')
const selectedProblem = ref<string>('')
const selectedProblemHash = ref<string>('')
const searchQuery = ref<string>('')
const problems = ref<ProblemDto[]>([])
const totalProblems = ref<number>(0)
const page = ref<number>(1)
const itemsPerPage = 10
const isLoading = ref<boolean>(false)
const isCreatingChat = ref<boolean>(false)
const hasDuplicateName = ref<boolean>(false)
const needsConfirmation = ref<boolean>(false)

const pageCount = computed(() => {
  return Math.ceil(totalProblems.value / itemsPerPage)
})

const isCreateDisabled = computed(() => {
  if (!chatName.value || (chatMode.value === 'problem-solving' && !selectedProblem.value) || isCreatingChat.value) {
    return true
  }
  if (hasDuplicateName.value && !needsConfirmation.value) {
    return false
  }
  return false
})

const sidebarOpen = ref<boolean>(false)
const chats = ref<Chat[]>([])

const chat = ref<Chat>()
const messages = ref<Message[]>([])
const currentMessageText = ref<string>('')
const isSending = ref<boolean>(false)

const userTaskId = ref<number | null>(null)
const taskStatus = ref<UserTaskStatus | null>(null)
const taskType = ref<number | null>(null)
const markingSolved = ref(false)

function scrollToBottom() {
  if (messagesCard.value) {
    const card = messagesCard.value;
    setTimeout(() => {
      if (card) {
        card.scrollTop = card.scrollHeight;
      }
    }, 100)
  }
}

onMounted(async () => {
  chatId.value = route.params.chatId as string | undefined;
  emit('update:chatId', chatId.value);
  await onChatUpdate();
  await fetchProblems();
  await updateTaskInfo();
})

watch(() => route.params.chatId, async (newChatId) => {
  console.log('New chatId from URL:', newChatId);
  chatId.value = newChatId as string | undefined;
  emit('update:chatId', chatId.value);
  await onChatUpdate();
})

async function onChatUpdate() {
  chats.value = await getChats();

  if (!chatId.value) {
    chat.value = undefined;
    messages.value = [];
    return;
  }

  const receivedChat = await getChatById(chatId.value);
  chat.value = receivedChat;
  const receivedMessages = await getChatMessages(chatId.value);
  messages.value = receivedMessages;
  console.log(receivedMessages);
  scrollToBottom();
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

async function sendMessage() {
  if (!chatId?.value)
  {
    console.log('send message called but chat id not specified yet')
    return;
  }

  if (!currentMessageText.value)
  {
    alert('Message cannot be sent because it is empty')
    return;
  }
  isSending.value = true

  const userMessage : Message = {
    id: "",
    chatId: chatId!.value,
    type: 'user',
    text: currentMessageText.value,
    time: new Date()
  }

  messages.value.push(userMessage)
  scrollToBottom()

  const messageText = currentMessageText.value
  currentMessageText.value = ""

  const botMessage = await getNextMessage(messageText, chatId!.value)

  const message : Message = {
      id: "",
      chatId: chatId!.value,
      type: 'bot',
      text: "",
      time: new Date()
    }

  messages.value.push(message)
  scrollToBottom()

  for await (const chunk of botMessage) {
    message.text = message.text + chunk
    messages.value = [...messages.value]
    scrollToBottom()
  }

  isSending.value = false
}

async function onChatCreate() {
  try {
    if (hasDuplicateName.value && !needsConfirmation.value) {
      needsConfirmation.value = true
      return
    }

    isCreatingChat.value = true
    const dto: CreateChatDto = {
      name: chatName.value,
      problemHash: chatMode.value === 'problem-solving' ? selectedProblemHash.value : undefined,
      type: chatMode.value === 'problem-solving' ? 'ProblemSolver' : 'Chat'
    }
    const id = await createChat(dto)
    emit('chatSelected', id)
    sidebarOpen.value = false

    await onChatUpdate()
  } finally {
    isCreatingChat.value = false
  }
}

async function onChatSelect(id: string) {
  console.log('chat with id ' + id + ' selected')
  emit('chatSelected', id)
  sidebarOpen.value = false
  chatId.value = id;
  await onChatUpdate();
}

async function onChatDelete(id: string) {
  try {
    await deleteChat(id)
    if (id === chatId.value) {
      chatName.value = ''
      chat.value = undefined
      messages.value = []
      chatId.value = undefined
      emit('chatDeleted')
      router.push('/chat')
    }
    await onChatUpdate()
  } catch (error) {
    console.error('Error deleting chat:', error)
    alert('Failed to delete chat. Please try again.')
  }
}

function createNewChat() {
  chatName.value = ''
  chat.value = undefined
  messages.value = []
  emit('update:chatId', undefined)
  router.push('/chat')
  sidebarOpen.value = false
}

async function onSearch() {
  page.value = 1
  await fetchProblems()
}

async function fetchProblems() {
  try {
    isLoading.value = true
    const response = await getProblems(page.value, searchQuery.value)
    problems.value = response.problems
    totalProblems.value = response.number
  } catch (error) {
    console.error('Error fetching problems:', error)
  } finally {
    isLoading.value = false
  }
}

watch(page, async () => {
  await fetchProblems()
})

watch(chatName, () => {
  hasDuplicateName.value = chats.value.some(chat => chat.name === chatName.value)
  needsConfirmation.value = false
})

async function updateTaskInfo() {
  if (!chatId.value) {
    userTaskId.value = null;
    taskStatus.value = null;
    taskType.value = null;
    return;
  }
  // Загружаем задачи пользователя (все типы)
  try {
    // Сначала проверяем тип 0
    const tasks0 = await fetchUserTasks(0);
    let task = tasks0.find(t => t.associatedChatId === chatId.value);
    
    // Если не нашли в типе 0, проверяем типы 1-3
    if (!task) {
      for (let i = 1; i <= 3; i++) {
        const tasksI = await fetchUserTasks(i);
        task = tasksI.find(t => t.associatedChatId === chatId.value);
        if (task) {
          taskType.value = i;
          break;
        }
      }
    } else {
      taskType.value = 0;
    }
    
    if (task) {
      userTaskId.value = task.id as unknown as number; // id is number
      taskStatus.value = task.status;
    } else {
      // Это обычный чат, не связанный с задачей
      userTaskId.value = null;
      taskStatus.value = null;
      taskType.value = null;
    }
  } catch (err) {
    console.error('Failed to fetch user tasks for chat view:', err);
  }
}

watch(chatId, () => {
  updateTaskInfo();
});

async function markTaskSolved() {
  if (!userTaskId.value) return;
  markingSolved.value = true;
  try {
    const res = await completeUserTask(userTaskId.value);
    if (res) {
      taskStatus.value = UserTaskStatus.Solved;
      // Переходим к списку задач с правильным типом
      if (taskType.value !== null) {
        router.push(`/select-task?taskType=${taskType.value}`);
      } else {
        router.push('/select-task');
      }
    }
  } catch (err) {
    console.error('Failed to mark task solved:', err);
  } finally {
    markingSolved.value = false;
  }
}
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

.message-item {
  min-height: 0;
  padding: 0.25rem 0;
  border-radius: 1rem !important;
  margin-bottom: 0.5rem;
  overflow: hidden;
}

.message-item::before,
.message-item::after {
  display: none;
}

.message-time {
  display: block;
  font-size: 0.7rem;
  color: rgba(var(--v-theme-on-surface), 0.4);
  margin-top: 0.25rem;
}

.user-message {
  text-align: left;
  border-radius: 1.25rem !important;
  padding: 0.75rem 1rem;
  margin: 0.5rem 0 0.5rem auto;
  max-width: 80%;
  background: rgba(var(--v-theme-primary), 0.25);
  overflow: hidden;
}

.bot-message {
  text-align: left;
  border-radius: 1rem;
  padding: 0.75rem 1rem 0.5rem 1rem;
  margin: 0.5rem 0;
  max-width: 80%;
  background: transparent;
  color: rgba(var(--v-theme-on-surface), 0.87);
}

.problem-condition-message {
  background: #1E293B !important; /* Темный серо-синий фон */
  color: white !important;
  border-radius: 1rem !important;
  padding: 1rem !important;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
  margin-bottom: 1.5rem;
  max-width: 95% !important; /* Чуть шире обычных сообщений */
}

.problem-condition-message :deep(h1),
.problem-condition-message :deep(h2),
.problem-condition-message :deep(h3),
.problem-condition-message :deep(h4),
.problem-condition-message :deep(h5),
.problem-condition-message :deep(h6) {
  color: #50E3C2; /* Бирюзовый цвет для заголовков внутри условия */
}

.problem-condition-message :deep(.katex) {
  color: #F0F4F8; /* Светлый цвет для формул внутри условия */
}

/* Выделяем зеленую метку условия задачи */
.problem-condition-message :deep(b) {
  color: #50E3C2;
  font-size: 1.1em;
}

.input-container {
  flex: 0 0 auto;
  padding: 1rem;
  background: var(--v-theme-background);
  position: sticky;
  bottom: 0;
  z-index: 10;
}

.input-panel {
  width: 90%;
  max-width: 75rem;
  margin: 0 auto;
  display: flex;
  align-items: center;
  border-radius: 1.25rem;
  border: 2px solid rgba(var(--v-theme-primary), 0.7);
  padding: 0.5rem 0.5rem 0.5rem 1.5rem;
  background-color: var(--v-theme-background);
  min-height: 4.25rem;
}

.input-panel:focus-within {
  border-color: white;
}

.message-input {
  flex: 1;
}

.message-input :deep(.v-field__input) {
  padding: 0.5rem 0;
  min-height: 2.75rem;
}

.message-input :deep(.v-field__outline) {
  display: none;
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
  color: #00ff32 !important;
  font-size: 0.875rem;
  text-transform: none;
  font-weight: 500;
  letter-spacing: 0.5px;
  opacity: 1 !important;
}

.new-chat-card {
  width: 90%;
  max-width: 75rem;
  margin: 2rem auto;
  padding: 1rem;
  border-radius: 1rem;
}

.margin-before {
  margin: 0 2rem 1rem 2rem;
}

.chat-sidebar {
  display: flex;
  flex-direction: column;
  height: 100vh;
}

.sidebar-header {
  flex-shrink: 0;
}

.chat-list {
  flex-grow: 1;
  overflow-y: auto;
  overflow-x: hidden;
}

.new-chat-button {
  width: 100%;
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

.chat-input :deep(.v-field__input) {
  min-height: 48px !important;
  padding: 0 1rem;
}

.chat-input :deep(.v-field) {
  border-radius: 0.75rem;
}

.action-button {
  height: 48px;
  min-width: 100px;
}

.problems-card {
  border-radius: 0.75rem;
  overflow: hidden;
}

.problems-list {
  max-height: 300px;
  overflow-y: auto;
}

.problem-item {
  transition: all 0.2s ease;
  border-radius: 0.5rem;
  margin: 0.25rem;
}

.problem-item:hover {
  background-color: rgba(var(--v-theme-primary), 0.1);
}
</style>
