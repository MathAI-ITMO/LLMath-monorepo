import type { Chat } from '@/models/Chat';
import type { Message } from '@/models/Message';
import moment from 'moment'
import { AuthService } from './AuthService';
import axios, { type AxiosInstance } from 'axios';
import type { ChatDto, CreateChatDto, MessageDto, SendMessageDto, SendMessageRequestDto } from '@/types/BackendDtos';

export class ChatService {
    client: AxiosInstance;
    authSvc: AuthService;

    constructor(baseUrl: string) {
      this.client = axios.create({
        baseURL: baseUrl,
      });

      this.authSvc = new AuthService(baseUrl);
    }

    async createChat(name: string): Promise<number> {
      const options = await this.authSvc.getAuthOptions()
      const dto : CreateChatDto = {name: name}
      console.log(dto)
      const res = await this.client.post('api/chat/create-chat', dto, options)
      return res.data.id;
    }

    deleteChat(id: number): void {
    }

    async getChats(): Promise<Chat[]> {
      const options = await this.authSvc.getAuthOptions()
      const resp = await this.client.get<ChatDto[]>('/api/chat/get-chats', options)
      return resp.data.map(c => ({ id: c.id, name: c.name } as Chat))
    }

    async getChatById(id: number): Promise<Chat | undefined> {
      const options = await this.authSvc.getAuthOptions()
      const resp = await this.client.get<ChatDto[]>('/api/chat/get-chats', options)
      return resp.data
        .map(c => ({ id: c.id, name: c.name } as Chat))
        .find(c => c.id == id);
    }

    async sendMessage(text: string, chatId: number): Promise<void>
    {
      const options = await this.authSvc.getAuthOptions()
      const dto : SendMessageRequestDto = { chatId: chatId, text: text }
      await this.client.post<ChatDto[]>('/api/Message/send-message', dto, options)
    }

    async getChatMessages(id: number): Promise<Message[]>
    {
      const options = await this.authSvc.getAuthOptions()
      const resp = await this.client.get<MessageDto[]>(`api/Message/get-messages-from-chat?chatId=${id}`, options);
      return resp.data.map(m =>
      ({
        id: m.id,
        time: m.creationTime,
        text: m.text,
        type: m.type === 'Assistant' ? 'bot' : 'user'
      } as Message))
    }
}
