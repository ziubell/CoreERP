<script setup lang="ts">
import { useIndirizziStore } from '@/stores/indirizzi'
import { useNotificheStore } from '@/stores/notifiche'
import type { IndirizzoApi, EgonComuneApi, EgonStradaApi, EgonCivicoApi } from '@/types/indirizzo'
import { TIPI_INDIRIZZO, SOTTO_TIPI_IMPIANTO, RETI } from '@/types/indirizzo'
import { formatNome } from '@/utils/formatters'

const props = defineProps<{
  modelValue: boolean
  anagraficaId: number
  indirizzo?: IndirizzoApi | null
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  'saved': []
}>()

const store = useIndirizziStore()
const notifiche = useNotificheStore()
const saving = ref(false)

const isEdit = computed(() => !!props.indirizzo?.id)
const dialogTitle = computed(() => isEdit.value ? 'Modifica Indirizzo' : 'Nuovo Indirizzo')

const form = ref({
  tipo: 'Fatturazione',
  sottoTipo: null as string | null,
  rete: null as string | null,
  citta: '',
  strada: '',
  numero: '',
  frazione: '',
  provincia: '',
  regione: '',
  cap: '',
  egonComune: '',
  egonStrada: '',
  egonCivico: '',
  latitudine: null as number | null,
  longitudine: null as number | null,
  egonLocalita: '',
  principale: false,
})

// Autocomplete state
const comuniResults = ref<EgonComuneApi[]>([])
const stradeResults = ref<EgonStradaApi[]>([])
const civiciResults = ref<EgonCivicoApi[]>([])
const searchingComuni = ref(false)
const searchingStrade = ref(false)
const searchingCivici = ref(false)

const comuneSelected = ref(false)
const stradaSelected = ref(false)
const civicoSelected = ref(false)

const showSottoTipo = computed(() => form.value.tipo === 'Impianto')

// Watch for edit mode — populate form
watch(() => props.modelValue, (val) => {
  if (val && props.indirizzo) {
    form.value = {
      tipo: props.indirizzo.tipo,
      sottoTipo: props.indirizzo.sottoTipo ?? null,
      rete: props.indirizzo.rete ?? null,
      citta: props.indirizzo.citta,
      strada: props.indirizzo.strada,
      numero: props.indirizzo.numero,
      frazione: props.indirizzo.frazione ?? '',
      provincia: props.indirizzo.provincia,
      regione: props.indirizzo.regione ?? '',
      cap: props.indirizzo.cap ?? '',
      egonComune: props.indirizzo.egonLocalita ?? '',
      egonStrada: props.indirizzo.egonStrada ?? '',
      egonCivico: props.indirizzo.egonCivico ?? '',
      latitudine: props.indirizzo.latitudine ?? null,
      longitudine: props.indirizzo.longitudine ?? null,
      egonLocalita: props.indirizzo.egonLocalita ?? '',
      principale: props.indirizzo.principale,
    }
    comuneSelected.value = true
    stradaSelected.value = true
    civicoSelected.value = !!props.indirizzo.egonCivico
  }
  else if (val) {
    resetForm()
  }
})

function resetForm() {
  form.value = {
    tipo: 'Fatturazione', sottoTipo: null, rete: null,
    citta: '', strada: '', numero: '', frazione: '',
    provincia: '', regione: '', cap: '',
    egonComune: '', egonStrada: '', egonCivico: '',
    latitudine: null, longitudine: null, egonLocalita: '',
    principale: false,
  }
  comuneSelected.value = false
  stradaSelected.value = false
  civicoSelected.value = false
  comuniResults.value = []
  stradeResults.value = []
  civiciResults.value = []
}

// EGON: Comune search
let comuneTimeout: ReturnType<typeof setTimeout>
function onComuneInput(val: string) {
  comuneSelected.value = false
  stradaSelected.value = false
  civicoSelected.value = false
  form.value.egonComune = ''
  form.value.strada = ''
  form.value.numero = ''
  form.value.egonStrada = ''
  form.value.egonCivico = ''
  clearTimeout(comuneTimeout)
  if (val.length < 3) { comuniResults.value = []; return }
  comuneTimeout = setTimeout(async () => {
    searchingComuni.value = true
    comuniResults.value = await store.searchComuni(val)
    searchingComuni.value = false
  }, 300)
}

