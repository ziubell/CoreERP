<script setup lang="ts">
import { useAnagraficheStore } from '@/stores/anagrafiche'
import { useMessaggiStore } from '@/stores/messaggi'
import { useNotificheStore } from '@/stores/notifiche'
import type { StoricoModificaApi } from '@/types/anagrafica'
import type { MessaggioApi } from '@/types/messaggio'

const props = defineProps<{
  anagraficaId: number
}>()

const store = useAnagraficheStore()
const messaggiStore = useMessaggiStore()
const notificheStore = useNotificheStore()

const loading = ref(false)
const loadingMore = ref(false)
const storico = ref<StoricoModificaApi[]>([])
const messaggi = ref<MessaggioApi[]>([])
const pagina = ref(1)
const hasMore = ref(true)
const dimensionePagina = 20

// Input messaggio
const nuovoMessaggio = ref('')
const invioLoading = ref(false)
const fileInputRef = ref<HTMLInputElement | null>(null)
const allegatiPending = ref<File[]>([])

// Edit inline
const editingId = ref<number | null>(null)
const editingText = ref('')

// Delete confirm
const deleteDialogOpen = ref(false)
const deleteTarget = ref<{ id: number; type: 'messaggio' } | null>(null)
const deleteLoading = ref(false)

// Tipo unificato per la timeline
interface TimelineEntry {
  type: 'modifica' | 'messaggio'
  date: string
  storico?: StoricoModificaApi
  messaggio?: MessaggioApi
}

const timelineEntries = computed<TimelineEntry[]>(() => {
  const entries: TimelineEntry[] = []

  for (const s of storico.value) {
    entries.push({ type: 'modifica', date: s.dataModifica, storico: s })
  }

  for (const m of messaggi.value) {
    entries.push({ type: 'messaggio', date: m.dataCreazione, messaggio: m })
  }

  entries.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime())
  return entries
})

async function loadAll() {
  loading.value = true
  pagina.value = 1
  try {
    await Promise.all([
      store.fetchStorico(props.anagraficaId, 1, dimensionePagina),
      messaggiStore.fetchByEntita('Anagrafica', props.anagraficaId, 1, dimensionePagina),
    ])
    storico.value = store.storico
    messaggi.value = messaggiStore.items
    hasMore.value = store.storico.length >= dimensionePagina || messaggiStore.items.length >= dimensionePagina
  }
  finally {
    loading.value = false
  }
}

async function loadMore() {
  loadingMore.value = true
  pagina.value++
  try {
    await Promise.all([
      store.fetchStorico(props.anagraficaId, pagina.value, dimensionePagina),
      messaggiStore.fetchByEntita('Anagrafica', props.anagraficaId, pagina.value, dimensionePagina),
    ])
    storico.value.push(...store.storico)
    messaggi.value = messaggiStore.items
    hasMore.value = store.storico.length >= dimensionePagina || messaggiStore.totalCount > messaggi.value.length
  }
  finally {
    loadingMore.value = false
  }
}

// Invio messaggio
async function inviaMessaggio() {
  if (!nuovoMessaggio.value.trim()) return
  invioLoading.value = true
  try {
    const msg = await messaggiStore.create({
      entitaTipo: 'Anagrafica',
      entitaId: props.anagraficaId,
      testo: nuovoMessaggio.value.trim(),
    })

    // Upload allegati pending
    for (const file of allegatiPending.value) {
      await messaggiStore.uploadAllegato(msg.id, file)
    }

    // Reload per avere allegati aggiornati
    if (allegatiPending.value.length > 0) {
      await messaggiStore.fetchByEntita('Anagrafica', props.anagraficaId, 1, dimensionePagina)
    }

    messaggi.value = messaggiStore.items
    nuovoMessaggio.value = ''
    allegatiPending.value = []
  }
  catch {
    notificheStore.addToast('Errore invio messaggio', null, null, 'error')
  }
  finally {
    invioLoading.value = false
  }
}

function onKeydown(e: KeyboardEvent) {
  if (e.key === 'Enter' && (e.ctrlKey || e.metaKey)) {
    e.preventDefault()
    inviaMessaggio()
  }
}

