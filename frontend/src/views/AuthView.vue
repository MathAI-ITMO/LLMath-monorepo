<template>
  <div class="authview">
    <h1>Авторизация</h1>

    <div id="authForm">
      <div class="form-group row">
        <label for="authISU" class="col-sm-3 col-form-label">Номер ИСУ:</label>
        <div class="col-sm-9">
          <input type="text" class="form-control" id="authISU" aria-describedby="helpISU" placeholder="123456"
            v-model="isuLogin">
          <small id="helpISU" class="form-text text-muted">Введите номер ИСУ в формате 123456.</small>
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
        <div class="col-sm-9 align-middle">
          <input type="checkbox" class="form-check-input align-middle m-3" id="saveFlag" v-model="saveOption">
          <label class="form-check-label align-middle" for="saveFlag">Запомнить меня</label>
        </div>
      </div>

    </div>

    <div v-if="requestMessage !== ''">
      <h5>
        Отправился бы следующий запрос:
      </h5>
      <pre>
        {{ requestMessage }}
      </pre>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, type Ref } from 'vue';

const requestMessage: Ref<string> = ref("");
const isuLogin: Ref<string> = ref("");
const password: Ref<string> = ref("");
const saveOption: Ref<boolean> = ref(false);


onMounted(() => { requestMessage.value = ""; })

function onAuth(evt: Event) {
  const req = {
    'ISU': isuLogin.value,
    'password': password.value,
    'saveOption': saveOption.value,
  };

  requestMessage.value = JSON.stringify(req);
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
