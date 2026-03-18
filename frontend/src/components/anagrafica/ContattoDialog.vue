<script setup lang="ts">
import { $api } from '@/utils/api'
import { useAnagraficheStore } from '@/stores/anagrafiche'
import type { ContattoListItemApi, RuoloContattoApi } from '@/types/anagrafica'
import { requiredValidator } from '@/@core/utils/validators'

export interface ContattoDialogData {
  id?: number | null
  contattoId?: number | null
  nome: string
  cognome: string
  email: string
  cellulare: string
  telefono: string
  note: string
  ruoloContattoId: number | null
  principale: boolean
  isExisting: boolean
}

const props = defineProps<{
  modelValue: boolean
  contatto?: ContattoDialogData | null
  ruoliContatto: RuoloContattoApi[]
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  'save': [data: ContattoDialogData]
}>()

const isOpen = computed({
  get: () => props.modelValue,
  set: val => emit('update:modelValue', val),
})

const isEditMode = computed(() => !!props.contatto?.contattoId || !!props.contatto?.id)

// Search
const searchQuery = ref('')
const searchResults = ref<ContattoListItemApi[]>([])
const searchLoading = ref(false)
const showCreateForm = ref(false)
const selectedExisting = ref<ContattoListItemApi | null>(null)

// Form
const form = ref<ContattoDialogData>({
  nome: '',
  cognome: '',
  email: '',
  cellulare: '',
  telefono: '',
  note: '',
  ruoloContattoId: null,
  principale: false,
  isExisting: false,
})

const formRef = ref()

watch(() => props.modelValue, (open) => {
  if (open) {
    searchQuery.value = ''
    searchResults.value = []
    selectedExisting.value = null

    if (props.contatto) {
      showCreateForm.value = true
      form.value = { ...props.contatto }
    }
    else {
      showCreateForm.value = false
      form.value = {
        nome: '',
        cognome: '',
        email: '',
        cellulare: '',
        telefono: '',
        note: '',
        ruoloContattoId: null,
        principale: false,
        isExisting: false,
      }
    }
  }
})

let searchTimeout: ReturnType<typeof setTimeout> | null = null

function onSearchInput() {
  if (searchTimeout) clearTimeout(searchTimeout)
  if (searchQuery.value.length < 2) {
    searchResults.value = []
    return
  }
  searchTimeout = setTimeout(async () => {
    searchLoading.value = true
    try {
      const data = await $api(`/v1/contatti?ricerca=${encodeURIComponent(searchQuery.value)}&dimensionePagina=10`)
      searchResults.value = data.items ?? []
    }
    catch {
      searchResults.value = []
    }
    finally {
      searchLoading.value = false
    }
  }, 300)
}

function selectExisting(contatto: ContattoListItemApi) {
  selectedExisting.value = contatto
  showCreateForm.value = true
  form.value = {
    contattoId: contatto.id,
    nome: contatto.nome,
    cognome: contatto.cognome,
    email: contatto.email ?? '',
    cellulare: contatto.cellulare ?? '',
    telefono: contatto.telefono ?? '',
    note: '',
    ruoloContattoId: null,
    principale: false,
    isExisting: true,
  }
}

function switchToCreate() {
  selectedExisting.value = null
  showCreateForm.value = true
  form.value = {
    nome: searchQuery.value,
    cognome: '',
    email: '',
    cellulare: '',
    telefono: '',
    note: '',
    ruoloContattoId: null,
    principale: false,
    isExisting: false,
  }
}

async function handleSave() {
  const { valid } = await formRef.value?.validate()
  if (!valid) return

  emit('save', { ...form.value })
  isOpen.value = false
}

const dialogTitle = computed(() => {
  if (isEditMode.value) return 'Modifica Contatto'
  return 'Aggiungi Contatto'
})
</script>

<template>
  <VDialog
    v-model="isOpen"
    max-width="600"
    persistent
  >
    <VCard :title="dialogTitle">
      <VCardText>
        <VForm ref="formRef" @submit.prevent="handleSave">
          <!-- Search section (only for new, not edit) -->
          <template v-if="!isEditMode && !showCreateForm">
            <AppTextField
              v-model="searchQuery"
              label="Cerca contatto esistente"
              placeholder="Nome, email o cellulare..."
              prepend-inner-icon="tabler-search"
              :loading="searchLoading"
              @input="onSearchInput"
            />

            <VList v-if="searchResults.length > 0" class="mt-2" density="compact" max-height="250" style="overflow-y: auto;">
              <VListItem
                v-for="c in searchResults"
                :key="c.id"
                :title="`${c.nome} ${c.cognome}`"
                :subtitle="[c.email, c.cellulare].filter(Boolean).join(' | ')"
                @click="selectExisting(c)"
              >
                <template #prepend>
                  <VAvatar size="32" color="primary" variant="tonal">
                    <span class="text-caption">{{ c.nome[0] }}{{ c.cognome[0] }}</span>
                  </VAvatar>
                </template>
              </VListItem>
            </VList>

            <div v-if="searchQuery.length >= 2 && !searchLoading && searchResults.length === 0" class="text-disabled text-body-2 mt-2">
              Nessun contatto trovato
            </div>

            <VBtn
              class="mt-4"
              color="primary"
              variant="tonal"
              prepend-icon="tabler-plus"
              block
              @click="switchToCreate"
            >
              Crea nuovo contatto
            </VBtn>
          </template>

          <!-- Form fields -->
          <template v-if="showCreateForm">
            <VRow>
              <VCol cols="12" md="6">
                <AppTextField
                  v-model="form.nome"
                  label="Nome"
                  :rules="[requiredValidator]"
                  :readonly="form.isExisting"
                />
              </VCol>
              <VCol cols="12" md="6">
                <AppTextField
                  v-model="form.cognome"
                  label="Cognome"
                  :rules="[requiredValidator]"
                  :readonly="form.isExisting"
                />
              </VCol>
              <VCol cols="12" md="6">
                <AppTextField
                  v-model="form.email"
                  label="Email"
                  :readonly="form.isExisting"
                />
              </VCol>
              <VCol cols="12" md="6">
                <AppTextField
                  v-model="form.cellulare"
                  label="Cellulare"
                  :readonly="form.isExisting"
                />
              </VCol>
              <VCol cols="12" md="6">
                <AppTextField
                  v-model="form.telefono"
                  label="Telefono"
                  :readonly="form.isExisting"
                />
              </VCol>
              <VCol v-if="!form.isExisting" cols="12">
                <AppTextarea
                  v-model="form.note"
                  label="Note"
                  rows="2"
                />
              </VCol>

              <VCol cols="12">
                <VDivider class="mb-4" />
              </VCol>

              <VCol cols="12" md="8">
                <AppSelect
                  v-model="form.ruoloContattoId"
                  :items="ruoliContatto.map(r => ({ title: r.nome, value: r.id }))"
                  label="Ruolo"
                  :rules="[requiredValidator]"
                />
              </VCol>
              <VCol cols="12" md="4" class="d-flex align-center">
                <VSwitch
                  v-model="form.principale"
                  label="Principale"
                  density="compact"
                  hide-details
                />
              </VCol>
            </VRow>
          </template>
        </VForm>
      </VCardText>

      <VCardActions>
        <VSpacer />
        <VBtn variant="text" @click="isOpen = false">
          Annulla
        </VBtn>
        <VBtn
          v-if="showCreateForm"
          color="primary"
          @click="handleSave"
        >
          {{ isEditMode ? 'Salva' : 'Aggiungi' }}
        </VBtn>
      </VCardActions>
    </VCard>
  </VDialog>
</template>
