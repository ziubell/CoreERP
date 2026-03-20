<script setup lang="ts">
import AppStepper from '@/@core/components/AppStepper.vue'
import { useAnagraficheStore } from '@/stores/anagrafiche'
import { useNotificheStore } from '@/stores/notifiche'
import type { CreateAnagraficaRequest, TipoSoggetto, TipoAnagrafica, PeriodicitaPagamento } from '@/types/anagrafica'
import { PERIODICITA_LABELS } from '@/types/anagrafica'
import { requiredValidator, partitaIvaValidator, codiceFiscaleValidator } from '@/@core/utils/validators'
import { formatNome, formatCognome, formatRagioneSociale } from '@/utils/formatters'
import ContattoDialog from '@/components/anagrafica/ContattoDialog.vue'
import type { ContattoDialogData } from '@/components/anagrafica/ContattoDialog.vue'
import IndirizzoDialog from '@/components/anagrafica/IndirizzoDialog.vue'
import { useIndirizziStore } from '@/stores/indirizzi'
import type { IndirizzoApi } from '@/types/indirizzo'

const props = defineProps<{
  id?: number
}>()

const router = useRouter()
const store = useAnagraficheStore()
const notificheStore = useNotificheStore()

const indirizziStore = useIndirizziStore()
const isEditMode = computed(() => props.id !== undefined)
const loading = ref(false)
const saving = ref(false)
const currentStep = ref(0)
const isStepValid = ref(true)

// Steps
const steps = [
  { title: 'Soggetto', subtitle: 'Tipo e denominazione' },
  { title: 'Dati Fiscali', subtitle: 'P.IVA, CF e pagamento' },
  { title: 'Indirizzi', subtitle: 'Sedi e impianti' },
  { title: 'Contatti', subtitle: 'Referenti associati' },
]

// Form refs for each step
const stepFormRefs = ref<any[]>([])
function setStepFormRef(index: number) {
  return (el: any) => { stepFormRefs.value[index] = el }
}

// Form data
const form = ref<CreateAnagraficaRequest>({
  tipoSoggetto: 1 as TipoSoggetto,
  tipo: 0 as TipoAnagrafica,
  ragioneSociale: null,
  nome: null,
  cognome: null,
  nazione: 'Italia',
})

const periodicitaItems = Object.entries(PERIODICITA_LABELS).map(([value, title]) => ({
  title,
  value: Number(value) as PeriodicitaPagamento,
}))

// Primo contatto (solo in creazione)
const showPrimoContatto = ref(false)
const primoContatto = ref({
  nome: '',
  cognome: '',
  email: '',
  cellulare: '',
  ruoloId: null as number | null,
})

// Contatti (modifica)
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

const contatti = ref<LocalContatto[]>([])
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

// Indirizzi
const indirizzoDialogOpen = ref(false)
const editingIndirizzo = ref<IndirizzoApi | null>(null)
const indirizziList = ref<IndirizzoApi[]>([])

function openAddIndirizzo() {
  editingIndirizzo.value = null
  indirizzoDialogOpen.value = true
}

function openEditIndirizzo(indirizzo: IndirizzoApi) {
  editingIndirizzo.value = indirizzo
  indirizzoDialogOpen.value = true
}

async function onIndirizzoSaved() {
  if (isEditMode.value && props.id) {
    indirizziList.value = await indirizziStore.fetchByAnagrafica(props.id)
  }
}

async function removeIndirizzo(id: number) {
  await indirizziStore.remove(id)
  indirizziList.value = indirizziList.value.filter(i => i.id !== id)
  notificheStore.addToast('Indirizzo rimosso', null, null, 'success')
}

// Duplicate check
const duplicateCheckResult = ref<{
  isDuplicate: boolean
  anagraficaId: number | null
  denominazione: string | null
  field: 'partitaIva' | 'codiceFiscale' | null
}>({ isDuplicate: false, anagraficaId: null, denominazione: null, field: null })

let duplicateTimeout: ReturnType<typeof setTimeout> | null = null

