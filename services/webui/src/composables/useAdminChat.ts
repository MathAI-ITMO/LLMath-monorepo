import { ref, onMounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useChat } from '@/composables/useChat';
import type { Chat } from '@/models/Chat';
import type { Message } from '@/models/Message';
import { backendApi } from '@/utils/apiClient';

export function useAdminChat() {
  const route = useRoute();
  const router = useRouter();
  const { getChatById, getChatMessages } = useChat();

  const chatId = ref<string | undefined>();
  const chat = ref<Chat>();
  const messages = ref<Message[]>([]);
  const messagesCard = ref<HTMLElement | null>(null);
  const taskModeTitles = ref<Record<string, string>>({});
  const taskModeTitlesReady = ref(false);

  async function loadChatData() {
    if (!chatId.value) return;

    const receivedChat = await getChatById(chatId.value);
    chat.value = receivedChat;
    console.log('Loaded chat details:', receivedChat);

    const receivedMessages = await getChatMessages(chatId.value);
    messages.value = receivedMessages;

    scrollToBottom();
  }

  function scrollToBottom() {
    setTimeout(() => {
      window.scrollTo(0, document.body.scrollHeight);
    }, 100);
  }

  function goBack() {
    router.go(-1);
  }

  const formatTaskTypeForChat = (type: number | undefined): string => {
    if (type === undefined) return 'Тип задачи не определен';
    const typeStr = type.toString();
    if (taskModeTitles.value && taskModeTitles.value[typeStr]) {
      return taskModeTitles.value[typeStr];
    }
    if (type === 0) return 'Упражнение (из списка)';
    return `Тип задачи (${type})`;
  };

  onMounted(async () => {
    const titlesResponse = await backendApi.get<Record<string, string>>('/api/stats/task-mode-titles');
    taskModeTitles.value = titlesResponse.data;
    taskModeTitlesReady.value = true;
    console.log('AdminChatView: Loaded task mode titles:', taskModeTitles.value);

    chatId.value = route.params.chatId as string;
    if (chatId.value) {
      await loadChatData();
    }
  });

  watch(() => route.params.chatId, async (newChatId) => {
    chatId.value = newChatId as string;
    if (chatId.value) {
      await loadChatData();
    }
  });

  return {
    chatId,
    chat,
    messages,
    messagesCard,
    taskModeTitlesReady,
    formatTaskTypeForChat,
    goBack,
    loadChatData,
    scrollToBottom
  };
}
