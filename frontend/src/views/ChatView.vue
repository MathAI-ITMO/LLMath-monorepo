<template>
  <div class="chatview">
    <button class="back-button btn-success" @click="goHome">Назад</button>
    <h1>Chat View.</h1>

    <main>
      <gradio-lite>
        <gradio-file name="app.py" entrypoint>
          import gradio as gr
          from utils import add

          demo = gr.Interface(fn=add, inputs=["number", "number"], outputs="number")

          demo.launch()
        </gradio-file>

        <gradio-file name="utils.py">
          def add(a, b):
          return a + b
        </gradio-file>
      </gradio-lite>
    </main>
  </div>
</template>

<script setup lang="ts">
//import "@gradio/lite";
//import "@gradio/lite/dist/lite.css";
import { useRouter } from "vue-router";

const router = useRouter();

const goHome = () => {
  router.push("/");
};
</script>

<style lang="css" scoped>
.back-button {
  position: absolute;
  top: 10px;
  left: 10px;
  background: #42b983;
  color: white;
  border: none;
  padding: 10px;
  cursor: pointer;
  border-radius: 5px;
}

.back-button:hover {
  background: #36936d;
}
</style>
