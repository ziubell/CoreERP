<script setup lang="ts">
import { useAnagraficheStore } from '@/stores/anagrafiche'
import { TIPO_ANAGRAFICA_LABELS } from '@/types/anagrafica'
import { exportToCsv, exportToExcel, exportToPdf, printTable, copyTable } from '@/utils/tableExport'

const router = useRouter()
const store = useAnagraficheStore()

const ricerca = ref('')
const pagina = ref(1)
const dimensionePagina = ref(10)

const headers = [
  { title: 'Denominazione', key: 'denominazione', sortable: false },
  { title: 'Cod.', key: 'codiceCliente', sortable: false, width: '80px' },
  { title: 'Tipo', key: 'tipo', sortable: false, width: '120px' },
  { title: 'Stato', key: 'attivo', sortable: false, width: '100px' },
  { title: 'P.IVA', key: 'partitaIva', sortable: false, width: '140px' },
  { title: 'Città', key: 'citta', sortable: false, width: '130px' },
  { title: '', key: 'actions', sortable: false, width: '60px' },
]

async function loadData() {
  await store.fetchList({
    ricerca: ricerca.value || undefined,
    pagina: pagina.value,
    dimensionePagina: dimensionePagina.value,
  })
}

function goToDetail(id: number) {
  router.push(`/anagrafiche/${id}`)
}

watch(pagina, loadData)

watch(dimensionePagina, () => {
  pagina.value = 1
  loadData()
})

let searchTimeout: ReturnType<typeof setTimeout>
watch(ricerca, () => {
  clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    pagina.value = 1
    loadData()
  }, 300)
})

onMounted(loadData)

// Export
const exportColumns = [
  { key: 'codiceCliente', title: 'Codice' },
  { key: 'denominazione', title: 'Denominazione' },
  { key: 'tipoLabel', title: 'Tipo' },
  { key: 'statoLabel', title: 'Stato' },
  { key: 'partitaIva', title: 'P.IVA' },
  { key: 'citta', title: 'Città' },
  { key: 'telefono', title: 'Telefono' },
]

function getExportData() {
  return store.items.map(item => ({
    ...item,
    codiceCliente: item.codiceCliente ?? '—',
    tipoLabel: TIPO_ANAGRAFICA_LABELS[item.tipo],
    statoLabel: item.attivo ? 'Attivo' : 'Non attivo',
  }))
}

function handleExport(type: string) {
  const data = getExportData()
  const filename = 'Anagrafiche'

  switch (type) {
    case 'print':
      printTable(data, exportColumns, filename)
      break
    case 'csv':
      exportToCsv(data, exportColumns, filename)
      break
    case 'excel':
      exportToExcel(data, exportColumns, filename)
      break
    case 'pdf':
      exportToPdf(data, exportColumns, filename)
      break
    case 'copy':
      copyTable(data, exportColumns)
      break
  }
}
</script>

<template>
  <div>
    <VCard>
      <!-- Toolbar -->
      <VCardText class="d-flex align-center flex-wrap gap-4">
        <AppTextField
          v-model="ricerca"
          placeholder="Cerca anagrafica..."
          prepend-inner-icon="tabler-search"
          style="max-inline-size: 250px;"
          clearable
        />

        <VSpacer />

        <div class="d-flex align-center gap-4 flex-wrap">
          <AppSelect
            v-model="dimensionePagina"
            :items="[10, 20, 50]"
            style="max-inline-size: 100px;"
          />

          <!-- Export Menu -->
          <VBtn
            variant="tonal"
            color="secondary"
            prepend-icon="tabler-upload"
            append-icon="tabler-chevron-down"
          >
            Export
            <VMenu activator="parent">
              <VList>
                <VListItem
                  prepend-icon="tabler-printer"
                  title="Print"
                  @click="handleExport('print')"
                />
                <VListItem
                  prepend-icon="tabler-file-text"
                  title="Csv"
                  @click="handleExport('csv')"
                />
                <VListItem
                  prepend-icon="tabler-file-spreadsheet"
                  title="Excel"
                  @click="handleExport('excel')"
                />
                <VListItem
                  prepend-icon="tabler-file-description"
                  title="Pdf"
                  @click="handleExport('pdf')"
                />
                <VListItem
                  prepend-icon="tabler-copy"
                  title="Copy"
                  @click="handleExport('copy')"
                />
              </VList>
            </VMenu>
          </VBtn>

          <VBtn
            color="primary"
            prepend-icon="tabler-plus"
            :to="{ path: '/anagrafiche/nuovo' }"
          >
            Nuova Anagrafica
          </VBtn>
        </div>
      </VCardText>

      <VDivider />

      <!-- DataTable -->
      <VDataTableServer
        :headers="headers"
        :items="store.items"
        :items-length="store.totalCount"
        :loading="store.loading"
        :page="pagina"
        :items-per-page="dimensionePagina"
        @update:page="pagina = $event"
        @update:items-per-page="dimensionePagina = $event"
        class="text-no-wrap"
      >
        <!-- Denominazione with avatar -->
        <template #item.denominazione="{ item }">
          <div class="d-flex align-center gap-x-3 py-2">
            <VAvatar
              size="34"
              :color="item.tipo === 1 ? 'primary' : 'secondary'"
              variant="tonal"
            >
              <VIcon
                :icon="item.tipoSoggetto === 1 ? 'tabler-building' : 'tabler-user'"
                size="22"
              />
            </VAvatar>
            <div>
              <a
                class="text-body-1 font-weight-medium text-high-emphasis cursor-pointer"
                @click="goToDetail(item.id)"
              >
                {{ item.denominazione }}
              </a>
            </div>
          </div>
        </template>

        <!-- Codice Cliente -->
        <template #item.codiceCliente="{ item }">
          <span v-if="item.codiceCliente" class="font-weight-medium">#{{ item.codiceCliente }}</span>
          <span v-else class="text-disabled">—</span>
        </template>

        <!-- Tipo -->
        <template #item.tipo="{ item }">
          <VChip
            :color="item.tipo === 1 ? 'primary' : 'secondary'"
            size="small"
            label
          >
            {{ TIPO_ANAGRAFICA_LABELS[item.tipo] }}
          </VChip>
        </template>

        <!-- Stato -->
        <template #item.attivo="{ item }">
          <VChip
            :color="item.attivo ? 'success' : 'error'"
            size="small"
            label
          >
            {{ item.attivo ? 'Attivo' : 'Non attivo' }}
          </VChip>
        </template>

        <!-- P.IVA -->
        <template #item.partitaIva="{ item }">
          <span v-if="item.partitaIva">{{ item.partitaIva }}</span>
          <span v-else class="text-disabled">—</span>
        </template>

        <!-- Città -->
        <template #item.citta="{ item }">
          <span v-if="item.citta">{{ item.citta }}</span>
          <span v-else class="text-disabled">—</span>
        </template>

        <!-- Actions -->
        <template #item.actions="{ item }">
          <IconBtn @click="goToDetail(item.id)">
            <VIcon icon="tabler-eye" />
          </IconBtn>
        </template>

        <!-- No data -->
        <template #no-data>
          <div class="text-center py-6 text-disabled">
            <VIcon icon="tabler-database-off" size="48" class="mb-2" />
            <p class="text-body-1">Nessuna anagrafica trovata</p>
          </div>
        </template>
      </VDataTableServer>
    </VCard>
  </div>
</template>
