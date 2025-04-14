<template>
  <v-layout class="rounded rounded-md fill-heights">
    <v-main>
      <div class="authview">
        <v-card class="mx-auto pa-6" max-width="600">
          <h1 class="text-h4 mb-6">{{ isLogin ? 'Авторизация' : 'Регистрация' }}</h1>

          <v-form @submit.prevent="onSubmit">
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

            <v-text-field
              v-if="!isLogin"
              v-model="confirmPassword"
              label="Подтвердите пароль"
              type="password"
              placeholder="Пароль"
              :rules="[
                v => !!v || 'Password confirmation is required',
                v => v === password || 'Passwords do not match'
              ]"
              required
            ></v-text-field>

            <v-btn
              color="success"
              type="submit"
              variant="outlined"
              class="mt-4"
              block
            >
              {{ isLogin ? 'Войти' : 'Зарегистрироваться' }}
            </v-btn>

            <v-btn
              variant="text"
              class="mt-2"
              block
              @click="isLogin = !isLogin"
            >
              {{ isLogin ? 'Создать аккаунт' : 'Уже есть аккаунт? Войти' }}
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

const { login, register } = useAuth()

const errorMessage: Ref<string> = ref("");
const email: Ref<string> = ref("");
const password: Ref<string> = ref("");
const confirmPassword: Ref<string> = ref("");
const isLogin: Ref<boolean> = ref(true);

function onSubmit() {
  if (isLogin.value) {
    onAuth();
  } else {
    onRegister();
  }
}

function onAuth() {
  login(email.value, password.value)
    .then((resp: void) => {
      if(resp !== null) {
        router.push('/');
        return;
      }
      errorMessage.value = "Неверный логин или пароль"
    })
    .catch((err: Error) => {
      errorMessage.value = "Неверный логин или пароль"
      console.log(err);
    })
}

function onRegister() {
  if (password.value !== confirmPassword.value) {
    errorMessage.value = "Пароли не совпадают";
    return;
  }

  register(email.value, password.value)
    .then((result) => {
      if (result.success) {
        router.push('/');
        return;
      }

      if (result.error) {
        if (result.error.errors) {
          // Handle validation errors
          const errorMessages = Object.values(result.error.errors)
            .flat()
            .join('\n');
          errorMessage.value = errorMessages;
        } else if (result.error.detail) {
          // Handle general error message
          errorMessage.value = result.error.detail;
        } else {
          errorMessage.value = "Ошибка при регистрации";
        }
      }
    })
    .catch((err: Error) => {
      errorMessage.value = "Ошибка при регистрации";
      console.log(err);
    });
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
