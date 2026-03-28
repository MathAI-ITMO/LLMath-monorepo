export interface VideoInfo {
  name: string
  url: string
}

export interface SubtitleSegment {
  start: number
  end: number
  text: string
}

export interface SuggestionItem {
  key: string
  text: string
  startSec: number
  endSec: number
}

export interface ChatMessage {
  role: 'student' | 'lecturer'
  text: string
  kind?: 'frame'
  normX?: number
  normY?: number
}
