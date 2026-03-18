<script setup lang="ts">
import AppStepper from '@/@core/components/AppStepper.vue'
import { useAnagraficheStore } from '@/stores/anagrafiche'
import type { CreateAnagraficaRequest, TipoSoggetto, TipoAnagrafica, PeriodicitaPagamento } from '@/types/anagrafica'
import { PERIODICITA_LABELS } from '@/types/anagrafica'
import { useNotificheStore } from '@/stores/notifiche'
import { requiredValidator, partitaIvaValidator, codiceFiscaleValidator } from '@/@core/utils/validators'

const router = useRouter()
const store = useAnagraficheStore()

const saving = ref(false)
const currentStep = ref(0)
const notificheStore = useNotificheStore()

// Steps
const steps = [
  { title: 'Soggetto', subtitle: 'Tipo e denominazione', icon: 'tabler-user' },
  { title: 'Fisco', subtitle: 'Dati fiscali e pagamento', icon: 'tabler-receipt-tax' },
  { title: 'Contatto', subtitle: 'Referente principale', icon: 'tabler-users' },
]

// Form refs for each step
const stepFormRef1 = ref()
const stepFormRef2 = ref()
const isStepValid = ref(true)

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

// Primo contatto
const showPrimoContatto = ref(false)
const primoContatto = ref({
  nome: '',
  cognome: '',
  email: '',
  cellulare: '',
  telefono: '',
  ruoloId: null as number | null,
})

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
    await store.fetchLookups()
  }
  catch (e) {
    console.error('Errore caricamento lookups:', e)
  }
})

async function handleNext() {
  try {
    if (currentStep.value === 0) {
      if (stepFormRef1.value) {
        const { valid } = await stepFormRef1.value.validate()
        isStepValid.value = valid
        if (!valid) {
          notificheStore.addToast('Compila tutti i campi obbligatori prima di procedere', null, null, 'warning')
          return
        }
      }
    }
    else if (currentStep.value === 1) {
      if (stepFormRef2.value) {
        const { valid } = await stepFormRef2.value.validate()
        isStepValid.value = valid
        if (!valid) {
          notificheStore.addToast('Compila tutti i campi obbligatori prima di procedere', null, null, 'warning')
          return
        }
      }
      if (duplicateCheckResult.value.isDuplicate) {
        notificheStore.addToast('Risolvere il duplicato prima di procedere', null, null, 'warning')
        return
      }
    }
    isStepValid.value = true
    currentStep.value++
  }
  catch (e: any) {
    notificheStore.addToast('Errore durante la validazione', e?.message || null, null, 'error')
  }
}

function handleBack() {
  currentStep.value--
}

async function submit() {
  saving.value = true
  try {
    const request = { ...form.value } as CreateAnagraficaRequest

    if (showPrimoContatto.value && primoContatto.value.nome && primoContatto.value.cognome) {
      request.primoContatto = {
        nome: primoContatto.value.nome,
        cognome: primoContatto.value.cognome,
        email: primoContatto.value.email || undefined,
        cellulare: primoContatto.value.cellulare || undefined,
        telefono: primoContatto.value.telefono || undefined,
      }
      request.primoContattoRuoloId = primoContatto.value.ruoloId
    }

    const result = await store.create(request)
    router.push(`/anagrafiche/${result.id}`)
  }
  catch (error: any) {
    notificheStore.addToast('Errore durante la creazione', error?.data?.message || error?.message || null, null, 'error')
  }
  finally {
    saving.value = false
  }
}
</script>

