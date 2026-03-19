<script setup lang="ts">
import { useContattiStore } from '@/stores/contatti'
import { useNotificheStore } from '@/stores/notifiche'
import type { CreateContattoRequest } from '@/types/anagrafica'
import { requiredValidator } from '@/@core/utils/validators'
import { formatNome, formatCognome } from '@/utils/formatters'

const props = defineProps<{
  id?: number
}>()

const router = useRouter()
const store = useContattiStore()
const notificheStore = useNotificheStore()

const isEditMode = computed(() => props.id !== undefined)
const loading = ref(false)
const saving = ref(false)
const formRef = ref()

const form = ref<CreateContattoRequest>({
  nome: '',
  cognome: '',
})

onMounted(async () => {
  if (isEditMode.value) {
    loading.value = true
    try {
      await store.fetchById(props.id!)
      if (store.current) {
        const c = store.current
        form.value = {
          nome: c.nome,
          cognome: c.cognome,
          email: c.email,
          cellulare: c.cellulare,
          note: c.note,
        }
      }
    }
    finally {
      loading.value = false
    }
  }
})

async function submit() {
  const { valid } = await formRef.value?.validate()
  if (!valid) return

  saving.value = true
  try {
    if (form.value.nome) form.value.nome = formatNome(form.value.nome)
    if (form.value.cognome) form.value.cognome = formatCognome(form.value.cognome)

    if (isEditMode.value) {
      await store.update(props.id!, form.value)
      notificheStore.addToast('Contatto aggiornato con successo', null, null, 'success')
      router.push(`/contatti/${props.id}`)
    }
    else {
      const result = await store.create(form.value)
      router.push(`/contatti/${result.id}`)
    }
  }
  catch (error: any) {
    notificheStore.addToast('Errore durante il salvataggio', error?.data?.message || error?.message || null, null, 'error')
  }
  finally {
    saving.value = false
  }
}

const backRoute = computed(() => isEditMode.value ? `/contatti/${props.id}` : '/contatti')
</script>

<template>
  <div v-if="loading" class="d-flex justify-center py-12">
    <VProgressCircular indeterminate />
  </div>

  <VForm v-else ref="formRef" @submit.prevent="submit">
    <!-- Header -->
    <div class="d-flex align-center mb-6 flex-wrap gap-4">
      <VBtn variant="text" icon="tabler-arrow-left" :to="backRoute" />
      <h4 class="text-h4">
        {{ isEditMode ? 'Modifica Contatto' : 'Nuovo Contatto' }}
        <span v-if="isEditMode && store.current" class="text-disabled">
          — {{ store.current.nome }} {{ store.current.cognome }}
        </span>
      </h4>
    </div>

    <!-- Form -->
    <VCard>
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
          <VCol cols="12">
            <AppTextarea v-model="form.note" label="Note" rows="3" />
          </VCol>
          <VCol cols="12">
            <div class="d-flex justify-end">
              <VBtn type="submit" color="primary" :loading="saving" prepend-icon="tabler-device-floppy">
                Salva
              </VBtn>
            </div>
          </VCol>
        </VRow>
      </VCardText>
    </VCard>
  </VForm>
</template>
