import { fileURLToPath, URL } from 'node:url'

import { defineConfig, loadEnv, UserConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueJsx from '@vitejs/plugin-vue-jsx'
import vueDevTools from 'vite-plugin-vue-devtools'
//import basicSsl from '@vitejs/plugin-basic-ssl'

// https://vite.dev/config/
export default defineConfig(({ mode }): UserConfig => {
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
    //  vueDevTools(),
    //  basicSsl({
    //    name: 'SandboxWebUICert',
    //    /** custom trust domains */
    //    domains: ['textgen.net.ecm', 'engine.updatemirror.cc'],
    //    certDir: './.devServer/cert',
    //  }),
    ],
    resolve: {
      alias: {
        '@': fileURLToPath(new URL('./src', import.meta.url))
      },
    },
    server: {
      host: "0.0.0.0",
      allowedHosts: true,
      https: false,
      port: 8080,
      strictPort: true,
    },
    preview: {
      allowedHosts: true
    },
    base: env.VUE_APP_PATH_SUFFIX ? ('/' + env.VUE_APP_PATH_SUFFIX + '/') : undefined,
  }
})