<template>
  <div>
    <!-- Header -->
    <div class="d-flex align-center mb-6 flex-wrap gap-4">
      <VBtn
        variant="text"
        icon="tabler-arrow-left"
        :to="{ path: '/anagrafiche' }"
      />
      <h4 class="text-h4">Nuova Anagrafica</h4>
    </div>

    <!-- Vertical wizard layout -->
    <VCard>
      <VRow no-gutters>
        <!-- LEFT: Step navigation -->
        <VCol cols="12" md="3" class="border-e">
          <AppStepper
            :items="steps"
            :current-step="currentStep"
            :is-active-step-valid="isStepValid"
            direction="vertical"
            class="stepper-icon-step-bg pa-5"
          />
        </VCol>

        <!-- RIGHT: Step content -->
        <VCol cols="12" md="9">
          <VCardText>
            <VWindow :model-value="currentStep" class="disable-tab-transition">
              <!-- Step 1: Soggetto -->
              <VWindowItem :value="0">
                <div class="mb-6">
                  <h5 class="text-h5">Tipo Soggetto e Denominazione</h5>
                  <p class="text-body-2 text-disabled mb-0">Inserisci il tipo e i dati principali del soggetto</p>
                </div>
                <VForm ref="stepFormRef1">
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
                      />
                    </VCol>

                    <!-- Persona Giuridica -->
                    <VCol v-show="isGiuridica" cols="12">
                      <AppTextField
                        v-model="form.ragioneSociale"
                        label="Ragione Sociale"
                        :rules="isGiuridica ? [requiredValidator] : []"
                      />
                    </VCol>

                    <!-- Persona Fisica -->
                    <VCol v-show="!isGiuridica" cols="12" md="6">
                      <AppTextField
                        v-model="form.nome"
                        label="Nome"
                        :rules="!isGiuridica ? [requiredValidator] : []"
                      />
                    </VCol>
                    <VCol v-show="!isGiuridica" cols="12" md="6">
                      <AppTextField
                        v-model="form.cognome"
                        label="Cognome"
                        :rules="!isGiuridica ? [requiredValidator] : []"
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

              <!-- Step 2: Fisco -->
              <VWindowItem :value="1">
                <div class="mb-6">
                  <h5 class="text-h5">Dati Fiscali e Pagamento</h5>
                  <p class="text-body-2 text-disabled mb-0">Partita IVA, codice fiscale, indirizzo e condizioni di pagamento</p>
                </div>
                <VForm ref="stepFormRef2">
                  <VRow>
                    <!-- P.IVA -->
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
                        Esiste gia un'anagrafica con questa P.IVA: <strong>{{ duplicateCheckResult.denominazione }}</strong>.
                        <RouterLink :to="`/anagrafiche/${duplicateCheckResult.anagraficaId}`" class="text-warning font-weight-bold">
                          Clicca qui per vederla
                        </RouterLink>
                      </VAlert>
                    </VCol>

                    <!-- Codice Fiscale -->
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
                        Esiste gia un'anagrafica con questo Codice Fiscale: <strong>{{ duplicateCheckResult.denominazione }}</strong>.
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

                    <VCol cols="12">
                      <VDivider class="my-2" />
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

                    <VCol cols="12">
                      <VDivider class="my-2" />
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
                        label="Periodicita"
                        clearable
                      />
                    </VCol>
                  </VRow>
                </VForm>
              </VWindowItem>

              <!-- Step 3: Contatto -->
              <VWindowItem :value="2">
                <div class="mb-6">
                  <h5 class="text-h5">Referente Principale</h5>
                  <p class="text-body-2 text-disabled mb-0">Aggiungi il primo contatto associato all'anagrafica</p>
                </div>

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
                      />
                    </VCol>
                    <VCol cols="12" md="6">
                      <AppTextField
                        v-model="primoContatto.cognome"
                        label="Cognome"
                        :rules="[requiredValidator]"
                      />
                    </VCol>
                    <VCol cols="12" md="6">
                      <AppTextField v-model="primoContatto.email" label="Email" />
                    </VCol>
                    <VCol cols="12" md="6">
                      <AppTextField v-model="primoContatto.cellulare" label="Cellulare" />
                    </VCol>
                    <VCol cols="12" md="6">
                      <AppTextField v-model="primoContatto.telefono" label="Telefono" />
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
              </VWindowItem>
            </VWindow>
          </VCardText>

          <VDivider />

          <!-- Navigation buttons -->
          <VCardActions class="px-6 py-4">
            <VBtn
              v-if="currentStep > 0"
              variant="tonal"
              prepend-icon="tabler-arrow-left"
              @click="handleBack"
            >
              Indietro
            </VBtn>
            <VSpacer />
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
              color="success"
              prepend-icon="tabler-check"
              :loading="saving"
              @click="submit"
            >
              Crea Anagrafica
            </VBtn>
          </VCardActions>
        </VCol>
      </VRow>
    </VCard>

  </div>
</template>
