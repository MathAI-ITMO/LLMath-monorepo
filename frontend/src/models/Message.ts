export interface Message {
  id: number,
  chatId: number,
  type: string, //bot or user
  text: string,
  time: string
}
