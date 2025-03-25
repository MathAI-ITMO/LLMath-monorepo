<template>
  <v-layout class="rounded rounded-md fill-heights">
    <v-main>
      <div class="authview">
        <v-card class="mx-auto pa-6" max-width="600">
          <h1 class="text-h4 mb-6">Авторизация</h1>

          <v-form @submit.prevent="onAuth">
            <v-text-field
              v-model="email"
              label="Email"
              type="email"
              placeholder="test@test.test"
              :rules="[v => !!v || 'Email is required']"
              required
            ></v-text-field>

            <v-text-field
              v-model="password"
              label="Пароль"
              type="password"
              placeholder="Пароль"
              :rules="[v => !!v || 'Password is required']"
              required
            ></v-text-field>

            <v-btn
              color="success"
              type="submit"
              variant="outlined"
              class="mt-4"
              block
            >
              Войти
            </v-btn>
          </v-form>

          <v-alert
            v-if="errorMessage"
            type="error"
            class="mt-4"
            variant="tonal"
          >
            {{ errorMessage }}
          </v-alert>
        </v-card>
      </div>
    </v-main>
  </v-layout>
</template>

<script setup lang="ts">
import { ref, type Ref } from 'vue';
import { useAuth } from '@/composables/useAuth';
import router from '@/router'

const { login } = useAuth()

const errorMessage: Ref<string> = ref("");
const email: Ref<string> = ref("");
const password: Ref<string> = ref("");

function onAuth() {
  login(email.value, password.value)
    .then((resp) => {
      if(resp !== null) {
        router.push('/');
        return;
      }
      errorMessage.value = "Неверный логин или пароль"
    })
    .catch(err => {
      errorMessage.value = "Неверный логин или пароль"
      console.log(err);
    })
}
</script>

<style lang="css" scoped>
.authview {
  padding: 2rem;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
}
</style>
