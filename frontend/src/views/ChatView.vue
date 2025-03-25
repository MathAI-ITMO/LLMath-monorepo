<!-- ChatView.vue -->
<template>
  <v-layout ref="app" class="rounded rounded-md fill-heights">
    <v-main>
      <ChatMessages
        v-model:chatId="chatId"
        @chatSelected="chatSelected"
        @chatDeleted="onChatDeleted"
      />
    </v-main>
  </v-layout>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import ChatMessages from '@/components/chat/Chat.vue'

const route = useRoute()
const router = useRouter()

const chatId = ref<string | undefined>(route.params.chatId as string)

watch(() => route.params.chatId, (newChatId) => {
  chatId.value = newChatId as string | undefined
})

function chatSelected(id: string) {
  router.push(`/chat/${id}`)
}
</script>

<style lang="css" scoped>
.outer-div {
  min-height: 100vh;
  min-width: 100vw;
}
</style>
