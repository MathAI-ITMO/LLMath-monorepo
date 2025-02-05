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
        <gradio-file name="utils.py">
          {{ utilCode }}
        </gradio-file>
      </gradio-lite>
    </main>
  </div>
</template>

<script setup lang="ts">
import { ref, type Ref } from "vue";
import { useRouter } from "vue-router";

const router = useRouter();
const goHome = () => {
  router.push("/");
};

const mainCode: Ref<string> = ref("");
const utilCode: Ref<string> = ref("");

// Обновлённый код для Gradio-интерфейса (app.py)
// Формула теперь обёрнута в \\(...\\) для корректного рендеринга LaTeX.
mainCode.value = `
import gradio as gr

def echo_chat(user_message, history, provide_opts: bool = False):
    """
    Генерирует ответ, форматируя сообщение в Markdown:
      - Жирный текст
      - Курсив
      - Подчёркнутый текст (HTML)
      - Моноширинный текст
      - Формула LaTeX
    """
    bot_response = (
        f"**Жирный:** **{user_message}**\\n\\n"
        f"*Курсив:* *{user_message}*\\n\\n"
        f"<u>Подчёркнутый:</u> <u>{user_message}</u>\\n\\n"
        f"\`Моноширинный: \` \`{user_message}\`\\n\\n"
        r"Блочная Формула: $$\\int\\limits_{lol}^{kek} \\frac{dx}{x \\cdot e^{iz - \\frac{\\pi}{2}}} = \\operatorname{mem}(x) + C$$"
        "\\n"
        r"Строчная формула: $\\frac{\\partial}{\\partial x} \\sin(x) = \\cos(x)$ почему-то может не работать."
        "\\n"
    )
    if history is None:
        history = []
    history.append(gr.ChatMessage(role="user", content=user_message))
    if provide_opts:
      history.append(gr.ChatMessage(role="assistant", content=bot_response, options=[{"label": "Вариант1", "value": "Опция1"}, {"label": "Вариант2", "value": "Опция2"}]))
    else:
      history.append(gr.ChatMessage(role="assistant", content=bot_response))
    return history, ""

with gr.Blocks(css=".gradio-container { padding: 1rem; }", theme="default") as demo:
    gr.Markdown("<h2 class='mb-3'>Streaming Chat Echo Demo</h2>")
    # Передаём height и type в конструкторе Chatbot
    chatbot = gr.Chatbot(elem_id="chatbot", height=500, type="messages", latex_delimiters=[
        {"left": "$$", "right": "$$", "display": True}, 
        {"left": "$", "right": "$", "display": False },
        #{ "left": "$$", "right": "$$", "display": True }, 
    ])
    with gr.Row():
        txt = gr.Textbox(
            show_label=False,
            placeholder="Введите сообщение...",
            container=False,
            elem_classes="form-control"
        )
        send_btn = gr.Button("Отправить", variant="primary", elem_classes="btn btn-primary")
    with gr.Row(elem_classes="mt-3"):
        gr.Markdown("<h5>Или выберите один из вариантов:</h5>")
        btn1 = gr.Button("Ответ с опциями", elem_classes="btn btn-secondary")
        #btn2 = gr.Button("Ответ 2", elem_classes="btn btn-secondary")

    txt.submit(echo_chat, [txt, chatbot], [chatbot, txt], api_name="send_message")
    send_btn.click(echo_chat, [txt, chatbot], [chatbot, txt], api_name="send_message")
    btn1.click(lambda a, b: echo_chat(a,b,True), [txt, chatbot], [chatbot, txt], api_name="preset1")
    #btn2.click(lambda history: echo_chat("Это заранее подготовленный ответ 2", history),
    #           [chatbot], [chatbot], api_name="preset2")

    demo.launch(share=True)
`;

utilCode.value = `
# Здесь можно определить дополнительные функции, если потребуется.
# Пока оставляем файл пустым.
`;
</script>

<style lang="css" scoped>
.back-button {
  margin: 10px;
  border-radius: 5px;
}

.gradio-wrapper {
  width: 100%;
  min-height: 90vh;
}

.chatview {
  width: 60vw;
  overflow-x: hidden;
}
</style>