function checkDuplicate(field: 'partitaIva' | 'codiceFiscale') {
  if (duplicateTimeout) clearTimeout(duplicateTimeout)
  duplicateCheckResult.value = { isDuplicate: false, anagraficaId: null, denominazione: null, field: null }

  const value = field === 'partitaIva' ? form.value.partitaIva : form.value.codiceFiscale
  if (!value || value.trim().length < 5) return

  duplicateTimeout = setTimeout(async () => {
    try {
      const result = await store.verificaDuplicato(
        field === 'partitaIva' ? value.trim() : undefined,
        field === 'codiceFiscale' ? value.trim() : undefined,
        isEditMode.value ? props.id : undefined,
      )
      if (result.isDuplicate) {
        duplicateCheckResult.value = { ...result, field }
      }
    }
    catch {
      // ignore
    }
  }, 500)
}

// Dynamic required based on tipo soggetto
const isGiuridica = computed(() => form.value.tipoSoggetto === 1)
const pIvaRules = computed(() => {
  const rules: any[] = [partitaIvaValidator]
  if (isGiuridica.value) rules.unshift(requiredValidator)
  return rules
})
const cfRules = computed(() => {
  const rules: any[] = [codiceFiscaleValidator]
  if (!isGiuridica.value) rules.unshift(requiredValidator)
  return rules
})

