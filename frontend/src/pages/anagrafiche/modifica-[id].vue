<script setup lang="ts">
import { useAnagraficheStore } from '@/stores/anagrafiche'
import type { UpdateAnagraficaRequest, TipoSoggetto, PeriodicitaPagamento, AnagraficaContattoApi } from '@/types/anagrafica'
import { PERIODICITA_LABELS } from '@/types/anagrafica'
import { useNotificheStore } from '@/stores/notifiche'
import { requiredValidator, partitaIvaValidator, codiceFiscaleValidator } from '@/@core/utils/validators'
import ContattoDialog from '@/components/anagrafica/ContattoDialog.vue'
import type { ContattoDialogData } from '@/components/anagrafica/ContattoDialog.vue'

const route = useRoute()
const router = useRouter()
const store = useAnagraficheStore()

const id = computed(() => Number(route.params.id))
const loading = ref(false)
const saving = ref(false)
const formRef = ref()

const notificheStore = useNotificheStore()

// Contatto dialog
const contattoDialogOpen = ref(false)
const editingContattoIndex = ref<number | null>(null)

interface LocalContatto {
  contattoId: number | null
  nome: string
  cognome: string
  email: string
  cellulare: string
  telefono: string
  note: string
  ruoloContattoId: number | null
  ruoloContattoNome: string
  principale: boolean
  isExisting: boolean
  status: 'existing' | 'new' | 'modified' | 'removed'
}

const form = ref<UpdateAnagraficaRequest>({
  tipoSoggetto: 1 as TipoSoggetto,
  ragioneSociale: null,
  nome: null,
  cognome: null,
})

const contatti = ref<LocalContatto[]>([])

const periodicitaItems = Object.entries(PERIODICITA_LABELS).map(([value, title]) => ({
  title,
  value: Number(value) as PeriodicitaPagamento,
}))

onMounted(async () => {
  loading.value = true
  try {
    await Promise.all([store.fetchById(id.value), store.fetchLookups()])
    if (store.current) {
      const a = store.current
      form.value = {
        tipoSoggetto: a.tipoSoggetto,
        ragioneSociale: a.ragioneSociale,
        nome: a.nome,
        cognome: a.cognome,
        partitaIva: a.partitaIva,
        codiceFiscale: a.codiceFiscale,
        codiceSDI: a.codiceSDI,
        pec: a.pec,
        indirizzoFatturazione: a.indirizzoFatturazione,
        cap: a.cap,
        citta: a.citta,
        provincia: a.provincia,
        nazione: a.nazione,
        telefono: a.telefono,
        sitoWeb: a.sitoWeb,
        note: a.note,
        metodoPagamentoId: a.metodoPagamentoId,
        iban: a.iban,
        periodicitaPagamento: a.periodicitaPagamento,
      }
      contatti.value = (a.contatti ?? []).map(c => ({
        contattoId: c.contattoId,
        nome: c.nome,
        cognome: c.cognome,
        email: c.email ?? '',
        cellulare: c.cellulare ?? '',
        telefono: c.telefono ?? '',
        note: '',
        ruoloContattoId: c.ruoloContattoId,
        ruoloContattoNome: c.ruoloContattoNome,
        principale: c.principale,
        isExisting: true,
        status: 'existing',
      }))
    }
  }
  finally {
    loading.value = false
  }
})

const visibleContatti = computed(() => contatti.value.filter(c => c.status !== 'removed'))

function getRuoloNome(ruoloId: number | null): string {
  if (!ruoloId) return ''
  return store.ruoliContatto.find(r => r.id === ruoloId)?.nome ?? ''
}

function openAddContatto() {
  editingContattoIndex.value = null
  contattoDialogOpen.value = true
}

function openEditContatto(index: number) {
  const c = visibleContatti.value[index]
  editingContattoIndex.value = contatti.value.indexOf(c)
  contattoDialogOpen.value = true
}

