<script setup lang="ts">
import { useAnagraficheStore } from '@/stores/anagrafiche'
import { TIPO_ANAGRAFICA_LABELS, TIPO_SOGGETTO_LABELS, PERIODICITA_LABELS } from '@/types/anagrafica'

definePage({ meta: { navActiveLink: 'anagrafiche' } })
import type { AnagraficaContattoApi, PeriodicitaPagamento } from '@/types/anagrafica'
import AnagraficaTimeline from '@/components/anagrafica/AnagraficaTimeline.vue'
import ContattoDialog from '@/components/anagrafica/ContattoDialog.vue'
import type { ContattoDialogData } from '@/components/anagrafica/ContattoDialog.vue'
import IndirizzoDialog from '@/components/anagrafica/IndirizzoDialog.vue'
import { useIndirizziStore } from '@/stores/indirizzi'
import type { IndirizzoApi } from '@/types/indirizzo'
import { useNotificheStore } from '@/stores/notifiche'
import { requiredValidator } from '@/@core/utils/validators'

const route = useRoute()
const router = useRouter()
const store = useAnagraficheStore()

const indirizziStore = useIndirizziStore()
const id = computed(() => Number(route.params.id))
const anagrafica = computed(() => store.current)

// Indirizzi
const indirizziList = ref<IndirizzoApi[]>([])
const indirizzoDialogOpen = ref(false)
const editingIndirizzo = ref<IndirizzoApi | null>(null)

function openAddIndirizzo() {
  editingIndirizzo.value = null
  indirizzoDialogOpen.value = true
}

function openEditIndirizzo(indirizzo: IndirizzoApi) {
  editingIndirizzo.value = indirizzo
  indirizzoDialogOpen.value = true
}

async function onIndirizzoSaved() {
  indirizziList.value = await indirizziStore.fetchByAnagrafica(id.value)
}

// Conferma rimozione (contatti e indirizzi)
const removeDialogOpen = ref(false)
const removeDialogType = ref<'contatto' | 'indirizzo'>('contatto')
const removeDialogId = ref<number>(0)
const removeDialogName = ref('')
const removeLoading = ref(false)

function askRemoveContatto(contattoId: number, nome: string) {
  removeDialogType.value = 'contatto'
  removeDialogId.value = contattoId
  removeDialogName.value = nome
  removeDialogOpen.value = true
}

function askRemoveIndirizzo(indirizzoId: number, label: string) {
  removeDialogType.value = 'indirizzo'
  removeDialogId.value = indirizzoId
  removeDialogName.value = label
  removeDialogOpen.value = true
}

async function confirmRemoveAssociation() {
  removeLoading.value = true
  try {
    if (removeDialogType.value === 'contatto') {
      await store.rimuoviContatto(id.value, removeDialogId.value)
      await store.fetchById(id.value)
      notificheStore.addToast('Contatto rimosso dall\'anagrafica', null, null, 'success')
    }
    else {
      await indirizziStore.remove(removeDialogId.value)
      indirizziList.value = indirizziList.value.filter(i => i.id !== removeDialogId.value)
      notificheStore.addToast('Indirizzo eliminato', null, null, 'success')
    }
    removeDialogOpen.value = false
  }
  catch (error: any) {
    notificheStore.addToast('Errore durante la rimozione', error?.data?.message || null, null, 'error')
  }
  finally {
    removeLoading.value = false
  }
}

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

function openContattoAdd() {
  contattoDialogData.value = null
  contattoDialogOpen.value = true
}

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
    const isEditing = contattoDialogData.value !== null

    if (isEditing && data.contattoId) {
      // Modifica ruolo/principale di un contatto già associato
      await store.aggiornaRuoloContatto(id.value, data.contattoId, {
        ruoloContattoId: data.ruoloContattoId!,
        principale: data.principale,
      })
      notificheStore.addToast('Contatto aggiornato con successo', null, null, 'success')
    }
    else if (data.isExisting && data.contattoId) {
      // Associa contatto esistente
      await store.associaContatto(id.value, {
        contattoId: data.contattoId,
        ruoloContattoId: data.ruoloContattoId,
        principale: data.principale,
      })
      notificheStore.addToast('Contatto associato con successo', null, null, 'success')
    }
    else {
      // Crea nuovo contatto e associa
      await store.associaContatto(id.value, {
        ruoloContattoId: data.ruoloContattoId,
        principale: data.principale,
        nuovoContatto: {
          nome: data.nome,
          cognome: data.cognome,
          email: data.email || undefined,
          cellulare: data.cellulare || undefined,
        },
      })
      notificheStore.addToast('Contatto aggiunto con successo', null, null, 'success')
    }
    await store.fetchById(id.value)
  }
  catch (error: any) {
    notificheStore.addToast('Errore salvataggio contatto', error?.data?.message || error?.message || null, null, 'error')
  }
}

