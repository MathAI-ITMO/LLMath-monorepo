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
  video: {
    input: '../video/openapi.json',
    output: {
      target: './src/api/generated/videoApi.ts',
      client: 'axios',
      mode: 'single',
      useNativeEnums: true,
      override: {
        mutator: {
          path: './src/api/video/index.ts',
          name: 'videoApiInstance',
        },
      },
    },
  },
})
