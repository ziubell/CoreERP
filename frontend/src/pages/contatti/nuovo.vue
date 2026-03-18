<script setup lang="ts">
import { useContattiStore } from '@/stores/contatti'
import type { CreateContattoRequest } from '@/types/anagrafica'
import { requiredValidator } from '@/@core/utils/validators'

const router = useRouter()
const store = useContattiStore()

const saving = ref(false)
const formRef = ref()

// Snackbar
const snackbar = ref(false)
const snackbarMessage = ref('')
const snackbarColor = ref('success')

const form = ref<CreateContattoRequest>({
  nome: '',
  cognome: '',
})

async function submit() {
  const { valid } = await formRef.value?.validate()
  if (!valid) return

  saving.value = true
  try {
    const result = await store.create(form.value)
    router.push(`/contatti/${result.id}`)
  }
  catch (error: any) {
    snackbarMessage.value = error?.data?.message || error?.message || 'Errore durante la creazione'
    snackbarColor.value = 'error'
    snackbar.value = true
  }
  finally {
    saving.value = false
  }
}
</script>

<template>
  <div>
    <VForm ref="formRef" @submit.prevent="submit">
      <!-- Header with top submit button -->
      <div class="d-flex align-center mb-6 flex-wrap gap-4">
        <VBtn
          variant="text"
          icon="tabler-arrow-left"
          :to="{ path: '/contatti' }"
        />
        <h4 class="text-h4">Nuovo Contatto</h4>
        <VSpacer />
        <VBtn type="submit" color="primary" :loading="saving" prepend-icon="tabler-device-floppy">
          Crea Contatto
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
          :to="{ path: '/contatti' }"
        >
          Annulla
        </VBtn>
        <VBtn type="submit" color="primary" :loading="saving" prepend-icon="tabler-device-floppy">
          Crea Contatto
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
