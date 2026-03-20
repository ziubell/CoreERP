export interface IndirizzoApi {
  id: number
  anagraficaId: number
  isFatturazione: boolean
  isImpianto: boolean
  sottoTipo?: string | null
  rete?: string | null
  strada: string
  numero: string
  frazione?: string | null
  citta: string
  provincia: string
  regione?: string | null
  cap?: string | null
  latitudine?: number | null
  longitudine?: number | null
  egonCivico?: string | null
  egonStrada?: string | null
  egonLocalita?: string | null
  anagraficaDenominazione?: string | null
  dataCreazione: string
}

export interface IndirizzoListItemApi {
  id: number
  anagraficaId: number
  anagraficaDenominazione: string
  isFatturazione: boolean
  isImpianto: boolean
  sottoTipo?: string | null
  rete?: string | null
  indirizzoCompleto: string
  citta: string
  provincia: string
}

export interface CreateIndirizzoRequest {
  anagraficaId: number
  isFatturazione: boolean
  isImpianto: boolean
  sottoTipo?: string | null
  rete?: string | null
  strada: string
  numero: string
  frazione?: string | null
  citta: string
  provincia: string
  regione?: string | null
  cap?: string | null
  latitudine?: number | null
  longitudine?: number | null
  egonCivico?: string | null
  egonStrada?: string | null
  egonLocalita?: string | null
}

export interface UpdateIndirizzoRequest {
  isFatturazione: boolean
  isImpianto: boolean
  sottoTipo?: string | null
  rete?: string | null
  strada: string
  numero: string
  frazione?: string | null
  citta: string
  provincia: string
  regione?: string | null
  cap?: string | null
  latitudine?: number | null
  longitudine?: number | null
  egonCivico?: string | null
  egonStrada?: string | null
  egonLocalita?: string | null
}

// EGON autocomplete types
export interface EgonComuneApi {
  egonComune: string
  comune: string
}

export interface EgonStradaApi {
  egonStrada: string
  strada: string
  comune: string
  provincia: string
  cap?: string | null
  frazione?: string | null
}

export interface EgonCivicoApi {
  egonCivico: string
  civico: string
}

export interface TipoTecnologiaApi {
  id: number
  nome: string
  descrizione?: string | null
  attivo: boolean
  ordine: number
}

export interface ReteRiferimentoApi {
  id: number
  nome: string
  descrizione?: string | null
  attivo: boolean
  ordine: number
}
