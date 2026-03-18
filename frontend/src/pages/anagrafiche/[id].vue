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
          variant="tonal"
          prepend-icon="tabler-edit"
          :to="`/anagrafiche/modifica-${id}`"
        >
          Modifica
        </VBtn>
        <VBtn
          v-if="anagrafica.tipo === 0"
          color="primary"
          variant="tonal"
          prepend-icon="tabler-transform"
          :loading="convertiLoading"
          @click="handleConverti"
        >
          Converti a Cliente
        </VBtn>
        <VBtn
          v-if="anagrafica.attivo && anagrafica.tipo === 1"
          color="warning"
          variant="tonal"
          prepend-icon="tabler-ban"
          @click="handleDisattiva"
        >
          Disattiva
        </VBtn>
        <VBtn
          v-if="!anagrafica.attivo"
          color="success"
          variant="tonal"
          prepend-icon="tabler-check"
          :loading="riattivaLoading"
          @click="handleRiattiva"
        >
          Riattiva
        </VBtn>
        <VBtn
          color="error"
          variant="tonal"
          icon="tabler-trash"
          @click="deleteDialogOpen = true"
        />
      </div>
    </div>

    <!-- 2-column layout -->
    <VRow>
      <!-- LEFT COLUMN - Customer Card + Contacts -->
      <VCol cols="12" md="4" lg="4">
        <!-- Customer Details Card -->
        <VCard>
          <VCardText class="text-center pt-6">
            <VChip
              :color="anagrafica.tipo === 1 ? 'primary' : 'secondary'"
              size="small"
              label
              class="mb-4"
            >
              {{ TIPO_ANAGRAFICA_LABELS[anagrafica.tipo] }}
            </VChip>

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
            <ul class="list-unstyled mb-6">
              <li class="mb-2">
                <span class="h6 me-1">Tipo Soggetto:</span>
                <span>{{ TIPO_SOGGETTO_LABELS[anagrafica.tipoSoggetto] }}</span>
              </li>
              <li v-if="anagrafica.codiceCliente" class="mb-2">
                <span class="h6 me-1">Codice Cliente:</span>
                <span>{{ anagrafica.codiceCliente }}</span>
              </li>
              <li v-if="anagrafica.telefono" class="mb-2">
                <span class="h6 me-1">Telefono:</span>
                <span>{{ anagrafica.telefono }}</span>
              </li>
              <li v-if="anagrafica.pec" class="mb-2">
                <span class="h6 me-1">PEC:</span>
                <span>{{ anagrafica.pec }}</span>
              </li>
              <li v-if="anagrafica.sitoWeb" class="mb-2">
                <span class="h6 me-1">Sito Web:</span>
                <a :href="anagrafica.sitoWeb" target="_blank">{{ anagrafica.sitoWeb }}</a>
              </li>
              <li v-if="anagrafica.partitaIva" class="mb-2">
                <span class="h6 me-1">P.IVA:</span>
                <span>{{ anagrafica.partitaIva }}</span>
              </li>
              <li v-if="anagrafica.codiceFiscale" class="mb-2">
                <span class="h6 me-1">Codice Fiscale:</span>
                <span>{{ anagrafica.codiceFiscale }}</span>
              </li>
              <li v-if="anagrafica.codiceSDI" class="mb-2">
                <span class="h6 me-1">Codice SDI:</span>
                <span>{{ anagrafica.codiceSDI }}</span>
              </li>
              <li v-if="fullAddress" class="mb-2">
                <span class="h6 me-1">Indirizzo:</span>
                <span>{{ fullAddress }}</span>
              </li>
              <li v-if="anagrafica.metodoPagamentoNome" class="mb-2">
                <span class="h6 me-1">Metodo Pagamento:</span>
                <span>{{ anagrafica.metodoPagamentoNome }}</span>
              </li>
              <li v-if="anagrafica.iban" class="mb-2">
                <span class="h6 me-1">IBAN:</span>
                <span>{{ anagrafica.iban }}</span>
              </li>
              <li v-if="anagrafica.periodicitaPagamento" class="mb-2">
                <span class="h6 me-1">Periodicità:</span>
                <span>{{ PERIODICITA_LABELS[anagrafica.periodicitaPagamento as PeriodicitaPagamento] }}</span>
              </li>
              <li v-if="anagrafica.dataConversione" class="mb-2">
                <span class="h6 me-1">Data Conversione:</span>
                <span>{{ new Date(anagrafica.dataConversione).toLocaleDateString('it-IT') }}</span>
              </li>
              <li class="mb-2">
                <span class="h6 me-1">Creato il:</span>
                <span>{{ new Date(anagrafica.dataCreazione).toLocaleDateString('it-IT') }}</span>
              </li>
              <li v-if="anagrafica.dataModifica" class="mb-2">
                <span class="h6 me-1">Modificato il:</span>
                <span>{{ new Date(anagrafica.dataModifica).toLocaleDateString('it-IT') }}</span>
              </li>
              <li v-if="!anagrafica.attivo && anagrafica.motivoDisattivazioneNome" class="mb-2">
                <span class="h6 me-1">Motivo Disattivazione:</span>
                <VChip size="x-small" color="error" label>{{ anagrafica.motivoDisattivazioneNome }}</VChip>
              </li>
              <li v-if="anagrafica.note" class="mb-2">
                <span class="h6 me-1">Note:</span>
                <span>{{ anagrafica.note }}</span>
              </li>
            </ul>
          </VCardText>

          <VDivider />

          <!-- Contatti -->
          <VCardItem>
            <VCardTitle>Contatti</VCardTitle>
          </VCardItem>
          <VCardText>
            <template v-if="anagrafica.contatti && anagrafica.contatti.length > 0">
              <div
                v-for="contatto in anagrafica.contatti"
                :key="contatto.contattoId"
                class="d-flex align-center gap-3 mb-3 cursor-pointer"
                @click="openContattoEdit(contatto)"
              >
                <VAvatar size="32" color="primary" variant="tonal">
                  <span class="text-caption">{{ contatto.nome[0] }}{{ contatto.cognome[0] }}</span>
                </VAvatar>
                <div class="flex-grow-1" style="min-width: 0;">
                  <span class="text-body-1 font-weight-medium d-block text-truncate">
                    {{ contatto.nome }} {{ contatto.cognome }}
                  </span>
                  <span class="text-body-2 text-disabled">{{ contatto.ruoloContattoNome }}</span>
                  <div v-if="contatto.email || contatto.cellulare" class="text-body-2 text-disabled">
                    {{ [contatto.email, contatto.cellulare].filter(Boolean).join(' | ') }}
                  </div>
                </div>
                <VChip v-if="contatto.principale" size="x-small" color="primary" label>
                  Principale
                </VChip>
              </div>
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
      <VCardActions>
        <VSpacer />
        <VBtn variant="text" @click="disattivaDialogOpen = false">
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
      </VCardActions>
    </VCard>
  </VDialog>

  <!-- Delete Confirmation Dialog -->
  <VDialog v-model="deleteDialogOpen" max-width="400" persistent>
    <VCard title="Elimina Anagrafica">
      <VCardText>
        Sei sicuro di voler eliminare questa anagrafica? L'operazione non può essere annullata.
      </VCardText>
      <VCardActions>
        <VSpacer />
        <VBtn variant="text" @click="deleteDialogOpen = false">
          Annulla
        </VBtn>
        <VBtn
          color="error"
          :loading="deleteLoading"
          @click="confirmDelete"
        >
          Elimina
        </VBtn>
      </VCardActions>
    </VCard>
  </VDialog>

</template>

<style lang="scss" scoped>
.list-unstyled {
  padding-left: 0;
  list-style: none;
}

.cursor-pointer {
  cursor: pointer;

  &:hover {
    background-color: rgba(var(--v-theme-on-surface), 0.04);
    border-radius: 8px;
    margin-inline: -8px;
    padding-inline: 8px;
  }
}
</style>
