<template>
  <div :class="classes" v-html="html" />
  <small class="message-time">{{ time }}</small>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import moment from 'moment'
import { renderMessage } from '@/utils/renderMessage'
import type { Message } from '@/models/Message'

const props = defineProps<{ message: Message }>()

const html   = computed(() => renderMessage(props.message.text))
const time   = computed(() => moment(props.message.time).fromNow())
const classes = computed(() => ({
  'message-item' : true,
  'user-message' : props.message.type === 'user',
  'problem-condition-message':
      props.message.type === 'bot' && props.message.text.includes('Условие задачи'),
  'bot-message':
      props.message.type === 'bot' && !props.message.text.includes('Условие задачи')
}))
</script>

<style scoped>
.message-item {
  min-height: 0;
  padding: 0.25rem 0;
  border-radius: 1rem !important;
  margin-bottom: 0.5rem;
  overflow: hidden;
}

.message-item::before,
.message-item::after {
  display: none;
}

.message-time {
  display: block;
  font-size: 0.7rem;
  color: rgba(var(--v-theme-on-surface), 0.4);
  margin-top: 0.25rem;
}

.user-message {
  text-align: left;
  border-radius: 1.25rem !important;
  padding: 0.75rem 1rem;
  margin: 0.5rem 0 0.5rem auto;
  max-width: 80%;
  background: rgba(var(--v-theme-primary), 0.25);
  overflow: hidden;
}

.bot-message {
  text-align: left;
  border-radius: 1rem;
  padding: 0.75rem 1rem 0.5rem 1rem;
  margin: 0.5rem 0;
  max-width: 80%;
  background: transparent;
  color: rgba(var(--v-theme-on-surface), 0.87);
}

.problem-condition-message {
  background: #1E293B !important; /* Темный серо-синий фон */
  color: white !important;
  border-radius: 1rem !important;
  padding: 1rem !important;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
  margin-bottom: 1.5rem;
  max-width: 95% !important; /* Чуть шире обычных сообщений */
}

.problem-condition-message :deep(h1),
.problem-condition-message :deep(h2),
.problem-condition-message :deep(h3),
.problem-condition-message :deep(h4),
.problem-condition-message :deep(h5),
.problem-condition-message :deep(h6) {
  color: #50E3C2; /* Бирюзовый цвет для заголовков внутри условия */
}

.problem-condition-message :deep(.katex) {
  color: #F0F4F8; /* Светлый цвет для формул внутри условия */
}

/* Выделяем зеленую метку условия задачи */
.problem-condition-message :deep(b) {
  color: #50E3C2;
  font-size: 1.1em;
}

/* Стили для правильного отображения матриц KaTeX */
:deep(.katex) {
  display: inline-block;
}

:deep(.katex .base) {
  display: inline-block;
}
</style> 