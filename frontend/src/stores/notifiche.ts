import { defineStore } from 'pinia'
import { HubConnectionBuilder, type HubConnection } from '@microsoft/signalr'
import type { ImpostazioniNotificaApi, NotificaApi, PreferenzaNotificaApi, TipoNotificaApi } from '@/types/notifiche'
import { $api } from '@/utils/api'

export const useNotificheStore = defineStore('notifiche', () => {
  const notifiche = ref<NotificaApi[]>([])
  const nonLette = ref(0)
  const tipiNotifica = ref<TipoNotificaApi[]>([])
  const preferenze = ref<PreferenzaNotificaApi[]>([])
  const canali = ref<{ email: boolean; browser: boolean; teams: boolean }>({ email: true, browser: true, teams: false })
  const loading = ref(false)

  // Toast queue for stacked notifications
  interface ToastItem {
    id: number
    titolo: string
    messaggio: string | null
    link: string | null
    color: string
  }

  let toastCounter = 0
  const toasts = ref<ToastItem[]>([])

  function addToast(titolo: string, messaggio: string | null = null, link: string | null = null, color = 'info') {
    const id = ++toastCounter
    const item: ToastItem = { id, titolo, messaggio, link, color }
    toasts.value.unshift(item)
    setTimeout(() => dismissToast(id), 5000)
  }

  function dismissToast(id: number) {
    toasts.value = toasts.value.filter(t => t.id !== id)
  }


  let connection: HubConnection | null = null

  async function fetchNotifiche(soloNonLette = false, pagina = 1, ricerca?: string, modulo?: string) {
    loading.value = true
    try {
      const params = new URLSearchParams()
      if (soloNonLette) params.set('soloNonLette', 'true')
      params.set('pagina', pagina.toString())
      if (ricerca) params.set('ricerca', ricerca)
      if (modulo) params.set('modulo', modulo)

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

  async function eliminaMultiple(ids: number[]) {
    await $api('/v1/notifiche/bulk', {
      method: 'DELETE',
      body: { ids },
    })
    notifiche.value = notifiche.value.filter(n => !ids.includes(n.id))
    await fetchContaNonLette()
  }

  async function eliminaTutte() {
    await $api('/v1/notifiche/tutte', { method: 'DELETE' })
    notifiche.value = []
    nonLette.value = 0
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

  async function fetchCanali() {
    canali.value = await $api<{ email: boolean; browser: boolean; teams: boolean }>('/v1/notifiche/canali')
  }


  async function fetchImpostazioni() {
    return await $api<ImpostazioniNotificaApi>('/v1/notifiche/impostazioni')
  }

  async function salvaImpostazioni(giorniRetention: number) {
    await $api('/v1/notifiche/impostazioni', {
      method: 'PUT',
      body: { giorniRetention },
    })
  }

  function startConnection() {
    const accessToken = useCookie('accessToken').value
    if (!accessToken || connection) return

    connection = new HubConnectionBuilder()
      .withUrl('/hubs/notifiche', {
        accessTokenFactory: () => useCookie('accessToken').value ?? '',
      })
      .withAutomaticReconnect()
      .build()

    connection.on('NuovaNotifica', (notifica: NotificaApi) => {
      notifiche.value.unshift(notifica)
      nonLette.value++

      // Show toast notification
      addToast(notifica.titolo, notifica.messaggio, notifica.link)
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
    try {
      await Promise.all([
        fetchNotifiche(),
        fetchContaNonLette(),
      ])
    }
    catch (err) {
      console.warn('Notifiche: impossibile caricare le notifiche', err)
    }
    startConnection()
  }

  return {
    notifiche,
    nonLette,
    tipiNotifica,
    preferenze,
    canali,
    loading,
    toasts,
    addToast,
    dismissToast,
    fetchNotifiche,
    fetchContaNonLette,
    segnaComeLetta,
    segnaTutteComeLette,
    elimina,
    eliminaMultiple,
    eliminaTutte,
    fetchTipiNotifica,
    fetchPreferenze,
    salvaPreferenze,
    fetchCanali,
    fetchImpostazioni,
    salvaImpostazioni,
    startConnection,
    stopConnection,
    init,
  }
})
