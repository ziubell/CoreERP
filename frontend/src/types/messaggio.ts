export interface MessaggioApi {
  id: number
  entitaTipo: string
  entitaId: number
  testo: string
  userId: string
  userNome: string
  userAvatar?: string | null
  dataCreazione: string
  dataModifica?: string | null
  isOwner: boolean
  allegati: AllegatoMessaggioApi[]
}

export interface AllegatoMessaggioApi {
  id: number
  nomeFile: string
  contentType: string
  dimensione: number
  dataCaricamento: string
}

export interface CreateMessaggioRequest {
  entitaTipo: string
  entitaId: number
  testo: string
}
