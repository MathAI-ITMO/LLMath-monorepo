<template>
  <v-navigation-drawer
    v-model="modelValue"
    app
    class="chat-sidebar"
  >
    <div class="sidebar-header">
      <v-divider></v-divider>
      <v-card-actions class="justify-center pa-4">
        <v-btn
          class="new-chat-button"
          variant="tonal"
          block
          :disabled="props.isChatCreation"
          @click="newChat()"
        >
          <v-icon icon="mdi-plus" class="mr-2" />
          Новый чат
        </v-btn>
      </v-card-actions>
      <v-divider></v-divider>
    </div>

    <v-list class="chat-list">
      <v-list-item
        v-for="chat in chats"
        :key="chat.id"
        link
        :active="selectedChatId === chat.id"
        @click="onChatSelect(chat.id)"
        class="chat-list-item"
      >
        <template v-slot:prepend>
          <v-icon icon="mdi-chat" class="mr-2"></v-icon>
        </template>

        <v-list-item-title>
          {{ chat.name }}
        </v-list-item-title>

        <template v-slot:append>
          <v-btn
            variant="text"
            density="comfortable"
            icon="mdi-delete"
            color="error"
            @click.stop="onChatDelete(chat.id)"
            class="delete-btn"
          >
          </v-btn>
        </template>
      </v-list-item>
    </v-list>
  </v-navigation-drawer>
</template>

<script setup lang="ts">
defineOptions({
  name: 'ChatSidebar'
})

import { onMounted, ref, toRefs, watch } from 'vue'
import { useChat } from '@/composables/useChat.ts'
import type { Chat } from '@/models/Chat'
import { useRoute } from 'vue-router'

const route = useRoute()
const selectedChatId = ref<string>(route.params.chatId as string)

const { getChats, deleteChat } = useChat()

const props = defineProps({
  isChatCreation: Boolean,
  modelValue: {
    type: Boolean,
    default: true
  }
})

const { isChatCreation, modelValue } = toRefs(props)

const emit = defineEmits<{
  (e: 'chatDeleted'): void
  (e: 'createChat'): void
  (e: 'chatSelected', id: string): void
  (e: 'update:modelValue', value: boolean): void
}>()

const chats = ref<Chat[]>([])

watch(isChatCreation, onChatUpdate)

watch(() => route.params.chatId, (newChatId) => {
  selectedChatId.value = newChatId as string
})

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
  margin: 4px 8px;
  border-radius: 8px;
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