function openFileDialog() {
  fileInputRef.value?.click()
}

function onFileSelected(e: Event) {
  const input = e.target as HTMLInputElement
  if (input.files) {
    allegatiPending.value.push(...Array.from(input.files))
    input.value = ''
  }
}

function removeAllegatoPending(index: number) {
  allegatiPending.value.splice(index, 1)
}

// Edit messaggio
function startEdit(msg: MessaggioApi) {
  editingId.value = msg.id
  editingText.value = msg.testo
}

function cancelEdit() {
  editingId.value = null
  editingText.value = ''
}

async function saveEdit() {
  if (!editingId.value || !editingText.value.trim()) return
  try {
    await messaggiStore.update(editingId.value, editingText.value.trim())
    messaggi.value = messaggiStore.items
    editingId.value = null
    editingText.value = ''
  }
  catch {
    notificheStore.addToast('Errore modifica messaggio', null, null, 'error')
  }
}

// Upload allegato su messaggio esistente
async function uploadAllegatoToMsg(msgId: number) {
  const input = document.createElement('input')
  input.type = 'file'
  input.onchange = async () => {
    if (input.files?.[0]) {
      try {
        await messaggiStore.uploadAllegato(msgId, input.files[0])
        messaggi.value = messaggiStore.items
      }
      catch {
        notificheStore.addToast('Errore upload allegato', null, null, 'error')
      }
    }
  }
  input.click()
}

// Delete
function askDelete(id: number) {
  deleteTarget.value = { id, type: 'messaggio' }
  deleteDialogOpen.value = true
}

async function confirmDelete() {
  if (!deleteTarget.value) return
  deleteLoading.value = true
  try {
    await messaggiStore.remove(deleteTarget.value.id)
    messaggi.value = messaggiStore.items
    deleteDialogOpen.value = false
    notificheStore.addToast('Messaggio eliminato', null, null, 'success')
  }
  catch {
    notificheStore.addToast('Errore eliminazione', null, null, 'error')
  }
  finally {
    deleteLoading.value = false
  }
}

// Download allegato
async function downloadAllegato(allegatoId: number, nomeFile: string) {
  try {
    const accessToken = useCookie('accessToken').value
    const response = await fetch(`/api/v1/messaggi/allegati/${allegatoId}/download`, {
      headers: { Authorization: `Bearer ${accessToken}` },
    })
    if (!response.ok) throw new Error('Download fallito')

    const blob = await response.blob()
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = nomeFile
    a.click()
    URL.revokeObjectURL(url)
  }
  catch {
    notificheStore.addToast('Errore download', null, null, 'error')
  }
}

// Restore storico
async function handleRestore(storicoId: number) {
  try {
    await store.restore(props.anagraficaId, storicoId)
    await Promise.all([loadAll(), store.fetchById(props.anagraficaId)])
  }
  catch (error: any) {
    console.error(error)
  }
}

function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
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

