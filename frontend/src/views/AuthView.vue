<template>
  <div class="authview">
    <h1>Авторизация</h1>

    <div id="authForm">
      <div class="form-group row">
        <label for="email" class="col-sm-3 col-form-label">Номер ИСУ:</label>
        <div class="col-sm-9">
          <input type="text" class="form-control" id="email" aria-describedby="helpemail" placeholder="test@test.test"
            v-model="email">
          <small id="helpemail" class="form-text text-muted">Введите email</small>
        </div>

      </div>
      <div class="form-group row">
        <label for="authPassword" class="col-sm-3 col-form-label">Пароль:</label>
        <div class="col-sm-9">
          <input type="password" class="form-control" id="authPassword" placeholder="Пароль" v-model="password">
        </div>
      </div>
      <div class="form-group row">
        <div class="col-sm-3">
          <button class="btn btn-outline-success" @click="onAuth">Войти</button>
        </div>
      </div>

    </div>

    <div v-if="errorMessage !== ''">
      <pre>
        {{ errorMessage }}
      </pre>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, type Ref, inject } from 'vue';
import { AuthService } from '@/services/AuthService'
import router from '@/router'

const authService = new AuthService(import.meta.env.VITE_MATHLLM_BACKEND_ADDRES)

const errorMessage: Ref<string> = ref("");
const email: Ref<string> = ref("");
const password: Ref<string> = ref("");

const refreshAuthInHeader = inject('refreshAuthInHeader');

onMounted(() => {
    errorMessage.value = "";
    authService.login(email.value, password.value)
    .then(res =>
    {
      if (res !== null)
      {
        refreshAuthInHeader()
        router.push('/');
      }
    })
  })

function onAuth() {
  authService.login(email.value, password.value)
  .then((resp) =>
  {
    if(resp !== null)
    {
      refreshAuthInHeader()
      router.push('/');
    }

  }
  )
  .catch(err =>
  {
    console.log(err);
  })

  errorMessage.value = "Неверный логин или пароль"
  email.value = ''
  password.value= ''
}

</script>

<style lang="css" scoped>
/* #authForm * input {
  margin: 5px;
}

#authForm>button {
  margin: 15px;
} */
</style>
