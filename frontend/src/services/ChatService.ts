import type { Chat } from '@/models/Chat';
import type { Message } from '@/models/Message';
import moment from 'moment'

class ChatService {
    private id: number;
    private chats: Chat[];
    private messages: Message[];

    constructor() {
      this.id = 0;
        this.chats = [
            { id: this.id++, name: "General Discussion" },
            { id: this.id++, name: "Project Updates" },
            { id: this.id++, name: "Team Meetings" }
        ];
      this.messages = [
        { id: 1, chatId: 0, type: "bot", text: "Hello, welcome to the chat!", time: new Date().toISOString() },
        { id: 2, chatId: 0, type: "user", text: "Hi there, how can I help you today?", time: new Date().toISOString() },
        { id: 3, chatId: 0, type: "bot", text: "I'm here to answer any questions you have.", time: new Date().toISOString() }
      ];
  
    }

    addChat(chat: Chat): void {
      console.log(this.chats)
        this.chats.push(chat);
        console.log(this)

    }

    deleteChat(id: number): void {
      this.chats = this.chats.filter(c => c.id != id)
    }

    getChats(): Chat[] {
        return this.chats;
    }

    getChatById(id: number): Chat | undefined {
        console.log(this.chats)
        return this.chats.find((chat) => chat.id === id);
    }

    createChat(name: string): number
    {
      const newId = this.id++
      const chat: Chat = {id: newId, name: name };
      this.chats.push(chat)
      return newId
    }

    sendMessage(text: string, chatId: number): void
    {
      const currentDate = moment();
      const id = this.id++;
      const message: Message = { id: id, chatId: chatId, type: "user", text: text, time: currentDate.toISOString() }
      this.messages.push(message)
    }

    getChatMessages(id: number): Message[]
    {
        return this.messages.filter(message => message.chatId === id);
    }
}

export const chatService = new ChatService();
