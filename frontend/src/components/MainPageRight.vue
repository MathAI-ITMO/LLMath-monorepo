<script setup lang="ts">
import WelcomeItem from './WelcomeItem.vue'
import ToolingIcon from './icons/IconTooling.vue'
import EcosystemIcon from './icons/IconEcosystem.vue'

/**
 * onSampleButtonClicked
 */
function onSampleButtonClicked(evt: Event) {
  console.log(evt);
  alert("Вы кликнули по кнопке. Event в консоли.")
}

</script>

<template>
  <WelcomeItem>
    <template #icon>
      <EcosystemIcon />
    </template>

    Какая-то очень шаблонная страница, где будет много букв о том, зачем всё это нужно.
  </WelcomeItem>

  <WelcomeItem>
    <template #icon>
      <ToolingIcon />
    </template>

    Можно разбивать по подпунктам. <s>Можно грабить корованы.</s>
  </WelcomeItem>

  <WelcomeItem>
    <template #icon>
      <button class="btn btn-outline-primary" type="button" data-bs-toggle="tooltip" data-bs-placement="right"
        title="Reverse selection" @click="onSampleButtonClicked">
        <i class="bi bi-arrow-repeat"></i>
      </button>
    </template>

    Можно использовать иконки от Bootstrap.
  </WelcomeItem>
</template>
