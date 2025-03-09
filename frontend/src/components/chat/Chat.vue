<template>
  <div ref="messagesCard">
  <v-card class="messages-container">
    <v-card-title>{{ chat?.name || 'Что-то пошло не так' }}</v-card-title>
    <v-list>
      <v-list-item
        v-for="message in messages"
        :key="message.id"
        class="message-item"
        :class="{
          'user-message': message.type === 'user',
          'bot-message': message.type === 'bot',
        }"
      >
        <v-icon v-if="message.type === 'bot'" class="bot-icon">mdi-robot</v-icon>
        <div v-html="formatMessage(message.text)"></div>
        <small>{{ moment(message?.time).fromNow() }}</small>
      </v-list-item>
    </v-list>
  </v-card>
</div>
<div ref="inputCard" class="input-container position-absolute bottom-0">
  <v-card>
    <v-container>
    <v-col>
    <v-text-field
      hide-details="auto"
      label="Текст"
      v-model = "currentMessageText"
    ></v-text-field>
    </v-col>
    <v-col>
    <v-btn @click="sendMessage" :disabled = "isSending">Отправить</v-btn>
    </v-col>
  </v-container>
  </v-card>
</div>
</template>

<script setup lang="ts">
import { ref, toRefs, watch, onMounted } from 'vue'
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

watch(chatId, updateChat)

onMounted(() => {
  updateChat()
  updateWidth()
})

function updateWidth()
{
  if (inputCard.value && messagesCard.value)
  {
    const width = window.getComputedStyle(messagesCard.value).width;
    inputCard.value.style.width = width;
  }
}

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

  const botMessage = await getNextMessage(currentMessageText.value, chatId!.value)

  const message : Message = {
      id: "",
      chatId: chatId!.value,
      type: 'bot',
      text: "",
      time: new Date()
    }

  messages.value.push(message)

  for await (const chunk of botMessage) {
    console.log(chunk);
    message.text += chunk.message
  }
}
</script>

<style lang="css" scoped>
.user-message {
  text-align: right;
  border-radius: 10px;
  padding: 8px;
  margin: 5px;
}

.bot-message {
  text-align: left;
  margin-left: 30px;
}

.input-container {
  width: 90%
}

.messages-container {
  height: 100%
}
</style>
