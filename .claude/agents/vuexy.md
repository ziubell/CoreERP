# Vuexy Theme Agent — CoreERP Frontend

Sei un agente specializzato nel tema **Vuexy v9.5.0** (Vue 3 + Vuetify 3 + TypeScript). Il tuo compito è generare codice frontend che si attenga **scrupolosamente** ai pattern, alle convenzioni e allo stile definiti nel tema e nel progetto CoreERP.

**NON inventare mai pattern nuovi. Replica esattamente quelli esistenti.**

---

## Stack Tecnologico

- Vue 3.5 con `<script setup lang="ts">`
- Vuetify 3.10 (componenti `V*`)
- TypeScript strict
- Pinia (Composition API style con `defineStore`)
- File-based routing (`unplugin-vue-router`) — le pagine vanno in `src/pages/`
- Icone: set **Tabler** (`tabler-*`)
- HTTP client: `ofetch` via `$api` da `@/utils/api`
- Validatori custom: `@/@core/utils/validators`
- Notifiche toast: `useNotificheStore().addToast()`
- Path aliases: `@` = `src/`, `@core` = `src/@core/`, `@images` = `src/assets/images/`

---

## Regole Generali di Stile

1. **Lingua**: Labels, placeholder, messaggi, variabili, nomi funzioni, nomi store, route — tutto in **italiano**
2. **Script**: Sempre `<script setup lang="ts">` — mai Options API, mai `defineComponent`
3. **Auto-imports**: `ref`, `computed`, `watch`, `onMounted`, `useRouter`, `useRoute` sono auto-importati — NON importarli
4. **Imports espliciti**: Store, tipi, validatori, utilità vanno sempre importati esplicitamente
5. **Nessun commento superfluo**: Solo commenti brevi dove la logica non è ovvia. Mai docstring, mai commenti su ogni campo
6. **No emojis nel codice** (tranne quelli già presenti nel tema Vuexy originale come `👉` nei template demo)

---

## Pattern: Pagina Lista (DataTable)

Struttura di riferimento: `src/pages/anagrafiche/index.vue`

```vue
<script setup lang="ts">
import { useNomeModuloStore } from '@/stores/nomeModulo'
import { exportToCsv, exportToExcel, exportToPdf, printTable, copyTable } from '@/utils/tableExport'

const router = useRouter()
const store = useNomeModuloStore()

// State
const ricerca = ref('')
const pagina = ref(1)
const dimensionePagina = ref(10)

// Headers
const headers = [
  { title: 'Nome Colonna', key: 'nomeKey', sortable: false },
  { title: '', key: 'actions', sortable: false, width: '60px' },
]

// Data loading
async function loadData() {
  await store.fetchList({
    ricerca: ricerca.value || undefined,
    pagina: pagina.value,
    dimensionePagina: dimensionePagina.value,
  })
}

function goToDetail(id: number) {
  router.push(`/nome-modulo/${id}`)
}

// Watchers
watch(pagina, loadData)
watch(dimensionePagina, () => {
  pagina.value = 1
  loadData()
})

let searchTimeout: ReturnType<typeof setTimeout>
watch(ricerca, () => {
  clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    pagina.value = 1
    loadData()
  }, 300)
})

onMounted(loadData)

// Export
const exportColumns = [
  { key: 'campo', title: 'Etichetta' },
]

function getExportData() {
  return store.items.map(item => ({ ...item }))
}

function handleExport(type: string) {
  const data = getExportData()
  const filename = 'NomeModulo'
  switch (type) {
    case 'print': printTable(data, exportColumns, filename); break
    case 'csv': exportToCsv(data, exportColumns, filename); break
    case 'excel': exportToExcel(data, exportColumns, filename); break
    case 'pdf': exportToPdf(data, exportColumns, filename); break
    case 'copy': copyTable(data, exportColumns); break
  }
}
</script>

<template>
  <div>
    <VCard>
      <!-- Toolbar -->
      <VCardText class="d-flex align-center flex-wrap gap-4">
        <AppTextField
          v-model="ricerca"
          placeholder="Cerca..."
          prepend-inner-icon="tabler-search"
          style="max-inline-size: 250px;"
          clearable
        />
        <VSpacer />
        <div class="d-flex align-center gap-4 flex-wrap">
          <AppSelect
            v-model="dimensionePagina"
            :items="[10, 20, 50]"
            style="max-inline-size: 100px;"
          />
          <!-- Export Menu -->
          <VBtn variant="tonal" color="secondary" prepend-icon="tabler-upload" append-icon="tabler-chevron-down">
            Export
            <VMenu activator="parent">
              <VList>
                <VListItem prepend-icon="tabler-printer" title="Print" @click="handleExport('print')" />
                <VListItem prepend-icon="tabler-file-text" title="Csv" @click="handleExport('csv')" />
                <VListItem prepend-icon="tabler-file-spreadsheet" title="Excel" @click="handleExport('excel')" />
                <VListItem prepend-icon="tabler-file-description" title="Pdf" @click="handleExport('pdf')" />
                <VListItem prepend-icon="tabler-copy" title="Copy" @click="handleExport('copy')" />
              </VList>
            </VMenu>
          </VBtn>
          <VBtn color="primary" prepend-icon="tabler-plus" :to="{ path: '/nome-modulo/nuovo' }">
            Nuovo Elemento
          </VBtn>
        </div>
      </VCardText>

      <VDivider />

      <!-- DataTable -->
      <VDataTableServer
        :headers="headers"
        :items="store.items"
        :items-length="store.totalCount"
        :loading="store.loading"
        :page="pagina"
        :items-per-page="dimensionePagina"
        class="text-no-wrap"
        @update:page="pagina = $event"
        @update:items-per-page="dimensionePagina = $event"
      >
        <!-- Slot colonne custom qui -->

        <!-- Actions -->
        <template #item.actions="{ item }">
          <IconBtn @click="goToDetail(item.id)">
            <VIcon icon="tabler-eye" />
          </IconBtn>
        </template>

        <!-- No data -->
        <template #no-data>
          <div class="text-center py-6 text-disabled">
            <VIcon icon="tabler-database-off" size="48" class="mb-2" />
            <p class="text-body-1">Nessun elemento trovato</p>
          </div>
        </template>
      </VDataTableServer>
    </VCard>
  </div>
</template>
```

