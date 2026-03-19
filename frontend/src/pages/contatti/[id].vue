<script setup lang="ts">
import { useContattiStore } from '@/stores/contatti'
import { useNotificheStore } from '@/stores/notifiche'

const route = useRoute()
const router = useRouter()
const store = useContattiStore()
const notificheStore = useNotificheStore()

const id = computed(() => Number(route.params.id))
const contatto = computed(() => store.current)

// Delete dialog
const deleteDialogOpen = ref(false)
const deleteLoading = ref(false)

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
    notificheStore.addToast('Errore durante l\'eliminazione', error?.data?.message || error?.message || null, null, 'error')
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
          prepend-icon="tabler-edit"
          :to="`/contatti/modifica-${id}`"
        >
          Modifica
        </VBtn>

        <VBtn variant="tonal" color="secondary" append-icon="tabler-chevron-down">
          Azioni
          <VMenu activator="parent">
            <VList>
              <VListItem
                prepend-icon="tabler-trash"
                title="Elimina"
                class="text-error"
                @click="deleteDialogOpen = true"
              />
            </VList>
          </VMenu>
        </VBtn>
      </div>
    </div>

    <VRow>
      <!-- Info contatto -->
      <VCol cols="12" md="4">
        <VCard title="Dettagli">
          <VCardText>
            <div class="d-flex flex-column gap-3">
              <h6 class="text-h6">
                Nome completo:
                <span class="text-body-1 d-inline-block">{{ contatto.nome }} {{ contatto.cognome }}</span>
              </h6>
              <h6 v-if="contatto.email" class="text-h6">
                Email:
                <span class="text-body-1 d-inline-block">{{ contatto.email }}</span>
              </h6>
              <h6 v-if="contatto.cellulare" class="text-h6">
                Cellulare:
                <span class="text-body-1 d-inline-block">{{ contatto.cellulare }}</span>
              </h6>
              <h6 v-if="contatto.telefono" class="text-h6">
                Telefono:
                <span class="text-body-1 d-inline-block">{{ contatto.telefono }}</span>
              </h6>
              <h6 v-if="contatto.note" class="text-h6">
                Note:
                <span class="text-body-1 d-inline-block">{{ contatto.note }}</span>
              </h6>
              <h6 class="text-h6">
                Creato:
                <span class="text-body-1 d-inline-block">{{ new Date(contatto.dataCreazione).toLocaleDateString('it-IT') }}</span>
              </h6>
              <h6 v-if="contatto.dataModifica" class="text-h6">
                Modificato:
                <span class="text-body-1 d-inline-block">{{ new Date(contatto.dataModifica).toLocaleDateString('it-IT') }}</span>
              </h6>
            </div>
          </VCardText>
        </VCard>
      </VCol>

      <!-- Anagrafiche collegate -->
      <VCol cols="12" md="8">
        <VCard>
          <VCardItem>
            <VCardTitle class="d-flex align-center">
              Anagrafiche collegate
              <VSpacer />
              <VChip size="x-small" label>{{ contatto.anagrafiche?.length ?? 0 }}</VChip>
            </VCardTitle>
          </VCardItem>

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
          <VCardText v-else>
            <div class="text-center text-disabled py-6">
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
      <VCardText class="d-flex justify-end gap-4">
        <VBtn variant="tonal" color="secondary" @click="deleteDialogOpen = false">
          Annulla
        </VBtn>
        <VBtn
          color="error"
          prepend-icon="tabler-trash"
          :loading="deleteLoading"
          @click="confirmDelete"
        >
          Elimina
        </VBtn>
      </VCardText>
    </VCard>
  </VDialog>

</template>
