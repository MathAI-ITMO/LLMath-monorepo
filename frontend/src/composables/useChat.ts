import axios, { type AxiosInstance } from 'axios';
import type { Chat } from '@/models/Chat';
import type { Message } from '@/models/Message';
import moment from 'moment';
import type { ChatDto, CreateChatDto, MessageDto, SendMessageRequestDto } from '@/types/BackendDtos';
import { useAuth } from '@/composables/useAuth';

const baseUrl = import.meta.env.VITE_MATHLLM_BACKEND_ADDRES;

export function useChat() {
  const client: AxiosInstance = axios.create({
    baseURL: baseUrl,
  });

  const { getAuthOptions } = useAuth();


  async function createChat(name: string): Promise<number> {
    const options = await getAuthOptions();
    const dto: CreateChatDto = { name };
    console.log(dto);
    const res = await client.post('/api/chat/create-chat', dto, options);
    return res.data.id;
  }

  function deleteChat(id: number): void {
    console.warn('deleteChat is not implemented.');
  }

  async function getChats(): Promise<Chat[]> {
    const options = await getAuthOptions();
    const resp = await client.get<ChatDto[]>('/api/chat/get-chats', options);
    return resp.data.map(c => ({ id: c.id, name: c.name } as Chat));
  }

  async function getChatById(id: number): Promise<Chat | undefined> {
    const options = await getAuthOptions();
    const resp = await client.get<ChatDto[]>('/api/chat/get-chats', options);
    return resp.data
      .map(c => ({ id: c.id, name: c.name } as Chat))
      .find(c => c.id === id);
  }

  async function sendMessage(text: string, chatId: number): Promise<void> {
    const options = await getAuthOptions();
    const dto: SendMessageRequestDto = { chatId, text };
    await client.post('/api/Message/send-message', dto, options);
  }

  async function getChatMessages(chatId: number): Promise<Message[]> {
    const options = await getAuthOptions();
    const resp = await client.get<MessageDto[]>(`/api/Message/get-messages-from-chat?chatId=${chatId}`, options);
    return resp.data.map(m => ({
      id: m.id,
      time: m.creationTime,
      text: m.text,
      type: m.type === 'Assistant' ? 'bot' : 'user'
    } as Message));
  }

  return {
    createChat,
    deleteChat,
    getChats,
    getChatById,
    sendMessage,
    getChatMessages,
  };
}
