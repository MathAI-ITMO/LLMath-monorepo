<template>
    <h1>{{text}}</h1>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useAuth } from '@/composables/useAuth.ts'
import router from '@/router'

const text = ref("Подождите")

const { logout } = useAuth()


onMounted(() => {
    logout().then(() => {
      text.value = 'Вы успешно вышли'
      // router.push('/')
    })
      .catch(err =>
      {
        text.value = 'Произошла ошибка, проверьте консоль'
        console.log(err)
      });
})

</script>

<style lang="css" scoped>
</style>
