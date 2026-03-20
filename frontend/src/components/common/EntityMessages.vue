<script setup lang="ts">
import { useMessaggiStore } from '@/stores/messaggi'
import { useNotificheStore } from '@/stores/notifiche'
import type { MessaggioApi } from '@/types/messaggio'

const props = defineProps<{
  entitaTipo: string
  entitaId: number
}>()

const store = useMessaggiStore()
const notificheStore = useNotificheStore()

const loading = ref(false)
const loadingMore = ref(false)
const pagina = ref(1)
const dimensionePagina = 20
const hasMore = ref(true)

// Nuovo messaggio
const nuovoTesto = ref('')
const invioLoading = ref(false)

// Modifica inline
const editingId = ref<number | null>(null)
const editingTesto = ref('')
const editLoading = ref(false)

// Upload allegato
const fileInputRef = ref<HTMLInputElement | null>(null)
const uploadTargetId = ref<number | null>(null)
const uploadLoading = ref(false)

// Elimina conferma
const deleteDialogOpen = ref(false)
const deleteTargetId = ref<number | null>(null)
const deleteLoading = ref(false)

async function loadMessaggi() {
  loading.value = true
  pagina.value = 1
  try {
    const data = await store.fetchByEntita(props.entitaTipo, props.entitaId, 1, dimensionePagina)
    hasMore.value = (data?.items?.length ?? 0) >= dimensionePagina
  }
  finally {
    loading.value = false
  }
}

async function loadMore() {
  loadingMore.value = true
  pagina.value++
  try {
    const data = await store.fetchByEntita(props.entitaTipo, props.entitaId, pagina.value, dimensionePagina)
    hasMore.value = (data?.items?.length ?? 0) >= dimensionePagina
  }
  finally {
    loadingMore.value = false
  }
}

async function inviaMessaggio() {
  if (!nuovoTesto.value.trim()) return

  invioLoading.value = true
  try {
    await store.create({
      entitaTipo: props.entitaTipo,
      entitaId: props.entitaId,
      testo: nuovoTesto.value.trim(),
    })

    nuovoTesto.value = ''
    notificheStore.addToast('Messaggio inviato', null, null, 'success')
  }
  catch (error: any) {
    notificheStore.addToast('Errore invio messaggio', error?.data?.message || null, null, 'error')
  }
  finally {
    invioLoading.value = false
  }
}

function startEdit(messaggio: MessaggioApi) {
  editingId.value = messaggio.id
  editingTesto.value = messaggio.testo
}

function cancelEdit() {
  editingId.value = null
  editingTesto.value = ''
}

async function salvaModifica() {
  if (!editingId.value || !editingTesto.value.trim()) return

  editLoading.value = true
  try {
    await store.update(editingId.value, editingTesto.value.trim())
    editingId.value = null
    editingTesto.value = ''
    notificheStore.addToast('Messaggio aggiornato', null, null, 'success')
  }
  catch (error: any) {
    notificheStore.addToast('Errore aggiornamento', error?.data?.message || null, null, 'error')
  }
  finally {
    editLoading.value = false
  }
}

function askDelete(id: number) {
  deleteTargetId.value = id
  deleteDialogOpen.value = true
}

async function confirmDelete() {
  if (!deleteTargetId.value) return

  deleteLoading.value = true
  try {
    await store.remove(deleteTargetId.value)
    deleteDialogOpen.value = false
    notificheStore.addToast('Messaggio eliminato', null, null, 'success')
  }
  catch (error: any) {
    notificheStore.addToast('Errore eliminazione', error?.data?.message || null, null, 'error')
  }
  finally {
    deleteLoading.value = false
  }
}

function triggerUpload(messaggioId: number) {
  uploadTargetId.value = messaggioId
  fileInputRef.value?.click()
}

async function handleFileChange(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  if (!file || !uploadTargetId.value) return

  uploadLoading.value = true
  try {
    await store.uploadAllegato(uploadTargetId.value, file)
    notificheStore.addToast('Allegato caricato', null, null, 'success')
  }
  catch (error: any) {
    notificheStore.addToast('Errore caricamento allegato', error?.data?.message || null, null, 'error')
  }
  finally {
    uploadLoading.value = false
    // Reset input
    input.value = ''
    uploadTargetId.value = null
  }
}

async function handleDownload(allegatoId: number, nomeFile: string) {
  try {
    const url = await store.downloadAllegato(allegatoId)
    const a = document.createElement('a')
    a.href = url
    a.download = nomeFile
    document.body.appendChild(a)
    a.click()
    document.body.removeChild(a)
    URL.revokeObjectURL(url)
  }
  catch (error: any) {
    notificheStore.addToast('Errore download', error?.data?.message || null, null, 'error')
  }
}

