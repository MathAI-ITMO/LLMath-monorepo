<script setup lang="ts">
import { computed, ref, onMounted, provide } from "vue";
import { useRoute } from "vue-router";
import { useAuth } from "./composables/useAuth";

// provide('refreshAuthInHeader', refreshAuthInfo);

// // const isAuthentificated : Ref<boolean> = ref(false);
// const authService = new AuthService(import.meta.env.VITE_MATHLLM_BACKEND_ADDRES)

// function refreshAuthInfo()
// {
//   console.log(import.meta.env.VITE_MATHLLM_BACKEND_ADDRES)
//   authService.getCurrentUser()
//   .then(res =>
//   {
//     console.log(res)
//     isAuthentificated.value = res !== null
//   }
//   )
//   .catch(err =>
//   {
//     console.log(err)
//     isAuthentificated.value = false
//   }
//   )
// }

// onMounted(() => refreshAuthInfo())


const route = useRoute();
const isChatRoute = computed(() => route.path === "/chat");

const { isAuthenticated } = useAuth();
</script>

<template>
  <link rel="preconnect" href="https://fonts.googleapis.com" />
  <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin="" />
  <link href="https://fonts.googleapis.com/css2?family=Roboto:wght@100;300;400;500;700;900&display=swap"
    rel="stylesheet" />

  <div v-if="!isChatRoute" class="layout">
    <aside class="sidebar">
      <img alt="Vue logo" class="logo" src="@/assets/logo.svg" width="125" height="125" />
      <nav>
        <RouterLink to="/">На главную</RouterLink>
        <RouterLink v-if="!isAuthenticated" to="/auth">Авторизация</RouterLink>
        <RouterLink v-if="isAuthenticated" to="/logout">Выйти</RouterLink>
        <RouterLink v-if="isAuthenticated" to="/chat">Чат</RouterLink>
      </nav>
    </aside>
    <main class="content">
      <RouterView />
    </main>
  </div>

  <RouterView v-else />
</template>

<style scoped>


header {
  line-height: 1.5;
  max-height: 100vh;
}

.logo {
  display: block;
  margin: 0 auto 2rem;
}

nav {
  width: 100%;
  font-size: 12px;
  text-align: center;
  margin-top: 2rem;
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

@media (min-width: 1024px) {
  header {
    display: flex;
    place-items: center;
    padding-right: calc(var(--section-gap) / 2);
  }

  .logo {
    margin: 0 2rem 0 0;
  }

  header .wrapper {
    display: flex;
    place-items: flex-start;
    flex-wrap: wrap;
  }

  nav {
    text-align: left;
    margin-left: -1rem;
    font-size: 1rem;

    padding: 1rem 0;
    margin-top: 1rem;
  }
}

.main-app {
  width: 100vw;
  height: 100vh;
}
</style>
