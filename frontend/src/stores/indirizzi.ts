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
  TipoTecnologiaApi,
  ReteRiferimentoApi,
} from '@/types/indirizzo'

export const useIndirizziStore = defineStore('indirizzi', () => {
  const items = ref<IndirizzoListItemApi[]>([])
  const totalCount = ref(0)
  const loading = ref(false)

  const tipiTecnologia = ref<TipoTecnologiaApi[]>([])
  const retiRiferimento = ref<ReteRiferimentoApi[]>([])

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

  async function normalize(city: string, street: string, fraction?: string) {
    const params = new URLSearchParams({ city, street })
    if (fraction) params.set('fraction', fraction)
    return await $api(`/v1/egon/normalizza?${params}`)
  }

  async function verificaCopertura(street: string, city: string, fraction?: string) {
    const params = new URLSearchParams({ street, city })
    if (fraction) params.set('fraction', fraction)
    return await $api(`/v1/egon/copertura?${params}`)
  }

  async function fetchTipiTecnologia() {
    tipiTecnologia.value = await $api('/v1/tipi-tecnologia?attivo=true')
  }

  async function fetchRetiRiferimento() {
    retiRiferimento.value = await $api('/v1/reti-riferimento?attivo=true')
  }

  async function fetchLookups() {
    await Promise.all([fetchTipiTecnologia(), fetchRetiRiferimento()])
  }

  return {
    items,
    totalCount,
    loading,
    tipiTecnologia,
    retiRiferimento,
    fetchList,
    fetchByAnagrafica,
    fetchById,
    create,
    update,
    remove,
    searchComuni,
    searchStrade,
    searchCivici,
    normalize,
    verificaCopertura,
    fetchTipiTecnologia,
    fetchRetiRiferimento,
    fetchLookups,
  }
})
