import { ref, watch, onMounted, computed, nextTick } from 'vue'
import type { Chat } from '@/models/Chat'
import type { Message } from '@/models/Message'
import { useChat } from '@/composables/useChat'
import { useRouter, useRoute } from 'vue-router'
import type { ProblemDto, CreateChatDto } from '@/types/BackendDtos'
import { useUserTasks } from '@/composables/useUserTasks'
import { UserTaskStatus } from '@/types/BackendDtos'

export function useChatView(props: { chatId?: string }, emit: any) {
  const route = useRoute()
  const router = useRouter()

  const { getChatById, getChatMessages, getNextMessage, createChat, getChats, deleteChat } = useChat()
  const { completeUserTask, fetchUserTasks } = useUserTasks()

  const chatId = ref<string | undefined>(props.chatId)

  const messagesCard = ref<HTMLElement | null>(null)
  const sidebarOpen = ref<boolean>(false)
  const chats = ref<Chat[]>([])

  const chat = ref<Chat>()
  const messages = ref<Message[]>([])
  const currentMessageText = ref<string>('')
  const isSending = ref<boolean>(false)

  const userTaskId = ref<number | null>(null)
  const taskStatus = ref<UserTaskStatus | null>(null)
  const taskType = ref<number | null>(null)
  const markingSolved = ref(false)

  function scrollToBottom() {
    nextTick(() => {
      if (messagesCard.value) {
        try {
          messagesCard.value.scrollTop = messagesCard.value.scrollHeight;
        } catch (error) {
          console.warn('Error scrolling to bottom:', error);
        }
      }
    });
  }

  async function onChatUpdate() {
    chats.value = await getChats();

    if (!chatId.value) {
      chat.value = undefined;
      messages.value = [];
      return;
    }

    const receivedChat = await getChatById(chatId.value);
    chat.value = receivedChat;
    const receivedMessages = await getChatMessages(chatId.value);

    messages.value = receivedMessages;

    console.log('Loaded', receivedMessages?.length || 0, 'messages for chat', chatId.value);
    scrollToBottom();
  }

  async function sendMessage() {
    if (!chatId?.value)
    {
      console.log('send message called but chat id not specified yet')
      return;
    }

    if (!currentMessageText.value)
    {
      // Using a more user-friendly way for errors in real apps is better than alert
      console.warn('Message cannot be sent because it is empty')
      return;
    }
    isSending.value = true

    const userMessage : Message = {
      id: `temp-user-${Date.now()}-${Math.random()}`,
      chatId: chatId!.value,
      type: 'user',
      text: currentMessageText.value,
      time: new Date()
    }

    messages.value.push(userMessage)
    scrollToBottom()

    const messageText = currentMessageText.value
    currentMessageText.value = ""

    try {
      const botResponseText = await getNextMessage(messageText, chatId!.value)

      const message : Message = {
        id: `temp-bot-${Date.now()}-${Math.random()}`,
        chatId: chatId!.value,
        type: 'bot',
        text: botResponseText,
        time: new Date()
      }

      messages.value.push(message)
      scrollToBottom()
    } finally {
      isSending.value = false
    }
  }

  async function onChatSelect(id: string) {
    console.log('chat with id ' + id + ' selected')
    emit('chatSelected', id)
    sidebarOpen.value = false
    chatId.value = id;
    await onChatUpdate();
  }

  async function onChatDelete(id: string) {
    await deleteChat(id)
    if (id === chatId.value) {
      chat.value = undefined
      messages.value = []
      chatId.value = undefined
      emit('chatDeleted')
      router.push('/chat')
    }
    await onChatUpdate()
  }

  async function createNewChat() {
    const now = new Date()
    const year = now.getFullYear()
    const month = String(now.getMonth() + 1).padStart(2, '0')
    const day = String(now.getDate()).padStart(2, '0')
    const hours = String(now.getHours()).padStart(2, '0')
    const minutes = String(now.getMinutes()).padStart(2, '0')

    const defaultName = `Чат ${year}-${month}-${day} ${hours}:${minutes}`

    const chatName = window.prompt('Введите название чата', defaultName)

    if (chatName === null) {
      // User cancelled
      return
    }

    const dto: CreateChatDto = {
      name: chatName || defaultName,
      type: 'Chat'
    }
    const newChatId = await createChat(dto)
    await onChatUpdate()
    router.push(`/chat/${newChatId}`)
  }

  async function updateTaskInfo() {
    if (!chatId.value) {
      userTaskId.value = null;
      taskStatus.value = null;
      taskType.value = null;
      return;
    }
      const tasks0 = await fetchUserTasks(0);
      let task = tasks0.find(t => t.associatedChatId === chatId.value);

      if (!task) {
        for (let i = 1; i <= 3; i++) {
          const tasksI = await fetchUserTasks(i);
          task = tasksI.find(t => t.associatedChatId === chatId.value);
          if (task) {
            taskType.value = i;
            break;
          }
        }
      } else {
        taskType.value = 0;
      }

      if (task) {
        userTaskId.value = task.id as unknown as number; // id is number
        taskStatus.value = task.status;
      } else {
        // Это обычный чат, не связанный с задачей
        userTaskId.value = null;
        taskStatus.value = null;
        taskType.value = null;
      }
  }

  async function markTaskSolved() {
    if (!userTaskId.value) return;
    markingSolved.value = true;
    try {
      const res = await completeUserTask(userTaskId.value);
      if (res) {
        taskStatus.value = UserTaskStatus.Solved;
        // Переходим к списку задач с правильным типом
        if (taskType.value !== null) {
          router.push(`/select-task?taskType=${taskType.value}`);
        } else {
          router.push('/select-task');
        }
      }
    } finally {
      markingSolved.value = false;
    }
  }

  onMounted(async () => {
    chatId.value = route.params.chatId as string | undefined;
    emit('update:chatId', chatId.value);
    await onChatUpdate();
    await updateTaskInfo();
  })

  watch(() => route.params.chatId, async (newChatId) => {
    console.log('New chatId from URL:', newChatId);
    chatId.value = newChatId as string | undefined;
    emit('update:chatId', chatId.value);
    await onChatUpdate();
  })

  watch(chatId, () => {
    updateTaskInfo();
  });

  return {
    chatId,
    messagesCard,
    sidebarOpen,
    chats,
    chat,
    messages,
    currentMessageText,
    isSending,
    userTaskId,
    taskStatus,
    taskType,
    markingSolved,
    scrollToBottom,
    onChatUpdate,
    sendMessage,
    onChatSelect,
    onChatDelete,
    createNewChat,
    updateTaskInfo,
    markTaskSolved,
    UserTaskStatus
  }
}
