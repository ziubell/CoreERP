export interface IndirizzoApi {
  id: number
  anagraficaId: number
  tipo: string
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
  principale: boolean
  anagraficaDenominazione?: string | null
  dataCreazione: string
}

export interface IndirizzoListItemApi {
  id: number
  anagraficaId: number
  anagraficaDenominazione: string
  tipo: string
  sottoTipo?: string | null
  rete?: string | null
  indirizzoCompleto: string
  citta: string
  provincia: string
  principale: boolean
}

export interface CreateIndirizzoRequest {
  anagraficaId: number
  tipo: string
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
  principale: boolean
}

export interface UpdateIndirizzoRequest {
  tipo: string
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
  principale: boolean
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

export const TIPI_INDIRIZZO = ['Fatturazione', 'Impianto'] as const
export const SOTTO_TIPI_IMPIANTO = ['FTTH', 'FTTC', 'FWA'] as const
export const RETI = ['FIBERCOP', 'OPENFIBER', 'SPADHAUSEN', 'EOLO'] as const