function getInitials(name: string): string {
  const parts = name.trim().split(' ')
  if (parts.length >= 2) return (parts[0][0] + parts[1][0]).toUpperCase()
  return name[0]?.toUpperCase() ?? '?'
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

function formatDimensione(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

onMounted(loadMessaggi)

// Ricarica quando cambiano le props
watch(() => [props.entitaTipo, props.entitaId], loadMessaggi)
</script>

<template>
  <VCard>
    <VCardItem>
      <VCardTitle>Messaggi</VCardTitle>
      <template #append>
        <VChip size="x-small" label>
          {{ store.totalCount }}
        </VChip>
      </template>
    </VCardItem>

    <VCardText>
      <!-- Input nuovo messaggio -->
      <div class="d-flex gap-3 mb-4">
        <div class="flex-grow-1">
          <AppTextarea
            v-model="nuovoTesto"
            placeholder="Scrivi un messaggio..."
            rows="2"
            auto-grow
            hide-details
            @keydown.ctrl.enter="inviaMessaggio"
          />
        </div>
        <div class="d-flex flex-column justify-end">
          <VBtn
            color="primary"
            rounded
            :loading="invioLoading"
            :disabled="!nuovoTesto.trim()"
            @click="inviaMessaggio"
          >
            Invia
          </VBtn>
        </div>
      </div>

      <VDivider class="mb-4" />

      <!-- Loading -->
      <div v-if="loading" class="d-flex justify-center py-6">
        <VProgressCircular indeterminate size="32" />
      </div>

      <!-- Nessun messaggio -->
      <div v-else-if="store.items.length === 0" class="text-center text-disabled py-6">
        <p>Nessun messaggio</p>
      </div>

      <!-- Lista messaggi -->
      <template v-else>
        <div
          v-for="messaggio in store.items"
          :key="messaggio.id"
          class="d-flex gap-3 mb-4"
        >
          <!-- Avatar -->
          <VAvatar size="36" color="primary" variant="tonal">
            <VImg v-if="messaggio.userAvatar" :src="messaggio.userAvatar" />
            <span v-else class="text-caption">{{ getInitials(messaggio.userNome) }}</span>
          </VAvatar>

          <!-- Contenuto -->
          <div class="flex-grow-1">
            <!-- Header: nome + data -->
            <div class="d-flex align-center gap-2 mb-1">
              <span class="text-body-2 font-weight-medium">{{ messaggio.userNome }}</span>
              <span class="text-body-2 text-disabled">{{ formatTimeAgo(messaggio.dataCreazione) }}</span>
              <span v-if="messaggio.dataModifica" class="text-caption text-disabled">(modificato)</span>
            </div>

            <!-- Testo (modalità visualizzazione) -->
            <template v-if="editingId !== messaggio.id">
              <p class="text-body-1 mb-1" style="white-space: pre-wrap;">{{ messaggio.testo }}</p>

              <!-- Allegati -->
              <div v-if="messaggio.allegati.length > 0" class="d-flex flex-wrap gap-2 mt-2">
                <VChip
                  v-for="allegato in messaggio.allegati"
                  :key="allegato.id"
                  size="small"
                  label
                  prepend-icon="tabler-paperclip"
                  class="cursor-pointer"
                  @click="handleDownload(allegato.id, allegato.nomeFile)"
                >
                  {{ allegato.nomeFile }}
                  <span class="text-disabled ms-1">({{ formatDimensione(allegato.dimensione) }})</span>
                </VChip>
              </div>

              <!-- Azioni proprietario -->
              <div v-if="messaggio.isOwner" class="d-flex gap-1 mt-1">
                <VBtn
                  size="x-small"
                  variant="text"
                  color="default"
                  @click="startEdit(messaggio)"
                >
                  Modifica
                </VBtn>
                <VBtn
                  size="x-small"
                  variant="text"
                  color="default"
                  @click="triggerUpload(messaggio.id)"
                >
                  Allega
                </VBtn>
                <VBtn
                  size="x-small"
                  variant="text"
                  color="error"
                  @click="askDelete(messaggio.id)"
                >
                  Elimina
                </VBtn>
              </div>
            </template>

            <!-- Testo (modalità modifica) -->
            <template v-else>
              <AppTextarea
                v-model="editingTesto"
                rows="2"
                auto-grow
                hide-details
                class="mb-2"
                @keydown.ctrl.enter="salvaModifica"
              />
              <div class="d-flex gap-2">
                <VBtn
                  size="small"
                  color="primary"
                  rounded
                  :loading="editLoading"
                  :disabled="!editingTesto.trim()"
                  @click="salvaModifica"
                >
                  Salva
                </VBtn>
                <VBtn
                  size="small"
                  variant="tonal"
                  @click="cancelEdit"
                >
                  Annulla
                </VBtn>
              </div>
            </template>
          </div>
        </div>

        <!-- Carica altri -->
        <div v-if="hasMore" class="text-center mt-2">
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

  <!-- Input file nascosto per upload -->
  <input
    ref="fileInputRef"
    type="file"
    hidden
    @change="handleFileChange"
  >

  <!-- Upload loading overlay -->
  <VOverlay v-model="uploadLoading" persistent class="d-flex align-center justify-center">
    <VProgressCircular indeterminate size="48" />
  </VOverlay>

  <!-- Conferma eliminazione -->
  <VDialog v-model="deleteDialogOpen" max-width="400" persistent>
    <VCard title="Elimina Messaggio">
      <VCardText>
        Sei sicuro di voler eliminare questo messaggio? L'operazione non può essere annullata.
      </VCardText>
      <VCardText class="d-flex justify-end gap-4">
        <VBtn variant="tonal" @click="deleteDialogOpen = false">
          Annulla
        </VBtn>
        <VBtn
          color="error"
          :loading="deleteLoading"
          @click="confirmDelete"
        >
          Elimina
        </VBtn>
      </VCardText>
    </VCard>
  </VDialog>
</template>