function selectComune(item: EgonComuneApi) {
  form.value.citta = item.comune
  form.value.egonComune = item.egonComune
  form.value.egonLocalita = item.egonComune
  comuneSelected.value = true
  comuniResults.value = []
}

// EGON: Strada search
let stradaTimeout: ReturnType<typeof setTimeout>
function onStradaInput(val: string) {
  stradaSelected.value = false
  civicoSelected.value = false
  form.value.egonStrada = ''
  form.value.numero = ''
  form.value.egonCivico = ''
  clearTimeout(stradaTimeout)
  if (val.length < 3 || !form.value.egonComune) { stradeResults.value = []; return }
  stradaTimeout = setTimeout(async () => {
    searchingStrade.value = true
    stradeResults.value = await store.searchStrade(form.value.egonComune, val)
    searchingStrade.value = false
  }, 300)
}

function selectStrada(item: EgonStradaApi) {
  form.value.strada = item.strada
  form.value.egonStrada = item.egonStrada
  form.value.provincia = item.provincia
  form.value.cap = item.cap ?? ''
  form.value.frazione = item.frazione ?? ''
  stradaSelected.value = true
  stradeResults.value = []
}

// EGON: Civico search
let civicoTimeout: ReturnType<typeof setTimeout>
function onCivicoInput(val: string) {
  civicoSelected.value = false
  form.value.egonCivico = ''
  clearTimeout(civicoTimeout)
  if (!val || !form.value.egonStrada) { civiciResults.value = []; return }
  civicoTimeout = setTimeout(async () => {
    searchingCivici.value = true
    civiciResults.value = await store.searchCivici(form.value.egonStrada, val)
    searchingCivici.value = false
  }, 300)
}

function selectCivico(item: EgonCivicoApi) {
  form.value.numero = item.civico
  form.value.egonCivico = item.egonCivico
  civicoSelected.value = true
  civiciResults.value = []
}

const canSave = computed(() =>
  form.value.tipo
  && form.value.citta
  && form.value.strada
  && form.value.numero
  && form.value.egonCivico
  && form.value.provincia,
)

async function save() {
  if (!canSave.value) return
  saving.value = true
  try {
    const payload = {
      tipo: form.value.tipo,
      sottoTipo: showSottoTipo.value ? form.value.sottoTipo : null,
      rete: showSottoTipo.value ? form.value.rete : null,
      strada: form.value.strada,
      numero: form.value.numero,
      frazione: form.value.frazione || null,
      citta: formatNome(form.value.citta),
      provincia: form.value.provincia.toUpperCase(),
      regione: form.value.regione || null,
      cap: form.value.cap || null,
      latitudine: form.value.latitudine,
      longitudine: form.value.longitudine,
      egonCivico: form.value.egonCivico || null,
      egonStrada: form.value.egonStrada || null,
      egonLocalita: form.value.egonLocalita || null,
      principale: form.value.principale,
    }

    if (isEdit.value && props.indirizzo) {
      await store.update(props.indirizzo.id, payload)
      notifiche.addToast('Indirizzo aggiornato.', null, null, 'success')
    }
    else {
      await store.create({ ...payload, anagraficaId: props.anagraficaId })
      notifiche.addToast('Indirizzo aggiunto.', null, null, 'success')
    }

    emit('saved')
    emit('update:modelValue', false)
  }
  catch {
    notifiche.addToast('Errore nel salvataggio.', null, null, 'error')
  }
  finally {
    saving.value = false
  }
}
</script>

