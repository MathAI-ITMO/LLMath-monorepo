<template>
  <VideoApp :filename="filename" />
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import VideoApp from '@/components/video/VideoApp.vue'

const route = useRoute()
const filename = computed(() => route.params.filename as string | undefined)
</script>
