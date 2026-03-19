export type TipoSoggetto = 0 | 1 // PersonaFisica = 0, PersonaGiuridica = 1
export type TipoAnagrafica = 0 | 1 // Potenziale = 0, Cliente = 1
export type PeriodicitaPagamento = 1 | 2 | 3 | 4 | 6 | 12

export const TIPO_SOGGETTO_LABELS: Record<TipoSoggetto, string> = {
  0: 'Persona Fisica',
  1: 'Persona Giuridica',
}

export const TIPO_ANAGRAFICA_LABELS: Record<TipoAnagrafica, string> = {
  0: 'Potenziale',
  1: 'Cliente',
}

export const PERIODICITA_LABELS: Record<PeriodicitaPagamento, string> = {
  1: 'Mensile',
  2: 'Bimestrale',
  3: 'Trimestrale',
  4: 'Quadrimestrale',
  6: 'Semestrale',
  12: 'Annuale',
}

export interface AnagraficaApi {
  id: number
  codiceCliente: number | null
  tipoSoggetto: TipoSoggetto
  ragioneSociale: string | null
  nome: string | null
  cognome: string | null
  denominazione: string
  tipo: TipoAnagrafica
  attivo: boolean
  motivoDisattivazioneId: number | null
  motivoDisattivazioneNome: string | null
  dataConversione: string | null
  partitaIva: string | null
  codiceFiscale: string | null
  codiceSDI: string | null
  pec: string | null
  indirizzoFatturazione: string | null
  cap: string | null
  citta: string | null
  provincia: string | null
  nazione: string | null
  telefono: string | null
  sitoWeb: string | null
  note: string | null
  metodoPagamentoId: number | null
  metodoPagamentoNome: string | null
  iban: string | null
  periodicitaPagamento: PeriodicitaPagamento | null
  dataCreazione: string
  dataModifica: string | null
  contatti: AnagraficaContattoApi[] | null
}

export interface AnagraficaListItemApi {
  id: number
  codiceCliente: number | null
  denominazione: string
  tipoSoggetto: TipoSoggetto
  tipo: TipoAnagrafica
  attivo: boolean
  partitaIva: string | null
  citta: string | null
  provincia: string | null
  telefono: string | null
  dataCreazione: string
}

export interface AnagraficaContattoApi {
  contattoId: number
  nome: string
  cognome: string
  email: string | null
  cellulare: string | null
  telefono: string | null
  ruoloContattoId: number
  ruoloContattoNome: string
  principale: boolean
}

export interface ContattoApi {
  id: number
  nome: string
  cognome: string
  email: string | null
  cellulare: string | null
  telefono: string | null
  note: string | null
  dataCreazione: string
  dataModifica: string | null
  anagrafiche: ContattoAnagraficaApi[] | null
}

export interface ContattoListItemApi {
  id: number
  nome: string
  cognome: string
  email: string | null
  cellulare: string | null
  telefono: string | null
  dataCreazione: string
}

export interface ContattoAnagraficaApi {
  anagraficaId: number
  denominazione: string
  ruoloContattoNome: string
  principale: boolean
}

export interface StoricoModificaApi {
  id: number
  entitaTipo: string
  entitaId: number
  campo: string
  valorePrecedente: string | null
  valoreNuovo: string | null
  valorePrecedenteLabel: string | null
  valoreNuovoLabel: string | null
  dataModifica: string
  modificatoDa: string
  modificatoDaNome: string | null
  modificatoDaAvatar: string | null
  note: string | null
}

export interface RuoloContattoApi {
  id: number
  nome: string
  descrizione: string | null
  attivo: boolean
  ordine: number
}

export interface MetodoPagamentoApi {
  id: number
  nome: string
  codice: string
  richiedeIBAN: boolean
  attivo: boolean
  ordine: number
}

export interface MotivoDisattivazioneApi {
  id: number
  nome: string
  descrizione: string | null
  attivo: boolean
  ordine: number
}

export interface CreateAnagraficaRequest {
  tipoSoggetto: TipoSoggetto
  ragioneSociale?: string | null
  nome?: string | null
  cognome?: string | null
  tipo: TipoAnagrafica
  partitaIva?: string | null
  codiceFiscale?: string | null
  codiceSDI?: string | null
  pec?: string | null
  indirizzoFatturazione?: string | null
  cap?: string | null
  citta?: string | null
  provincia?: string | null
  nazione?: string | null
  telefono?: string | null
  sitoWeb?: string | null
  note?: string | null
  metodoPagamentoId?: number | null
  iban?: string | null
  periodicitaPagamento?: PeriodicitaPagamento | null
  primoContatto?: CreateContattoRequest | null
  primoContattoRuoloId?: number | null
}

export interface UpdateAnagraficaRequest {
  tipoSoggetto: TipoSoggetto
  ragioneSociale?: string | null
  nome?: string | null
  cognome?: string | null
  partitaIva?: string | null
  codiceFiscale?: string | null
  codiceSDI?: string | null
  pec?: string | null
  indirizzoFatturazione?: string | null
  cap?: string | null
  citta?: string | null
  provincia?: string | null
  nazione?: string | null
  telefono?: string | null
  sitoWeb?: string | null
  note?: string | null
  metodoPagamentoId?: number | null
  iban?: string | null
  periodicitaPagamento?: PeriodicitaPagamento | null
}

export interface CreateContattoRequest {
  nome: string
  cognome: string
  email?: string | null
  cellulare?: string | null
  telefono?: string | null
  note?: string | null
}

export interface UpdateContattoRequest {
  nome: string
  cognome: string
  email?: string | null
  cellulare?: string | null
  telefono?: string | null
  note?: string | null
}