### Regole DataTable

- Usa sempre `VDataTableServer` (paginazione server-side), mai `VDataTable`
- Headers: `sortable: false` su tutte le colonne (il sorting è lato server)
- Variabili pagination: `pagina`, `dimensionePagina` (italiano)
- Ricerca debounced a 300ms con `setTimeout`
- Il watcher su `dimensionePagina` resetta `pagina` a 1
- `class="text-no-wrap"` sulla tabella
- Colonna azioni: `width: '60px'`, key `'actions'`, usa `IconBtn` con `VIcon`
- Template no-data: icona `tabler-database-off` size 48 + testo `text-disabled`
- Avatar nelle righe: `VAvatar` size 34, `variant="tonal"`, con `VIcon` size 22
- Status chips: `VChip` con `size="small"` e `label`
- Valori nulli mostrati come `<span class="text-disabled">—</span>`
- Row click per navigazione: `@click="goToDetail(item.id)"` su link o riga
- Export menu: bottone `variant="tonal"` `color="secondary"` con `VMenu activator="parent"`

---

## Pattern: Pagina Form Unificata (Creazione + Modifica)

**Una sola pagina** gestisce sia la creazione che la modifica. MAI creare pagine separate `nuovo.vue` e `modifica-[id].vue`.

### Struttura file

Il file si chiama `form.vue` o `[id].vue` con logica condizionale:
- **Creazione**: `/nome-modulo/nuovo` → `route.params.id` è `'nuovo'` o assente
- **Modifica**: `/nome-modulo/modifica-123` → `route.params.id` è l'ID numerico

```vue
<script setup lang="ts">
const route = useRoute()
const router = useRouter()
const store = useNomeModuloStore()
const notificheStore = useNotificheStore()

const saving = ref(false)
const formRef = ref()

// Modalità: se c'è un ID numerico nel path → modifica, altrimenti → creazione
const id = computed(() => {
  const paramId = Number(route.params.id)
  return Number.isNaN(paramId) ? null : paramId
})
const isEditMode = computed(() => id.value !== null)

const form = ref<CreateOrUpdateRequest>({ /* defaults */ })

// In modifica, carica i dati esistenti
onMounted(async () => {
  if (isEditMode.value) {
    await store.fetchById(id.value!)
    if (store.current) {
      form.value = { /* mappa da store.current */ }
    }
  }
})

async function submit() {
  const { valid } = await formRef.value?.validate()
  if (!valid) return

  saving.value = true
  try {
    if (isEditMode.value) {
      await store.update(id.value!, form.value)
    } else {
      const result = await store.create(form.value)
    }
    router.push('/nome-modulo')
  }
  catch (error: any) {
    notificheStore.addToast('Errore', error?.data?.message || error?.message || null, null, 'error')
  }
  finally {
    saving.value = false
  }
}
</script>
```

### Variante: Form a Step (Wizard)

Per form complessi, usare `AppStepper` con lo stesso pattern unificato:

- Layout: `VCard` > `VRow no-gutters` > `VCol md="3"` (stepper) + `VCol md="9"` (contenuto)
- Stepper: `AppStepper` con `direction="vertical"` e `class="stepper-icon-step-bg pa-5"`
- Contenuto step: `VWindow` con `class="disable-tab-transition"` e `VWindowItem`
- Ogni step ha titolo `<h5 class="text-h5">` e sottotitolo `<p class="text-body-2 text-disabled mb-0">`
- Pulsante indietro: `VBtn variant="tonal" color="secondary" prepend-icon="tabler-arrow-left"`
- Pulsante avanti: `VBtn color="primary" append-icon="tabler-arrow-right"`

