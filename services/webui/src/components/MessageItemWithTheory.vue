<template>
  <!-- When theory link exists: enhanced layout with tabs + sliding panel -->
  <template v-if="hasTheory">
    <div :class="[classes, 'with-theory']">
      <!-- Tabs -->
      <div class="tp-tabs">
        <button
          class="tp-tab"
          :class="{ active: activeTab === 'practice' }"
          @click="setTab('practice')"
        >практика</button>
        <button
          class="tp-tab"
          :class="{ active: activeTab === 'theory' }"
          @click="setTab('theory')"
        >теория</button>
      </div>

      <!-- Sliding theory panel on the left -->
      <div class="tp-panel" :class="{ open: activeTab === 'theory' }">
        <!-- Visible only when collapsed -->
        <div v-if="activeTab !== 'theory'" class="tp-strip">
          <span class="tp-strip-text">теория</span>
        </div>
        <iframe
          v-show="activeTab === 'theory'"
          class="tp-iframe"
          :src="theoryUrlFull"
          title="codai theory"
          frameborder="0"
          allow="accelerometer; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
          allowfullscreen
        ></iframe>
      </div>

      <!-- Message content (codai link line removed) -->
      <div class="tp-content" v-html="html"></div>
    </div>
    <small class="message-time">{{ time }}</small>
  </template>

  <!-- Fallback: regular message rendering -->
  <template v-else>
    <div :class="classes" v-html="html" />
    <small class="message-time">{{ time }}</small>
  </template>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import moment from 'moment'
import { renderMessage } from '@/utils/renderMessage'
import type { Message } from '@/models/Message'

const props = defineProps<{ message: Message }>()

// Detect codai.ru mp4 link in the original text
const codaiRe = /(?:https?:\/\/)?codai\.ru\/[\w\-/.%?#=&]+\.mp4\b/i
const foundMatch = computed(() => props.message.text.match(codaiRe))
const theoryUrlFull = computed(() => {
  const m = foundMatch.value?.[0]
  if (!m) return ''
  return m.startsWith('http') ? m : `https://${m}`
})

// Remove any lines containing codai link from text before rendering
const textWithoutCodai = computed(() => {
  if (!foundMatch.value) return props.message.text
  return props.message.text
    .split(/\r?\n/)
    .filter(line => !codaiRe.test(line))
    .join('\n')
})

const html   = computed(() => renderMessage(textWithoutCodai.value))
const time   = computed(() => moment(props.message.time).fromNow())
const classes = computed(() => ({
  'message-item' : true,
  'user-message' : props.message.type === 'user',
  // "problem-condition" messages are styled differently (color scheme)
  'problem-condition-message': props.message.type === 'bot' && props.message.text.includes('условие задачи'),
  'bot-message': props.message.type === 'bot' && !props.message.text.includes('условие задачи')
}))

// Tabs / panel state
const activeTab = ref<'practice' | 'theory'>('practice')
const hasTheory = computed(() => !!theoryUrlFull.value)
function setTab(tab: 'practice' | 'theory') {
  activeTab.value = tab
}
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
  background: #1E293B !important;
  color: white !important;
  border-radius: 1rem !important;
  padding: 1rem !important;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
  margin-bottom: 1.5rem;
  max-width: 95% !important;
}

.problem-condition-message :deep(h1),
.problem-condition-message :deep(h2),
.problem-condition-message :deep(h3),
.problem-condition-message :deep(h4),
.problem-condition-message :deep(h5),
.problem-condition-message :deep(h6) {
  color: #50E3C2;
}

.problem-condition-message :deep(.katex) {
  color: #F0F4F8;
}

/* Bold accent in condition */
.problem-condition-message :deep(b) {
  color: #50E3C2;
  font-size: 1.1em;
}

/* KaTeX inline consistency */
:deep(.katex) {
  display: inline-block;
}

:deep(.katex .base) {
  display: inline-block;
}

/* --- Theory integration styles --- */
.with-theory {
  position: relative;
  overflow: visible; /* allow sliding panel to extend out */
}

.tp-tabs {
  display: inline-flex;
  gap: 0.25rem;
  margin-bottom: 0.5rem;
  background: rgba(255,255,255,0.06);
  border-radius: 0.75rem;
  padding: 0.25rem;
}

.tp-tab {
  appearance: none;
  border: none;
  padding: 0.35rem 0.9rem;
  border-radius: 0.6rem;
  font-size: 0.9rem;
  color: rgba(255,255,255,0.85);
  background: transparent;
  cursor: pointer;
  transition: all 0.2s ease;
}

.tp-tab.active {
  background: rgba(80, 227, 194, 0.22);
  color: #fff;
}

.tp-panel {
  position: absolute;
  top: 0.5rem;
  left: 0;
  width: min(42vw, 520px);
  max-width: 90%;
  height: min(70vh, 560px);
  background: #0f172a; /* slate-900 */
  border: 1px solid rgba(255,255,255,0.08);
  border-left: none;
  border-radius: 0 1rem 1rem 0;
  box-shadow: 0 6px 24px rgba(0,0,0,0.35);
  transform: translateX(calc(-100% + 26px));
  transition: transform 280ms ease;
  z-index: 5;
  overflow: hidden;
}

.tp-panel.open {
  transform: translateX(0);
}

.tp-strip {
  position: absolute;
  left: 0;
  top: 0;
  bottom: 0;
  width: 26px;
  background: rgba(255,255,255,0.9);
  color: #0f172a;
  display: flex;
  align-items: center;
  justify-content: center;
}

.tp-strip-text {
  writing-mode: vertical-rl;
  transform: rotate(180deg);
  letter-spacing: 0.075em;
  font-weight: 600;
}

.tp-iframe {
  width: 100%;
  height: 100%;
  border: 0;
}

.tp-content {
  position: relative;
}
</style>
