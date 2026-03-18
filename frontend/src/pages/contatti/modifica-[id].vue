<script setup lang="ts">
import { useContattiStore } from '@/stores/contatti'
import type { UpdateContattoRequest } from '@/types/anagrafica'
import { requiredValidator } from '@/@core/utils/validators'

const route = useRoute()
const router = useRouter()
const store = useContattiStore()

const id = computed(() => Number(route.params.id))
const loading = ref(false)
const saving = ref(false)
const formRef = ref()

// Snackbar
const snackbar = ref(false)
const snackbarMessage = ref('')
const snackbarColor = ref('success')

const form = ref<UpdateContattoRequest>({
  nome: '',
  cognome: '',
})

onMounted(async () => {
  loading.value = true
  try {
    await store.fetchById(id.value)
    if (store.current) {
      const c = store.current
      form.value = {
        nome: c.nome,
        cognome: c.cognome,
        email: c.email,
        cellulare: c.cellulare,
        telefono: c.telefono,
        note: c.note,
      }
    }
  }
  finally {
    loading.value = false
  }
})

async function submit() {
  const { valid } = await formRef.value?.validate()
  if (!valid) return

  saving.value = true
  try {
    await store.update(id.value, form.value)
    snackbarMessage.value = 'Contatto aggiornato con successo'
    snackbarColor.value = 'success'
    snackbar.value = true
    router.push(`/contatti/${id.value}`)
  }
  catch (error: any) {
    snackbarMessage.value = error?.data?.message || error?.message || 'Errore durante il salvataggio'
    snackbarColor.value = 'error'
    snackbar.value = true
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
          :to="`/contatti/${id}`"
        />
        <h4 class="text-h4">
          Modifica Contatto
          <span v-if="store.current" class="text-disabled">
            — {{ store.current.nome }} {{ store.current.cognome }}
          </span>
        </h4>
        <VSpacer />
        <VBtn type="submit" color="primary" :loading="saving" prepend-icon="tabler-device-floppy">
          Salva
        </VBtn>
      </div>

      <!-- Dati Contatto -->
      <VCard title="Dati Contatto" class="mb-6">
        <VCardText>
          <VRow>
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
            <VCol cols="12" md="6">
              <AppTextField
                v-model="form.email"
                label="Email"
              />
            </VCol>
            <VCol cols="12" md="6">
              <AppTextField
                v-model="form.cellulare"
                label="Cellulare"
              />
            </VCol>
            <VCol cols="12" md="6">
              <AppTextField
                v-model="form.telefono"
                label="Telefono"
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

      <!-- Bottom submit button -->
      <div class="d-flex justify-end">
        <VBtn
          variant="text"
          class="me-4"
          :to="`/contatti/${id}`"
        >
          Annulla
        </VBtn>
        <VBtn type="submit" color="primary" :loading="saving" prepend-icon="tabler-device-floppy">
          Salva
        </VBtn>
      </div>
    </VForm>

    <!-- Snackbar -->
    <VSnackbar
      v-model="snackbar"
      :color="snackbarColor"
      :timeout="5000"
    >
      {{ snackbarMessage }}
    </VSnackbar>
  </div>
</template>
