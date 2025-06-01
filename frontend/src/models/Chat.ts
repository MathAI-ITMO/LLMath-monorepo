export interface Chat {
  id: string;
  name: string;
  type: 'ProblemSolver' | 'Chat';
  taskType?: number;
}
