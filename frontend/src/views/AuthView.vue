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

            <template v-if="!isLogin">
              <v-text-field
                v-model="firstName"
                label="Имя"
                type="text"
                placeholder="Иван"
                :rules="[v => !!v || 'Имя обязательно']"
                maxlength="20"
                counter
                required
              ></v-text-field>

              <v-text-field
                v-model="lastName"
                label="Фамилия"
                type="text"
                placeholder="Иванов"
                :rules="[v => !!v || 'Фамилия обязательна']"
                maxlength="20"
                counter
                required
              ></v-text-field>

              <v-text-field
                v-model="studentGroup"
                label="Номер группы"
                type="text"
                placeholder="М-123"
                :rules="[v => !!v || 'Номер группы обязателен']"
                maxlength="20"
                counter
                required
              ></v-text-field>
            </template>

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
import { ref, type Ref, onMounted } from 'vue';
import { useAuth } from '@/composables/useAuth';
import router from '@/router';
import { useRoute } from 'vue-router';

const route = useRoute();

const { login, register } = useAuth();

const errorMessage: Ref<string> = ref("");
const email: Ref<string> = ref("");
const password: Ref<string> = ref("");
const confirmPassword: Ref<string> = ref("");
const firstName: Ref<string> = ref("");
const lastName: Ref<string> = ref("");
const studentGroup: Ref<string> = ref("");
const isLogin: Ref<boolean> = ref(true);

onMounted(() => {
  // Проверяем наличие параметра register в URL и переключаемся на форму регистрации
  if (route.query.register === 'true') {
    isLogin.value = false;
  }
});

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

  register(
    email.value, 
    password.value, 
    firstName.value, 
    lastName.value, 
    studentGroup.value
  )
    .then((result) => {
      if (result.success) {
        router.push('/');
        return;
      }

      if (result.error) {
        if (result.error.detail) {
          // Приоритет отображения конкретной ошибки
          errorMessage.value = result.error.detail;
        } else if (result.error.errors) {
          // Объединяем все сообщения об ошибках
          const errorMessages = Object.values(result.error.errors)
            .flat()
            .join('\n');
          errorMessage.value = errorMessages;
        } else {
          errorMessage.value = "Ошибка при регистрации";
        }
      }
    })
    .catch((err: Error) => {
      console.error("Непредвиденная ошибка:", err);
      errorMessage.value = "Ошибка при регистрации. Проверьте правильность введенных данных.";
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
