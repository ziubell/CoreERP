<script setup lang="ts">
import { useAnagraficheStore } from '@/stores/anagrafiche'
import { TIPO_ANAGRAFICA_LABELS, TIPO_SOGGETTO_LABELS, PERIODICITA_LABELS } from '@/types/anagrafica'
import type { AnagraficaContattoApi, PeriodicitaPagamento } from '@/types/anagrafica'
import AnagraficaTimeline from '@/components/anagrafica/AnagraficaTimeline.vue'
import ContattoDialog from '@/components/anagrafica/ContattoDialog.vue'
import type { ContattoDialogData } from '@/components/anagrafica/ContattoDialog.vue'
import { useNotificheStore } from '@/stores/notifiche'
import { requiredValidator } from '@/@core/utils/validators'

const route = useRoute()
const router = useRouter()
const store = useAnagraficheStore()

const id = computed(() => Number(route.params.id))
const anagrafica = computed(() => store.current)

// Delete dialog
const deleteDialogOpen = ref(false)
const deleteLoading = ref(false)

// Disattiva dialog
const disattivaDialogOpen = ref(false)
const selectedMotivoId = ref<number | null>(null)
const disattivaLoading = ref(false)

// Contact edit dialog
const contattoDialogOpen = ref(false)
const contattoDialogData = ref<ContattoDialogData | null>(null)

function openContattoEdit(contatto: AnagraficaContattoApi) {
  contattoDialogData.value = {
    id: contatto.contattoId,
    contattoId: contatto.contattoId,
    nome: contatto.nome,
    cognome: contatto.cognome,
    email: contatto.email ?? '',
    cellulare: contatto.cellulare ?? '',
    telefono: contatto.telefono ?? '',
    note: '',
    ruoloContattoId: contatto.ruoloContattoId,
    principale: contatto.principale,
    isExisting: true,
  }
  contattoDialogOpen.value = true
}

const notificheStore = useNotificheStore()

async function handleContattoSave(data: ContattoDialogData) {
  try {
    await store.aggiornaRuoloContatto(id.value, data.contattoId!, {
      ruoloContattoId: data.ruoloContattoId!,
      principale: data.principale,
    })
    await store.fetchById(id.value)
    notificheStore.addToast('Contatto aggiornato con successo', null, null, 'success')
  }
  catch (error: any) {
    notificheStore.addToast('Errore aggiornamento contatto', error?.data?.message || error?.message || null, null, 'error')
  }
}

onMounted(async () => {
  await Promise.all([store.fetchById(id.value), store.fetchLookups()])
})

function handleDisattiva() {
  selectedMotivoId.value = null
  disattivaDialogOpen.value = true
}

async function confirmDisattiva() {
  if (!selectedMotivoId.value) return
  disattivaLoading.value = true
  try {
    await store.disattiva(id.value, selectedMotivoId.value)
    disattivaDialogOpen.value = false
    notificheStore.addToast('Cliente disattivato con successo', null, null, 'success')
  }
  catch (error: any) {
    notificheStore.addToast('Errore durante la disattivazione', error?.data?.message || error?.message || null, null, 'error')
  }
  finally {
    disattivaLoading.value = false
  }
}

const convertiLoading = ref(false)
async function handleConverti() {
  convertiLoading.value = true
  try {
    await store.converti(id.value)
    notificheStore.addToast('Anagrafica convertita a cliente con successo', null, null, 'success')
  }
  catch (error: any) {
    notificheStore.addToast('Errore durante la conversione', error?.data?.message || error?.message || null, null, 'error')
  }
  finally {
    convertiLoading.value = false
  }
}

const riattivaLoading = ref(false)
async function handleRiattiva() {
  riattivaLoading.value = true
  try {
    await store.riattiva(id.value)
    notificheStore.addToast('Cliente riattivato con successo', null, null, 'success')
  }
  catch (error: any) {
    notificheStore.addToast('Errore durante la riattivazione', error?.data?.message || error?.message || null, null, 'error')
  }
  finally {
    riattivaLoading.value = false
  }
}

