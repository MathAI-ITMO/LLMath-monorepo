<template>
    <v-container class="text-center">
        <v-card class="mx-auto pa-6 mt-8" max-width="600">
            <h1 class="text-h4 mb-6">{{text}}</h1>
            
            <v-btn 
                color="primary" 
                variant="elevated" 
                size="large" 
                to="/"
                class="white-hover mt-4"
            >
                На главную
            </v-btn>
        </v-card>
    </v-container>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useAuth } from '@/composables/useAuth.ts'
import router from '@/router'

const text = ref("Подождите, выполняется выход...")

const { logout } = useAuth()


onMounted(() => {
    logout().then(() => {
      text.value = 'Вы успешно вышли из системы'
      // router.push('/')
    })
      .catch(err =>
      {
        text.value = 'Произошла ошибка при выходе из системы'
        console.log(err)
      });
})

</script>

<style lang="css" scoped>
.white-hover:hover {
    color: rgba(0,0,0,0.87) !important;
    background-color: white !important;
}
</style>