const editingContatto = computed<ContattoDialogData | null>(() => {
  if (editingContattoIndex.value === null) return null
  const c = contatti.value[editingContattoIndex.value]
  return {
    contattoId: c.contattoId,
    nome: c.nome,
    cognome: c.cognome,
    email: c.email,
    cellulare: c.cellulare,
    telefono: c.telefono,
    note: c.note,
    ruoloContattoId: c.ruoloContattoId,
    principale: c.principale,
    isExisting: c.isExisting,
  }
})

function handleContattoSave(data: ContattoDialogData) {
  const ruoloNome = getRuoloNome(data.ruoloContattoId)

  if (editingContattoIndex.value !== null) {
    const existing = contatti.value[editingContattoIndex.value]
    existing.ruoloContattoId = data.ruoloContattoId
    existing.ruoloContattoNome = ruoloNome
    existing.principale = data.principale
    if (!existing.isExisting) {
      existing.nome = data.nome
      existing.cognome = data.cognome
      existing.email = data.email
      existing.cellulare = data.cellulare
      existing.telefono = data.telefono
      existing.note = data.note
    }
    if (existing.status === 'existing') existing.status = 'modified'
  }
  else {
    contatti.value.push({
      contattoId: data.contattoId ?? null,
      nome: data.nome,
      cognome: data.cognome,
      email: data.email,
      cellulare: data.cellulare,
      telefono: data.telefono,
      note: data.note,
      ruoloContattoId: data.ruoloContattoId,
      ruoloContattoNome: ruoloNome,
      principale: data.principale,
      isExisting: data.isExisting,
      status: 'new',
    })
  }
}

function removeContatto(index: number) {
  const c = visibleContatti.value[index]
  const realIndex = contatti.value.indexOf(c)
  if (c.status === 'new') {
    contatti.value.splice(realIndex, 1)
  }
  else {
    contatti.value[realIndex].status = 'removed'
  }
}

function moveContatto(index: number, direction: -1 | 1) {
  const visible = visibleContatti.value
  const targetIndex = index + direction
  if (targetIndex < 0 || targetIndex >= visible.length) return

  const realFrom = contatti.value.indexOf(visible[index])
  const realTo = contatti.value.indexOf(visible[targetIndex])

  const temp = contatti.value[realFrom]
  contatti.value[realFrom] = contatti.value[realTo]
  contatti.value[realTo] = temp
}

async function submit() {
  const { valid } = await formRef.value?.validate()
  if (!valid) return

  saving.value = true
  try {
    // 1. Update anagrafica data
    await store.update(id.value, form.value)

    // 2. Process contatti changes
    for (const c of contatti.value) {
      if (c.status === 'removed' && c.contattoId) {
        await store.rimuoviContatto(id.value, c.contattoId)
      }
      else if (c.status === 'new') {
        if (c.isExisting && c.contattoId) {
          await store.associaContatto(id.value, {
            contattoId: c.contattoId,
            ruoloContattoId: c.ruoloContattoId,
            principale: c.principale,
          })
        }
        else {
          await store.associaContatto(id.value, {
            ruoloContattoId: c.ruoloContattoId,
            principale: c.principale,
            nuovoContatto: {
              nome: c.nome,
              cognome: c.cognome,
              email: c.email || undefined,
              cellulare: c.cellulare || undefined,
              telefono: c.telefono || undefined,
              note: c.note || undefined,
            },
          })
        }
      }
      else if (c.status === 'modified' && c.contattoId) {
        await store.aggiornaRuoloContatto(id.value, c.contattoId, {
          ruoloContattoId: c.ruoloContattoId,
          principale: c.principale,
        })
      }
    }

    router.push(`/anagrafiche/${id.value}`)
  }
  catch (error: any) {
    notificheStore.addToast('Errore durante il salvataggio', error?.data?.message || error?.message || null, null, 'error')
  }
  finally {
    saving.value = false
  }
}
</script>

