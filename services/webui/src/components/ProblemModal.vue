<template>
  <v-dialog
    v-model="internalShow"
    max-width="1400"
    scrollable
    persistent
  >
    <v-card>
      <v-card-title class="d-flex justify-space-between align-center">
        <span class="text-h5">{{ title }}</span>
        <v-btn icon="mdi-close" variant="text" @click="$emit('close')"></v-btn>
      </v-card-title>

      <v-divider></v-divider>

      <v-card-text style="max-height: 70vh;">
        <slot></slot>
      </v-card-text>

      <v-divider></v-divider>

      <v-card-actions class="pa-4">
        <v-spacer></v-spacer>
        <slot name="footer">
          <v-btn
            variant="text"
            @click="$emit('close')"
          >
            Отмена
          </v-btn>
          <v-btn
            color="primary"
            variant="elevated"
            :disabled="submitDisabled"
            @click="$emit('submit')"
          >
            {{ submitText }}
          </v-btn>
        </slot>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  show: boolean
  title: string
  submitText?: string
  submitDisabled?: boolean
}>()

const emit = defineEmits<{
  close: []
  submit: []
}>()

const internalShow = computed({
  get: () => props.show,
  set: (val) => {
    if (!val) emit('close')
  }
})
</script>

<style scoped>
</style>
