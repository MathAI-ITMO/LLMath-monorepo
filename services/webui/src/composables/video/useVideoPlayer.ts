import { ref, watch } from 'vue'
import type { Ref } from 'vue'

export function useVideoPlayer(videoRef: Ref<HTMLVideoElement | null>) {
  const currentTime = ref(0)
  const duration = ref(0)
  const playing = ref(false)
  const volume = ref(1)
  const speed = ref(1)

  function formatTime(secs: number): string {
    const s = Math.floor(secs % 60).toString().padStart(2, '0')
    const m = Math.floor((secs / 60) % 60).toString().padStart(2, '0')
    const h = Math.floor(secs / 3600)
    return h > 0 ? `${h}:${m}:${s}` : `${m}:${s}`
  }

  function togglePlay() {
    const v = videoRef.value
    if (!v) return
    if (v.paused) v.play().catch(() => {})
    else v.pause()
  }

  function seek(t: number) {
    const v = videoRef.value
    if (!v) return
    v.currentTime = t
  }

  function setVolume(val: number) {
    const v = videoRef.value
    if (!v) return
    volume.value = val
    v.volume = val
    v.muted = val === 0
  }

  function setSpeed(val: number) {
    const v = videoRef.value
    if (!v) return
    speed.value = val
    v.playbackRate = val
  }

  function captureFrameWithMarker(normX: number, normY: number): string {
    const v = videoRef.value
    if (!v) return ''
    const maxH = 720
    const scale = v.videoHeight > maxH ? maxH / v.videoHeight : 1
    const w = Math.round(v.videoWidth * scale)
    const h = Math.round(v.videoHeight * scale)
    const canvas = document.createElement('canvas')
    canvas.width = w
    canvas.height = h
    const ctx = canvas.getContext('2d')
    if (!ctx) return ''
    ctx.drawImage(v, 0, 0, w, h)
    const cx = normX * w
    const cy = normY * h
    const r = Math.max(10, Math.min(w, h) * 0.03)
    ctx.beginPath()
    ctx.arc(cx, cy, r, 0, Math.PI * 2)
    ctx.strokeStyle = 'red'
    ctx.lineWidth = Math.max(2, r * 0.25)
    ctx.stroke()
    return canvas.toDataURL('image/jpeg', 0.8)
  }

  watch(videoRef, (v) => {
    if (!v) return
    v.addEventListener('timeupdate', () => { currentTime.value = v.currentTime })
    v.addEventListener('durationchange', () => { duration.value = v.duration || 0 })
    v.addEventListener('play', () => { playing.value = true })
    v.addEventListener('pause', () => { playing.value = false })
    v.addEventListener('ended', () => { playing.value = false })
    v.volume = volume.value
    v.playbackRate = speed.value
  })

  return {
    currentTime,
    duration,
    playing,
    volume,
    speed,
    formatTime,
    togglePlay,
    seek,
    setVolume,
    setSpeed,
    captureFrameWithMarker,
  }
}
