<script lang="ts" setup>
import type { Notification } from '@layouts/types'
import { useNotificheStore } from '@/stores/notifiche'

const store = useNotificheStore()
const router = useRouter()

// Init store on mount
onMounted(() => {
  store.init()
})

// Map API notifications to the Notification type expected by the component
const notifications = computed<Notification[]>(() => {
  return store.notifiche.map(n => {
    const base = {
      id: n.id,
      title: n.titolo,
      subtitle: n.messaggio ?? n.tipoNotifica.descrizione,
      time: formatRelativeTime(n.dataCreazione),
      isSeen: n.letta,
      color: n.tipoNotifica.colore,
    }

    // If sender has an avatar, show it; otherwise use icon
    if (n.mittenteAvatar)
      return { ...base, img: n.mittenteAvatar }
    if (n.mittenteNome)
      return { ...base, text: n.mittenteNome }

    return { ...base, icon: n.tipoNotifica.icona ?? 'tabler-bell' }
  }) as Notification[]
})

function formatRelativeTime(dateStr: string): string {
  const date = new Date(dateStr)
  const now = new Date()
  const diffMs = now.getTime() - date.getTime()
  const diffMins = Math.floor(diffMs / 60000)
  const diffHours = Math.floor(diffMs / 3600000)
  const diffDays = Math.floor(diffMs / 86400000)

  if (diffMins < 1) return 'Adesso'
  if (diffMins < 60) return `${diffMins} min fa`
  if (diffHours < 24) return `${diffHours} ore fa`
  if (diffDays < 7) return `${diffDays} giorni fa`

  return date.toLocaleDateString('it-IT')
}

const removeNotification = (notificationId: number) => {
  store.elimina(notificationId)
}

const markRead = (notificationIds: number[]) => {
  notificationIds.forEach(id => store.segnaComeLetta(id))
}

const markUnRead = (_notificationIds: number[]) => {
  // Backend doesn't support marking as unread — no-op
}

const handleNotificationClick = (notification: Notification) => {
  if (!notification.isSeen)
    store.segnaComeLetta(notification.id)

  // Navigate to link if present
  const notificaApi = store.notifiche.find(n => n.id === notification.id)
  if (notificaApi?.link)
    router.push(notificaApi.link)
}
</script>

<template>
  <Notifications
    :notifications="notifications"
    @remove="removeNotification"
    @read="markRead"
    @unread="markUnRead"
    @click:notification="handleNotificationClick"
    @click:view-all="router.push('/notifiche')"
  />
</template>
