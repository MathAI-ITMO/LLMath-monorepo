<template>
    <h1>Вы успешно вышли</h1>
</template>

<script setup lang="ts">
import { onMounted } from 'vue';
import { AuthService } from '@/services/AuthService'
import router from '@/router'

const authService = new AuthService(process.env.VITE_MATHLLM_BACKEND_ADDRES)

onMounted(() => {
    authService.logout()
    .then(() => router.push('/'))
    .finally(() => router.push('/'))
})

</script>

<style lang="css" scoped>
/* #authForm * input {
  margin: 5px;
}

#authForm>button {
  margin: 15px;
} */
</style>
