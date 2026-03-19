<script setup lang="ts">
import { useContattiStore } from '@/stores/contatti'
import { useNotificheStore } from '@/stores/notifiche'
import type { UpdateContattoRequest } from '@/types/anagrafica'
import { requiredValidator } from '@/@core/utils/validators'
import { formatNome, formatCognome } from '@/utils/formatters'

const route = useRoute()
const router = useRouter()
const store = useContattiStore()
const notificheStore = useNotificheStore()

const id = computed(() => Number(route.params.id))
const loading = ref(false)
const saving = ref(false)
const formRef = ref()

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
    if (form.value.nome) form.value.nome = formatNome(form.value.nome)
    if (form.value.cognome) form.value.cognome = formatCognome(form.value.cognome)

    await store.update(id.value, form.value)
    notificheStore.addToast('Contatto aggiornato con successo', null, null, 'success')
    router.push(`/contatti/${id.value}`)
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
          :to="`/contatti/${id}`"
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
