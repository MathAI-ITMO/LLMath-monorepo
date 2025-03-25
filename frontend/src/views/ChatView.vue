<!-- ChatView.vue -->
<template>
    <v-layout ref="app" class="rounded rounded-md fill-heights">
      <ChatSidebar
        :isChatCreation="isChatCreation"
        @createChat="createChat"
        @chatSelected="chatSelected"
        v-model="sidebarOpen"
      />

      <v-main>
        <NewChat v-if="isChatCreation" @chatCreated="chatSelected" />
        <ChatMessages
          v-if="!isChatCreation"
          :chatId="chatId"
          @toggleSidebar="sidebarOpen = !sidebarOpen"
        />
      </v-main>
    </v-layout>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import ChatSidebar from '@/components/chat/Sidebar.vue'
import NewChat from '@/components/chat/NewChat.vue'
import ChatMessages from '@/components/chat/Chat.vue'

const route = useRoute()
const router = useRouter()

const isChatCreation = ref<boolean>(!route.params.chatId)
const chatId = ref<string | undefined>(route.params.chatId as string)
const sidebarOpen = ref<boolean>(true)

watch(() => route.params.chatId, (newChatId) => {
  if (newChatId) {
    chatId.value = newChatId as string
    isChatCreation.value = false
  }
})

function chatSelected(id: string) {
  chatId.value = id
  isChatCreation.value = false
  router.push(`/chat/${id}`)
}

function createChat() {
  isChatCreation.value = true
  router.push('/chat')
}
</script>

<style lang="css" scoped>
.outer-div {
  min-height: 100vh;
  min-width: 100vw;
}
</style>
