<script setup lang="ts">
import { useContattiStore } from '@/stores/contatti'

const route = useRoute()
const router = useRouter()
const store = useContattiStore()

const id = computed(() => Number(route.params.id))
const contatto = computed(() => store.current)

// Delete dialog
const deleteDialogOpen = ref(false)
const deleteLoading = ref(false)

// Snackbar
const snackbar = ref(false)
const snackbarMessage = ref('')
const snackbarColor = ref('success')

onMounted(async () => {
  await store.fetchById(id.value)
})

async function confirmDelete() {
  deleteLoading.value = true
  try {
    await store.remove(id.value)
    router.push('/contatti')
  }
  catch (error: any) {
    snackbarMessage.value = error?.data?.message || error?.message || 'Errore durante l\'eliminazione'
    snackbarColor.value = 'error'
    snackbar.value = true
    deleteDialogOpen.value = false
  }
  finally {
    deleteLoading.value = false
  }
}
</script>

<template>
  <div v-if="contatto">
    <!-- Header -->
    <div class="d-flex align-center mb-6 gap-4">
      <VBtn variant="text" icon="tabler-arrow-left" :to="{ path: '/contatti' }" />
      <div>
        <h4 class="text-h4">{{ contatto.nome }} {{ contatto.cognome }}</h4>
      </div>
      <VSpacer />
      <div class="d-flex gap-2">
        <VBtn
          color="primary"
          variant="tonal"
          prepend-icon="tabler-edit"
          :to="`/contatti/modifica-${id}`"
        >
          Modifica
        </VBtn>
        <VBtn color="error" variant="tonal" icon="tabler-trash" @click="deleteDialogOpen = true" />
      </div>
    </div>

    <VRow>
      <!-- Info contatto -->
      <VCol cols="12" md="4">
        <VCard title="Informazioni">
          <VCardText>
            <div class="d-flex flex-column gap-3">
              <div>
                <span class="text-caption text-disabled">Nome completo</span>
                <p class="text-body-1 mb-0 font-weight-medium">{{ contatto.nome }} {{ contatto.cognome }}</p>
              </div>
              <div v-if="contatto.email">
                <span class="text-caption text-disabled">Email</span>
                <p class="text-body-1 mb-0">
                  <VIcon icon="tabler-mail" size="16" class="me-1" />{{ contatto.email }}
                </p>
              </div>
              <div v-if="contatto.cellulare">
                <span class="text-caption text-disabled">Cellulare</span>
                <p class="text-body-1 mb-0">
                  <VIcon icon="tabler-phone" size="16" class="me-1" />{{ contatto.cellulare }}
                </p>
              </div>
              <div v-if="contatto.telefono">
                <span class="text-caption text-disabled">Telefono</span>
                <p class="text-body-1 mb-0">
                  <VIcon icon="tabler-phone-call" size="16" class="me-1" />{{ contatto.telefono }}
                </p>
              </div>
              <div v-if="contatto.note">
                <span class="text-caption text-disabled">Note</span>
                <p class="text-body-2 mb-0">{{ contatto.note }}</p>
              </div>
              <VDivider />
              <div class="d-flex justify-space-between">
                <span class="text-caption text-disabled">Creato</span>
                <span class="text-body-2">{{ new Date(contatto.dataCreazione).toLocaleDateString('it-IT') }}</span>
              </div>
              <div v-if="contatto.dataModifica" class="d-flex justify-space-between">
                <span class="text-caption text-disabled">Modificato</span>
                <span class="text-body-2">{{ new Date(contatto.dataModifica).toLocaleDateString('it-IT') }}</span>
              </div>
            </div>
          </VCardText>
        </VCard>
      </VCol>

      <!-- Anagrafiche collegate -->
      <VCol cols="12" md="8">
        <VCard>
          <VCardItem>
            <VCardTitle class="d-flex align-center">
              <VIcon icon="tabler-building" size="20" class="me-2" />
              Anagrafiche collegate
              <VSpacer />
              <VChip size="x-small" label>{{ contatto.anagrafiche?.length ?? 0 }}</VChip>
            </VCardTitle>
          </VCardItem>

          <VCardText>
            <VTable v-if="contatto.anagrafiche && contatto.anagrafiche.length > 0">
              <thead>
                <tr>
                  <th>Denominazione</th>
                  <th>Ruolo</th>
                  <th>Principale</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="a in contatto.anagrafiche" :key="a.anagraficaId">
                  <td>
                    <RouterLink :to="`/anagrafiche/${a.anagraficaId}`" class="font-weight-medium">
                      {{ a.denominazione }}
                    </RouterLink>
                  </td>
                  <td>
                    <VChip size="small" label>{{ a.ruoloContattoNome }}</VChip>
                  </td>
                  <td>
                    <VIcon v-if="a.principale" icon="tabler-star-filled" color="warning" size="18" />
                  </td>
                </tr>
              </tbody>
            </VTable>
            <div v-else class="text-center text-disabled py-6">
              Nessuna anagrafica collegata
            </div>
          </VCardText>
        </VCard>
      </VCol>
    </VRow>
  </div>

  <div v-else-if="store.loading" class="d-flex justify-center py-12">
    <VProgressCircular indeterminate />
  </div>

  <!-- Delete Confirmation Dialog -->
  <VDialog v-model="deleteDialogOpen" max-width="400" persistent>
    <VCard title="Elimina Contatto">
      <VCardText>
        Sei sicuro di voler eliminare questo contatto? L'operazione non può essere annullata.
      </VCardText>
      <VCardActions>
        <VSpacer />
        <VBtn variant="text" @click="deleteDialogOpen = false">
          Annulla
        </VBtn>
        <VBtn
          color="error"
          :loading="deleteLoading"
          @click="confirmDelete"
        >
          Elimina
        </VBtn>
      </VCardActions>
    </VCard>
  </VDialog>

  <!-- Snackbar -->
  <VSnackbar
    v-model="snackbar"
    :color="snackbarColor"
    :timeout="5000"
  >
    {{ snackbarMessage }}
  </VSnackbar>
</template>
