<script setup lang="ts">
import { useContattiStore } from '@/stores/contatti'
import { useNotificheStore } from '@/stores/notifiche'
import type { CreateContattoRequest } from '@/types/anagrafica'
import { requiredValidator } from '@/@core/utils/validators'
import { formatNome, formatCognome } from '@/utils/formatters'

const router = useRouter()
const store = useContattiStore()
const notificheStore = useNotificheStore()

const saving = ref(false)
const formRef = ref()

const form = ref<CreateContattoRequest>({
  nome: '',
  cognome: '',
})

async function submit() {
  const { valid } = await formRef.value?.validate()
  if (!valid) return

  saving.value = true
  try {
    if (form.value.nome) form.value.nome = formatNome(form.value.nome)
    if (form.value.cognome) form.value.cognome = formatCognome(form.value.cognome)

    const result = await store.create(form.value)
    router.push(`/contatti/${result.id}`)
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
                @blur="form.nome && (form.nome = formatNome(form.nome))"
              />
            </VCol>
            <VCol cols="12" md="6">
              <AppTextField
                v-model="form.cognome"
                label="Cognome"
                :rules="[requiredValidator]"
                @blur="form.cognome && (form.cognome = formatCognome(form.cognome))"
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
          variant="tonal"
          color="secondary"
          class="me-4"
          :to="{ path: '/contatti' }"
        >
          Annulla
        </VBtn>
        <VBtn type="submit" color="primary" :loading="saving" prepend-icon="tabler-device-floppy">
          Salva
        </VBtn>
      </div>
    </VForm>

  </div>
</template>