function getInitials(name: string | null | undefined): string {
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

onMounted(loadAll)
</script>

<template>
  <VCard>
    <VCardItem>
      <VCardTitle>Timeline Attività</VCardTitle>
    </VCardItem>

    <VCardText>
      <!-- Input messaggio -->
      <div class="timeline-input mb-6">
        <div class="timeline-input-box">
          <textarea
            v-model="nuovoMessaggio"
            class="timeline-textarea"
            placeholder="Scrivi un messaggio..."
            rows="1"
            @keydown="onKeydown"
            @input="(e: Event) => {
              const t = e.target as HTMLTextAreaElement
              t.style.height = 'auto'
              t.style.height = t.scrollHeight + 'px'
            }"
          />
          <div class="timeline-input-actions">
            <IconBtn size="small" @click="openFileDialog">
              <VIcon icon="tabler-paperclip" size="18" />
            </IconBtn>
            <VBtn
              size="small"
              color="primary"
              :loading="invioLoading"
              :disabled="!nuovoMessaggio.trim()"
              @click="inviaMessaggio"
            >
              Invia
            </VBtn>
          </div>
        </div>
        <input
          ref="fileInputRef"
          type="file"
          multiple
          hidden
          @change="onFileSelected"
        >
        <!-- Allegati pending -->
        <div v-if="allegatiPending.length > 0" class="d-flex flex-wrap gap-1 mt-2">
          <VChip
            v-for="(file, i) in allegatiPending"
            :key="i"
            size="small"
            closable
            @click:close="removeAllegatoPending(i)"
          >
            {{ file.name }}
          </VChip>
        </div>
      </div>

      <!-- Loading -->
      <div v-if="loading" class="d-flex justify-center py-6">
        <VProgressCircular indeterminate size="32" />
      </div>

      <div v-else-if="timelineEntries.length === 0" class="text-center text-disabled py-6">
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
            v-for="entry in timelineEntries"
            :key="`${entry.type}-${entry.type === 'messaggio' ? entry.messaggio!.id : entry.storico!.id}`"
            :dot-color="entry.type === 'messaggio' ? 'info' : getColor(entry.storico!)"
            size="x-small"
          >
            <!-- MESSAGGIO -->
            <template v-if="entry.type === 'messaggio'">
              <!-- Edit inline -->
              <template v-if="editingId === entry.messaggio!.id">
                <div class="d-flex align-center gap-2 mb-1">
                  <VAvatar size="24" color="info" variant="tonal">
                    <VImg v-if="entry.messaggio!.userAvatar" :src="entry.messaggio!.userAvatar" />
                    <span v-else class="text-caption">{{ getInitials(entry.messaggio!.userNome) }}</span>
                  </VAvatar>
                  <span class="text-body-2 font-weight-medium">{{ entry.messaggio!.userNome }}</span>
                </div>
                <textarea
                  v-model="editingText"
                  class="timeline-textarea timeline-textarea-edit"
                  rows="2"
                  @keydown.ctrl.enter.prevent="saveEdit"
                  @keydown.meta.enter.prevent="saveEdit"
                  @keydown.escape="cancelEdit"
                />
                <div class="d-flex gap-2 mt-1">
                  <VBtn size="x-small" color="primary" @click="saveEdit">Salva</VBtn>
                  <VBtn size="x-small" variant="tonal" @click="cancelEdit">Annulla</VBtn>
                </div>
              </template>

              <!-- Visualizzazione normale -->
              <template v-else>
                <div class="d-flex align-center flex-wrap gap-2">
                  <div class="d-flex align-center gap-2">
                    <VAvatar size="24" color="info" variant="tonal">
                      <VImg v-if="entry.messaggio!.userAvatar" :src="entry.messaggio!.userAvatar" />
                      <span v-else class="text-caption">{{ getInitials(entry.messaggio!.userNome) }}</span>
                    </VAvatar>
                    <span class="text-body-2 font-weight-medium">{{ entry.messaggio!.userNome }}</span>
                    <span class="text-body-2 text-disabled">· {{ formatTimeAgo(entry.messaggio!.dataCreazione) }}</span>
                    <span v-if="entry.messaggio!.dataModifica" class="text-caption text-disabled">(modificato)</span>
                  </div>

                  <div class="d-flex align-center gap-2">
                    <VChip color="info" size="x-small" label>Messaggio</VChip>
                    <span class="text-body-2" style="white-space: pre-wrap;">{{ entry.messaggio!.testo }}</span>
                  </div>

                  <VSpacer />

                  <template v-if="entry.messaggio!.isOwner">
                    <IconBtn size="x-small" @click="startEdit(entry.messaggio!)">
                      <VIcon icon="tabler-edit" size="14" />
                    </IconBtn>
                    <IconBtn size="x-small" @click="uploadAllegatoToMsg(entry.messaggio!.id)">
                      <VIcon icon="tabler-paperclip" size="14" />
                    </IconBtn>
                    <IconBtn size="x-small" color="error" @click="askDelete(entry.messaggio!.id)">
                      <VIcon icon="tabler-trash" size="14" />
                    </IconBtn>
                  </template>
                </div>

                <!-- Allegati -->
                <div v-if="entry.messaggio!.allegati.length > 0" class="d-flex flex-wrap gap-1 mt-1">
                  <VChip
                    v-for="all in entry.messaggio!.allegati"
                    :key="all.id"
                    size="small"
                    prepend-icon="tabler-file"
                    class="cursor-pointer"
                    @click="downloadAllegato(all.id, all.nomeFile)"
                  >
                    {{ all.nomeFile }}
                    <span class="text-disabled ms-1">({{ formatFileSize(all.dimensione) }})</span>
                  </VChip>
                </div>
              </template>
            </template>

            <!-- MODIFICA (storico) -->
            <template v-else>
              <div class="d-flex align-center flex-wrap gap-2">
                <div class="d-flex align-center gap-2">
                  <VAvatar size="24" color="primary" variant="tonal">
                    <VImg v-if="entry.storico!.modificatoDaAvatar" :src="entry.storico!.modificatoDaAvatar" />
                    <span v-else class="text-caption">{{ getInitials(entry.storico!.modificatoDaNome) }}</span>
                  </VAvatar>
                  <span class="text-body-2 font-weight-medium">{{ entry.storico!.modificatoDaNome ?? entry.storico!.modificatoDa }}</span>
                  <span class="text-body-2 text-disabled">· {{ formatTimeAgo(entry.storico!.dataModifica) }}</span>
                </div>

                <div class="d-flex align-center gap-2">
                  <VChip :color="getColor(entry.storico!)" size="x-small" label>
                    {{ getBadgeLabel(entry.storico!) }}
                  </VChip>
                  <span class="text-body-2">
                    <span class="font-weight-medium">{{ entry.storico!.campo }}:</span>
                    <template v-if="entry.storico!.valorePrecedente || entry.storico!.valoreNuovo">
                      <span class="text-disabled text-decoration-line-through">{{ entry.storico!.valorePrecedenteLabel ?? entry.storico!.valorePrecedente ?? '(vuoto)' }}</span>
                      <VIcon icon="tabler-arrow-right" size="14" class="mx-1" />
                      <span>{{ entry.storico!.valoreNuovoLabel ?? entry.storico!.valoreNuovo ?? '(vuoto)' }}</span>
                    </template>
                  </span>
                </div>

                <VSpacer />

                <VBtn
                  v-if="!entry.storico!.note?.includes('Ripristinato')"
                  size="x-small"
                  variant="text"
                  color="warning"
                  @click="handleRestore(entry.storico!.id)"
                >
                  Ripristina
                </VBtn>
              </div>
            </template>
          </VTimelineItem>
        </VTimeline>

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

  <!-- Delete Dialog -->
  <VDialog v-model="deleteDialogOpen" max-width="400" persistent>
    <VCard title="Elimina messaggio">
      <VCardText>
        Sei sicuro di voler eliminare questo messaggio? L'operazione non può essere annullata.
      </VCardText>
      <VCardText class="d-flex justify-end gap-4">
        <VBtn variant="tonal" @click="deleteDialogOpen = false">Annulla</VBtn>
        <VBtn color="error" prepend-icon="tabler-trash" :loading="deleteLoading" @click="confirmDelete">
          Elimina
        </VBtn>
      </VCardText>
    </VCard>
  </VDialog>
</template>

<style>
.timeline-input-box {
  display: flex;
  align-items: flex-end;
  gap: 8px;
  border: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
  border-radius: 8px;
  padding: 8px 8px 8px 16px;
}

.timeline-textarea {
  flex: 1;
  border: none;
  outline: none;
  resize: none;
  font-family: inherit;
  font-size: 14px;
  line-height: 1.5;
  background: transparent;
  color: rgba(var(--v-theme-on-surface), var(--v-high-emphasis-opacity));
  min-block-size: 24px;
  max-block-size: 200px;
  overflow-y: auto;
}

.timeline-textarea::placeholder {
  color: rgba(var(--v-theme-on-surface), 0.4);
}

.timeline-textarea-edit {
  inline-size: 100%;
  border: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
  border-radius: 6px;
  padding: 8px 12px;
}

.timeline-input-actions {
  display: flex;
  align-items: center;
  gap: 4px;
  flex-shrink: 0;
}
</style>
