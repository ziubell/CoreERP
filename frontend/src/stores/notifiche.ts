import { defineStore } from 'pinia'
import { HubConnectionBuilder, type HubConnection } from '@microsoft/signalr'
import type { NotificaApi, PreferenzaNotificaApi, TipoNotificaApi } from '@/types/notifiche'
import { $api } from '@/utils/api'

export const useNotificheStore = defineStore('notifiche', () => {
  const notifiche = ref<NotificaApi[]>([])
  const nonLette = ref(0)
  const tipiNotifica = ref<TipoNotificaApi[]>([])
  const preferenze = ref<PreferenzaNotificaApi[]>([])
  const loading = ref(false)

  let connection: HubConnection | null = null

  async function fetchNotifiche(soloNonLette = false, pagina = 1) {
    loading.value = true
    try {
      const params = new URLSearchParams()
      if (soloNonLette) params.set('soloNonLette', 'true')
      params.set('pagina', pagina.toString())

      const data = await $api<NotificaApi[]>(`/v1/notifiche?${params}`)
      if (pagina === 1)
        notifiche.value = data
      else
        notifiche.value.push(...data)
    }
    finally {
      loading.value = false
    }
  }

  async function fetchContaNonLette() {
    const data = await $api<{ count: number }>('/v1/notifiche/non-lette/count')
    nonLette.value = data.count
  }

  async function segnaComeLetta(id: number) {
    await $api(`/v1/notifiche/${id}/letta`, { method: 'PUT' })
    const notifica = notifiche.value.find(n => n.id === id)
    if (notifica) {
      notifica.letta = true
      nonLette.value = Math.max(0, nonLette.value - 1)
    }
  }

  async function segnaTutteComeLette() {
    await $api('/v1/notifiche/lette', { method: 'PUT' })
    notifiche.value.forEach(n => n.letta = true)
    nonLette.value = 0
  }

  async function elimina(id: number) {
    await $api(`/v1/notifiche/${id}`, { method: 'DELETE' })
    notifiche.value = notifiche.value.filter(n => n.id !== id)
    await fetchContaNonLette()
  }

  async function fetchTipiNotifica() {
    tipiNotifica.value = await $api<TipoNotificaApi[]>('/v1/notifiche/tipi')
  }

  async function fetchPreferenze() {
    preferenze.value = await $api<PreferenzaNotificaApi[]>('/v1/notifiche/preferenze')
  }

  async function salvaPreferenze(prefs: PreferenzaNotificaApi[]) {
    await $api('/v1/notifiche/preferenze', {
      method: 'PUT',
      body: prefs,
    })
    preferenze.value = prefs
  }

  function startConnection() {
    const accessToken = useCookie('accessToken').value
    if (!accessToken || connection) return

    const baseUrl = import.meta.env.VITE_API_BASE_URL || ''

    connection = new HubConnectionBuilder()
      .withUrl(`${baseUrl}/hubs/notifiche`, {
        accessTokenFactory: () => useCookie('accessToken').value ?? '',
      })
      .withAutomaticReconnect()
      .build()

    connection.on('NuovaNotifica', (notifica: NotificaApi) => {
      notifiche.value.unshift(notifica)
      nonLette.value++
    })

    connection.start().catch(err => {
      console.error('SignalR connection error:', err)
    })
  }

  function stopConnection() {
    if (connection) {
      connection.stop()
      connection = null
    }
  }

  async function init() {
    await Promise.all([
      fetchNotifiche(),
      fetchContaNonLette(),
    ])
    startConnection()
  }

  return {
    notifiche,
    nonLette,
    tipiNotifica,
    preferenze,
    loading,
    fetchNotifiche,
    fetchContaNonLette,
    segnaComeLetta,
    segnaTutteComeLette,
    elimina,
    fetchTipiNotifica,
    fetchPreferenze,
    salvaPreferenze,
    startConnection,
    stopConnection,
    init,
  }
})
