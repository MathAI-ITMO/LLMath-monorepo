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
    <header>
      <nav>
        <RouterLink to="/">На главную</RouterLink>
        <RouterLink v-if="!isAuthenticated" to="/auth">Войти</RouterLink>
        <template v-if="isAuthenticated">
          <RouterLink to="/chat">Чат</RouterLink>
          <RouterLink to="/logout">Выйти</RouterLink>
        </template>
      </nav>
    </header>
    <main class="content">
      <RouterView />
    </main>
  </div>

  <RouterView v-else />
</template>

<style scoped>
header {
  width: 100%;
  padding: 1rem;
  border-bottom: 1px solid var(--color-border);
  background: var(--color-background);
}

nav {
  width: 100%;
  font-size: 1rem;
  text-align: center;
}

nav a.router-link-exact-active {
  color: var(--color-text);
}

nav a.router-link-exact-active:hover {
  background-color: transparent;
}

nav a {
  display: inline-block;
  padding: 0 1rem;
  border-left: 1px solid var(--color-border);
}

nav a:first-of-type {
  border: 0;
}

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
