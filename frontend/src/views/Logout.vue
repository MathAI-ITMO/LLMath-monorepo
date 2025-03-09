<template>
    <h1>Вы успешно вышли</h1>
</template>

<script setup lang="ts">
import { onMounted, inject } from 'vue';
import { AuthService } from '@/services/AuthService'
import router from '@/router'

const refreshAuthInHeader = inject('refreshAuthInHeader');

const authService = new AuthService(import.meta.env.VITE_MATHLLM_BACKEND_ADDRES)

onMounted(() => {
    authService.logout()
    .finally(() =>
    {
      refreshAuthInHeader()
      router.push('/')
    })
})

</script>

<style lang="css" scoped>
</style>