<template>
  <div v-if="loading" class="d-flex justify-center py-12">
    <VProgressCircular indeterminate />
  </div>

  <div v-else>
    <VForm ref="formRef" @submit.prevent="submit">
      <!-- Header with top submit button -->
      <div class="d-flex align-center mb-6 flex-wrap gap-4">
        <VBtn
          variant="text"
          icon="tabler-arrow-left"
          @click="router.push(`/anagrafiche/${id}`)"
        />
        <h4 class="text-h4">
          Modifica Anagrafica
          <span v-if="store.current" class="text-disabled">
            — {{ store.current.denominazione }}
          </span>
        </h4>
        <VSpacer />
        <VBtn type="submit" color="primary" :loading="saving" prepend-icon="tabler-device-floppy">
          Salva
        </VBtn>
      </div>

      <!-- Dati Principali -->
      <VCard title="Dati Principali" class="mb-6">
        <VCardText>
          <VRow>
            <VCol cols="12" md="6">
              <AppSelect
                v-model="form.tipoSoggetto"
                :items="[
                  { title: 'Persona Giuridica', value: 1 },
                  { title: 'Persona Fisica', value: 0 },
                ]"
                label="Tipo Soggetto"
                :rules="[requiredValidator]"
              />
            </VCol>
            <VCol cols="12" md="6">
              <!-- Tipo is read-only on edit (use converti/disattiva actions) -->
              <AppSelect
                :model-value="store.current?.tipo"
                :items="[
                  { title: 'Potenziale', value: 0 },
                  { title: 'Cliente', value: 1 },
                ]"
                label="Tipo"
                disabled
              />
            </VCol>

            <VCol v-if="form.tipoSoggetto === 1" cols="12">
              <AppTextField
                v-model="form.ragioneSociale"
                label="Ragione Sociale"
                :rules="[requiredValidator]"
              />
            </VCol>

            <template v-if="form.tipoSoggetto === 0">
              <VCol cols="12" md="6">
                <AppTextField
                  v-model="form.nome"
                  label="Nome"
                  :rules="[requiredValidator]"
                />
              </VCol>
              <VCol cols="12" md="6">
                <AppTextField
                  v-model="form.cognome"
                  label="Cognome"
                  :rules="[requiredValidator]"
                />
              </VCol>
            </template>

            <VCol cols="12" md="6">
              <AppTextField v-model="form.telefono" label="Telefono" />
            </VCol>
            <VCol cols="12" md="6">
              <AppTextField v-model="form.sitoWeb" label="Sito Web" />
            </VCol>
          </VRow>
        </VCardText>
      </VCard>

      <!-- Dati Fiscali -->
      <VCard title="Dati Fiscali" class="mb-6">
        <VCardText>
          <VRow>
            <VCol cols="12" md="6">
              <AppTextField
                v-model="form.partitaIva"
                label="Partita IVA"
                :rules="[partitaIvaValidator]"
              />
            </VCol>
            <VCol cols="12" md="6">
              <AppTextField
                v-model="form.codiceFiscale"
                label="Codice Fiscale"
                :rules="[codiceFiscaleValidator]"
              />
            </VCol>
            <VCol cols="12" md="4">
              <AppTextField v-model="form.codiceSDI" label="Codice SDI" maxlength="7" />
            </VCol>
            <VCol cols="12" md="8">
              <AppTextField v-model="form.pec" label="PEC" />
            </VCol>
            <VCol cols="12">
              <AppTextField v-model="form.indirizzoFatturazione" label="Indirizzo Fatturazione" />
            </VCol>
            <VCol cols="12" md="3">
              <AppTextField v-model="form.cap" label="CAP" maxlength="5" />
            </VCol>
            <VCol cols="12" md="4">
              <AppTextField v-model="form.citta" label="Citta" />
            </VCol>
            <VCol cols="12" md="2">
              <AppTextField v-model="form.provincia" label="Prov." maxlength="2" />
            </VCol>
            <VCol cols="12" md="3">
              <AppTextField v-model="form.nazione" label="Nazione" />
            </VCol>
          </VRow>
        </VCardText>
      </VCard>

      <!-- Pagamento -->
      <VCard title="Condizioni di Pagamento" class="mb-6">
        <VCardText>
          <VRow>
            <VCol cols="12" md="4">
              <AppSelect
                v-model="form.metodoPagamentoId"
                :items="store.metodiPagamento.map(m => ({ title: m.nome, value: m.id }))"
                label="Metodo Pagamento"
                clearable
              />
            </VCol>
            <VCol cols="12" md="4">
              <AppTextField
                v-model="form.iban"
                label="IBAN"
                :rules="store.metodiPagamento.find(m => m.id === form.metodoPagamentoId)?.richiedeIBAN ? [requiredValidator] : []"
              />
            </VCol>
            <VCol cols="12" md="4">
              <AppSelect
                v-model="form.periodicitaPagamento"
                :items="periodicitaItems"
                label="Periodicita"
                clearable
              />
            </VCol>
          </VRow>
        </VCardText>
      </VCard>

      <!-- Note -->
      <VCard title="Note" class="mb-6">
        <VCardText>
          <AppTextarea v-model="form.note" label="Note" rows="3" />
        </VCardText>
      </VCard>

      <!-- Contatti Collegati -->
      <VCard title="Contatti Collegati" class="mb-6">
        <VCardText>
          <VTable v-if="visibleContatti.length > 0" density="comfortable">
            <thead>
              <tr>
                <th>Nome</th>
                <th>Ruolo</th>
                <th>Email</th>
                <th>Cellulare</th>
                <th>Principale</th>
                <th class="text-center" style="width: 140px;">Azioni</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(c, index) in visibleContatti" :key="index">
                <td class="font-weight-medium">{{ c.nome }} {{ c.cognome }}</td>
                <td>
                  <VChip size="small" label>{{ c.ruoloContattoNome || getRuoloNome(c.ruoloContattoId) }}</VChip>
                </td>
                <td>{{ c.email || '—' }}</td>
                <td>{{ c.cellulare || '—' }}</td>
                <td>
                  <VIcon v-if="c.principale" icon="tabler-check" color="primary" size="20" />
                </td>
                <td class="text-center">
                  <IconBtn size="small" @click="moveContatto(index, -1)" :disabled="index === 0">
                    <VIcon icon="tabler-arrow-up" size="18" />
                  </IconBtn>
                  <IconBtn size="small" @click="moveContatto(index, 1)" :disabled="index === visibleContatti.length - 1">
                    <VIcon icon="tabler-arrow-down" size="18" />
                  </IconBtn>
                  <IconBtn size="small" @click="openEditContatto(index)">
                    <VIcon icon="tabler-edit" size="18" />
                  </IconBtn>
                  <IconBtn size="small" color="error" @click="removeContatto(index)">
                    <VIcon icon="tabler-trash" size="18" />
                  </IconBtn>
                </td>
              </tr>
            </tbody>
          </VTable>

          <div v-else class="text-disabled text-body-2 py-4 text-center">
            Nessun contatto collegato
          </div>

          <VBtn
            class="mt-4"
            color="primary"
            variant="tonal"
            prepend-icon="tabler-plus"
            @click="openAddContatto"
          >
            Aggiungi Contatto
          </VBtn>
        </VCardText>
      </VCard>

      <!-- Bottom submit button -->
      <div class="d-flex justify-end">
        <VBtn
          variant="text"
          class="me-4"
          @click="router.push(`/anagrafiche/${id}`)"
        >
          Annulla
        </VBtn>
        <VBtn type="submit" color="primary" :loading="saving" prepend-icon="tabler-device-floppy">
          Salva
        </VBtn>
      </div>
    </VForm>

    <!-- Contatto Dialog -->
    <ContattoDialog
      v-model="contattoDialogOpen"
      :contatto="editingContatto"
      :ruoli-contatto="store.ruoliContatto"
      @save="handleContattoSave"
    />

  </div>
</template>
