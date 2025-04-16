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
            <v-icon icon="mdi-chat" class="mr-2"></v-icon>
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
        >
          <v-icon>mdi-menu</v-icon>
        </v-btn>

        <v-btn
          icon
          variant="text"
          to="/"
          class="ml-2"
        >
          <v-icon>mdi-home</v-icon>
        </v-btn>

        <v-app-bar-title class="ml-4">
          {{ chat?.name || 'Новый чат' }}
        </v-app-bar-title>
      </v-app-bar>
    </div>

    <div class="chat-content">
      <template v-if="!chatId">
        <v-card class="new-chat-card">
          <v-card-title class="pb-4">Создание нового чата</v-card-title>
          <v-row align="center" no-gutters>
            <v-col>
              <v-text-field
                hide-details="auto"
                label="Название чата"
                variant="solo"
                density="comfortable"
                bg-color="surface"
                v-model="chatName"
              ></v-text-field>
            </v-col>
            <v-col cols="auto" class="pl-2">
              <v-btn
                variant="tonal"
                :disabled="!chatName"
                @click="onChatCreate"
                height="48"
              >Создать</v-btn>
            </v-col>
          </v-row>
        </v-card>
      </template>
      <template v-else>
        <div ref="messagesCard" class="messages-wrapper">
          <v-card class="messages-container">
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
                  'bot-message': message.type === 'bot',
                }"
                density="compact"
              >
                <v-icon v-if="message.type === 'bot'" class="bot-icon">mdi-robot</v-icon>                
                <div v-html="formatMessage(message.text)"></div>
                <small>{{ moment(message?.time).fromNow() }}</small>
              </v-list-item>
            </v-list>
          </v-card>
        </div>

        <div ref="inputCard" class="input-container">
          <v-card class="input-card" elevation="4">
            <v-container class="pa-2">
              <v-row align="center" no-gutters>
                <v-col>
                  <v-text-field
                    hide-details="auto"
                    placeholder="Введите сообщение..."
                    v-model="currentMessageText"
                    auto-grow
                    rows="1"
                    max-rows="1"
                    variant="solo"
                    density="comfortable"
                    bg-color="surface"
                    class="message-input"
                    @keyup.enter="sendMessage"
                  ></v-text-field>
                </v-col>
                <v-col cols="auto" class="pl-2">
                  <v-btn
                    @click="sendMessage"
                    :disabled="isSending"
                    :loading="isSending"
                    color="primary"
                    variant="elevated"
                  >
                    <v-icon icon="mdi-send" class="mr-1"></v-icon>
                    Отправить
                  </v-btn>
                </v-col>
              </v-row>
            </v-container>
          </v-card>
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

import { ref, watch, onMounted } from 'vue'
import type { Chat } from '@/models/Chat'
import type { Message } from '@/models/Message'
import moment from 'moment'
import { useChat } from '@/composables/useChat.ts'
import { useRoute, useRouter } from 'vue-router'

import 'katex/dist/katex.min.css'
import katex from 'katex'

const route = useRoute()
const router = useRouter()

const { getChatById, getChatMessages, getNextMessage, createChat, getChats, deleteChat } = useChat()

const props = defineProps({
  chatId: String,
})

const chatId = ref<string | undefined>(props.chatId)

const inputCard = ref<HTMLElement | null>(null)
const messagesCard = ref<HTMLElement | null>(null)
const chatName = ref<string>('')
const sidebarOpen = ref<boolean>(false)
const chats = ref<Chat[]>([])

const chat = ref<Chat>()
const messages = ref<Message[]>([])
const currentMessageText = ref<string>('')
const isSending = ref<boolean>(false)

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

function bigFormula(str:string):string {
    return katex.renderToString(str.substring(2, str.length - 2))
}

function normalFormula(str:string):string {
    return katex.renderToString(str.substring(1, str.length - 1))
}

function proceedWrappedSubStrings(str: string, lSymbols: string, rSymbols: string, replacer: (str: string) => string): string {
  let res = str
  let start = res.indexOf(lSymbols)
  let end = res.indexOf(rSymbols, start + lSymbols.length)
  while (start != -1 && end != -1) {
    res = res.substring(0, start) + replacer(res.substring(start, end + rSymbols.length)) + res.substring(end + rSymbols.length)
    start = res.indexOf(lSymbols)
    end = res.indexOf(rSymbols, start + lSymbols.length)
  }
  return res
}

function formatMessage(message: string): string {
  let buff = message ?? ''
  buff = proceedWrappedSubStrings(message, '$$', '$$', bigFormula)
  buff = proceedWrappedSubStrings(buff, '$', '$', normalFormula)
  buff = proceedWrappedSubStrings(buff, '**', '**', (str: string)=>{return `<b>${str.substring(2, str.length - 2)}<\/b>`})
  buff = proceedWrappedSubStrings(buff, '\`\`\`', '\`\`\`', (str: string)=>{return `<code>${str.substring(3, str.length - 3)}<\/code>`})
  buff = proceedWrappedSubStrings(buff, '\`', '\`', (str: string)=>{return `<u>${str.substring(1, str.length - 1)}<\/u>`})
  buff = proceedWrappedSubStrings(buff, '\\textbf{', '}', (str: string)=>{return `<b>${str.substring(8, str.length - 1)}<\/b>`})
  buff = buff.replace(/\\\\/g, "<br>")
  buff = buff.replace(/\n/g, '<br>')
 return buff
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

  const botMessage = await getNextMessage(currentMessageText.value, chatId!.value)

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
  currentMessageText.value = ""
}

async function onChatCreate() {
  const id = await createChat(chatName.value)
  emit('chatSelected', id)
  sidebarOpen.value = false

  await onChatUpdate()
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
  border-radius: 1rem;
  overflow: hidden;
}

.messages-list {
  padding: 1rem;
  --v-border-opacity: 0;
}

.message-item {
  min-height: 0;
  padding: 0.25rem 0;
  border-radius: 1rem;
  margin-bottom: 0.5rem;
}

.message-item::before,
.message-item::after {
  display: none;
}

.user-message {
  text-align: right;
  border-radius: 1rem;
  padding: 0.75rem 1rem;
  margin: 0.5rem 0;
  background: rgba(var(--v-theme-primary), 0.1);
}

.bot-message {
  text-align: left;
  margin-left: 2rem;
  border-radius: 1rem;
  padding: 0.75rem 1rem;
  margin: 0.5rem 0;
  background: rgba(var(--v-theme-surface-variant), 0.1);
}

.input-container {
  flex: 0 0 auto;
  padding: 1rem;
  background: var(--v-theme-background);
  position: sticky;
  bottom: 0;
  z-index: 10;
}

.input-card {
  width: 90%;
  max-width: 75rem;
  margin: 0 auto;
  border-radius: 1.25rem;
  backdrop-filter: blur(0.625rem);
  overflow: hidden;
}

.message-input {
  border-radius: 0.75rem;
}

.message-input :deep(.v-field__input) {
  padding: 0.5rem 1rem;
  min-height: 2.75rem;
  border-radius: 0.75rem;
}

.message-input :deep(.v-field) {
  border-radius: 0.75rem;
}

.v-btn {
  border-radius: 0.75rem;
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
</style>
