<template>
  <v-navigation-drawer app>
    <v-divider></v-divider>
    <v-card-actions class="justify-center">
      <v-btn
        class="new-chat-button"
        variant="tonal"
        :disabled="props.isChatCreation"
        @click="newChat()"
      >
        <v-icon icon="mdi-plus" />
        Новый чат
      </v-btn>
    </v-card-actions>

    <v-divider></v-divider>
    <v-list-item @click="selectChat(chat.id)" v-for="chat in chats" :key="chat.id" link>
      <v-list-item-content>
        <v-list-item-title>
          {{ chat.name }}
        </v-list-item-title>
      </v-list-item-content>

      <v-list-item-action>
        <v-btn variant="text" @click="deleteChat(chat.id)">
          <v-icon icon="mdi-delete-forever"></v-icon>
          Удалить чат
        </v-btn>
      </v-list-item-action>
    </v-list-item>
  </v-navigation-drawer>
</template>

<script setup lang="ts">
import { ref, toRefs, watch, onMounted } from 'vue'
import { ChatService } from '@/services/ChatService'
import { AuthService } from '@/services/AuthService'
import type { Chat } from '@/models/Chat'
import router from '@/router'

const authService = new AuthService(import.meta.env.VITE_MATHLLM_BACKEND_ADDRES)
const chatService = new ChatService(import.meta.env.VITE_MATHLLM_BACKEND_ADDRES)

const emit = defineEmits<{
  (e: 'chatDeleted'): void
  (e: 'createChat'): void
  (e: 'chatSelected', id: number): void
}>()

const chats = ref<Chat[]>([])

const props = defineProps({
  isChatCreation: Boolean,
})

const { isChatCreation } = toRefs(props)


watch(isChatCreation, updateChatList)

onMounted(() => {
  updateChatList()
})

function processError(err)
{
  console.log(err)
  authService.logout()
  router.push('/auth')
}

function selectChat(id: number) {
  console.log('chat with id ' + id + ' selected')
  emit('chatSelected', id)
}

function deleteChat(id: number) {
  chatService.deleteChat(id)
  updateChatList()
  emit('chatDeleted')
}

function updateChatList() {
  chatService.getChats()
  .then(res => chats.value = res)
  .catch(err => processError(err)
  )
}

function newChat() {
  emit('createChat')
}
</script>

<style lang="css" scoped>
.new-chat-button {
  align: center;
  display: block;
}
</style>
