<script setup lang="ts">
import MainPageRight from '@/components/MainPageRight.vue';
</script>

<template>
  <v-layout class="rounded rounded-md fill-heights">
    <v-main>
      <MainPageRight />
    </v-main>
  </v-layout>
</template>

<style lang="css" scoped>
.outer-div {
  min-height: 100vh;
  min-width: 100vw;
}
</style>
