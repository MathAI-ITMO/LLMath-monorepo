import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import AuthView from '@/views/AuthView.vue'
import ChatView from '@/views/ChatView.vue'
import Logout from '@/views/Logout.vue'
import TaskSelectionView from '@/views/TaskSelectionView.vue'
import UserDetailView from '@/views/UserDetailView.vue'
import AdminChatView from '@/views/AdminChatView.vue'
import VideoAppView from '@/views/VideoAppView.vue'
import AdminView from '@/views/AdminView.vue'
import { useAuth } from '@/composables/useAuth'
import { USER_ROLES } from '@/config/roles.constants'
import { getBaseUrl } from '@/config/runtime.config'

const router = createRouter({
  history: createWebHistory(getBaseUrl() || import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView,
    },
    {
      path: '/auth',
      name: 'auth',
      component:  AuthView,
    },
    {
      path: '/logout',
      name: 'logout',
      component: Logout,
      meta: { requiresAuth: true },
    },
    {
      path: '/chat/:chatId?',
      name: 'chat',
      component:  ChatView,
      meta: { requiresAuth: true },
    },
    {
      path: '/admin/chat/:chatId',
      name: 'admin-chat',
      component: AdminChatView,
      meta: { requiresAdmin: true },
    },
    {
      path: '/select-task',
      name: 'select-task',
      component: TaskSelectionView,
      meta: { requiresAuth: true },
    },
    {
      path: '/admin',
      name: 'admin',
      component: AdminView,
      meta: { requiresAdmin: true },
    },
    {
      path: '/admin/statistics',
      name: 'statistics',
      component: AdminView,
      meta: { requiresAdmin: true },
    },
    {
      path: '/admin/llmath-problems',
      name: 'llmath-problems',
      component: AdminView,
      meta: { requiresAdmin: true },
    },
    {
      path: '/admin/statistics/:userId',
      name: 'user-details',
      component: UserDetailView,
      meta: { requiresAdmin: true },
    },
    {
      path: '/video-app',
      name: 'video-app',
      component: VideoAppView,
    },
  ],
})

const { isAuthenticated, fetchCurrentUser } = useAuth();

router.beforeEach(async (to, _from, next) => {
  if (to.meta.requiresAdmin) {
    const user = await fetchCurrentUser();
    if (!user) {
      return next({ name: 'home' });
    }
    if (user.role !== USER_ROLES.ADMIN) {
      return next({ name: 'home' });
    }
    return next();
  }

  if (to.meta.requiresAuth) {
    const user = await fetchCurrentUser();
    if (user) {
      return next();
    } else {
      return next({ name: 'home' });
    }
  }

  next();
});

export default router
