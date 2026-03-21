import { AxiosError } from 'axios'
import type { Chat } from '@/models/Chat'
import type { Message } from '@/models/Message'
import type {
  ChatDto,
  MessageDto,
  CreateChatRequestDto,
  MessageCreateDto,
} from '@/api/generated/api'
import { api } from '@/api'

export function useChat() {
  async function createChat(dto: CreateChatRequestDto): Promise<string> {
    const res = await api.postApiChatCreate(dto)
    return (res.data as unknown as { id: string }).id
  }

  async function deleteChat(id: string): Promise<void> {
    try {
      await api.postApiChatDeleteId(id)
    } catch (error) {
      if (error instanceof AxiosError && error.response?.status === 404) {
        return
      }
      throw error
    }
  }

  async function getChats(): Promise<Chat[]> {
    const resp = await api.getApiChatGet()
    const chats = resp.data as unknown as ChatDto[]
    return chats.map(
      (c) => ({ id: c.id, name: c.name, type: c.type, theoryLink: c.theoryLink }) as Chat,
    )
  }

  async function getChatById(id: string): Promise<Chat | undefined> {
    try {
      const resp = await api.getApiChatGetChatId(id)
      const data = resp.data as unknown as ChatDto
      if (data) {
        return {
          id: data.id,
          name: data.name,
          type: data.type,
          taskType: data.taskType,
          theoryLink: data.theoryLink,
        } as Chat
      }
      return undefined
    } catch (error) {
      console.error(`Error fetching chat by ID ${id}:`, error)
      return undefined
    }
  }

  async function getNextMessage(text: string, chatId: string): Promise<string> {
    const dto: MessageCreateDto = { chatId, text }
    const resp = await api.postApiMessageComplete(dto)
    const data = resp.data as unknown as string
    return data
  }

  async function getChatMessages(chatId: string): Promise<Message[]> {
    const resp = await api.getApiMessageGetMessagesFromChat({ chatId })
    const messages = resp.data as unknown as MessageDto[]
    return messages.map(
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
