import { fileURLToPath } from 'node:url'
import { mergeConfig, defineConfig, configDefaults, ConfigEnv } from 'vitest/config'
import viteConfig from './vite.config'

const resolvedViteConfig = typeof viteConfig === 'function'
  ? viteConfig({ mode: 'test', command: 'serve' } as ConfigEnv)
  : viteConfig

export default mergeConfig(
  resolvedViteConfig,
  defineConfig({
    test: {
      environment: 'jsdom',
      exclude: [...configDefaults.exclude, 'e2e/**'],
      root: fileURLToPath(new URL('./', import.meta.url)),
    },
  }),
)
