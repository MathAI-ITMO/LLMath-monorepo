import axios from 'axios'
import type { Chat } from '@/models/Chat'
import type { Message } from '@/models/Message'
import type { ChatDto, CreateChatDto, MessageDto, SendMessageRequestDto, ProblemsResponseDto, ProblemDto } from '@/types/BackendDtos'
import { createBackendApiClient } from '@/utils/apiClient'

interface Stream<T> {
  [Symbol.asyncIterator](): AsyncIterator<T>;
}

export function useChat() {
  const client = createBackendApiClient()

  async function createChat(dto: CreateChatDto): Promise<string> {
    console.log(dto)
    const res = await client.post('/api/chat/create', dto)
    return res.data.id
  }

  async function deleteChat(id: string): Promise<void> {
    try {
      await client.post(`/api/chat/delete/${id}`, {})
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.status === 404) {
        return
      }
      throw error
    }
  }

  async function getChats(): Promise<Chat[]> {
    const resp = await client.get<ChatDto[]>('/api/chat/get', { withCredentials: true })
    return resp.data.map((c) => ({ id: c.id, name: c.name, type: c.type, theoryLink: c.theoryLink }) as Chat)
  }

  async function getChatById(id: string): Promise<Chat | undefined> {
    try {
      const resp = await client.get<ChatDto>(`/api/chat/get/${id}`);
      if (resp.data) {
        return {
          id: resp.data.id,
          name: resp.data.name,
          type: resp.data.type,
          taskType: resp.data.taskType,
          theoryLink: resp.data.theoryLink
        } as Chat;
      }
      return undefined;
    } catch (error) {
      console.error(`Error fetching chat by ID ${id}:`, error);
      return undefined;
    }
  }

  async function getNextMessage(text: string, chatId: string): Promise<string> {
    const dto: SendMessageRequestDto = { chatId, text }
    const resp = await client.post<string>('/api/Message/complete', dto)

    // Логируем ответ в консоль для диагностики
    console.log('🔍 LLM Response:', resp.data);

    return resp.data
  }

  async function getChatMessages(chatId: string): Promise<Message[]> {
    const resp = await client.get<MessageDto[]>(
      `/api/Message/get-messages-from-chat?chatId=${chatId}`
    )
    return resp.data.map(
      (m) =>
        ({
          id: m.id,
          time: m.creationTime,
          text: m.text,
          type: m.type === 'Assistant' ? 'bot' : 'user',
        }) as Message,
    )
  }

  return {
    createChat,
    deleteChat,
    getChats,
    getChatById,
    getNextMessage,
    getChatMessages
  }
}
