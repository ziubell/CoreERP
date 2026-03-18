<script setup lang="ts">
import { useAnagraficheStore } from '@/stores/anagrafiche'
import type { StoricoModificaApi } from '@/types/anagrafica'

const props = defineProps<{
  anagraficaId: number
}>()

const store = useAnagraficheStore()
const loading = ref(false)
const storico = ref<StoricoModificaApi[]>([])

async function loadStorico() {
  loading.value = true
  try {
    await store.fetchStorico(props.anagraficaId)
    storico.value = store.storico
  }
  finally {
    loading.value = false
  }
}

async function handleRestore(storicoId: number) {
  try {
    await store.restore(props.anagraficaId, storicoId)
    await Promise.all([loadStorico(), store.fetchById(props.anagraficaId)])
  }
  catch (error: any) {
    console.error(error)
  }
}

function getColor(entry: StoricoModificaApi): string {
  if (entry.note?.includes('Ripristinato')) return 'warning'
  if (entry.note?.includes('Conversione')) return 'success'
  if (entry.note?.includes('Disattivazione')) return 'error'
  if (entry.note?.includes('Riattivazione')) return 'success'
  return 'primary'
}

function getIcon(entry: StoricoModificaApi): string {
  if (entry.note?.includes('Conversione')) return 'tabler-transform'
  if (entry.note?.includes('Disattivazione')) return 'tabler-ban'
  if (entry.note?.includes('Riattivazione')) return 'tabler-check'
  if (entry.note?.includes('Ripristinato')) return 'tabler-arrow-back-up'
  if (entry.campo === 'Attivo') return 'tabler-toggle-right'
  if (entry.campo === 'Tipo') return 'tabler-transform'
  if (entry.campo.includes('Pagamento') || entry.campo === 'IBAN') return 'tabler-credit-card'
  if (entry.campo.includes('Partita') || entry.campo.includes('Codice')) return 'tabler-receipt-tax'
  return 'tabler-edit'
}

function getTitle(entry: StoricoModificaApi): string {
  if (entry.note?.includes('Conversione')) return 'Conversione a Cliente'
  if (entry.note?.includes('Disattivazione')) return 'Disattivazione'
  if (entry.note?.includes('Riattivazione')) return 'Riattivazione'
  if (entry.note?.includes('Ripristinato')) return `Ripristino campo ${entry.campo}`
  return `Modifica ${entry.campo}`
}

function formatTimeAgo(dateStr: string): string {
  const date = new Date(dateStr)
  const now = new Date()
  const diffMs = now.getTime() - date.getTime()
  const diffMins = Math.floor(diffMs / 60000)
  const diffHours = Math.floor(diffMs / 3600000)
  const diffDays = Math.floor(diffMs / 86400000)

  if (diffMins < 1) return 'Adesso'
  if (diffMins < 60) return `${diffMins}m fa`
  if (diffHours < 24) return `${diffHours}h fa`
  if (diffDays < 7) return `${diffDays}gg fa`
  return date.toLocaleDateString('it-IT')
}

onMounted(loadStorico)
</script>

<template>
  <VCard>
    <VCardItem>
      <VCardTitle>Timeline Attività</VCardTitle>
    </VCardItem>

    <VCardText>
      <div v-if="loading" class="d-flex justify-center py-6">
        <VProgressCircular indeterminate size="32" />
      </div>

      <div v-else-if="storico.length === 0" class="text-center text-disabled py-6">
        <VIcon icon="tabler-clock" size="48" class="mb-2" />
        <p>Nessuna attività registrata</p>
      </div>

      <VTimeline
        v-else
        density="compact"
        side="end"
        truncate-line="both"
      >
        <VTimelineItem
          v-for="entry in storico"
          :key="entry.id"
          :dot-color="getColor(entry)"
          size="x-small"
        >
          <div class="d-flex justify-space-between align-center flex-wrap mb-2">
            <span class="app-timeline-title">
              <VIcon :icon="getIcon(entry)" size="16" class="me-1" />
              {{ getTitle(entry) }}
            </span>
            <span class="app-timeline-meta">{{ formatTimeAgo(entry.dataModifica) }}</span>
          </div>

          <p class="app-timeline-text mb-1">
            <span v-if="entry.valorePrecedenteLabel || entry.valorePrecedente" class="text-decoration-line-through">
              {{ entry.valorePrecedenteLabel ?? entry.valorePrecedente ?? '(vuoto)' }}
            </span>
            <VIcon v-if="entry.valorePrecedente || entry.valoreNuovo" icon="tabler-arrow-right" size="14" class="mx-1" />
            <span class="font-weight-medium">
              {{ entry.valoreNuovoLabel ?? entry.valoreNuovo ?? '(vuoto)' }}
            </span>
          </p>

          <div class="d-flex align-center justify-space-between">
            <span class="app-timeline-meta">
              <VIcon icon="tabler-user" size="12" class="me-1" />
              {{ entry.modificatoDaNome ?? entry.modificatoDa }}
            </span>
            <VBtn
              v-if="!entry.note?.includes('Ripristinato')"
              size="x-small"
              variant="text"
              color="warning"
              @click="handleRestore(entry.id)"
            >
              Ripristina
            </VBtn>
          </div>
        </VTimelineItem>
      </VTimeline>
    </VCardText>
  </VCard>
</template>
