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
    @click:viewAll="router.push('/notifiche')"
  />

  <!-- Global stacked toasts -->
  <Teleport to="body">
    <div class="notification-toast-container">
      <TransitionGroup name="toast">
        <div
          v-for="t in store.toasts"
          :key="t.id"
          class="notification-toast-item"
          :style="{ background: `rgb(var(--v-theme-${t.color}))` }"
        >
          <VBtn
            icon
            variant="text"
            size="x-small"
            color="white"
            class="notification-toast-close"
            @click="store.dismissToast(t.id)"
          >
            <VIcon
              size="16"
              icon="tabler-x"
            />
          </VBtn>
          <div class="notification-toast-content">
            <strong>{{ t.titolo }}</strong>
            <div
              v-if="t.messaggio"
              class="notification-toast-message"
            >
              {{ t.messaggio }}
            </div>
          </div>
          <VBtn
            v-if="t.link"
            variant="text"
            size="small"
            color="white"
            class="notification-toast-link"
            @click="router.push(t.link!); store.dismissToast(t.id)"
          >
            Apri
          </VBtn>
        </div>
      </TransitionGroup>
    </div>
  </Teleport>
</template>

<style>
.notification-toast-container {
  position: fixed;
  top: 16px;
  right: 16px;
  z-index: 2100;
  display: flex;
  flex-direction: column;
  gap: 8px;
  min-width: 300px;
  max-width: 400px;
  pointer-events: none;
}

.notification-toast-item {
  position: relative;
  pointer-events: auto;
  border-radius: 8px;
  color: #fff;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.25);
  padding: 12px 40px 12px 16px;
  display: flex;
  align-items: center;
  gap: 8px;
}

.notification-toast-close {
  position: absolute !important;
  top: 4px;
  right: 4px;
}

.notification-toast-content {
  flex: 1;
  min-width: 0;
}

.notification-toast-message {
  font-size: 0.75rem;
  margin-top: 4px;
  opacity: 0.85;
}

.notification-toast-link {
  flex-shrink: 0;
}

/* Transitions */
.toast-enter-active {
  transition: all 0.3s ease-out;
}

.toast-leave-active {
  transition: all 0.2s ease-in;
}

.toast-enter-from {
  opacity: 0;
  transform: translateX(40px);
}

.toast-leave-to {
  opacity: 0;
  transform: translateX(40px);
}

.toast-move {
  transition: transform 0.3s ease;
}
</style>
