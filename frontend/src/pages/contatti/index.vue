<script setup lang="ts">
import { useContattiStore } from '@/stores/contatti'

definePage({ meta: { navActiveLink: 'contatti' } })

const router = useRouter()
const store = useContattiStore()

const ricerca = ref('')
const pagina = ref(1)
const dimensionePagina = ref(20)

const headers = [
  { title: 'Nome', key: 'nome', sortable: false },
  { title: 'Cognome', key: 'cognome', sortable: false },
  { title: 'Email', key: 'email', sortable: false },
  { title: 'Cellulare', key: 'cellulare', sortable: false, width: '140px' },
  { title: 'Telefono', key: 'telefono', sortable: false, width: '140px' },
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
  router.push(`/contatti/${id}`)
}

watch(pagina, loadData)

let searchTimeout: ReturnType<typeof setTimeout>
watch(ricerca, () => {
  clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    pagina.value = 1
    loadData()
  }, 300)
})

onMounted(loadData)
</script>

<template>
  <div>
    <VCard>
      <VCardItem>
        <VCardTitle class="d-flex align-center">
          Contatti
          <VSpacer />
          <VBtn
            color="primary"
            prepend-icon="tabler-plus"
            :to="{ path: '/contatti/nuovo' }"
          >
            Nuovo Contatto
          </VBtn>
        </VCardTitle>
      </VCardItem>

      <VDivider />

      <VCardText>
        <VRow>
          <VCol cols="12" md="6">
            <AppTextField
              v-model="ricerca"
              placeholder="Cerca per nome, cognome, email, cellulare..."
              prepend-inner-icon="tabler-search"
              clearable
            />
          </VCol>
        </VRow>
      </VCardText>

      <VDivider />

      <VDataTableServer
        :headers="headers"
        :items="store.items"
        :items-length="store.totalCount"
        :loading="store.loading"
        :page="pagina"
        :items-per-page="dimensionePagina"
        class="text-no-wrap"
        @update:page="pagina = $event"
        @update:items-per-page="dimensionePagina = $event; loadData()"
      >
        <template #item.nome="{ item }">
          <a
            class="text-body-1 font-weight-medium text-high-emphasis cursor-pointer"
            @click="goToDetail(item.id)"
          >
            {{ item.nome }}
          </a>
        </template>

        <template #item.email="{ item }">
          <span v-if="item.email">
            <VIcon icon="tabler-mail" size="14" class="me-1" />{{ item.email }}
          </span>
          <span v-else class="text-disabled">-</span>
        </template>

        <template #item.actions="{ item }">
          <IconBtn @click="goToDetail(item.id)">
            <VIcon icon="tabler-eye" />
          </IconBtn>
        </template>

        <template #no-data>
          <div class="text-center py-6 text-disabled">
            <VIcon icon="tabler-database-off" size="48" class="mb-2" />
            <p class="text-body-1">Nessun contatto trovato</p>
          </div>
        </template>
      </VDataTableServer>
    </VCard>
  </div>
</template>