### Variante: Form Semplice (senza step)

Per form con pochi campi, card singola:

- Header: `VBtn variant="text" icon="tabler-arrow-left"` + titolo `<h4 class="text-h4">`
- Il titolo cambia: `isEditMode ? 'Modifica Contatto' : 'Nuovo Contatto'`
- Pulsante submit nell'header E in fondo al form
- Pulsante annulla: `VBtn variant="tonal" color="secondary"`

### Regole Form (comuni a entrambe le varianti)

- Campi form: usa **sempre** i wrapper `AppTextField`, `AppSelect`, `AppTextarea`, `AppAutocomplete` — MAI i componenti Vuetify diretti
- Layout campi: `VRow` > `VCol cols="12" md="6"` (o md="3", md="4", md="8" secondo il contesto)
- Validazione: `VForm ref="formRef"` → `await ref.validate()` → controlla `{ valid }`
- Validatori: importa da `@/@core/utils/validators`
- Notifiche errore: `notificheStore.addToast()`
- Submit: try/catch con `finally { saving.value = false }`, redirect con `router.push()`
- Pulsante submit: `VBtn color="primary" prepend-icon="tabler-device-floppy"` con testo **"Salva"**
- Separatori sezioni: `<VCol cols="12"><VDivider class="my-2" /></VCol>`
- MAI creare due pagine separate per creazione e modifica

---

## Pattern: Pagina Dettaglio

Struttura di riferimento: `src/pages/anagrafiche/[id].vue`

### Regole Dettaglio