<template>
  <VDialog
    :model-value="modelValue"
    max-width="700"
    @update:model-value="$emit('update:modelValue', $event)"
  >
    <VCard>
      <VCardTitle>{{ dialogTitle }}</VCardTitle>

      <VCardText>
        <!-- Tipo -->
        <VRow>
          <VCol cols="12" sm="4">
            <AppSelect
              v-model="form.tipo"
              :items="[...TIPI_INDIRIZZO]"
              label="Tipo"
              :rules="[v => !!v || 'Obbligatorio']"
            />
          </VCol>
          <VCol v-if="showSottoTipo" cols="12" sm="4">
            <AppSelect
              v-model="form.sottoTipo"
              :items="[...SOTTO_TIPI_IMPIANTO]"
              label="Sotto tipo"
              clearable
            />
          </VCol>
          <VCol v-if="showSottoTipo" cols="12" sm="4">
            <AppSelect
              v-model="form.rete"
              :items="[...RETI]"
              label="Rete"
              clearable
            />
          </VCol>
        </VRow>

        <!-- Comune (autocomplete EGON) -->
        <VRow>
          <VCol cols="12">
            <AppAutocomplete
              v-model="form.citta"
              :items="comuniResults"
              item-title="comune"
              item-value="comune"
              label="Comune"
              :loading="searchingComuni"
              :rules="[v => !!v || 'Obbligatorio']"
              no-filter
              @update:search="onComuneInput"
              @update:model-value="val => {
                const item = comuniResults.find(c => c.comune === val)
                if (item) selectComune(item)
              }"
            >
              <template #no-data>
                <VListItem v-if="form.citta.length >= 3">
                  <VListItemTitle>Nessun risultato</VListItemTitle>
                </VListItem>
                <VListItem v-else>
                  <VListItemTitle>Digita almeno 3 caratteri</VListItemTitle>
                </VListItem>
              </template>
            </AppAutocomplete>
          </VCol>
        </VRow>

        <!-- Strada + Civico (autocomplete EGON) -->
        <VRow>
          <VCol cols="12" sm="8">
            <AppAutocomplete
              v-model="form.strada"
              :items="stradeResults.map(s => ({ ...s, label: s.frazione ? `${s.strada} - ${s.frazione}` : s.strada }))"
              item-title="label"
              item-value="strada"
              label="Strada"
              :loading="searchingStrade"
              :readonly="!comuneSelected"
              :rules="[v => !!v || 'Obbligatorio']"
              no-filter
              @update:search="onStradaInput"
              @update:model-value="val => {
                const item = stradeResults.find(s => s.strada === val)
                if (item) selectStrada(item)
              }"
            >
              <template #no-data>
                <VListItem>
                  <VListItemTitle>{{ !comuneSelected ? 'Seleziona prima un comune' : form.strada.length < 3 ? 'Digita almeno 3 caratteri' : 'Nessun risultato' }}</VListItemTitle>
                </VListItem>
              </template>
            </AppAutocomplete>
          </VCol>
          <VCol cols="12" sm="4">
            <AppAutocomplete
              v-model="form.numero"
              :items="civiciResults"
              item-title="civico"
              item-value="civico"
              label="Civico"
              :loading="searchingCivici"
              :readonly="!stradaSelected"
              :rules="[v => !!v || 'Obbligatorio', () => !!form.egonCivico || 'Seleziona un civico EGON']"
              no-filter
              @update:search="onCivicoInput"
              @update:model-value="val => {
                const item = civiciResults.find(c => c.civico === val)
                if (item) selectCivico(item)
              }"
            >
              <template #no-data>
                <VListItem>
                  <VListItemTitle>{{ !stradaSelected ? 'Seleziona prima una strada' : 'Nessun risultato' }}</VListItemTitle>
                </VListItem>
              </template>
            </AppAutocomplete>
          </VCol>
        </VRow>

        <!-- Dettagli compilati automaticamente -->
        <VRow>
          <VCol cols="12" sm="4">
            <AppTextField v-model="form.frazione" label="Frazione" readonly />
          </VCol>
          <VCol cols="12" sm="4">
            <AppTextField v-model="form.provincia" label="Provincia" readonly />
          </VCol>
          <VCol cols="12" sm="4">
            <AppTextField v-model="form.cap" label="CAP" readonly />
          </VCol>
        </VRow>

        <!-- EGON info (readonly, informativo) -->
        <VRow v-if="form.egonCivico">
          <VCol cols="12">
            <VAlert type="success" variant="tonal" density="compact">
              Codice EGON Civico: <strong>{{ form.egonCivico }}</strong>
            </VAlert>
          </VCol>
        </VRow>

        <!-- Principale -->
        <VRow>
          <VCol cols="12">
            <VSwitch v-model="form.principale" label="Indirizzo principale" />
          </VCol>
        </VRow>
      </VCardText>

      <VCardText class="d-flex justify-end gap-4">
        <VBtn variant="tonal" @click="$emit('update:modelValue', false)">
          Annulla
        </VBtn>
        <VBtn
          color="primary"
          prepend-icon="tabler-device-floppy"
          :loading="saving"
          :disabled="!canSave"
          @click="save"
        >
          Salva
        </VBtn>
      </VCardText>
    </VCard>
  </VDialog>
</template>
