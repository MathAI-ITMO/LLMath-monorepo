<!-- ChatView.vue -->
<template>
    <v-layout ref="app" class="rounded rounded-md fill-heights">
      <Sidebar
        :isChatCreation="isChatCreation"
        @createChat="createChat"
        @chatSelected="chatSelected"
      />

      <v-main>
        <NewChat v-if="isChatCreation" @chatCreated="chatSelected" />
        <Chat v-if="!isChatCreation" :chatId="chatId" />
      </v-main>
    </v-layout>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Sidebar from '@/components/chat/Sidebar.vue'
import NewChat from '@/components/chat/NewChat.vue'
import Chat from '@/components/chat/Chat.vue'

const route = useRoute()
const router = useRouter()

const isChatCreation = ref<boolean>(!route.params.chatId)
const chatId = ref<string | undefined>(route.params.chatId as string)

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
