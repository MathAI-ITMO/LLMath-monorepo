<!-- ChatView.vue -->
<template>
  <v-layout ref="app" class="rounded rounded-md fill-heights">
    <v-main>
      <ChatMessages
        @chatSelected="chatSelected"
      />
    </v-main>
  </v-layout>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'
import ChatMessages from '@/components/chat/Chat.vue'

const router = useRouter()

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
