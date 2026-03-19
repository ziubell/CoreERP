/**
 * Nome: iniziale maiuscola, resto minuscolo per ogni parola
 * "pietro" → "Pietro", "maria grazia" → "Maria Grazia"
 */
export function formatNome(value: string): string {
  return value
    .toLowerCase()
    .replace(/(?:^|\s)\S/g, char => char.toUpperCase())
}

/**
 * Cognome: tutto maiuscolo
 * "bello" → "BELLO", "de rossi" → "DE ROSSI"
 */
export function formatCognome(value: string): string {
  return value.toUpperCase()
}

/**
 * Ragione Sociale: tutto maiuscolo
 * "spadhausen srl" → "SPADHAUSEN SRL"
 */
export function formatRagioneSociale(value: string): string {
  return value.toUpperCase()
}
