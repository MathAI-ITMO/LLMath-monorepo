window.GRADIO_API_BASE = process.env.GRADIO_API_BASE;
console.log("GRADIO_API_BASE", window.GRADIO_API_BASE);

import "normalize.css";
import "bootstrap/dist/css/bootstrap.min.css";
import "bootstrap";
import "bootstrap-icons/font/bootstrap-icons.css";
import "@gradio/lite";
import "@gradio/lite/dist/lite.css";
import './assets/main.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'

const app = createApp(App)

app.use(createPinia())
app.use(router)

app.mount('#app')
