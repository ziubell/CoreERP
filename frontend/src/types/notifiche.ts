export interface TipoNotificaApi {
  id: number
  codice: string
  modulo: string
  descrizione: string
  icona?: string
  colore?: string
}

export interface NotificaApi {
  id: number
  titolo: string
  messaggio?: string
  link?: string
  letta: boolean
  dataCreazione: string
  dataLettura?: string
  mittenteNome?: string
  mittenteAvatar?: string
  tipoNotifica: TipoNotificaApi
}

export interface PreferenzaNotificaApi {
  tipoNotificaId: number
  email: boolean
  browser: boolean
  teams: boolean
}
