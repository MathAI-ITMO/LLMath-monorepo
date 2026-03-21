import { defineConfig } from 'orval'

export default defineConfig({
  mathllm: {
    input: '../backend/MathLLMBackend.Presentation_openapi.json',
    output: {
      target: './src/api/generated/api.ts',
      client: 'axios',
      mode: 'single',
      useNativeEnums: true,
      override: {
        mutator: {
          path: './src/utils/apiClient.ts',
          name: 'backendApiInstance',
        },
      },
    },
  },
})
