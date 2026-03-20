import { defineStore } from 'pinia'
import { $api } from '@/utils/api'
import type {
  IndirizzoApi,
  IndirizzoListItemApi,
  CreateIndirizzoRequest,
  UpdateIndirizzoRequest,
  EgonComuneApi,
  EgonStradaApi,
  EgonCivicoApi,
} from '@/types/indirizzo'

export const useIndirizziStore = defineStore('indirizzi', () => {
  const items = ref<IndirizzoListItemApi[]>([])
  const totalCount = ref(0)
  const loading = ref(false)

  async function fetchList(params?: {
    tipo?: string
    ricerca?: string
    pagina?: number
    dimensionePagina?: number
  }) {
    loading.value = true
    try {
      const query = new URLSearchParams()
      if (params?.tipo) query.set('tipo', params.tipo)
      if (params?.ricerca) query.set('ricerca', params.ricerca)
      if (params?.pagina) query.set('pagina', String(params.pagina))
      if (params?.dimensionePagina) query.set('dimensionePagina', String(params.dimensionePagina))

      const data = await $api(`/v1/indirizzi?${query}`)
      items.value = data.items
      totalCount.value = data.totalCount
    }
    finally {
      loading.value = false
    }
  }

  async function fetchByAnagrafica(anagraficaId: number): Promise<IndirizzoApi[]> {
    return await $api(`/v1/indirizzi/anagrafica/${anagraficaId}`)
  }

  async function fetchById(id: number): Promise<IndirizzoApi> {
    return await $api(`/v1/indirizzi/${id}`)
  }

  async function create(request: CreateIndirizzoRequest): Promise<IndirizzoApi> {
    return await $api('/v1/indirizzi', { method: 'POST', body: request })
  }

  async function update(id: number, request: UpdateIndirizzoRequest): Promise<IndirizzoApi> {
    return await $api(`/v1/indirizzi/${id}`, { method: 'PUT', body: request })
  }

  async function remove(id: number) {
    await $api(`/v1/indirizzi/${id}`, { method: 'DELETE' })
  }

  // EGON search
  async function searchComuni(q: string): Promise<EgonComuneApi[]> {
    if (q.length < 3) return []
    return await $api(`/v1/egon/comuni?q=${encodeURIComponent(q)}`)
  }

  async function searchStrade(egonComune: string, q: string): Promise<EgonStradaApi[]> {
    if (q.length < 3) return []
    return await $api(`/v1/egon/strade?egonComune=${encodeURIComponent(egonComune)}&q=${encodeURIComponent(q)}`)
  }

  async function searchCivici(egonStrada: string, q: string): Promise<EgonCivicoApi[]> {
    if (!q) return []
    return await $api(`/v1/egon/civici?egonStrada=${encodeURIComponent(egonStrada)}&q=${encodeURIComponent(q)}`)
  }

  return {
    items,
    totalCount,
    loading,
    fetchList,
    fetchByAnagrafica,
    fetchById,
    create,
    update,
    remove,
    searchComuni,
    searchStrade,
    searchCivici,
  }
})
