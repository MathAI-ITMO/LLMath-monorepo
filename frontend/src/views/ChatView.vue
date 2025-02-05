<!-- ChatView.vue -->
<template>
  <div class="chatview container-fluid p-0">
    <div class="row align-items-center mb-2">
      <div class="col-12">
        <button class="back-button btn btn-success" @click="goHome">Назад</button>
        <h1 class="d-inline-block ms-3 mb-0">Chat View.</h1>
      </div>
    </div>
    <main>
      <gradio-lite class="w-100 gradio-wrapper">
        <gradio-file name="app.py" entrypoint>
          {{ mainCode }}
        </gradio-file>
      </gradio-lite>
    </main>
  </div>
</template>

<script setup lang="ts">
import { ref, type Ref } from "vue";
import { useRouter } from "vue-router";
import mainCodeContent from "@/assets/python/gradio/main.py?raw";

const router = useRouter();
const goHome = () => {
  router.push("/");
};

const mainCode: Ref<string> = ref(mainCodeContent);
</script>

<style lang="css" scoped>
.back-button {
  margin: 10px;
  border-radius: 5px;
}

.gradio-wrapper {
  min-width: 100%;
  min-height: 90vh;
}

.chatview {
  min-width: 60vw;
  overflow-x: hidden;
}
</style>
