import { defineStore } from 'pinia'
import { $api } from '@/utils/api'
import type { MessaggioApi, AllegatoMessaggioApi, CreateMessaggioRequest } from '@/types/messaggio'

export const useMessaggiStore = defineStore('messaggi', () => {
  const items = ref<MessaggioApi[]>([])
  const totalCount = ref(0)
  const loading = ref(false)

  async function fetchByEntita(
    entitaTipo: string,
    entitaId: number,
    pagina = 1,
    dimensionePagina = 20,
  ) {
    loading.value = true
    try {
      const query = new URLSearchParams({
        entitaTipo,
        entitaId: String(entitaId),
        pagina: String(pagina),
        dimensionePagina: String(dimensionePagina),
      })

      const data = await $api<{ items: MessaggioApi[]; totalCount: number }>(`/v1/messaggi?${query}`)

      if (pagina === 1)
        items.value = data.items
      else
        items.value.push(...data.items)

      totalCount.value = data.totalCount

      return data
    }
    finally {
      loading.value = false
    }
  }

  async function create(request: CreateMessaggioRequest) {
    const result = await $api<MessaggioApi>('/v1/messaggi', {
      method: 'POST',
      body: request,
    })

    // Aggiungi in testa alla lista
    items.value.unshift(result)
    totalCount.value++

    return result
  }

  async function update(id: number, testo: string) {
    const result = await $api<MessaggioApi>(`/v1/messaggi/${id}`, {
      method: 'PUT',
      body: { testo },
    })

    // Aggiorna nella lista locale
    const index = items.value.findIndex(m => m.id === id)
    if (index !== -1)
      items.value[index] = result

    return result
  }

  async function remove(id: number) {
    await $api(`/v1/messaggi/${id}`, { method: 'DELETE' })

    // Rimuovi dalla lista locale
    items.value = items.value.filter(m => m.id !== id)
    totalCount.value--
  }

  async function uploadAllegato(messaggioId: number, file: File) {
    const formData = new FormData()
    formData.append('file', file)

    const result = await $api<AllegatoMessaggioApi>(`/v1/messaggi/${messaggioId}/allegati`, {
      method: 'POST',
      body: formData,
    })

    // Aggiorna allegati del messaggio nella lista locale
    const messaggio = items.value.find(m => m.id === messaggioId)
    if (messaggio)
      messaggio.allegati.push(result)

    return result
  }

  async function downloadAllegato(allegatoId: number) {
    const blob = await $api<Blob>(`/v1/messaggi/allegati/${allegatoId}/download`, {
      responseType: 'blob',
    })

    return URL.createObjectURL(blob)
  }

  function reset() {
    items.value = []
    totalCount.value = 0
  }

  return {
    items,
    totalCount,
    loading,
    fetchByEntita,
    create,
    update,
    remove,
    uploadAllegato,
    downloadAllegato,
    reset,
  }
})
