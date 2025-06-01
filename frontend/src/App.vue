<script setup lang="ts">
import { computed } from "vue";
import { useRoute } from "vue-router";
import { useAuth } from "./composables/useAuth";

const route = useRoute();
const isChatRoute = computed(() => route.path.startsWith("/chat"));

const { isAuthenticated } = useAuth();
</script>

<template>
  <link rel="preconnect" href="https://fonts.googleapis.com" />
  <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin="" />
  <link href="https://fonts.googleapis.com/css2?family=Roboto:wght@100;300;400;500;700;900&display=swap"
    rel="stylesheet" />

  <div v-if="!isChatRoute" class="layout">
    <main class="content">
      <RouterView />
    </main>
  </div>

  <RouterView v-else />
</template>

<style scoped>
.main-app {
  width: 100vw;
  height: 100vh;
}

.layout {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}

.content {
  flex: 1;
  padding: 1rem;
}
</style>