async function confirmDelete() {
  deleteLoading.value = true
  try {
    await store.remove(id.value)
    router.push('/anagrafiche')
  }
  catch (error: any) {
    notificheStore.addToast('Errore durante l\'eliminazione', error?.data?.message || error?.message || null, null, 'error')
    deleteDialogOpen.value = false
  }
  finally {
    deleteLoading.value = false
  }
}

const fullAddress = computed(() => {
  if (!anagrafica.value) return ''
  const parts = [
    anagrafica.value.indirizzoFatturazione,
    [anagrafica.value.cap, anagrafica.value.citta, anagrafica.value.provincia].filter(Boolean).join(' '),
    anagrafica.value.nazione,
  ].filter(Boolean)
  return parts.join(', ')
})
</script>

<template>
  <div v-if="anagrafica">
    <!-- Header -->
    <div class="d-flex align-center mb-6 flex-wrap gap-4">
      <VBtn variant="text" icon="tabler-arrow-left" :to="{ path: '/anagrafiche' }" />

      <div>
        <h4 class="text-h4">{{ anagrafica.denominazione }}</h4>
        <div class="d-flex gap-2 mt-1">
          <VChip
            :color="anagrafica.tipo === 1 ? 'primary' : 'secondary'"
            size="small"
            label
          >
            {{ TIPO_ANAGRAFICA_LABELS[anagrafica.tipo] }}
          </VChip>
          <VChip
            :color="anagrafica.attivo ? 'success' : 'error'"
            size="small"
            label
          >
            {{ anagrafica.attivo ? 'Attivo' : 'Non attivo' }}
          </VChip>
          <VChip v-if="anagrafica.codiceCliente" size="small" label variant="outlined">
            Cod. {{ anagrafica.codiceCliente }}
          </VChip>
        </div>
      </div>

      <VSpacer />

      <div class="d-flex gap-2">
        <VBtn
          color="primary"
          prepend-icon="tabler-edit"
          :to="`/anagrafiche/modifica-${id}`"
        >
          Modifica
        </VBtn>

        <VBtn
          variant="tonal"
          color="secondary"
          append-icon="tabler-chevron-down"
        >
          Azioni
          <VMenu activator="parent">
            <VList>
              <VListItem
                v-if="anagrafica.tipo === 0"
                prepend-icon="tabler-transform"
                title="Converti a Cliente"
                :disabled="convertiLoading"
                @click="handleConverti"
              />
              <VListItem
                v-if="anagrafica.attivo && anagrafica.tipo === 1"
                prepend-icon="tabler-ban"
                title="Disattiva"
                @click="handleDisattiva"
              />
              <VListItem
                v-if="!anagrafica.attivo"
                prepend-icon="tabler-check"
                title="Riattiva"
                :disabled="riattivaLoading"
                @click="handleRiattiva"
              />
              <VDivider />
              <VListItem
                prepend-icon="tabler-trash"
                title="Elimina"
                class="text-error"
                @click="deleteDialogOpen = true"
              />
            </VList>
          </VMenu>
        </VBtn>
      </div>
    </div>

    <!-- 2-column layout -->
    <VRow>
      <!-- LEFT COLUMN - Customer Card + Contacts -->
      <VCol cols="12" md="4" lg="4">
        <!-- Customer Details Card -->
        <VCard>
          <VCardText class="text-center pt-6">
            <!-- Score Icons -->
            <div class="d-flex justify-center gap-6 mb-2">
              <!-- Score -->
              <div class="d-flex align-center gap-2">
                <VAvatar size="40" color="primary" variant="tonal" rounded>
                  <VIcon icon="tabler-star" size="24" />
                </VAvatar>
                <div class="text-start">
                  <span class="text-h6 d-block">--</span>
                  <span class="text-body-2 text-disabled">Score</span>
                </div>
              </div>

              <!-- Contatti count -->
              <div class="d-flex align-center gap-2">
                <VAvatar size="40" color="info" variant="tonal" rounded>
                  <VIcon icon="tabler-users" size="24" />
                </VAvatar>
                <div class="text-start">
                  <span class="text-h6 d-block">{{ anagrafica.contatti?.length ?? 0 }}</span>
                  <span class="text-body-2 text-disabled">Contatti</span>
                </div>
              </div>
            </div>
          </VCardText>

          <VDivider />

          <!-- Dettagli -->
          <VCardItem>
            <VCardTitle>Dettagli</VCardTitle>
          </VCardItem>
          <VCardText>
            <div class="d-flex flex-column gap-3">
              <h6 class="text-h6">
                Tipo Soggetto:
                <span class="text-body-1 d-inline-block">{{ TIPO_SOGGETTO_LABELS[anagrafica.tipoSoggetto] }}</span>
              </h6>
              <h6 v-if="anagrafica.codiceCliente" class="text-h6">
                Codice Cliente:
                <span class="text-body-1 d-inline-block">{{ anagrafica.codiceCliente }}</span>
              </h6>
              <h6 v-if="anagrafica.telefono" class="text-h6">
                Telefono:
                <span class="text-body-1 d-inline-block">{{ anagrafica.telefono }}</span>
              </h6>
              <h6 v-if="anagrafica.pec" class="text-h6">
                PEC:
                <span class="text-body-1 d-inline-block">{{ anagrafica.pec }}</span>
              </h6>
              <h6 v-if="anagrafica.sitoWeb" class="text-h6">
                Sito Web:
                <a :href="anagrafica.sitoWeb" target="_blank" class="text-body-1 d-inline-block">{{ anagrafica.sitoWeb }}</a>
              </h6>
              <h6 v-if="anagrafica.partitaIva" class="text-h6">
                P.IVA:
                <span class="text-body-1 d-inline-block">{{ anagrafica.partitaIva }}</span>
              </h6>
              <h6 v-if="anagrafica.codiceFiscale" class="text-h6">
                Codice Fiscale:
                <span class="text-body-1 d-inline-block">{{ anagrafica.codiceFiscale }}</span>
              </h6>
              <h6 v-if="anagrafica.codiceSDI" class="text-h6">
                Codice SDI:
                <span class="text-body-1 d-inline-block">{{ anagrafica.codiceSDI }}</span>
              </h6>
              <h6 v-if="fullAddress" class="text-h6">
                Indirizzo:
                <span class="text-body-1 d-inline-block">{{ fullAddress }}</span>
              </h6>
              <h6 v-if="anagrafica.metodoPagamentoNome" class="text-h6">
                Metodo Pagamento:
                <span class="text-body-1 d-inline-block">{{ anagrafica.metodoPagamentoNome }}</span>
              </h6>
              <h6 v-if="anagrafica.iban" class="text-h6">
                IBAN:
                <span class="text-body-1 d-inline-block">{{ anagrafica.iban }}</span>
              </h6>
              <h6 v-if="anagrafica.periodicitaPagamento" class="text-h6">
                Periodicità:
                <span class="text-body-1 d-inline-block">{{ PERIODICITA_LABELS[anagrafica.periodicitaPagamento as PeriodicitaPagamento] }}</span>
              </h6>
              <h6 v-if="anagrafica.dataConversione" class="text-h6">
                Data Conversione:
                <span class="text-body-1 d-inline-block">{{ new Date(anagrafica.dataConversione).toLocaleDateString('it-IT') }}</span>
              </h6>
              <h6 class="text-h6">
                Creato il:
                <span class="text-body-1 d-inline-block">{{ new Date(anagrafica.dataCreazione).toLocaleDateString('it-IT') }}</span>
              </h6>
              <h6 v-if="anagrafica.dataModifica" class="text-h6">
                Modificato il:
                <span class="text-body-1 d-inline-block">{{ new Date(anagrafica.dataModifica).toLocaleDateString('it-IT') }}</span>
              </h6>
              <h6 v-if="!anagrafica.attivo && anagrafica.motivoDisattivazioneNome" class="text-h6">
                Motivo Disattivazione:
                <VChip size="x-small" color="error" label>{{ anagrafica.motivoDisattivazioneNome }}</VChip>
              </h6>
              <h6 v-if="anagrafica.note" class="text-h6">
                Note:
                <span class="text-body-1 d-inline-block">{{ anagrafica.note }}</span>
              </h6>
            </div>
          </VCardText>

          <VDivider />

          <!-- Contatti -->
          <VCardText>
            <h6 class="text-h6 mb-3">Contatti</h6>
            <template v-if="anagrafica.contatti && anagrafica.contatti.length > 0">
              <VList density="compact" class="pa-0">
                <VListItem
                  v-for="contatto in anagrafica.contatti"
                  :key="contatto.contattoId"
                  class="cursor-pointer px-0"
                  @click="openContattoEdit(contatto)"
                >
                  <VListItemTitle>
                    <span class="font-weight-medium">{{ contatto.nome }} {{ contatto.cognome }}</span>
                    <VChip size="x-small" label class="ms-2" :color="contatto.principale ? 'primary' : undefined">{{ contatto.ruoloContattoNome }}</VChip>
                  </VListItemTitle>
                  <VListItemSubtitle v-if="contatto.email || contatto.cellulare">
                    {{ [contatto.email, contatto.cellulare].filter(Boolean).join(' | ') }}
                  </VListItemSubtitle>
                </VListItem>
              </VList>
            </template>
            <div v-else class="text-disabled text-body-2">
              Nessun contatto associato
            </div>
          </VCardText>

        </VCard>
      </VCol>

      <!-- RIGHT COLUMN - Timeline + Opportunità -->
      <VCol cols="12" md="8" lg="8">
        <!-- Opportunità (placeholder) -->
        <VCard class="mb-4">
          <VCardItem>
            <VCardTitle>Opportunità</VCardTitle>
          </VCardItem>
          <VCardText class="text-disabled text-body-2">
            Modulo in sviluppo
          </VCardText>
        </VCard>

        <!-- Timeline -->
        <AnagraficaTimeline :anagrafica-id="id" />
      </VCol>
    </VRow>
  </div>

  <div v-else-if="store.loading" class="d-flex justify-center py-12">
    <VProgressCircular indeterminate />
  </div>

  <!-- Contatto Edit Dialog -->
  <ContattoDialog
    v-model="contattoDialogOpen"
    :contatto="contattoDialogData"
    :ruoli-contatto="store.ruoliContatto"
    @save="handleContattoSave"
  />

  <!-- Disattiva Dialog -->
  <VDialog v-model="disattivaDialogOpen" max-width="450" persistent>
    <VCard title="Disattiva Cliente">
      <VCardText>
        <p class="text-body-1 mb-4">
          Seleziona il motivo della disattivazione:
        </p>
        <AppSelect
          v-model="selectedMotivoId"
          :items="store.motiviDisattivazione.map(m => ({ title: m.nome, value: m.id }))"
          label="Motivo Disattivazione"
          :rules="[requiredValidator]"
        />
      </VCardText>
      <VCardText class="d-flex justify-end gap-4">
        <VBtn variant="tonal" color="secondary" @click="disattivaDialogOpen = false">
          Annulla
        </VBtn>
        <VBtn
          color="warning"
          :loading="disattivaLoading"
          :disabled="!selectedMotivoId"
          @click="confirmDisattiva"
        >
          Disattiva
        </VBtn>
      </VCardText>
    </VCard>
  </VDialog>

  <!-- Delete Confirmation Dialog -->
  <VDialog v-model="deleteDialogOpen" max-width="400" persistent>
    <VCard title="Elimina Anagrafica">
      <VCardText>
        Sei sicuro di voler eliminare questa anagrafica? L'operazione non può essere annullata.
      </VCardText>
      <VCardText class="d-flex justify-end gap-4">
        <VBtn variant="tonal" color="secondary" @click="deleteDialogOpen = false">
          Annulla
        </VBtn>
        <VBtn
          color="error"
          prepend-icon="tabler-trash"
          :loading="deleteLoading"
          @click="confirmDelete"
        >
          Elimina
        </VBtn>
      </VCardText>
    </VCard>
  </VDialog>

</template>
