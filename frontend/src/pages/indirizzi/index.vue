<script setup lang="ts">
import { useIndirizziStore } from '@/stores/indirizzi'
import { TIPI_INDIRIZZO } from '@/types/indirizzo'

definePage({
  meta: {
    navActiveLink: 'indirizzi',
  },
})

const store = useIndirizziStore()
const ricerca = ref('')
const tipoFiltro = ref<string | undefined>()
const pagina = ref(1)
const dimensionePagina = ref(20)

const debouncedSearch = refDebounced(ricerca, 400)

watch([debouncedSearch, tipoFiltro], () => {
  pagina.value = 1
  loadData()
})

watch(pagina, loadData)

async function loadData() {
  await store.fetchList({
    tipo: tipoFiltro.value,
    ricerca: debouncedSearch.value || undefined,
    pagina: pagina.value,
    dimensionePagina: dimensionePagina.value,
  })
}

onMounted(loadData)

const totalPages = computed(() => Math.ceil(store.totalCount / dimensionePagina.value))
</script>

<template>
  <div>
    <VCard>
      <VCardTitle class="d-flex align-center">
        Indirizzi
        <VSpacer />
      </VCardTitle>

      <VCardText>
        <VRow>
          <VCol cols="12" sm="6" md="4">
            <AppTextField
              v-model="ricerca"
              placeholder="Cerca indirizzo, città, anagrafica..."
              prepend-inner-icon="tabler-search"
              clearable
            />
          </VCol>
          <VCol cols="12" sm="4" md="3">
            <AppSelect
              v-model="tipoFiltro"
              :items="[{ title: 'Tutti', value: undefined }, ...[...TIPI_INDIRIZZO].map(t => ({ title: t, value: t }))]"
              label="Tipo"
              clearable
            />
          </VCol>
        </VRow>
      </VCardText>

      <VTable class="text-no-wrap">
        <thead>
          <tr>
            <th>Anagrafica</th>
            <th>Tipo</th>
            <th>Indirizzo</th>
            <th>Città</th>
            <th>Prov.</th>
            <th>Rete</th>
            <th class="text-center">Principale</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="store.loading">
            <td colspan="7" class="text-center pa-4">
              <VProgressCircular indeterminate />
            </td>
          </tr>
          <tr v-else-if="store.items.length === 0">
            <td colspan="7" class="text-center text-medium-emphasis pa-4">
              Nessun indirizzo trovato.
            </td>
          </tr>
          <tr
            v-for="item in store.items"
            v-else
            :key="item.id"
            class="cursor-pointer"
            @click="$router.push(`/anagrafiche/${item.anagraficaId}`)"
          >
            <td>{{ item.anagraficaDenominazione }}</td>
            <td>
              <VChip size="small" :color="item.tipo === 'Impianto' ? 'info' : 'warning'" variant="tonal">
                {{ item.tipo }}
              </VChip>
              <VChip v-if="item.sottoTipo" size="small" class="ms-1" variant="tonal">
                {{ item.sottoTipo }}
              </VChip>
            </td>
            <td>{{ item.indirizzoCompleto }}</td>
            <td>{{ item.citta }}</td>
            <td>{{ item.provincia }}</td>
            <td>{{ item.rete ?? '-' }}</td>
            <td class="text-center">
              <VIcon v-if="item.principale" icon="tabler-check" color="success" size="20" />
            </td>
          </tr>
        </tbody>
      </VTable>

      <VCardText v-if="totalPages > 1" class="d-flex justify-center">
        <VPagination v-model="pagina" :length="totalPages" :total-visible="5" />
      </VCardText>
    </VCard>
  </div>
</template>
