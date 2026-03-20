<script setup lang="ts">
import { useIndirizziStore } from '@/stores/indirizzi'
import { useNotificheStore } from '@/stores/notifiche'
import type { IndirizzoApi, EgonComuneApi, EgonStradaApi, EgonCivicoApi } from '@/types/indirizzo'

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

// Field refs for auto-focus
const stradaFieldRef = ref<any>(null)
const civicoFieldRef = ref<any>(null)

const form = ref({
  isFatturazione: true,
  isImpianto: false,
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
  egonLocalita: '',
  latitudine: null as number | null,
  longitudine: null as number | null,
})

// Autocomplete state
const comuneSearch = ref('')
const stradaSearch = ref('')
const civicoSearch = ref('')

const comuniResults = ref<EgonComuneApi[]>([])
const stradeResults = ref<EgonStradaApi[]>([])
const civiciResults = ref<EgonCivicoApi[]>([])

const searchingComuni = ref(false)
const searchingStrade = ref(false)
const searchingCivici = ref(false)

const comuneSelected = ref(false)
const stradaSelected = ref(false)
const civicoSelected = ref(false)

// Keyboard navigation
const comuneHighlight = ref(-1)
const stradaHighlight = ref(-1)
const civicoHighlight = ref(-1)

function onComuneKeydown(e: KeyboardEvent) {
  if (!comuniResults.value.length) return
  if (e.key === 'ArrowDown') { e.preventDefault(); comuneHighlight.value = Math.min(comuneHighlight.value + 1, comuniResults.value.length - 1) }
  else if (e.key === 'ArrowUp') { e.preventDefault(); comuneHighlight.value = Math.max(comuneHighlight.value - 1, 0) }
  else if (e.key === 'Enter' && comuneHighlight.value >= 0) { e.preventDefault(); selectComune(comuniResults.value[comuneHighlight.value]) }
}

function onStradaKeydown(e: KeyboardEvent) {
  if (!stradeResults.value.length) return
  if (e.key === 'ArrowDown') { e.preventDefault(); stradaHighlight.value = Math.min(stradaHighlight.value + 1, stradeResults.value.length - 1) }
  else if (e.key === 'ArrowUp') { e.preventDefault(); stradaHighlight.value = Math.max(stradaHighlight.value - 1, 0) }
  else if (e.key === 'Enter' && stradaHighlight.value >= 0) { e.preventDefault(); selectStrada(stradeResults.value[stradaHighlight.value]) }
}

function onCivicoKeydown(e: KeyboardEvent) {
  if (!civiciResults.value.length) return
  if (e.key === 'ArrowDown') { e.preventDefault(); civicoHighlight.value = Math.min(civicoHighlight.value + 1, civiciResults.value.length - 1) }
  else if (e.key === 'ArrowUp') { e.preventDefault(); civicoHighlight.value = Math.max(civicoHighlight.value - 1, 0) }
  else if (e.key === 'Enter' && civicoHighlight.value >= 0) { e.preventDefault(); selectCivico(civiciResults.value[civicoHighlight.value]) }
}

