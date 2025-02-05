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

  <WelcomeItem>
    <i> Перспективные модели повышения доступности и качества образования на базе развития электронного обучения во всем
      мире приводят к созданию новых организационных форм взаимодействия университетов между собой, с бизнесом и
      потребителями образовательных услуг. Резкий рост интереса к массовым открытым онлайн-курсам привел к началу этапа
      формирования международных образовательных сетей и площадок открытого образования, развитию высокотехнологичных
      обучающих платформ, которые сочетают в себе имитацию среды профессиональной деятельности, совместную работу и
      оценку результатов обучения.</i>
  </WelcomeItem>

    <WelcomeItem>
    <i>Домики набигают...</i>
  </WelcomeItem>
</template>
