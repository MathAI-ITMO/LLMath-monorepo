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
import { useChat } from '@/composables/useChat.ts'

const { createChat } = useChat()

const chatName = ref<string>('');

const emit = defineEmits<{
  (e: 'chatCreated', id: string): void
}>()

async function onChatCreate()
{
  const id = await createChat(chatName.value)
  emit('chatCreated', id)
}

</script>

<style lang="css" scoped>
.margin-before {
  margin-left: 2rem;
  margin-right: 2rem;
  margin-bottom: 1rem
}

</style>
