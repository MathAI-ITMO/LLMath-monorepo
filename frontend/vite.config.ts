import { fileURLToPath, URL } from 'node:url'
import fs from 'node:fs';

import { defineConfig, loadEnv } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueJsx from '@vitejs/plugin-vue-jsx'
import vueDevTools from 'vite-plugin-vue-devtools'
import basicSsl from '@vitejs/plugin-basic-ssl'



// https://vite.dev/config/
export default defineConfig(({ command, mode }) => {
  const env = loadEnv(mode, process.cwd(), '');
  return {
    plugins: [
      vue({
        template: {
          compilerOptions: {
            isCustomElement: (tag) => tag.startsWith("gradio-"),
          },
        },
      }),
      vueJsx(),
      vueDevTools(),
      basicSsl({
        name: 'SandboxWebUICert',
        /** custom trust domains */
        domains: ['textgen.net.ecm', 'engine.updatemirror.cc'],
        certDir: './.devServer/cert',
      }),
    ],
    resolve: {
      alias: {
        '@': fileURLToPath(new URL('./src', import.meta.url))
      },
    },
    server: {
      host: "0.0.0.0",
      allowedHosts: [
        '.net.ecm',
        '.updatemirror.cc',
      ],
      port: 23188,
      strictPort: true,
      https: {
        cert: fs.readFileSync('./.devServer/cert/_cert.pem')
      },
    },
    base: env.VUE_APP_PATH_SUFFIX ? ('/' + env.VUE_APP_PATH_SUFFIX + '/') : undefined,
    // base: '',
    optimizeDeps: {
      exclude: ["@gradio/lite"],
    },
    build: {
      rollupOptions: {
        external: ["@gradio/lite"],
      },
    },
    define: {
      "process.env.GRADIO_API_BASE": env.GRADIO_PREFIX ? JSON.stringify(env.GRADIO_PREFIX) : undefined,
    }
  }
})
