<script setup lang="ts">
import WelcomeItem from './WelcomeItem.vue'
import ToolingIcon from './icons/IconTooling.vue'
import EcosystemIcon from './icons/IconEcosystem.vue'
</script>

<template>
  <div class="welcome-container">
    <v-card class="mx-auto pa-4" max-width="800">
      <WelcomeItem>
        <template #icon>
          <EcosystemIcon />
        </template>
        <template #heading>О проекте</template>
        Какая-то очень шаблонная страница, где будет много букв о том, зачем всё это нужно.
      </WelcomeItem>

      <WelcomeItem>
        <template #icon>
          <ToolingIcon />
        </template>
        <template #heading>Возможности</template>
        Можно разбивать по подпунктам. <s>Можно грабить корованы.</s>
      </WelcomeItem>

      <WelcomeItem>
        <template #icon>
          <v-btn icon variant="outlined" color="primary">
            <v-icon>mdi-refresh</v-icon>
          </v-btn>
        </template>
        <template #heading>Интерфейс</template>
        Можно использовать иконки от Bootstrap.
      </WelcomeItem>

      <WelcomeItem>
        <template #heading>Описание</template>
        <p class="text-body-1">
          Перспективные модели повышения доступности и качества образования на базе развития электронного обучения во всем
          мире приводят к созданию новых организационных форм взаимодействия университетов между собой, с бизнесом и
          потребителями образовательных услуг. Резкий рост интереса к массовым открытым онлайн-курсам привел к началу этапа
          формирования международных образовательных сетей и площадок открытого образования, развитию высокотехнологичных
          обучающих платформ, которые сочетают в себе имитацию среды профессиональной деятельности, совместную работу и
          оценку результатов обучения.
        </p>
      </WelcomeItem>

      <WelcomeItem>
        <template #heading>Статус</template>
        <p class="text-body-1">Домики набигают...</p>
      </WelcomeItem>
    </v-card>
  </div>
</template>

<style scoped>
.welcome-container {
  padding: 2rem;
  max-width: 1200px;
  margin: 0 auto;
  max-height: 100dvh;
}
</style>
