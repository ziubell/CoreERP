import { defineStore } from 'pinia'
import { $api } from '@/utils/api'
import type {
  ContattoApi,
  ContattoListItemApi,
  CreateContattoRequest,
  UpdateContattoRequest,
  StoricoModificaApi,
} from '@/types/anagrafica'

export const useContattiStore = defineStore('contatti', () => {
  const items = ref<ContattoListItemApi[]>([])
  const totalCount = ref(0)
  const current = ref<ContattoApi | null>(null)
  const storico = ref<StoricoModificaApi[]>([])
  const loading = ref(false)

  async function fetchList(params?: {
    ricerca?: string
    pagina?: number
    dimensionePagina?: number
  }) {
    loading.value = true
    try {
      const query = new URLSearchParams()
      if (params?.ricerca) query.set('ricerca', params.ricerca)
      if (params?.pagina) query.set('pagina', String(params.pagina))
      if (params?.dimensionePagina) query.set('dimensionePagina', String(params.dimensionePagina))

      const data = await $api(`/v1/contatti?${query}`)
      items.value = data.items
      totalCount.value = data.totalCount
    }
    finally {
      loading.value = false
    }
  }

  async function fetchById(id: number) {
    loading.value = true
    try {
      current.value = await $api(`/v1/contatti/${id}`)
    }
    finally {
      loading.value = false
    }
  }

  async function create(request: CreateContattoRequest) {
    const result = await $api('/v1/contatti', { method: 'POST', body: request })
    return result as ContattoApi
  }

  async function update(id: number, request: UpdateContattoRequest) {
    const result = await $api(`/v1/contatti/${id}`, { method: 'PUT', body: request })
    current.value = result
    return result as ContattoApi
  }

  async function remove(id: number) {
    await $api(`/v1/contatti/${id}`, { method: 'DELETE' })
  }

  async function fetchStorico(id: number, pagina = 1) {
    const data = await $api(`/v1/contatti/${id}/storico?pagina=${pagina}`)
    storico.value = data
  }

  async function verificaDuplicato(email?: string, cellulare?: string, excludeId?: number) {
    const query = new URLSearchParams()
    if (email) query.set('email', email)
    if (cellulare) query.set('cellulare', cellulare)
    if (excludeId) query.set('excludeId', String(excludeId))
    const data = await $api(`/v1/contatti/verifica-duplicato?${query}`)
    return data.isDuplicate as boolean
  }

  return {
    items,
    totalCount,
    current,
    storico,
    loading,
    fetchList,
    fetchById,
    create,
    update,
    remove,
    fetchStorico,
    verificaDuplicato,
  }
})