onMounted(async () => {
  await Promise.all([
    store.fetchById(id.value),
    store.fetchLookups(),
    indirizziStore.fetchByAnagrafica(id.value).then(r => { indirizziList.value = r }),
  ])
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

        </VCard>

        <!-- Contatti Card -->
        <VCard class="mt-4">
          <VCardItem>
            <VCardTitle>Contatti</VCardTitle>
            <template #append>
              <VBtn size="small" variant="tonal" prepend-icon="tabler-plus" @click="openContattoAdd">
                Aggiungi
              </VBtn>
            </template>
          </VCardItem>
          <VCardText>
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
                  <template #append>
                    <IconBtn size="small" color="error" @click.stop="askRemoveContatto(contatto.contattoId, `${contatto.nome} ${contatto.cognome}`)">
                      <VIcon icon="tabler-trash" size="16" />
                    </IconBtn>
                  </template>
                </VListItem>
              </VList>
            </template>
            <div v-else class="text-disabled text-body-2">
              Nessun contatto associato
            </div>
          </VCardText>
        </VCard>

        <!-- Indirizzi Card -->
        <VCard class="mt-4">
          <VCardItem>
            <VCardTitle>Indirizzi</VCardTitle>
            <template #append>
              <VBtn size="small" variant="tonal" prepend-icon="tabler-plus" @click="openAddIndirizzo">
                Aggiungi
              </VBtn>
            </template>
          </VCardItem>
          <VCardText>
            <template v-if="indirizziList.length > 0">
              <VList density="compact" class="pa-0">
                <VListItem
                  v-for="ind in indirizziList"
                  :key="ind.id"
                  class="cursor-pointer px-0"
                  @click="openEditIndirizzo(ind)"
                >
                  <VListItemTitle class="text-body-1">
                    {{ ind.strada }} {{ ind.numero }}
                  </VListItemTitle>
                  <VListItemSubtitle>
                    {{ ind.frazione ? `${ind.frazione}, ` : '' }}{{ ind.cap }} {{ ind.citta }} ({{ ind.provincia }})
                  </VListItemSubtitle>
                  <VListItemSubtitle class="mt-1">
                    <VChip v-if="ind.isFatturazione" size="x-small" label color="warning">
                      Fatturazione
                    </VChip>
                    <VChip v-if="ind.isImpianto" size="x-small" label color="info" :class="ind.isFatturazione ? 'ms-1' : ''">
                      Impianto
                    </VChip>
                    <VChip v-if="ind.sottoTipo" size="x-small" label class="ms-1">
                      {{ ind.sottoTipo }}
                    </VChip>
                    <VChip v-if="ind.rete" size="x-small" label variant="outlined" class="ms-1">
                      {{ ind.rete }}
                    </VChip>
                  </VListItemSubtitle>
                  <template #append>
                    <IconBtn size="small" color="error" @click.stop="askRemoveIndirizzo(ind.id, `${ind.strada} ${ind.numero}, ${ind.citta}`)">
                      <VIcon icon="tabler-trash" size="16" />
                    </IconBtn>
                  </template>
                </VListItem>
              </VList>
            </template>
            <div v-else class="text-disabled text-body-2">
              Nessun indirizzo associato
            </div>
          </VCardText>
        </VCard>

      </VCol>

      <!-- RIGHT COLUMN - Timeline + Opportunità -->
      <VCol cols="12" md="8" lg="8">
        <!-- Warning indirizzi mancanti -->
        <VAlert
          v-if="!indirizziList.some(i => i.isImpianto)"
          type="warning"
          variant="tonal"
          density="compact"
          class="mb-4"
        >
          Nessun indirizzo di tipo <strong>Impianto</strong> associato
        </VAlert>
        <VAlert
          v-if="anagrafica.tipo === 1 && !indirizziList.some(i => i.isFatturazione)"
          type="warning"
          variant="tonal"
          density="compact"
          class="mb-4"
        >
          Nessun indirizzo di tipo <strong>Fatturazione</strong> associato (obbligatorio per clienti)
        </VAlert>

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

  <!-- Conferma Rimozione Dialog -->
  <VDialog v-model="removeDialogOpen" max-width="450" persistent>
    <VCard title="Conferma rimozione">
      <VCardText>
        <p class="text-body-1 mb-2">
          Stai per rimuovere <strong>{{ removeDialogName }}</strong>.
        </p>
        <p class="text-body-2 text-medium-emphasis">
          {{ removeDialogType === 'contatto'
            ? 'Il contatto verrà rimosso da questa anagrafica ma resterà disponibile nel sistema.'
            : 'L\'indirizzo verrà eliminato definitivamente.' }}
        </p>
      </VCardText>
      <VCardText class="d-flex justify-end gap-4">
        <VBtn variant="tonal" @click="removeDialogOpen = false">
          Annulla
        </VBtn>
        <VBtn
          color="error"
          prepend-icon="tabler-trash"
          :loading="removeLoading"
          @click="confirmRemoveAssociation"
        >
          {{ removeDialogType === 'contatto' ? 'Rimuovi' : 'Elimina' }}
        </VBtn>
      </VCardText>
    </VCard>
  </VDialog>

  <!-- Indirizzo Dialog -->
  <IndirizzoDialog
    v-model="indirizzoDialogOpen"
    :anagrafica-id="id"
    :indirizzo="editingIndirizzo"
    @saved="onIndirizzoSaved"
  />

  <!-- Contatto Edit Dialog -->
  <ContattoDialog
    v-model="contattoDialogOpen"
    :contatto="contattoDialogData"
    :ruoli-contatto="store.ruoliContatto"
    :anagrafica-id="id"
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
