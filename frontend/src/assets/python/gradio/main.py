# Внимание! Хоть этот файл и написан на Python в стиле библиотеки Gradio, в реальности он будет исполняться при помощи @gradio/light на стороне веб-браузера и поддерживает не все фишки Python, да и не все библиотеки.
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
        f"**Жирный:** **{user_message}**\n\n"
        f"*Курсив:* *{user_message}*\n\n"
        f"<u>Подчёркнутый:</u> <u>{user_message}</u>\n\n"
        f"`Моноширинный:` `{user_message}`\n\n"
        r"Блочная Формула: $$\int\limits_{lol}^{kek} \frac{dx}{x \cdot e^{iz - \frac{\pi}{2}}} = \operatorname{mem}(x) + C$$"
        "\n\n"
        r"Строчная формула: $\frac{\partial}{\partial x} \sin(x) = \cos(x)$ почему-то может не работать."
        "\n\n"
    )
    if history is None:
        history = []
    history.append(gr.ChatMessage(role="user", content=user_message))
    if provide_opts:
        history.append(
            gr.ChatMessage(
                role="assistant",
                content=bot_response,
                options=[
                    {"label": "Вариант1", "value": "Опция1"},
                    {"label": "Вариант2", "value": "Опция2"},
                ],
            )
        )
    else:
        history.append(gr.ChatMessage(role="assistant", content=bot_response))
    return history, ""


with gr.Blocks(css=".gradio-container { padding: 1rem; }", theme="default") as demo:
    gr.Markdown("<h2 class='mb-3'>Пример работы чата</h2>")
    # Передаём height и type в конструкторе Chatbot
    chatbot = gr.Chatbot(
        elem_id="chatbot",
        # height=500,
        type="messages",
        latex_delimiters=[
            {"left": "$$", "right": "$$", "display": True},
            {"left": "$", "right": "$", "display": False},
        ],
        container=False,
        resizeable=True,
        autoscroll=True,
        editable="all",
        show_copy_button=True,
        show_copy_all_button=True,
        show_share_button=None,
    )
    with gr.Row():
        txt = gr.Textbox(
            show_label=False,
            placeholder="Введите сообщение...",
            container=False,
            elem_classes="form-control",
        )
        send_btn = gr.Button(
            "Отправить", variant="primary", elem_classes="btn btn-primary"
        )
    with gr.Row(elem_classes="mt-3"):
        gr.Markdown("<h5>Или выберите один из вариантов:</h5>")
        btn1 = gr.Button("Ответ с опциями", elem_classes="btn btn-secondary")
        btn2 = gr.Button("Ответ вместо ввода", elem_classes="btn btn-secondary")

    txt.submit(echo_chat, [txt, chatbot], [chatbot, txt], api_name="send_message")
    send_btn.click(echo_chat, [txt, chatbot], [chatbot, txt], api_name="send_message")
    btn1.click(
        lambda a, b: echo_chat(a, b, True),
        [txt, chatbot],
        [chatbot, txt],
        api_name="preset1",
    )
    btn2.click(
        lambda history: echo_chat("Это заранее подготовленный ответ 2", history, False),
        [chatbot],
        [chatbot, txt],
        api_name="preset2",
    )

    demo.launch(share=True)
