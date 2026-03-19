<script setup lang="ts">
import { useAnagraficheStore } from '@/stores/anagrafiche'
import type { StoricoModificaApi } from '@/types/anagrafica'

const props = defineProps<{
  anagraficaId: number
}>()

const store = useAnagraficheStore()
const loading = ref(false)
const loadingMore = ref(false)
const storico = ref<StoricoModificaApi[]>([])
const pagina = ref(1)
const hasMore = ref(true)
const dimensionePagina = 20

async function loadStorico() {
  loading.value = true
  pagina.value = 1
  try {
    await store.fetchStorico(props.anagraficaId, 1, dimensionePagina)
    storico.value = store.storico
    hasMore.value = store.storico.length >= dimensionePagina
  }
  finally {
    loading.value = false
  }
}

async function loadMore() {
  loadingMore.value = true
  pagina.value++
  try {
    await store.fetchStorico(props.anagraficaId, pagina.value, dimensionePagina)
    storico.value.push(...store.storico)
    hasMore.value = store.storico.length >= dimensionePagina
  }
  finally {
    loadingMore.value = false
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

function getBadgeLabel(entry: StoricoModificaApi): string {
  if (entry.note?.includes('Conversione')) return 'Conversione'
  if (entry.note?.includes('Disattivazione')) return 'Disattivazione'
  if (entry.note?.includes('Riattivazione')) return 'Riattivazione'
  if (entry.note?.includes('Ripristinato')) return 'Ripristino'
  return 'Modifica'
}

function getInitials(name: string | null): string {
  if (!name) return '?'
  const parts = name.trim().split(' ')
  if (parts.length >= 2) return (parts[0][0] + parts[1][0]).toUpperCase()
  return name[0].toUpperCase()
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

      <template v-else>
        <VTimeline
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
            <div class="d-flex align-center flex-wrap gap-2">
              <!-- Gruppo 1: Avatar + Nome + Data -->
              <div class="d-flex align-center gap-2">
                <VAvatar size="24" color="primary" variant="tonal">
                  <VImg v-if="entry.modificatoDaAvatar" :src="entry.modificatoDaAvatar" />
                  <span v-else class="text-caption">{{ getInitials(entry.modificatoDaNome) }}</span>
                </VAvatar>
                <span class="text-body-2 font-weight-medium">{{ entry.modificatoDaNome ?? entry.modificatoDa }}</span>
                <span class="text-body-2 text-disabled">·</span>
                <span class="text-body-2 text-disabled">{{ formatTimeAgo(entry.dataModifica) }}</span>
              </div>

              <!-- Gruppo 2: Badge + Campo: vecchio → nuovo -->
              <div class="d-flex align-center gap-2">
                <VChip :color="getColor(entry)" size="x-small" label>
                  {{ getBadgeLabel(entry) }}
                </VChip>
                <span class="text-body-2">
                  <span class="font-weight-medium">{{ entry.campo }}:</span>
                  <template v-if="entry.valorePrecedente || entry.valoreNuovo">
                    <span class="text-disabled text-decoration-line-through">{{ entry.valorePrecedenteLabel ?? entry.valorePrecedente ?? '(vuoto)' }}</span>
                    <VIcon icon="tabler-arrow-right" size="14" class="mx-1" />
                    <span>{{ entry.valoreNuovoLabel ?? entry.valoreNuovo ?? '(vuoto)' }}</span>
                  </template>
                </span>
              </div>

              <VSpacer />

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

        <!-- Carica altri -->
        <div v-if="hasMore" class="text-center mt-4">
          <VBtn
            variant="tonal"
            color="secondary"
            :loading="loadingMore"
            @click="loadMore"
          >
            Carica altri
          </VBtn>
        </div>
      </template>
    </VCardText>
  </VCard>
</template>
