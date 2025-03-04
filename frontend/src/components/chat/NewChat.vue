<template>
  <v-card>
    <v-card-title>Создание нового чата</v-card-title>
    <v-text-field
      hide-details="auto"
      label="Название чата"
      class = "margin-before"
      v-model = "chatName"
    ></v-text-field>
    <v-btn
      class = "margin-before"
      variant = "tonal"
      :disabled="!chatName"
      @click="onChatCreate"
      >Создать</v-btn>
  </v-card>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { ChatService } from '@/services/ChatService'
import { AuthService } from '@/services/AuthService'
import router from '@/router'

const authService = new AuthService(import.meta.env.VITE_MATHLLM_BACKEND_ADDRES)
const chatService = new ChatService(import.meta.env.VITE_MATHLLM_BACKEND_ADDRES, authService)


const chatName = ref<string>('');

const emit = defineEmits<{
  (e: 'chatCreated', id: number): void
}>()

function processError(err)
{
  console.log(err)
  authService.logout()
  router.push('/auth')
}

function onChatCreate()
{
  chatService.createChat(chatName.value)
  .then(id => emit('chatCreated', id))
  .else(err => processError(err))
}

</script>

<style lang="css" scoped>
.margin-before {
  margin-left: 2rem;
  margin-right: 2rem;
  margin-bottom: 1rem
}

</style>