// Watch dialog open
watch(() => props.modelValue, (val) => {
  if (val) {
    store.fetchLookups()
  }
  if (val && props.indirizzo) {
    form.value = {
      isFatturazione: props.indirizzo.isFatturazione,
      isImpianto: props.indirizzo.isImpianto,
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
      egonLocalita: props.indirizzo.egonLocalita ?? '',
      latitudine: props.indirizzo.latitudine ?? null,
      longitudine: props.indirizzo.longitudine ?? null,
    }
    comuneSearch.value = props.indirizzo.citta
    stradaSearch.value = props.indirizzo.strada
    civicoSearch.value = props.indirizzo.numero
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
    isFatturazione: true, isImpianto: false, sottoTipo: null, rete: null,
    citta: '', strada: '', numero: '', frazione: '',
    provincia: '', regione: '', cap: '',
    egonComune: '', egonStrada: '', egonCivico: '', egonLocalita: '',
    latitudine: null, longitudine: null,
  }
  comuneSearch.value = ''
  stradaSearch.value = ''
  civicoSearch.value = ''
  comuneSelected.value = false
  stradaSelected.value = false
  civicoSelected.value = false
  comuniResults.value = []
  stradeResults.value = []
  civiciResults.value = []
}

// I campi indirizzo sono readonly in modifica (già salvati con EGON)
const addressFieldsLocked = computed(() => isEdit.value)

// EGON: Comune search
let comuneTimeout: ReturnType<typeof setTimeout>
function onComuneInput() {
  const val = comuneSearch.value
  comuneSelected.value = false
  stradaSelected.value = false
  civicoSelected.value = false
  form.value.egonComune = ''
  form.value.citta = ''
  stradaSearch.value = ''
  civicoSearch.value = ''
  form.value.strada = ''
  form.value.numero = ''
  form.value.egonStrada = ''
  form.value.egonCivico = ''
  form.value.provincia = ''
  form.value.cap = ''
  form.value.frazione = ''
  clearTimeout(comuneTimeout)
  if (val.length < 3) { comuniResults.value = []; return }
  comuneTimeout = setTimeout(async () => {
    searchingComuni.value = true
    comuniResults.value = await store.searchComuni(val)
    comuneHighlight.value = -1
    searchingComuni.value = false
  }, 300)
}

function selectComune(item: EgonComuneApi) {
  comuneSearch.value = item.comune
  form.value.citta = item.comune
  form.value.egonComune = item.egonComune
  form.value.egonLocalita = item.egonComune
  comuneSelected.value = true
  comuniResults.value = []
  nextTick(() => {
    const el = stradaFieldRef.value?.$el?.querySelector('input') ?? stradaFieldRef.value?.$el
    el?.focus()
  })
}

// EGON: Strada search
let stradaTimeout: ReturnType<typeof setTimeout>
function onStradaInput() {
  const val = stradaSearch.value
  stradaSelected.value = false
  civicoSelected.value = false
  form.value.egonStrada = ''
  form.value.strada = ''
  civicoSearch.value = ''
  form.value.numero = ''
  form.value.egonCivico = ''
  clearTimeout(stradaTimeout)
  if (val.length < 3 || !form.value.egonComune) { stradeResults.value = []; return }
  stradaTimeout = setTimeout(async () => {
    searchingStrade.value = true
    stradeResults.value = await store.searchStrade(form.value.egonComune, val)
    stradaHighlight.value = -1
    searchingStrade.value = false
  }, 300)
}

function selectStrada(item: EgonStradaApi) {
  stradaSearch.value = item.strada
  form.value.strada = item.strada
  form.value.egonStrada = item.egonStrada
  form.value.provincia = item.provincia
  form.value.cap = item.cap ?? ''
  form.value.frazione = item.frazione ?? ''
  stradaSelected.value = true
  stradeResults.value = []
  nextTick(() => {
    const el = civicoFieldRef.value?.$el?.querySelector('input') ?? civicoFieldRef.value?.$el
    el?.focus()
  })
}

// EGON: Civico search
let civicoTimeout: ReturnType<typeof setTimeout>
function onCivicoInput() {
  const val = civicoSearch.value
  civicoSelected.value = false
  form.value.egonCivico = ''
  form.value.numero = ''
  clearTimeout(civicoTimeout)
  if (!val || !form.value.egonStrada) { civiciResults.value = []; return }
  civicoTimeout = setTimeout(async () => {
    searchingCivici.value = true
    civiciResults.value = await store.searchCivici(form.value.egonStrada, val)
    civicoHighlight.value = -1
    searchingCivici.value = false
  }, 300)
}

async function selectCivico(item: EgonCivicoApi) {
  civicoSearch.value = item.civico
  form.value.numero = item.civico
  form.value.egonCivico = item.egonCivico
  civicoSelected.value = true
  civiciResults.value = []

  // Normalizza per ottenere CAP, lat, lng, provincia
  try {
    const norm = await store.normalize(form.value.citta, `${form.value.strada}, ${item.civico}`, form.value.frazione || undefined)
    if (norm) {
      form.value.cap = norm.zip ?? form.value.cap
      form.value.provincia = norm.provinceShort ?? form.value.provincia
      form.value.latitudine = norm.latitude ?? null
      form.value.longitudine = norm.longitude ?? null
      if (norm.egonId) form.value.egonCivico = String(norm.egonId)
    }
  }
  catch {
    // keep values from street selection
  }
}

const canSave = computed(() =>
  (form.value.isFatturazione || form.value.isImpianto)
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
      isFatturazione: form.value.isFatturazione,
      isImpianto: form.value.isImpianto,
      sottoTipo: form.value.isImpianto ? form.value.sottoTipo : null,
      rete: form.value.isImpianto ? form.value.rete : null,
      strada: form.value.strada,
      numero: form.value.numero,
      frazione: form.value.frazione || null,
      citta: form.value.citta,
      provincia: form.value.provincia.toUpperCase(),
      regione: form.value.regione || null,
      cap: form.value.cap || null,
      latitudine: form.value.latitudine,
      longitudine: form.value.longitudine,
      egonCivico: form.value.egonCivico || null,
      egonStrada: form.value.egonStrada || null,
      egonLocalita: form.value.egonLocalita || null,
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
        <!-- Comune -->
        <VRow>
          <VCol cols="12" class="egon-field">
            <AppTextField
              v-model="comuneSearch"
              label="Comune"
              :loading="searchingComuni"
              :disabled="addressFieldsLocked"
              placeholder="Digita almeno 3 caratteri..."
              @input="onComuneInput"
              @keydown="onComuneKeydown"
            />
            <div v-if="comuniResults.length > 0" class="egon-results">
              <div
                v-for="(item, i) in comuniResults"
                :key="item.egonComune"
                class="egon-result-item"
                :class="{ 'egon-result-highlighted': i === comuneHighlight }"
                @mousedown.prevent.stop="selectComune(item)"
              >
                {{ item.comune }}
              </div>
            </div>
          </VCol>
        </VRow>

        <!-- Strada + Civico -->
        <VRow>
          <VCol cols="12" sm="8" class="egon-field">
            <AppTextField
              ref="stradaFieldRef"
              v-model="stradaSearch"
              label="Strada"
              :loading="searchingStrade"
              :disabled="!comuneSelected || addressFieldsLocked"
              placeholder="Digita almeno 3 caratteri..."
              @input="onStradaInput"
              @keydown="onStradaKeydown"
            />
            <div v-if="stradeResults.length > 0" class="egon-results">
              <div
                v-for="(item, i) in stradeResults"
                :key="item.egonStrada"
                class="egon-result-item"
                :class="{ 'egon-result-highlighted': i === stradaHighlight }"
                @mousedown.prevent.stop="selectStrada(item)"
              >
                {{ item.strada }}
                <span v-if="item.frazione" class="text-disabled"> — {{ item.frazione }}</span>
              </div>
            </div>
          </VCol>
          <VCol cols="12" sm="4" class="egon-field">
            <AppTextField
              ref="civicoFieldRef"
              v-model="civicoSearch"
              label="Civico"
              :loading="searchingCivici"
              :disabled="!stradaSelected || addressFieldsLocked"
              @input="onCivicoInput"
              @keydown="onCivicoKeydown"
            />
            <div v-if="civiciResults.length > 0" class="egon-results">
              <div
                v-for="(item, i) in civiciResults"
                :key="item.egonCivico"
                class="egon-result-item"
                :class="{ 'egon-result-highlighted': i === civicoHighlight }"
                @mousedown.prevent.stop="selectCivico(item)"
              >
                {{ item.civico }}
              </div>
            </div>
          </VCol>
        </VRow>

        <!-- Dettagli compilati automaticamente (readonly) -->
        <VRow>
          <VCol cols="12" sm="4">
            <AppTextField v-model="form.frazione" label="Frazione" disabled />
          </VCol>
          <VCol cols="12" sm="4">
            <AppTextField v-model="form.provincia" label="Provincia" disabled />
          </VCol>
          <VCol cols="12" sm="4">
            <AppTextField v-model="form.cap" label="CAP" disabled />
          </VCol>
        </VRow>

        <!-- EGON info -->
        <VRow v-if="form.egonCivico">
          <VCol cols="12">
            <VAlert type="success" variant="tonal" density="compact">
              EGON Civico: <strong>{{ form.egonCivico }}</strong>
              <template v-if="form.latitudine">
                · Lat: {{ form.latitudine?.toFixed(6) }}, Lng: {{ form.longitudine?.toFixed(6) }}
              </template>
            </VAlert>
          </VCol>
        </VRow>

        <VDivider class="my-4" />

        <!-- Uso indirizzo (checkbox) -->
        <VRow>
          <VCol cols="12" sm="6">
            <VCheckbox v-model="form.isFatturazione" label="Fatturazione" hide-details />
          </VCol>
          <VCol cols="12" sm="6">
            <VCheckbox v-model="form.isImpianto" label="Impianto" hide-details />
          </VCol>
        </VRow>

        <!-- Sotto tipo + Rete (solo se Impianto) -->
        <VRow v-if="form.isImpianto" class="mt-2">
          <VCol cols="12" sm="6">
            <AppSelect
              v-model="form.sottoTipo"
              :items="store.tipiTecnologia.map(t => ({ title: t.nome, value: t.nome }))"
              label="Tecnologia"
              clearable
            />
          </VCol>
          <VCol cols="12" sm="6">
            <AppSelect
              v-model="form.rete"
              :items="store.retiRiferimento.map(r => ({ title: r.nome, value: r.nome }))"
              label="Rete"
              clearable
            />
          </VCol>
        </VRow>

        <!-- Validazione: almeno un uso selezionato -->
        <VAlert
          v-if="!form.isFatturazione && !form.isImpianto"
          type="warning"
          variant="tonal"
          density="compact"
          class="mt-4"
        >
          Seleziona almeno un uso per l'indirizzo
        </VAlert>
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

<style>
.egon-field {
  position: relative;
}

.egon-results {
  position: absolute;
  z-index: 10;
  inset-inline: 12px;
  background: rgb(var(--v-theme-surface));
  border: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
  border-radius: 6px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.15);
  max-block-size: 200px;
  overflow-y: auto;
}

.egon-result-item {
  padding: 8px 16px;
  cursor: pointer;
  font-size: 14px;
}

.egon-result-item:hover,
.egon-result-highlighted {
  background: rgba(var(--v-theme-primary), 0.08);
}
</style>
