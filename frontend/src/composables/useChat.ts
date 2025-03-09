import axios, { type AxiosInstance } from 'axios'
import type { Chat } from '@/models/Chat'
import type { Message } from '@/models/Message'
import type { ChatDto, CreateChatDto, MessageDto, SendMessageRequestDto } from '@/types/BackendDtos'
import { Stream } from 'stream'

const baseUrl = import.meta.env.VITE_MATHLLM_BACKEND_ADDRESS

export function useChat() {
  const client: AxiosInstance = axios.create({
    baseURL: baseUrl,
  })

  async function createChat(name: string): Promise<string> {
    const dto: CreateChatDto = { name }
    console.log(dto)
    const res = await client.post('/api/chat/create-chat', dto, { withCredentials: true })
    return res.data.id
  }

  async function deleteChat(id: string): Promise<void> {
    await client.delete(`/api/chat/delete-chat/${id}`, { withCredentials: true })
  }

  async function getChats(): Promise<Chat[]> {
    const resp = await client.get<ChatDto[]>('/api/chat/get-chats', { withCredentials: true })
    return resp.data.map((c) => ({ id: c.id, name: c.name }) as Chat)
  }

  async function getChatById(id: string): Promise<Chat | undefined> {
    const resp = await client.get<ChatDto[]>('/api/chat/get-chats', { withCredentials: true })
    return resp.data.map((c) => ({ id: c.id, name: c.name }) as Chat).find((c) => c.id === id)
  }

  async function getNextMessage(text: string, chatId: string): Promise<Stream<string>> {
    const dto: SendMessageRequestDto = { chatId, text }
    const resp = await client.post<Stream<string>>('/api/Message/complete', dto, {
      withCredentials: true,
      headers: {
        responseType: 'stream',
      },
    })

    return resp.data
  }

  async function getChatMessages(chatId: number): Promise<Message[]> {
    const resp = await client.get<MessageDto[]>(
      `/api/Message/get-messages-from-chat?chatId=${chatId}`,
      { withCredentials: true },
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
    getChatMessages,
  }
}
