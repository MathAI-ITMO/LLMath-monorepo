/* ------------------------------------------------------------------ */
/*  renderMessage.ts                                                   */
/*  + пост-фикс для "**Условие задачи:**"                              */
/* ------------------------------------------------------------------ */

import katex       from 'katex'
// import MarkdownIt  from 'markdown-it'
import 'katex/dist/katex.min.css'

// const md = MarkdownIt({ html: true, breaks: true, linkify: false })

type Delim = { left: string; right: string; display: boolean }
const DELIMS: readonly Delim[] = [
  { left: '$$',  right: '$$',  display: true  },
  { left: '$',   right: '$',   display: false },
  { left: '\\[', right: '\\]', display: true  },
  { left: '\\(', right: '\\)', display: false },
] as const

const ENV_NAMES = [
  'equation',  'equation*', 'align',   'align*', 'alignat',
  'gather',    'cases',     'pmatrix', 'bmatrix',
  'vmatrix',   'Vmatrix',   'CD',
] as const
const BEGIN_RE = /\\begin\{([\w*]+)\}/g
const END_RE   = (env: string) =>
  new RegExp('\\\\end\\{' + env.replace('*', '\\*') + '\\}', 'g')

/* ---------- мелкие подстановки ---------- */
const normalize = (s: string) =>
  s.replace(/\\textbf\{([^{}]*(?:\{[^{}]*\}[^{}]*)*)\}/g, '**$1**')

function sanitizeLatex(src: string): string {
  return src
    .replace(/\\\\\s*(?=\\end\{[a-zA-Z*]+\})/g, '')
    .replace(/\\begin\{(?:[pbv]matrix|Vmatrix)\}\s*\\\\/g,
             m => m.replace(/\\\\/, ''))
    .replace(/\\\\\s*\\\\/g, '\\\\')
    .replace(/\\sqrt\(\s*([^\)]+?)\s*\)/g, '\\sqrt{$1}')
}

/* ---------- поиск конца математического блока ---------- */
function findEndOfMath(right: string, text: string, start: number): number {
  let i = start, depth = 0
  while (i < text.length) {
    if (depth === 0 && text.startsWith(right, i)) return i
    const ch = text[i]
    if (ch === '\\') { i += 2; continue }
    if (ch === '{')   depth++
    else if (ch === '}' && depth) depth--
    i++
  }
  return -1
}

/* ---------- безопасный вызов KaTeX ---------- */
function safeKatex(src: string, display: boolean): string {
  try {
    return katex.renderToString(sanitizeLatex(src),
      { displayMode: display, throwOnError: false })
  } catch {
    return src
  }
}

/* ---------- главный парсер ---------- */
export function renderMessage(raw: string): string {
  //return raw
  let src = raw
  let out = ''
  let i   = 0

  while (i < src.length) {
    let matched = false

    /* --- 1. $, $$, \[, \( ---------------------------------------------- */
    for (const { left, right, display } of DELIMS) {
      if (!src.startsWith(left, i)) continue
      const end = findEndOfMath(right, src, i + left.length)
      if (end === -1) continue
      const body = src.slice(i + left.length, end)
      out += safeKatex(body, display)
      i = end + right.length
      matched = true
      break
    }
    if (matched) continue

    /* --- 2. \begin{env} … \end{env} ------------------------------------ */
    BEGIN_RE.lastIndex = i
    const mBegin = BEGIN_RE.exec(src)
    if (mBegin && mBegin.index === i &&
        (ENV_NAMES as readonly string[]).includes(mBegin[1])) {
      const env = mBegin[1]
      const endRe = END_RE(env)
      endRe.lastIndex = BEGIN_RE.lastIndex
      const mEnd = endRe.exec(src)
      if (mEnd) {
        const body = src.slice(BEGIN_RE.lastIndex, mEnd.index)
        out += safeKatex(`\\begin{${env}}${sanitizeLatex(body)}\\end{${env}}`,
                         true)
        i = mEnd.index + mEnd[0].length
        continue
      }
    }

    /* --- 3. обычный символ --------------------------------------------- */
    out += src[i++]
  }

  /* --- пост-обработка --------------------------------------------------- */
  out = normalize(out)
    .replace(/\*\*([\s\S]+?)\*\*/g, '<b>$1</b>')
    .replace(/\\\\/g, '<br/>')
    .replace(/^---$/gm, '<hr>')

  // return md.render(out)    // ← вернуть, если понадобится Markdown
  return out
}
