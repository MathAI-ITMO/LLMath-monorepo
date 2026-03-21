/**
 * Safely truncate text containing LaTeX math formulas
 * Ensures we don't cut in the middle of a formula
 */

export function truncateWithMath(text: string, maxLength: number): string {
  if (text.length <= maxLength) {
    return text
  }

  // Math delimiters to check
  const delimiters = [
    { open: '$$', close: '$$' },
    { open: '$', close: '$' },
    { open: '\\[', close: '\\]' },
    { open: '\\(', close: '\\)' },
  ]

  // Find all math regions
  const mathRegions: Array<{ start: number; end: number }> = []
  
  for (const delim of delimiters) {
    let pos = 0
    while (pos < text.length) {
      const start = text.indexOf(delim.open, pos)
      if (start === -1) break
      
      const end = text.indexOf(delim.close, start + delim.open.length)
      if (end === -1) break
      
      mathRegions.push({
        start,
        end: end + delim.close.length
      })
      
      pos = end + delim.close.length
    }
  }

  // Sort regions by start position
  mathRegions.sort((a, b) => a.start - b.start)

  // Check if maxLength falls inside a math region
  for (const region of mathRegions) {
    if (maxLength > region.start && maxLength < region.end) {
      // Cut before the math formula starts
      return text.substring(0, region.start).trim() + '...'
    }
  }

  // Safe to cut at maxLength
  return text.substring(0, maxLength).trim() + '...'
}

/**
 * Render truncated text with math formulas
 * Returns HTML string with rendered formulas
 */
export function renderTruncatedMath(text: string, maxLength: number): string {
  const truncated = truncateWithMath(text, maxLength)
  
  // Simple inline math rendering (basic version)
  // For full rendering, use renderMessage from renderMessage.ts
  return truncated
    .replace(/\$([^\$]+)\$/g, '<span class="math-inline">$$$1$$</span>')
    .replace(/\n/g, ' ')
}
