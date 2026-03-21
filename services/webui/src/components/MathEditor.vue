<template>
  <div class="math-editor">
    <div class="editor-panel">
      <div class="panel-header">
        <span class="panel-title">Редактор</span>
        <span class="panel-hint">Используйте $ для формул</span>
      </div>
      <textarea
        :value="modelValue"
        @input="$emit('update:modelValue', ($event.target as HTMLTextAreaElement).value)"
        :placeholder="placeholder"
        :rows="rows"
        class="editor-textarea"
      ></textarea>
    </div>
    <div class="preview-panel">
      <div class="panel-header">
        <span class="panel-title">Предпросмотр</span>
      </div>
      <div class="preview-content" v-html="renderedHtml"></div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { renderMessage } from '@/utils/renderMessage'

const props = defineProps<{
  modelValue: string
  placeholder?: string
  rows?: number
}>()

defineEmits<{
  'update:modelValue': [value: string]
}>()

const renderedHtml = computed(() => {
  if (!props.modelValue) return '<span class="empty-preview">Предпросмотр появится здесь...</span>'
  return renderMessage(props.modelValue)
})
</script>

<style scoped>
.math-editor {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  overflow: hidden;
  background: #fafafa;
}

.editor-panel,
.preview-panel {
  display: flex;
  flex-direction: column;
  min-height: 200px;
}

.panel-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 12px;
  background: #f5f5f5;
  border-bottom: 1px solid #e0e0e0;
  font-size: 12px;
  font-weight: 600;
  color: #666;
}

.panel-title {
  color: #333;
}

.panel-hint {
  font-weight: 400;
  font-style: italic;
  color: #999;
}

.editor-textarea {
  flex: 1;
  padding: 12px;
  border: none;
  resize: vertical;
  font-family: 'Consolas', 'Monaco', monospace;
  font-size: 14px;
  line-height: 1.6;
  background: white;
  color: #333;
  outline: none;
}

.editor-textarea:focus {
  background: #fffef8;
}

.preview-panel {
  background: white;
}

.preview-content {
  flex: 1;
  padding: 12px;
  overflow-y: auto;
  line-height: 1.8;
  font-size: 14px;
  color: #333;
}

.empty-preview {
  color: #999;
  font-style: italic;
}

/* Стили для формул в превью */
.preview-content :deep(.katex) {
  font-size: 1.1em;
}

.preview-content :deep(.katex-display) {
  margin: 1em 0;
}

.preview-content :deep(b) {
  color: #1976d2;
  font-weight: 600;
}

.preview-content :deep(hr) {
  margin: 1em 0;
  border: none;
  border-top: 1px solid #e0e0e0;
}

@media (max-width: 768px) {
  .math-editor {
    grid-template-columns: 1fr;
  }
  
  .preview-panel {
    border-top: 1px solid #e0e0e0;
  }
}
</style>
