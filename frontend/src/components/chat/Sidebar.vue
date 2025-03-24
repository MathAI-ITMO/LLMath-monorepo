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
    <v-list-item @click="onChatSelect(chat.id)" v-for="chat in chats" :key="chat.id" link>
      <v-list-item-content>
        <v-list-item-title>
          {{ chat.name }}
        </v-list-item-title>
      </v-list-item-content>

      <v-list-item-action>
        <v-btn variant="text" @click="onChatDelete(chat.id)">
          <v-icon icon="mdi-delete-forever"></v-icon>
          Удалить чат
        </v-btn>
      </v-list-item-action>
    </v-list-item>
  </v-navigation-drawer>
</template>

<script setup lang="ts">
import { onMounted, ref, toRefs, watch } from 'vue'
import { useChat } from '@/composables/useChat.ts'
import type { Chat } from '@/models/Chat'

const { getChats, deleteChat } = useChat()

const emit = defineEmits<{
  (e: 'chatDeleted'): void
  (e: 'createChat'): void
  (e: 'chatSelected', id: string): void
}>()

const chats = ref<Chat[]>([])

const props = defineProps({
  isChatCreation: Boolean,
})

const { isChatCreation } = toRefs(props)

watch(isChatCreation, onChatUpdate)

onMounted(() => {
  onChatUpdate()
})

function onChatSelect(id: string) {
  console.log('chat with id ' + id + ' selected')
  emit('chatSelected', id)
}

async function onChatDelete(id: string) {
  try {
    await deleteChat(id)
    onChatUpdate()
    emit('chatDeleted')
  } catch (error) {
    console.error('Error deleting chat:', error)
    alert('Failed to delete chat. Please try again.')
  }
}

async function onChatUpdate() {
  chats.value = await getChats()
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
