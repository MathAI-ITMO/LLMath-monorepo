<template>
  <div class="chat-container">
    <div class="top-panel">
      <v-app-bar flat class="px-4">
        <v-btn
          icon
          variant="text"
          @click="$emit('toggleSidebar')"
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
          {{ chat?.name || 'Что-то пошло не так' }}
        </v-app-bar-title>
      </v-app-bar>
    </div>

    <div class="chat-content">
      <div ref="messagesCard" class="messages-wrapper">
        <v-card class="messages-container">
          <v-list class="messages-list" lines="none">
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
    </div>
  </div>
</template>

<script setup lang="ts">
defineOptions({
  name: 'ChatMessages'
})

defineEmits(['toggleSidebar'])

import { ref, toRefs, watch, onMounted, onUnmounted } from 'vue'
import type { Chat } from '@/models/Chat'
import type { Message } from '@/models/Message'
import moment from 'moment'
import { useChat } from '@/composables/useChat.ts'

const { getChatById, getChatMessages, getNextMessage } = useChat()

const props = defineProps({
  chatId: String,
})

const { chatId } = toRefs(props)

const inputCard = ref(null)
const messagesCard = ref(null)

const chat = ref<Chat>()
const messages = ref<Message[]>([])
const currentMessageText = ref<string>('')
const isSending = ref<boolean>(false)

function scrollToBottom() {
  if (messagesCard.value) {
    setTimeout(() => {
      messagesCard.value.scrollTop = messagesCard.value.scrollHeight
    }, 100)
  }
}

watch(chatId, updateChat)

onMounted(() => {
  updateChat()
})

onUnmounted(() => {
})

async function updateChat() {
  if (!chatId?.value)
  {
    console.log('chat update called but id not specified yet')
    return;
  }

  const receivedChat = await getChatById(chatId!.value)
  chat.value = receivedChat
  const receivedMessages = await getChatMessages(chatId!.value)
  messages.value = receivedMessages
  console.log(receivedMessages)
  scrollToBottom()
}

function formatMessage(message: string): string {
  return message?.replace(/\n/g, '<br>')
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
  flex: 0 0 auto; /* Don't grow or shrink, use auto height */
}

.chat-content {
  flex: 1 1 auto;
  display: flex;
  flex-direction: column;
  min-height: 0;
  overflow: hidden;
  padding-top: var(--v-layout-top); /* Use Vuetify's built-in layout spacing */
}

.messages-wrapper {
  flex: 1 1 auto;
  overflow-y: auto;
  padding: 16px;
  min-height: 0; /* Important for Firefox */
}

.messages-container {
  width: 90%;
  max-width: 1200px;
  margin: 0 auto;
}

.messages-list {
  padding: 16px;
  --v-border-opacity: 0;
}

.message-item {
  min-height: 0;
  padding: 4px 0;
}

.message-item::before,
.message-item::after {
  display: none;
}

.user-message {
  text-align: right;
  border-radius: 16px;
  padding: 12px 16px;
  margin: 8px 0;
  background: rgba(var(--v-theme-primary), 0.1);
}

.bot-message {
  text-align: left;
  margin-left: 30px;
  border-radius: 16px;
  padding: 12px 16px;
  margin: 8px 0;
  background: rgba(var(--v-theme-surface-variant), 0.1);
}

.input-container {
  flex: 0 0 auto;
  padding: 16px;
  background: var(--v-theme-background);
  position: sticky;
  bottom: 0;
  z-index: 10;
}

.input-card {
  width: 90%;
  max-width: 1200px;
  margin: 0 auto;
  border-radius: 20px;
  backdrop-filter: blur(10px);
  overflow: hidden;
}

.message-input {
  border-radius: 12px;
}

.message-input :deep(.v-field__input) {
  padding: 8px 16px;
  min-height: 44px;
  border-radius: 12px;
}

.message-input :deep(.v-field) {
  border-radius: 12px;
}

.v-btn {
  border-radius: 12px;
}
</style>