onMounted(async () => {
  try {
    if (isEditMode.value) {
      loading.value = true
      await Promise.all([
        store.fetchById(props.id!),
        store.fetchLookups(),
        indirizziStore.fetchByAnagrafica(props.id!).then(r => { indirizziList.value = r }),
      ])
      if (store.current) {
        const a = store.current
        form.value = {
          tipoSoggetto: a.tipoSoggetto,
          tipo: a.tipo as TipoAnagrafica,
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
      loading.value = false
    }
    else {
      await store.fetchLookups()
    }
  }
  catch (e) {
    console.error('Errore caricamento:', e)
    loading.value = false
  }
})

async function validateCurrentStep(): Promise<boolean> {
  const formRef = stepFormRefs.value[currentStep.value]
  if (!formRef) return true

  const { valid } = await formRef.validate()
  if (!valid) {
    notificheStore.addToast('Compila tutti i campi obbligatori prima di procedere', null, null, 'warning')
  }
  return valid
}

async function handleNext() {
  const valid = await validateCurrentStep()
  isStepValid.value = valid
  if (!valid) return

  if (currentStep.value === 1 && duplicateCheckResult.value.isDuplicate) {
    notificheStore.addToast('Risolvere il duplicato prima di procedere', null, null, 'warning')
    return
  }

  isStepValid.value = true
  currentStep.value++
}

function handleBack() {
  currentStep.value--
}

async function submit() {
  saving.value = true
  try {
    if (form.value.ragioneSociale) form.value.ragioneSociale = formatRagioneSociale(form.value.ragioneSociale)
    if (form.value.nome) form.value.nome = formatNome(form.value.nome)
    if (form.value.cognome) form.value.cognome = formatCognome(form.value.cognome)

    if (isEditMode.value) {
      await store.update(props.id!, form.value)

      // Process contatti changes
      for (const c of contatti.value) {
        if (c.status === 'removed' && c.contattoId) {
          await store.rimuoviContatto(props.id!, c.contattoId)
        }
        else if (c.status === 'new') {
          if (c.isExisting && c.contattoId) {
            await store.associaContatto(props.id!, {
              contattoId: c.contattoId,
              ruoloContattoId: c.ruoloContattoId,
              principale: c.principale,
            })
          }
          else {
            await store.associaContatto(props.id!, {
              ruoloContattoId: c.ruoloContattoId,
              principale: c.principale,
              nuovoContatto: {
                nome: c.nome,
                cognome: c.cognome,
                email: c.email || undefined,
                cellulare: c.cellulare || undefined,
              },
            })
          }
        }
        else if (c.status === 'modified' && c.contattoId) {
          await store.aggiornaRuoloContatto(props.id!, c.contattoId, {
            ruoloContattoId: c.ruoloContattoId,
            principale: c.principale,
          })
        }
      }

      notificheStore.addToast('Anagrafica aggiornata con successo', null, null, 'success')
      router.push(`/anagrafiche/${props.id}`)
    }
    else {
      const request = { ...form.value } as CreateAnagraficaRequest

      if (showPrimoContatto.value && primoContatto.value.nome && primoContatto.value.cognome) {
        primoContatto.value.nome = formatNome(primoContatto.value.nome)
        primoContatto.value.cognome = formatCognome(primoContatto.value.cognome)

        request.primoContatto = {
          nome: primoContatto.value.nome,
          cognome: primoContatto.value.cognome,
          email: primoContatto.value.email || undefined,
          cellulare: primoContatto.value.cellulare || undefined,
        }
        request.primoContattoRuoloId = primoContatto.value.ruoloId
      }

      const result = await store.create(request)
      router.push(`/anagrafiche/${result.id}`)
    }
  }
  catch (error: any) {
    notificheStore.addToast('Errore durante il salvataggio', error?.data?.message || error?.message || null, null, 'error')
  }
  finally {
    saving.value = false
  }
}

const backRoute = computed(() => isEditMode.value ? `/anagrafiche/${props.id}` : '/anagrafiche')
</script>

<template>
  <div v-if="loading" class="d-flex justify-center py-12">
    <VProgressCircular indeterminate />
  </div>

  <div v-else>
    <!-- Header -->
    <div class="d-flex align-center mb-6 flex-wrap gap-4">
      <VBtn variant="text" icon="tabler-arrow-left" :to="backRoute" />
      <h4 class="text-h4">
        {{ isEditMode ? 'Modifica Anagrafica' : 'Nuova Anagrafica' }}
        <span v-if="isEditMode && store.current" class="text-disabled">
          — {{ store.current.denominazione }}
        </span>
      </h4>
    </div>

    <!-- Wizard -->
    <VCard>
      <VCardText>
        <!-- Stepper orizzontale numerato -->
        <AppStepper
          :items="steps"
          :current-step="currentStep"
          :is-active-step-valid="isStepValid"
          direction="horizontal"
          class="mb-6"
        />
      </VCardText>

      <VDivider />

      <VCardText>
        <VWindow :model-value="currentStep" class="disable-tab-transition">
          <!-- Step 1: Soggetto -->
          <VWindowItem :value="0">
            <VForm :ref="setStepFormRef(0)">
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
                  <AppSelect
                    v-model="form.tipo"
                    :items="[
                      { title: 'Potenziale', value: 0 },
                      { title: 'Cliente', value: 1 },
                    ]"
                    label="Tipo"
                    :rules="[requiredValidator]"
                    :disabled="isEditMode"
                  />
                </VCol>

                <!-- Persona Giuridica -->
                <VCol v-show="isGiuridica" cols="12">
                  <AppTextField
                    v-model="form.ragioneSociale"
                    label="Ragione Sociale"
                    :rules="isGiuridica ? [requiredValidator] : []"
                    @blur="form.ragioneSociale && (form.ragioneSociale = formatRagioneSociale(form.ragioneSociale))"
                  />
                </VCol>

                <!-- Persona Fisica -->
                <VCol v-show="!isGiuridica" cols="12" md="6">
                  <AppTextField
                    v-model="form.nome"
                    label="Nome"
                    :rules="!isGiuridica ? [requiredValidator] : []"
                    @blur="form.nome && (form.nome = formatNome(form.nome))"
                  />
                </VCol>
                <VCol v-show="!isGiuridica" cols="12" md="6">
                  <AppTextField
                    v-model="form.cognome"
                    label="Cognome"
                    :rules="!isGiuridica ? [requiredValidator] : []"
                    @blur="form.cognome && (form.cognome = formatCognome(form.cognome))"
                  />
                </VCol>

                <VCol cols="12" md="6">
                  <AppTextField v-model="form.telefono" label="Telefono" />
                </VCol>
                <VCol cols="12" md="6">
                  <AppTextField v-model="form.sitoWeb" label="Sito Web" />
                </VCol>
                <VCol cols="12">
                  <AppTextarea v-model="form.note" label="Note" rows="3" />
                </VCol>
              </VRow>
            </VForm>
          </VWindowItem>

          <!-- Step 2: Dati Fiscali -->
          <VWindowItem :value="1">
            <VForm :ref="setStepFormRef(1)">
              <VRow>
                <VCol cols="12" md="6">
                  <AppTextField
                    v-model="form.partitaIva"
                    label="Partita IVA"
                    :rules="pIvaRules"
                    @blur="checkDuplicate('partitaIva')"
                  />
                  <VAlert
                    v-if="duplicateCheckResult.isDuplicate && duplicateCheckResult.field === 'partitaIva'"
                    type="warning"
                    density="compact"
                    class="mt-2"
                  >
                    Esiste già un'anagrafica con questa P.IVA: <strong>{{ duplicateCheckResult.denominazione }}</strong>.
                    <RouterLink :to="`/anagrafiche/${duplicateCheckResult.anagraficaId}`" class="text-warning font-weight-bold">
                      Clicca qui per vederla
                    </RouterLink>
                  </VAlert>
                </VCol>

                <VCol cols="12" md="6">
                  <AppTextField
                    v-model="form.codiceFiscale"
                    label="Codice Fiscale"
                    :rules="cfRules"
                    @blur="checkDuplicate('codiceFiscale')"
                  />
                  <VAlert
                    v-if="duplicateCheckResult.isDuplicate && duplicateCheckResult.field === 'codiceFiscale'"
                    type="warning"
                    density="compact"
                    class="mt-2"
                  >
                    Esiste già un'anagrafica con questo Codice Fiscale: <strong>{{ duplicateCheckResult.denominazione }}</strong>.
                    <RouterLink :to="`/anagrafiche/${duplicateCheckResult.anagraficaId}`" class="text-warning font-weight-bold">
                      Clicca qui per vederla
                    </RouterLink>
                  </VAlert>
                </VCol>

                <VCol cols="12" md="4">
                  <AppTextField v-model="form.codiceSDI" label="Codice SDI" maxlength="7" />
                </VCol>
                <VCol cols="12" md="8">
                  <AppTextField v-model="form.pec" label="PEC" />
                </VCol>

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
                    label="Periodicità"
                    clearable
                  />
                </VCol>
              </VRow>
            </VForm>
          </VWindowItem>

          <!-- Step 3: Indirizzi -->
          <VWindowItem :value="2">
            <template v-if="isEditMode">
              <VTable v-if="indirizziList.length > 0" class="text-no-wrap mb-4 border rounded">
                <thead>
                  <tr>
                    <th>Tipo</th>
                    <th>Indirizzo</th>
                    <th>Città</th>
                    <th>Prov.</th>
                    <th>Rete</th>
                    <th class="text-center" style="width: 100px;">Principale</th>
                    <th class="text-center" style="width: 80px;">Azioni</th>
                  </tr>
                </thead>
                <tbody>
                  <tr
                    v-for="ind in indirizziList"
                    :key="ind.id"
                    class="cursor-pointer"
                    @click="openEditIndirizzo(ind)"
                  >
                    <td>
                      <VChip size="small" :color="ind.tipo === 'Impianto' ? 'info' : 'warning'" variant="tonal">
                        {{ ind.tipo }}
                      </VChip>
                      <VChip v-if="ind.sottoTipo" size="small" class="ms-1" variant="tonal">
                        {{ ind.sottoTipo }}
                      </VChip>
                    </td>
                    <td>{{ ind.strada }} {{ ind.numero }}</td>
                    <td>{{ ind.citta }}</td>
                    <td>{{ ind.provincia }}</td>
                    <td>{{ ind.rete ?? '—' }}</td>
                    <td class="text-center">
                      <VIcon v-if="ind.principale" icon="tabler-check" color="primary" size="20" />
                    </td>
                    <td class="text-center">
                      <IconBtn size="small" color="error" @click.stop="removeIndirizzo(ind.id)">
                        <VIcon icon="tabler-trash" size="18" />
                      </IconBtn>
                    </td>
                  </tr>
                </tbody>
              </VTable>

              <div v-else class="text-disabled text-body-2 py-4 text-center">
                Nessun indirizzo associato
              </div>

              <VBtn
                color="primary"
                variant="tonal"
                prepend-icon="tabler-plus"
                @click="openAddIndirizzo"
              >
                Aggiungi Indirizzo
              </VBtn>
            </template>

            <div v-else class="text-disabled text-body-2 py-4 text-center">
              Potrai aggiungere indirizzi dopo la creazione dell'anagrafica
            </div>
          </VWindowItem>

          <!-- Step 4: Contatti -->
          <VWindowItem :value="3">
            <!-- In creazione: primo contatto opzionale -->
            <template v-if="!isEditMode">
              <div class="d-flex align-center gap-4 mb-6">
                <VSwitch
                  v-model="showPrimoContatto"
                  label="Aggiungi primo contatto"
                  hide-details
                />
              </div>

              <template v-if="showPrimoContatto">
                <VRow>
                  <VCol cols="12" md="6">
                    <AppTextField
                      v-model="primoContatto.nome"
                      label="Nome"
                      :rules="[requiredValidator]"
                      @blur="primoContatto.nome && (primoContatto.nome = formatNome(primoContatto.nome))"
                    />
                  </VCol>
                  <VCol cols="12" md="6">
                    <AppTextField
                      v-model="primoContatto.cognome"
                      label="Cognome"
                      :rules="[requiredValidator]"
                      @blur="primoContatto.cognome && (primoContatto.cognome = formatCognome(primoContatto.cognome))"
                    />
                  </VCol>
                  <VCol cols="12" md="6">
                    <AppTextField v-model="primoContatto.email" label="Email" />
                  </VCol>
                  <VCol cols="12" md="6">
                    <AppTextField v-model="primoContatto.cellulare" label="Cellulare" />
                  </VCol>
                  <VCol cols="12" md="6">
                    <AppSelect
                      v-model="primoContatto.ruoloId"
                      :items="store.ruoliContatto.map(r => ({ title: r.nome, value: r.id }))"
                      label="Ruolo"
                    />
                  </VCol>
                </VRow>
              </template>

              <div v-else class="text-disabled text-body-2 py-4 text-center">
                Puoi aggiungere contatti anche dopo la creazione dell'anagrafica
              </div>
            </template>

            <!-- In modifica: tabella contatti -->
            <template v-else>
              <VTable v-if="visibleContatti.length > 0" class="text-no-wrap mb-4 border rounded">
                <thead>
                  <tr>
                    <th>Nome</th>
                    <th>Ruolo</th>
                    <th>Email</th>
                    <th>Cellulare</th>
                    <th class="text-center" style="width: 100px;">Principale</th>
                    <th class="text-center" style="width: 80px;">Azioni</th>
                  </tr>
                </thead>
                <tbody>
                  <tr
                    v-for="(contatto, index) in visibleContatti"
                    :key="index"
                    class="cursor-pointer"
                    @click="openEditContatto(index)"
                  >
                    <td class="font-weight-medium">{{ contatto.nome }} {{ contatto.cognome }}</td>
                    <td>
                      <VChip size="small" label>{{ contatto.ruoloContattoNome || getRuoloNome(contatto.ruoloContattoId) }}</VChip>
                    </td>
                    <td>{{ contatto.email || '—' }}</td>
                    <td>{{ contatto.cellulare || '—' }}</td>
                    <td class="text-center">
                      <VIcon v-if="contatto.principale" icon="tabler-check" color="primary" size="20" />
                    </td>
                    <td class="text-center">
                      <IconBtn size="small" color="error" @click.stop="removeContatto(index)">
                        <VIcon icon="tabler-trash" size="18" />
                      </IconBtn>
                    </td>
                  </tr>
                </tbody>
              </VTable>

              <div v-else class="text-disabled text-body-2 py-4 text-center">
                Nessun contatto associato
              </div>

              <VBtn
                color="primary"
                variant="tonal"
                prepend-icon="tabler-plus"
                @click="openAddContatto"
              >
                Aggiungi Contatto
              </VBtn>
            </template>
          </VWindowItem>
        </VWindow>
      </VCardText>

      <VDivider />

      <!-- Navigation buttons -->
      <VCardText class="d-flex justify-space-between">
        <VBtn
          v-if="currentStep > 0"
          variant="tonal"
          color="secondary"
          prepend-icon="tabler-arrow-left"
          @click="handleBack"
        >
          Indietro
        </VBtn>
        <div v-else />

        <VBtn
          v-if="currentStep < steps.length - 1"
          color="primary"
          append-icon="tabler-arrow-right"
          :disabled="currentStep === 1 && duplicateCheckResult.isDuplicate"
          @click="handleNext"
        >
          Avanti
        </VBtn>
        <VBtn
          v-else
          color="primary"
          prepend-icon="tabler-device-floppy"
          :loading="saving"
          @click="submit"
        >
          Salva
        </VBtn>
      </VCardText>
    </VCard>

    <!-- Indirizzo Dialog -->
    <IndirizzoDialog
      v-if="isEditMode"
      v-model="indirizzoDialogOpen"
      :anagrafica-id="props.id!"
      :indirizzo="editingIndirizzo"
      @saved="onIndirizzoSaved"
    />

    <!-- Contatto Dialog -->
    <ContattoDialog
      v-if="isEditMode"
      v-model="contattoDialogOpen"
      :contatto="editingContatto"
      :ruoli-contatto="store.ruoliContatto"
      @save="handleContattoSave"
    />
  </div>
</template>
