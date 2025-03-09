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
import { ref } from 'vue'
import Sidebar from '@/components/chat/Sidebar.vue'
import NewChat from '@/components/chat/NewChat.vue'
import Chat from '@/components/chat/Chat.vue'

const isChatCreation = ref<boolean>(true)
const chatId = ref<number>()

function chatSelected(id: number) {
  chatId.value = id
  isChatCreation.value = false
}

function createChat() {
  isChatCreation.value = true
}
</script>

<style lang="css" scoped>
.outer-div {
  min-height: 100vh;
  min-width: 100vw;
}
</style>
