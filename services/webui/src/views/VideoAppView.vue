<template>
  <div class="video-app-container">
    <iframe
      :src="videoAppUrl"
      class="video-app-iframe"
      frameborder="0"
      allow="accelerometer; clipboard-write; encrypted-media; gyroscope; picture-in-picture; microphone"
      allowfullscreen
    ></iframe>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { servicesConfig } from '@/config/services.config';

const videoAppUrl = computed(() => servicesConfig.videoServiceUrl);
</script>

<style scoped>
.video-app-container {
  width: 100%;
  height: 100vh;
  margin: 0;
  padding: 0;
  overflow: hidden;
}

.video-app-iframe {
  width: 100%;
  height: 100%;
  border: none;
}
</style>
