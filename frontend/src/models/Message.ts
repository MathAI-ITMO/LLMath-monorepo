export interface Message {
  id: string,
  chatId: string,
  type: string, //bot or user
  text: string,
  time: Date
}
