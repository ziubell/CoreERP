<script setup lang="ts">
import { $api } from '@/utils/api'
import { useNotificheStore } from '@/stores/notifiche'

const props = defineProps<{
  modelValue: boolean
  entitaTipo: string
  entitaId: number
  entitaNome?: string
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  'updated': []
}>()

const notifiche = useNotificheStore()
const loading = ref(true)
const saving = ref(false)
const isFollowing = ref(false)

const preferenze = ref({
  notificaModifiche: true,
  notificaMessaggi: true,
  notificaContatti: true,
  notificaIndirizzi: true,
})

watch(() => props.modelValue, async (val) => {
  if (val) {
    loading.value = true
    try {
      const data = await $api(`/v1/sottoscrizioni/${props.entitaTipo}/${props.entitaId}`)
      isFollowing.value = data.following
      if (data.following) {
        preferenze.value = {
          notificaModifiche: data.notificaModifiche ?? true,
          notificaMessaggi: data.notificaMessaggi ?? true,
          notificaContatti: data.notificaContatti ?? true,
          notificaIndirizzi: data.notificaIndirizzi ?? true,
        }
      }
      else {
        preferenze.value = {
          notificaModifiche: true,
          notificaMessaggi: true,
          notificaContatti: true,
          notificaIndirizzi: true,
        }
      }
    }
    catch {
      isFollowing.value = false
    }
    finally {
      loading.value = false
    }
  }
})

async function salva() {
  saving.value = true
  try {
    await $api(`/v1/sottoscrizioni/${props.entitaTipo}/${props.entitaId}`, {
      method: 'POST',
      body: preferenze.value,
    })
    isFollowing.value = true
    notifiche.addToast('Preferenze salvate', null, null, 'success')
    emit('updated')
    emit('update:modelValue', false)
  }
  catch {
    notifiche.addToast('Errore salvataggio', null, null, 'error')
  }
  finally {
    saving.value = false
  }
}

async function smettiDiSeguire() {
  saving.value = true
  try {
    await $api(`/v1/sottoscrizioni/${props.entitaTipo}/${props.entitaId}`, {
      method: 'DELETE',
    })
    isFollowing.value = false
    notifiche.addToast('Non segui più questa anagrafica', null, null, 'success')
    emit('updated')
    emit('update:modelValue', false)
  }
  catch {
    notifiche.addToast('Errore', null, null, 'error')
  }
  finally {
    saving.value = false
  }
}
</script>

<template>
  <VDialog
    :model-value="modelValue"
    max-width="450"
    @update:model-value="$emit('update:modelValue', $event)"
  >
    <VCard :title="isFollowing ? 'Modifica notifiche' : 'Segui anagrafica'">
      <VCardText v-if="loading" class="d-flex justify-center py-6">
        <VProgressCircular indeterminate />
      </VCardText>

      <VCardText v-else>
        <p v-if="entitaNome" class="text-body-2 text-medium-emphasis mb-4">
          {{ isFollowing ? 'Scegli quali notifiche ricevere per' : 'Ricevi notifiche per' }}
          <strong>{{ entitaNome }}</strong>
        </p>

        <VCheckbox
          v-model="preferenze.notificaModifiche"
          label="Modifiche ai dati"
          hide-details
          density="compact"
          class="mb-1"
        />
        <VCheckbox
          v-model="preferenze.notificaMessaggi"
          label="Nuovi messaggi"
          hide-details
          density="compact"
          class="mb-1"
        />
        <VCheckbox
          v-model="preferenze.notificaContatti"
          label="Contatti aggiunti/rimossi"
          hide-details
          density="compact"
          class="mb-1"
        />
        <VCheckbox
          v-model="preferenze.notificaIndirizzi"
          label="Indirizzi aggiunti/rimossi"
          hide-details
          density="compact"
        />
      </VCardText>

      <VCardText class="d-flex justify-space-between">
        <div>
          <VBtn
            v-if="isFollowing"
            variant="tonal"
            color="error"
            size="small"
            :loading="saving"
            @click="smettiDiSeguire"
          >
            Smetti di seguire
          </VBtn>
        </div>
        <div class="d-flex gap-4">
          <VBtn variant="tonal" @click="$emit('update:modelValue', false)">
            Annulla
          </VBtn>
          <VBtn
            color="primary"
            prepend-icon="tabler-device-floppy"
            :loading="saving"
            @click="salva"
          >
            Salva
          </VBtn>
        </div>
      </VCardText>
    </VCard>
  </VDialog>
</template>
