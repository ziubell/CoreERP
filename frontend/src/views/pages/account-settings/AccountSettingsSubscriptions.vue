<script setup lang="ts">
import { $api } from '@/utils/api'
import FollowDialog from '@/components/common/FollowDialog.vue'
import { TIPO_ANAGRAFICA_LABELS } from '@/types/anagrafica'

interface SottoscrizioneItem {
  id: number
  entitaTipo: string
  entitaId: number
  denominazione: string
  tipo: number
  attivo: boolean
  dataSottoscrizione: string
  notificaModifiche: boolean
  notificaMessaggi: boolean
  notificaContatti: boolean
  notificaIndirizzi: boolean
}

const loading = ref(true)
const items = ref<SottoscrizioneItem[]>([])
const ricerca = ref('')
const tipoFiltro = ref<number | undefined>()

const debouncedRicerca = refDebounced(ricerca, 400)

const followDialogOpen = ref(false)
const selectedItem = ref<SottoscrizioneItem | null>(null)

async function loadData() {
  loading.value = true
  try {
    const params = new URLSearchParams()
    if (debouncedRicerca.value) params.set('ricerca', debouncedRicerca.value)
    items.value = await $api(`/v1/sottoscrizioni/me?${params}`)
  }
  finally {
    loading.value = false
  }
}

const filteredItems = computed(() => {
  let result = items.value
  if (tipoFiltro.value !== undefined) {
    result = result.filter(i => i.tipo === tipoFiltro.value)
  }
  return result
})

function openFollowDialog(item: SottoscrizioneItem) {
  selectedItem.value = item
  followDialogOpen.value = true
}

function getActivePrefs(item: SottoscrizioneItem): string[] {
  const prefs: string[] = []
  if (item.notificaModifiche) prefs.push('Modifiche')
  if (item.notificaMessaggi) prefs.push('Messaggi')
  if (item.notificaContatti) prefs.push('Contatti')
  if (item.notificaIndirizzi) prefs.push('Indirizzi')
  return prefs
}

watch(debouncedRicerca, loadData)
onMounted(loadData)
</script>

<template>
  <VCard>
    <VCardText>
      <VRow dense>
        <VCol cols="12" sm="6">
          <AppTextField
            v-model="ricerca"
            placeholder="Cerca anagrafica..."
            prepend-inner-icon="tabler-search"
            clearable
            density="compact"
          />
        </VCol>
        <VCol cols="12" sm="4">
          <AppSelect
            v-model="tipoFiltro"
            :items="[
              { title: 'Potenziale', value: 0 },
              { title: 'Cliente', value: 1 },
            ]"
            placeholder="Filtra per tipo..."
            clearable
            density="compact"
          />
        </VCol>
      </VRow>

      <div v-if="loading" class="d-flex justify-center py-6">
        <VProgressCircular indeterminate />
      </div>

      <template v-else-if="filteredItems.length > 0">
        <VTable class="text-no-wrap mt-4">
          <thead>
            <tr>
              <th>Anagrafica</th>
              <th>Tipo</th>
              <th>Notifiche attive</th>
              <th class="text-center" style="width: 80px;">Azioni</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="item in filteredItems"
              :key="item.id"
              class="cursor-pointer"
              @click="openFollowDialog(item)"
            >
              <td>
                <RouterLink :to="`/anagrafiche/${item.entitaId}`" class="font-weight-medium" @click.stop>
                  {{ item.denominazione }}
                </RouterLink>
              </td>
              <td>
                <VChip
                  :color="item.tipo === 1 ? 'primary' : 'secondary'"
                  size="small"
                  label
                >
                  {{ TIPO_ANAGRAFICA_LABELS[item.tipo] }}
                </VChip>
              </td>
              <td>
                <VChip
                  v-for="pref in getActivePrefs(item)"
                  :key="pref"
                  size="x-small"
                  class="me-1"
                  variant="tonal"
                >
                  {{ pref }}
                </VChip>
                <span v-if="getActivePrefs(item).length === 0" class="text-disabled">Nessuna</span>
              </td>
              <td class="text-center">
                <IconBtn size="small" @click.stop="openFollowDialog(item)">
                  <VIcon icon="tabler-settings" size="18" />
                </IconBtn>
              </td>
            </tr>
          </tbody>
        </VTable>
      </template>

      <div v-else class="text-center text-disabled py-6">
        Non stai seguendo nessuna anagrafica
      </div>
    </VCardText>
  </VCard>

  <!-- Follow Dialog -->
  <FollowDialog
    v-if="selectedItem"
    v-model="followDialogOpen"
    :entita-tipo="selectedItem.entitaTipo"
    :entita-id="selectedItem.entitaId"
    :entita-nome="selectedItem.denominazione"
    @updated="loadData"
  />
</template>