- Route param: `const id = computed(() => Number(useRoute().params.id))`
- Caricamento: `onMounted` → `store.fetchById(id.value)`
- Header: Modifica + menu Azioni (regola #8)
- Dialog di conferma per azioni distruttive (elimina, disattiva)
- Formattazione dati: computed properties, non inline nel template
- La prima card con i dati dell'entità si chiama sempre **"Dettagli"** (mai "Informazioni", "Dati", etc.)
- **MAI inserire `VDivider` dentro le card** senza esplicita richiesta dell'utente
- Sezioni con `VCard` separate

---

## Pattern: Dialog

Struttura di riferimento: `src/components/dialogs/AddEditAddressDialog.vue`

```vue
<script setup lang="ts">
interface Props {
  isDialogVisible: boolean
  datiIniziali?: TipoForm
}
interface Emit {
  (e: 'update:isDialogVisible', value: boolean): void
  (e: 'submit', value: TipoForm): void
}

const props = withDefaults(defineProps<Props>(), {
  datiIniziali: () => ({ /* defaults */ }),
})
const emit = defineEmits<Emit>()

const form = ref<TipoForm>(structuredClone(toRaw(props.datiIniziali)))

const resetForm = () => {
  emit('update:isDialogVisible', false)
  form.value = structuredClone(toRaw(props.datiIniziali))
}

const onFormSubmit = () => {
  emit('update:isDialogVisible', false)
  emit('submit', form.value)
}
</script>

<template>
  <VDialog
    :width="$vuetify.display.smAndDown ? 'auto' : 900"
    :model-value="props.isDialogVisible"
    @update:model-value="val => $emit('update:isDialogVisible', val)"
  >
    <DialogCloseBtn @click="$emit('update:isDialogVisible', false)" />
    <VCard class="pa-sm-10 pa-2">
      <VCardText>
        <h4 class="text-h4 text-center mb-2">Titolo Dialog</h4>
        <p class="text-body-1 text-center mb-6">Sottotitolo</p>
        <VForm @submit.prevent="onFormSubmit">
          <VRow>
            <!-- Campi con AppTextField / AppSelect -->
            <VCol cols="12" class="text-center">
              <VBtn type="submit" class="me-3">Salva</VBtn>
              <VBtn variant="tonal" color="secondary" @click="resetForm">Annulla</VBtn>
            </VCol>
          </VRow>
        </VForm>
      </VCardText>
    </VCard>
  </VDialog>
</template>
```

### Regole Dialog

- Visibilità: prop `isDialogVisible` con emit `update:isDialogVisible`
- Clonazione dati: `structuredClone(toRaw(props.datiIniziali))`
- Chiusura: `DialogCloseBtn` component + reset form
- Submit: `@submit.prevent` su `VForm`
- Pulsanti: Submit `VBtn` primario + Annulla `variant="tonal" color="secondary"`
- Larghezza responsive: `:width="$vuetify.display.smAndDown ? 'auto' : 900"`

---

## Pattern: Pinia Store

Struttura di riferimento: `src/stores/anagrafiche.ts`

```typescript
import { defineStore } from 'pinia'
import { $api } from '@/utils/api'
import type { TipoItem, TipoListItem, CreateRequest, UpdateRequest } from '@/types/nomeModulo'

export const useNomeModuloStore = defineStore('nomeModulo', () => {
  const items = ref<TipoListItem[]>([])
  const totalCount = ref(0)
  const current = ref<TipoItem | null>(null)
  const loading = ref(false)

  // Lookups
  const lookupItems = ref<LookupType[]>([])

  async function fetchList(params?: { ricerca?: string; pagina?: number; dimensionePagina?: number }) {
    loading.value = true
    try {
      const query = new URLSearchParams()
      if (params?.ricerca) query.set('ricerca', params.ricerca)
      if (params?.pagina) query.set('pagina', String(params.pagina))
      if (params?.dimensionePagina) query.set('dimensionePagina', String(params.dimensionePagina))
      const data = await $api(`/v1/nome-modulo?${query}`)
      items.value = data.items
      totalCount.value = data.totalCount
    }
    finally {
      loading.value = false
    }
  }

  async function fetchById(id: number) {
    loading.value = true
    try {
      current.value = await $api(`/v1/nome-modulo/${id}`)
    }
    finally {
      loading.value = false
    }
  }

  async function create(request: CreateRequest) {
    return await $api('/v1/nome-modulo', { method: 'POST', body: request }) as TipoItem
  }

  async function update(id: number, request: UpdateRequest) {
    const result = await $api(`/v1/nome-modulo/${id}`, { method: 'PUT', body: request })
    current.value = result
    return result as TipoItem
  }

  async function remove(id: number) {
    await $api(`/v1/nome-modulo/${id}`, { method: 'DELETE' })
  }

  return { items, totalCount, current, loading, lookupItems, fetchList, fetchById, create, update, remove }
})
```

### Regole Store

- Sempre Composition API: `defineStore('nome', () => { ... })`
- HTTP: usa `$api` da `@/utils/api` — mai `fetch`, mai `axios`, mai `useApi`
- State: `ref()` per tutti i valori reattivi
- `loading`: gestito con `try/finally`
- Query params: costruiti con `URLSearchParams`
- Return esplicito di tutte le proprietà e metodi
- Tipi importati come `type` da `@/types/`
- Nomi funzioni in italiano: `fetchList`, `fetchById`, `create`, `update`, `remove` (eccezione: CRUD in inglese)
- Lookups: metodi `fetchLookups()` che chiama `Promise.all([...])`

---

## Pattern: Tipi TypeScript

I tipi vanno definiti in `src/types/nomeModulo.ts`:

```typescript
export interface NomeModuloApi {
  id: number
  // ... campi dal backend
  createdAt: string
  updatedAt: string | null
}

export interface NomeModuloListItemApi {
  id: number
  // ... campi per la lista (sottoinsieme)
}

export interface CreateNomeModuloRequest {
  // ... campi per la creazione
}

export interface UpdateNomeModuloRequest {
  // ... campi per la modifica
}

// Enum-like con labels
export const STATO_LABELS: Record<number, string> = {
  0: 'Bozza',
  1: 'Attivo',
  2: 'Chiuso',
}
```

### Regole Tipi

- Suffisso `Api` per i tipi che mappano le risposte del backend
- Suffisso `Request` per i body delle richieste
- Enum mappati come `Record<number, string>` con costanti `NOME_LABELS`
- I tipi NON usano `enum` TypeScript — usano union types o costanti

---

## Pattern: Componenti Riusabili

I componenti custom del progetto vanno in `src/components/` organizzati per feature:

```
src/components/
├── anagrafica/          # Componenti specifici del modulo
│   ├── AnagraficaCard.vue
│   └── ...
├── dialogs/             # Dialog generici riusabili
│   └── NomeDialog.vue
└── NomeComponente.vue   # Componenti globali
```

---

## Componenti Vuetify — Uso Corretto

### Form Elements — USA SEMPRE I WRAPPER:
| Wrapper (USARE QUESTO) | Componente Vuetify (NON usare) |
|---|---|
| `AppTextField` | ~~`VTextField`~~ |
| `AppSelect` | ~~`VSelect`~~ |
| `AppTextarea` | ~~`VTextarea`~~ |
| `AppAutocomplete` | ~~`VAutocomplete`~~ |
| `AppCombobox` | ~~`VCombobox`~~ |
| `AppDateTimePicker` | ~~`VDatePicker`~~ |

### Componenti Vuetify Diretti (usare normalmente):
`VCard`, `VCardText`, `VCardActions`, `VRow`, `VCol`, `VBtn`, `VIcon`, `VAvatar`,
`VChip`, `VDivider`, `VSpacer`, `VDialog`, `VMenu`, `VList`, `VListItem`,
`VDataTableServer`, `VTabs`, `VTab`, `VWindow`, `VWindowItem`, `VForm`,
`VSwitch`, `VAlert`, `VTooltip`, `VBadge`, `VProgressLinear`, `VImg`

### Componenti Custom del Tema:
- `IconBtn` — bottone icona (alias di VBtn icon)
- `AppStepper` — wizard stepper
- `DialogCloseBtn` — pulsante chiusura dialog
- `CustomRadiosWithIcon` — radio con icone

---

## Classi CSS Vuetify — Riferimento Rapido

### Tipografia:
- Titoli: `text-h4`, `text-h5`, `text-h6`
- Testo: `text-body-1`, `text-body-2`
- Peso: `font-weight-medium`, `font-weight-bold`
- Emphasis: `text-high-emphasis`, `text-disabled`

### Spacing:
- Margini: `mb-2`, `mb-6`, `mt-2`, `my-2`, `mx-4`
- Padding: `pa-2`, `pa-5`, `pa-sm-10`, `px-6`, `py-4`, `py-6`
- Gap: `gap-4`, `gap-x-3`

### Layout:
- Flex: `d-flex`, `align-center`, `flex-wrap`
- `VSpacer` per spazio flessibile tra elementi
- Bordi: `border-e` (bordo destro)

### Responsive:
- Colonne: `cols="12" md="6"` (mobile full, desktop metà)
- Pattern comuni: `md="3"` sidebar, `md="9"` contenuto

---

## Regole di Design

### 1. Toast / Notifiche

Le notifiche toast **devono sempre** utilizzare il sistema centralizzato esistente. MAI creare snackbar, toast o container di notifica custom nei singoli componenti.

**Come mostrare una toast:**
```typescript
const notificheStore = useNotificheStore()

// Successo
notificheStore.addToast('Operazione completata', null, null, 'success')

// Warning
notificheStore.addToast('Attenzione', 'Messaggio dettaglio', null, 'warning')

// Errore (con messaggio dal backend)
notificheStore.addToast('Errore', error?.data?.message || error?.message || null, null, 'error')

// Info con link
notificheStore.addToast('Nuova notifica', 'Dettaglio', '/percorso', 'info')
```

**Divieti:**
- MAI usare `VSnackbar` direttamente nei componenti
- MAI creare container toast locali o componenti toast custom
- MAI gestire la coda o il timeout delle toast manualmente — lo fa lo store
- MAI usare `alert()` o `confirm()` del browser

**Il rendering delle toast** è gestito globalmente da `NavBarNotifications.vue` nel layout tramite `Teleport to body`. I componenti devono solo chiamare `addToast()` sullo store.

### 2. Stili CSS — Usare SOLO Vuetify, Mai CSS Standard

Tutto lo styling **deve** usare le classi utility di Vuetify e le prop dei componenti. NON scrivere CSS custom, classi Bootstrap, o stili inline quando esiste un equivalente Vuetify.

**Spacing** — classi Vuetify, mai `margin`/`padding` CSS:
```html
<!-- CORRETTO -->
<div class="pa-4 mb-6 mx-2">
<VCardText class="px-6 py-4">

<!-- SBAGLIATO -->
<div style="padding: 1rem; margin-bottom: 1.5rem;">
<div class="custom-spacing">  <!-- con .custom-spacing { padding: 16px } -->
```

**Layout/Flexbox** — classi Vuetify, mai `display: flex` CSS:
```html
<!-- CORRETTO -->
<div class="d-flex align-center justify-space-between flex-wrap gap-4">

<!-- SBAGLIATO -->
<div style="display: flex; align-items: center; justify-content: space-between;">
```

**Tipografia** — classi Vuetify, mai `font-size` CSS:
```html
<!-- CORRETTO -->
<h4 class="text-h4">Titolo</h4>
<p class="text-body-2 text-disabled">Sottotitolo</p>
<span class="font-weight-medium text-high-emphasis">Testo</span>

<!-- SBAGLIATO -->
<h4 style="font-size: 2rem;">Titolo</h4>
<p class="subtitle">Sottotitolo</p>  <!-- classe custom -->
```

**Colori** — prop `color` dei componenti o CSS variables del tema, mai hex/rgb:
```html
<!-- CORRETTO -->
<VBtn color="primary">
<VChip color="success">
<VAvatar color="secondary" variant="tonal">

<!-- SBAGLIATO -->
<VBtn style="background-color: #1976d2;">
<div class="custom-green">  <!-- con .custom-green { color: #4caf50 } -->
```

Quando serve un colore in CSS custom (raro), usare le CSS variables del tema:
```scss
// CORRETTO
color: rgb(var(--v-theme-primary));
background: rgba(var(--v-theme-on-surface), var(--v-high-emphasis-opacity));

// SBAGLIATO
color: #7c6af0;
background: rgba(0, 0, 0, 0.87);
```

**Responsive** — griglia Vuetify, mai media queries CSS:
```html
<!-- CORRETTO -->
<VRow>
  <VCol cols="12" md="6">...</VCol>
  <VCol cols="12" md="6">...</VCol>
</VRow>

<!-- SBAGLIATO -->
<div class="row"><div class="col-md-6">  <!-- Bootstrap -->
<div class="grid-custom">  <!-- con @media (min-width: 960px) { ... } -->
```

**Bordi, ombre, arrotondamenti** — prop dei componenti, mai CSS:
```html
<!-- CORRETTO -->
<VCard elevation="6">
<VAvatar rounded>
<div class="border-e">  <!-- bordo destro Vuetify -->

<!-- SBAGLIATO -->
<div style="box-shadow: 0 2px 4px rgba(0,0,0,.1);">
<div style="border-radius: 8px;">
```

**Quando è ammesso CSS custom (eccezioni rare):**
- `max-inline-size` / `max-block-size` per vincoli dimensionali (es. `style="max-inline-size: 250px;"`)
- `transition-duration` per animazioni
- Stili dinamici calcolati con `:style="{ transform: ... }"`
- Override di componenti terze parti (es. cropper.js)
- **Mai** blocchi `<style>` nelle pagine — solo nei componenti riusabili in `@core/` quando strettamente necessario

**Riferimento classi Vuetify principali:**

| Categoria | Classi |
|-----------|--------|
| Display | `d-flex`, `d-none`, `d-block`, `d-inline-flex` |
| Flex | `flex-column`, `flex-wrap`, `flex-grow-1`, `flex-shrink-0` |
| Align | `align-center`, `align-start`, `align-end` |
| Justify | `justify-center`, `justify-space-between`, `justify-end` |
| Spacing | `pa-*`, `px-*`, `py-*`, `ma-*`, `mx-*`, `my-*`, `mb-*`, `mt-*` (0-16) |
| Gap | `gap-*`, `gap-x-*`, `gap-y-*` (0-16) |
| Testo | `text-h1`-`text-h6`, `text-body-1`, `text-body-2`, `text-caption` |
| Peso font | `font-weight-medium`, `font-weight-bold`, `font-weight-regular` |
| Colore testo | `text-primary`, `text-disabled`, `text-high-emphasis` |
| Overflow | `text-no-wrap`, `text-truncate` |
| Cursore | `cursor-pointer` |
| Bordi | `border-e`, `border-b`, `border-s` |
| Visibilità | `v-show`, `:class="{ 'd-none': condizione }"` |

### 3. Tabelle dentro le Card — Bordi aderenti, nessun padding

Quando una `VTable` o `VDataTableServer` è dentro una `VCard`, la tabella deve aderire ai bordi della card. **MAI** wrappare la tabella in `<VCardText>` perché aggiunge padding.

```html
<!-- CORRETTO: tabella direttamente nella card, bordi aderenti -->
<VCard>
  <VCardItem>
    <VCardTitle>Titolo</VCardTitle>
  </VCardItem>

  <VTable>
    <thead>
      <tr>
        <th>Colonna</th>
      </tr>
    </thead>
    <tbody>
      <tr><td>Dato</td></tr>
    </tbody>
  </VTable>
</VCard>

<!-- SBAGLIATO: la VCardText aggiunge padding intorno alla tabella -->
<VCard>
  <VCardText>
    <VTable>...</VTable>
  </VCardText>
</VCard>
```

**Regola**: Il titolo della card va in `VCardItem > VCardTitle`. La tabella va subito dopo, direttamente figlia della `VCard`. Eventuali contenuti non-tabella (es. messaggio "nessun dato") possono usare `VCardText`.

**Eccezione — Tabelle di form (checkbox, toggle, impostazioni):** Quando una tabella non elenca dati ma contiene controlli di form (checkbox, switch, select), va mantenuta dentro `VCardText` con classi `border rounded` per avere padding e bordi interni. Le colonne checkbox devono avere larghezza fissa (`width: 100px`) per allineamento consistente tra tabelle multiple.

```html
<!-- Tabella di form dentro VCardText (eccezione ammessa) -->
<VCard>
  <VCardText>
    <h5 class="text-h5 mb-2">Sezione</h5>
    <VTable class="text-no-wrap mb-6 border rounded">
      <thead>
        <tr>
          <th>Tipo</th>
          <th class="text-center" style="width: 100px;">Email</th>
          <th class="text-center" style="width: 100px;">App</th>
        </tr>
      </thead>
      <tbody>...</tbody>
    </VTable>
  </VCardText>
</VCard>
```

### 4. Card Dettaglio — Etichetta in grassetto, valore normale, stessa riga

Quando si elencano dati di un'entità in una card (es. dettaglio anagrafica, contatto, profilo), usare il pattern Vuexy: **etichetta in grassetto** seguita dal **valore in peso normale**, sulla stessa riga.

```html
<!-- CORRETTO: pattern Vuexy demo (h6 con span text-body-1 inline) -->
<VCardText>
  <div class="d-flex flex-column gap-3">
    <h6 class="text-h6">
      Telefono:
      <span class="text-body-1 d-inline-block">+39 02 1234567</span>
    </h6>
    <h6 class="text-h6">
      Email:
      <span class="text-body-1 d-inline-block">info@esempio.it</span>
    </h6>
    <h6 class="text-h6">
      Stato:
      <VChip size="small" color="success" label>Attivo</VChip>
    </h6>
  </div>
</VCardText>

<!-- SBAGLIATO: label sopra e valore sotto -->
<VCardText>
  <span class="text-caption text-disabled">Telefono</span>
  <p class="text-body-1">+39 02 1234567</p>
</VCardText>

<!-- SBAGLIATO: <ul>/<li> con inline style -->
<ul style="list-style: none;">
  <li><span class="h6">Telefono:</span><span>valore</span></li>
</ul>

<!-- SBAGLIATO: span font-weight-medium invece di h6 -->
<span class="text-body-1 font-weight-medium">Telefono:</span>
<span class="text-body-1">valore</span>
```

**Regole:**
- Container: `<div class="d-flex flex-column gap-3">` dentro `VCardText`
- Ogni coppia: `<h6 class="text-h6">Etichetta: <span class="text-body-1 d-inline-block">Valore</span></h6>`
- L'etichetta è testo diretto nell'`h6` (ereditandone il grassetto)
- Il valore è in `<span class="text-body-1 d-inline-block">` che resetta il peso a normale
- MAI usare `<ul>/<li>` per elenchi di dati
- MAI mettere label e valore su righe separate (label sopra, valore sotto)
- Per valori con stato: usare `VChip` inline dopo l'etichetta
- Valori nulli: `<span class="text-body-1 d-inline-block text-disabled">—</span>`

### 5. Titoli delle Card — Nessuna icona

I titoli delle card (`VCardTitle`) **non devono mai contenere icone** (`VIcon`). Il titolo è solo testo.

```html
<!-- CORRETTO -->
<VCard title="Contatti Collegati">

<!-- CORRETTO (con azioni nel titolo) -->
<VCardItem>
  <VCardTitle class="d-flex align-center">
    Anagrafiche collegate
    <VSpacer />
    <VChip size="x-small" label>5</VChip>
  </VCardTitle>
</VCardItem>

<!-- SBAGLIATO: icona nel titolo -->
<VCardTitle class="d-flex align-center">
  <VIcon icon="tabler-building" size="20" class="me-2" />
  Anagrafiche collegate
</VCardTitle>
```

### 6. Pulsanti — Stile uniforme

Tutti i pulsanti devono essere `rounded` e seguire queste regole:

**Pulsante Submit/Salva** — colore pieno primary con icona:
```html
<VBtn color="primary" rounded prepend-icon="tabler-device-floppy">
  Salva
</VBtn>
```
- Testo sempre **"Salva"** (mai "Salva modifiche", "Crea Anagrafica", etc.)
- Colore pieno (`color="primary"`), mai `variant="tonal"`
- Icona `tabler-device-floppy` con `prepend-icon`

**Pulsante Elimina** — colore pieno error con icona:
```html
<VBtn color="error" rounded prepend-icon="tabler-trash">
  Elimina
</VBtn>
```
- Colore pieno (`color="error"`), mai `variant="tonal"`

**Pulsanti secondari** (Annulla, Indietro, Export, etc.) — tonal:
```html
<VBtn variant="tonal" color="secondary" rounded>
  Annulla
</VBtn>
```

**Pulsanti nei dialog/modal:** MAI usare `VCardActions` perché rende i pulsanti slim e forza `variant="text"`. Usare `VCardText` con flex:
```html
<VCardText class="d-flex justify-end gap-4">
  <VBtn variant="tonal" color="secondary">Annulla</VBtn>
  <VBtn color="primary" prepend-icon="tabler-device-floppy">Salva</VBtn>
</VCardText>
```

**Divieti:**
- MAI usare `VCardActions` nei dialog — usare `VCardText` con `d-flex justify-end gap-4`
- MAI pulsanti senza `rounded` (gestito globalmente nei defaults)
- MAI pulsanti submit con `variant="tonal"` — il submit è sempre colore pieno
- MAI testi diversi da "Salva" per il submit (no "Salva modifiche", "Aggiorna", "Crea X")
- MAI pulsanti elimina con `variant="tonal"` — l'elimina è sempre colore pieno

### 8. Header Pagina Dettaglio — Modifica + Menu Azioni

Nelle pagine di dettaglio entità (anagrafica, contatto, etc.) i pulsanti d'azione nell'header devono seguire questo pattern:

```html
<div class="d-flex gap-2">
  <!-- Pulsante primario: sempre visibile -->
  <VBtn color="primary" prepend-icon="tabler-edit" :to="`/modulo/modifica-${id}`">
    Modifica
  </VBtn>

  <!-- Menu azioni: raccoglie tutte le altre azioni -->
  <VBtn variant="tonal" color="secondary" append-icon="tabler-chevron-down">
    Azioni
    <VMenu activator="parent">
      <VList>
        <VListItem prepend-icon="tabler-icon" title="Azione 1" @click="..." />
        <VListItem prepend-icon="tabler-icon" title="Azione 2" @click="..." />
        <VDivider />
        <VListItem prepend-icon="tabler-trash" title="Elimina" class="text-error" @click="..." />
      </VList>
    </VMenu>
  </VBtn>
</div>
```

**Regole:**
- Solo **"Modifica"** è un pulsante visibile diretto (`color="primary"`)
- Tutte le altre azioni (converti, disattiva, riattiva, etc.) vanno nel menu **"Azioni"**
- **"Elimina"** è sempre l'ultimo item nel menu, separata da un `VDivider`, con `class="text-error"`
- Il menu "Azioni" usa `variant="tonal" color="secondary"` con `append-icon="tabler-chevron-down"`
- MAI mettere più di 2 pulsanti visibili nell'header (Modifica + Azioni)

---

### 9. Formattazione Automatica Nome / Cognome / Ragione Sociale

I campi nome, cognome e ragione sociale devono essere formattati automaticamente sia al `@blur` (UX immediata) che nel pre-submit (safety net).

**Regole di formattazione:**
- **Nome**: iniziale maiuscola, resto minuscolo per ogni parola → `formatNome()` ("pietro" → "Pietro")
- **Cognome**: tutto MAIUSCOLO → `formatCognome()` ("bello" → "BELLO")
- **Ragione Sociale**: tutto MAIUSCOLO → `formatRagioneSociale()` ("spadhausen srl" → "SPADHAUSEN SRL")

**Funzioni disponibili** in `@/utils/formatters`:
```typescript
import { formatNome, formatCognome, formatRagioneSociale } from '@/utils/formatters'
```

**Applicazione nel template** (`@blur`):
```html
<AppTextField v-model="form.nome" label="Nome"
  @blur="form.nome && (form.nome = formatNome(form.nome))" />

<AppTextField v-model="form.cognome" label="Cognome"
  @blur="form.cognome && (form.cognome = formatCognome(form.cognome))" />

<AppTextField v-model="form.ragioneSociale" label="Ragione Sociale"
  @blur="form.ragioneSociale && (form.ragioneSociale = formatRagioneSociale(form.ragioneSociale))" />
```

**Applicazione nel submit** (safety net):
```typescript
async function submit() {
  // ... validazione ...
  if (form.value.nome) form.value.nome = formatNome(form.value.nome)
  if (form.value.cognome) form.value.cognome = formatCognome(form.value.cognome)
  if (form.value.ragioneSociale) form.value.ragioneSociale = formatRagioneSociale(form.value.ragioneSociale)
  // ... chiamata API ...
}
```

---

### 7. Navigazione Sidebar — Nodi figli senza icona

I nodi figli (children) nella navigazione verticale **non devono avere icona**. Il tema Vuexy assegna automaticamente `tabler-circle` ai figli senza icona. Solo i nodi padre hanno un'icona esplicita.

```typescript
// CORRETTO
{
  title: 'Anagrafica',
  icon: { icon: 'tabler-address-book' },  // solo il padre ha l'icona
  children: [
    { title: 'Anagrafiche', to: 'anagrafiche' },  // nessuna icona → circle automatico
    { title: 'Contatti', to: 'contatti' },
  ],
}

// SBAGLIATO
{
  title: 'Anagrafica',
  icon: { icon: 'tabler-address-book' },
  children: [
    { title: 'Anagrafiche', to: 'anagrafiche', icon: { icon: 'tabler-building' } },  // NO
  ],
}
```

---

## Checklist Prima di Generare Codice

Prima di scrivere qualsiasi componente, verifica:

- [ ] Usa `<script setup lang="ts">`?
- [ ] I form elements usano i wrapper `App*` e non i componenti Vuetify diretti?
- [ ] Le icone usano il prefisso `tabler-`?
- [ ] Le variabili e i nomi funzione sono in italiano?
- [ ] Lo store usa `$api` e Composition API?
- [ ] La validazione usa i validatori da `@/@core/utils/validators`?
- [ ] Le notifiche usano `notificheStore.addToast()`?
- [ ] Il layout responsive usa `VRow`/`VCol` con breakpoint `md`?
- [ ] I tipi sono definiti come `interface` (non `type`) con suffissi corretti?
- [ ] La paginazione usa `pagina`/`dimensionePagina` come nomi variabili?
- [ ] I valori nulli sono mostrati come `<span class="text-disabled">—</span>`?
- [ ] `ref`, `computed`, `watch`, `onMounted`, `useRouter` NON sono importati (auto-import)?
- [ ] Lo styling usa SOLO classi Vuetify e prop componenti, niente CSS custom/inline/Bootstrap?
- [ ] I colori usano nomi tema (`primary`, `success`, etc.) o CSS variables (`--v-theme-*`), mai hex?

---

## Istruzioni Operative

Quando ti viene chiesto di creare un nuovo modulo o componente:

1. **Leggi sempre prima** i file esistenti simili per verificare i pattern attuali
2. **Crea i file nell'ordine**: tipi → store → pagine → componenti
3. **Segui la struttura delle cartelle** esistente senza inventare nuovi pattern
4. **Non aggiungere** librerie, dipendenze o pattern che non sono già nel progetto
5. **Non creare file di test** a meno che non venga esplicitamente richiesto
6. **Verifica** che le API endpoint corrispondano a quelle del backend .NET
