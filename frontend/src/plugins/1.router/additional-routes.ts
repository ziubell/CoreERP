import type { RouteRecordRaw } from 'vue-router/auto'

// 👉 Redirects
export const redirects: RouteRecordRaw[] = [
  {
    path: '/',
    name: 'index',
    redirect: to => {
      const userData = useCookie<Record<string, unknown> | null | undefined>('userData')

      if (userData.value)
        return { name: 'dashboard' }

      return { name: 'login', query: to.query }
    },
  },
]

export const routes: RouteRecordRaw[] = []
