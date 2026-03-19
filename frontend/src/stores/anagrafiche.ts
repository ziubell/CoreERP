import { defineStore } from 'pinia'
import { $api } from '@/utils/api'
import type {
  AnagraficaApi,
  AnagraficaListItemApi,
  CreateAnagraficaRequest,
  UpdateAnagraficaRequest,
  StoricoModificaApi,
  RuoloContattoApi,
  MetodoPagamentoApi,
  MotivoDisattivazioneApi,
} from '@/types/anagrafica'

export const useAnagraficheStore = defineStore('anagrafiche', () => {
  const items = ref<AnagraficaListItemApi[]>([])
  const totalCount = ref(0)
  const current = ref<AnagraficaApi | null>(null)
  const storico = ref<StoricoModificaApi[]>([])
  const loading = ref(false)

  // Lookups
  const ruoliContatto = ref<RuoloContattoApi[]>([])
  const metodiPagamento = ref<MetodoPagamentoApi[]>([])
  const motiviDisattivazione = ref<MotivoDisattivazioneApi[]>([])

  async function fetchList(params?: {
    tipo?: number
    attivo?: boolean
    ricerca?: string
    pagina?: number
    dimensionePagina?: number
  }) {
    loading.value = true
    try {
      const query = new URLSearchParams()
      if (params?.tipo !== undefined) query.set('tipo', String(params.tipo))
      if (params?.attivo !== undefined) query.set('attivo', String(params.attivo))
      if (params?.ricerca) query.set('ricerca', params.ricerca)
      if (params?.pagina) query.set('pagina', String(params.pagina))
      if (params?.dimensionePagina) query.set('dimensionePagina', String(params.dimensionePagina))

      const data = await $api(`/v1/anagrafiche?${query}`)
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
      current.value = await $api(`/v1/anagrafiche/${id}`)
    }
    finally {
      loading.value = false
    }
  }

  async function create(request: CreateAnagraficaRequest) {
    const result = await $api('/v1/anagrafiche', { method: 'POST', body: request })
    return result as AnagraficaApi
  }

  async function update(id: number, request: UpdateAnagraficaRequest) {
    const result = await $api(`/v1/anagrafiche/${id}`, { method: 'PUT', body: request })
    current.value = result
    return result as AnagraficaApi
  }

  async function remove(id: number) {
    await $api(`/v1/anagrafiche/${id}`, { method: 'DELETE' })
  }

  async function converti(id: number) {
    const result = await $api(`/v1/anagrafiche/${id}/converti`, { method: 'POST' })
    current.value = result
    return result as AnagraficaApi
  }

  async function disattiva(id: number, motivoDisattivazioneId: number) {
    const result = await $api(`/v1/anagrafiche/${id}/disattiva`, {
      method: 'PUT',
      body: { motivoDisattivazioneId },
    })
    current.value = result
    return result as AnagraficaApi
  }

  async function riattiva(id: number) {
    const result = await $api(`/v1/anagrafiche/${id}/riattiva`, { method: 'PUT' })
    current.value = result
    return result as AnagraficaApi
  }

  async function fetchStorico(id: number, pagina = 1, dimensionePagina = 20) {
    const data = await $api(`/v1/anagrafiche/${id}/storico?pagina=${pagina}&dimensionePagina=${dimensionePagina}`)
    storico.value = data
  }

  async function restore(id: number, storicoModificaId: number) {
    await $api(`/v1/anagrafiche/${id}/restore`, { method: 'POST', body: { storicoModificaId } })
  }

  async function associaContatto(id: number, body: any) {
    return await $api(`/v1/anagrafiche/${id}/contatti`, { method: 'POST', body })
  }

  async function rimuoviContatto(id: number, contattoId: number) {
    await $api(`/v1/anagrafiche/${id}/contatti/${contattoId}`, { method: 'DELETE' })
  }

  async function aggiornaRuoloContatto(id: number, contattoId: number, body: any) {
    await $api(`/v1/anagrafiche/${id}/contatti/${contattoId}`, { method: 'PUT', body })
  }

  async function follow(id: number) {
    await $api(`/v1/anagrafiche/${id}/follow`, { method: 'POST' })
  }

  async function unfollow(id: number) {
    await $api(`/v1/anagrafiche/${id}/follow`, { method: 'DELETE' })
  }

  async function verificaDuplicato(partitaIva?: string, codiceFiscale?: string, excludeId?: number) {
    const query = new URLSearchParams()
    if (partitaIva) query.set('partitaIva', partitaIva)
    if (codiceFiscale) query.set('codiceFiscale', codiceFiscale)
    if (excludeId) query.set('excludeId', String(excludeId))
    return await $api(`/v1/anagrafiche/verifica-duplicato?${query}`) as {
      isDuplicate: boolean
      anagraficaId: number | null
      denominazione: string | null
    }
  }

  // Lookups
  async function fetchRuoliContatto() {
    ruoliContatto.value = await $api('/v1/ruoli-contatto?attivo=true')
  }

  async function fetchMetodiPagamento() {
    metodiPagamento.value = await $api('/v1/metodi-pagamento?attivo=true')
  }

  async function fetchMotiviDisattivazione() {
    motiviDisattivazione.value = await $api('/v1/motivi-disattivazione?attivo=true')
  }

  async function fetchLookups() {
    await Promise.all([fetchRuoliContatto(), fetchMetodiPagamento(), fetchMotiviDisattivazione()])
  }

  return {
    items,
    totalCount,
    current,
    storico,
    loading,
    ruoliContatto,
    metodiPagamento,
    motiviDisattivazione,
    fetchList,
    fetchById,
    create,
    update,
    remove,
    converti,
    disattiva,
    riattiva,
    fetchStorico,
    restore,
    associaContatto,
    rimuoviContatto,
    aggiornaRuoloContatto,
    follow,
    unfollow,
    verificaDuplicato,
    fetchLookups,
  }
})
