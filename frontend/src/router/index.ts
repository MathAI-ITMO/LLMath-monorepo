import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import AuthView from '@/views/AuthView.vue'
import ChatView from '@/views/ChatView.vue'
import Logout from '@/views/Logout.vue'
import TaskSelectionView from '@/views/TaskSelectionView.vue'
import TestLLMathProblemsView from '@/views/TestLLMathProblemsView.vue'
import StatisticsView from '@/views/StatisticsView.vue'
import UserDetailView from '@/views/UserDetailView.vue'
import AdminChatView from '@/views/AdminChatView.vue'
import { useAuth } from '@/composables/useAuth'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
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
      path: '/admin-chat/:chatId',
      name: 'admin-chat',
      component: AdminChatView,
    },
    {
      path: '/select-task',
      name: 'select-task',
      component: TaskSelectionView,
      meta: { requiresAuth: true },
    },
    {
      path: '/llmath-problems',
      name: 'llmath-problems',
      component: TestLLMathProblemsView,
    },
    {
      path: '/statistics',
      name: 'statistics',
      component: StatisticsView,
    },
    {
      path: '/statistics/:userId',
      name: 'user-details',
      component: UserDetailView,
    },
  ],
})

const { isAuthenticated, fetchCurrentUser } = useAuth();

router.beforeEach(async (to, from, next) => {
  if (to.meta.requiresAuth) {
    if (isAuthenticated.value) {
      try {
        const user = await fetchCurrentUser();
        if (user) {
          return next();
        }
        return next();
      } catch {
        return next();
      }
    } else {
      return next({ name: 'home' });
    }
  }

  next();
});

export default router
