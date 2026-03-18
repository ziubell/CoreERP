<script setup lang="ts">
import { useNotificheStore } from '@/stores/notifiche'

const store = useNotificheStore()
const router = useRouter()

const filtroNonLette = ref(false)
const ricerca = ref('')
const moduloFiltro = ref('')
const pagina = ref(1)
const selezionati = ref<number[]>([])
const dialogResetVisible = ref(false)

// Debounce search
let searchTimeout: ReturnType<typeof setTimeout> | null = null
const ricercaDebounced = ref('')

watch(ricerca, (val) => {
  if (searchTimeout) clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    ricercaDebounced.value = val
  }, 300)
})

// Fetch notification types for module filter
onMounted(async () => {
  await store.fetchTipiNotifica()
  caricaNotifiche()
})

// Distinct modules from notification types
const moduli = computed(() => {
  const set = new Set(store.tipiNotifica.map(t => t.modulo))
  return Array.from(set).sort()
})

// Reload on filter changes
watch([filtroNonLette, ricercaDebounced, moduloFiltro], () => {
  pagina.value = 1
  selezionati.value = []
  caricaNotifiche()
})

function caricaNotifiche() {
  store.fetchNotifiche(
    filtroNonLette.value,
    pagina.value,
    ricercaDebounced.value || undefined,
    moduloFiltro.value || undefined,
  )
}

const caricaAltro = () => {
  pagina.value++
  store.fetchNotifiche(
    filtroNonLette.value,
    pagina.value,
    ricercaDebounced.value || undefined,
    moduloFiltro.value || undefined,
  )
}

const clickNotifica = (notifica: typeof store.notifiche[0]) => {
  if (!notifica.letta)
    store.segnaComeLetta(notifica.id)

  if (notifica.link)
    router.push(notifica.link)
}

const eliminaNotifica = (id: number) => {
  store.elimina(id)
  selezionati.value = selezionati.value.filter(s => s !== id)
}

const eliminaSelezionati = async () => {
  if (selezionati.value.length === 0) return
  await store.eliminaMultiple(selezionati.value)
  selezionati.value = []
}

const resetNotifiche = async () => {
  await store.eliminaTutte()
  selezionati.value = []
  dialogResetVisible.value = false
}

const tuttiSelezionati = computed({
  get: () => store.notifiche.length > 0 && selezionati.value.length === store.notifiche.length,
  set: (val: boolean) => {
    selezionati.value = val ? store.notifiche.map(n => n.id) : []
  },
})

const alcuniSelezionati = computed(() => {
  return selezionati.value.length > 0 && selezionati.value.length < store.notifiche.length
})

function formatRelativeTime(dateStr: string): string {
  const date = new Date(dateStr)
  const now = new Date()
  const diffMs = now.getTime() - date.getTime()
  const diffMins = Math.floor(diffMs / 60000)
  const diffHours = Math.floor(diffMs / 3600000)
  const diffDays = Math.floor(diffMs / 86400000)

  if (diffMins < 1) return 'Adesso'
  if (diffMins < 60) return `${diffMins} min fa`
  if (diffHours < 24) return `${diffHours} ore fa`
  if (diffDays < 7) return `${diffDays} giorni fa`

  return date.toLocaleDateString('it-IT')
}
</script>

<template>
  <VRow>
    <VCol cols="12">
      <VCard>
        <VCardItem>
          <VCardTitle class="d-flex align-center justify-space-between">
            <span class="text-h5">Notifiche</span>
            <div class="d-flex gap-2">
              <VBtn
                v-if="store.nonLette > 0"
                variant="tonal"
                size="small"
                @click="store.segnaTutteComeLette()"
              >
                Segna tutte come lette
              </VBtn>
              <VChip
                v-if="store.nonLette > 0"
                color="primary"
                size="small"
              >
                {{ store.nonLette }} non lette
              </VChip>
            </div>
          </VCardTitle>
        </VCardItem>

        <VDivider />

        <!-- Filters -->
        <VCardText class="pa-4">
          <VRow
            dense
            align="center"
          >
            <VCol
              cols="12"
              sm="4"
            >
              <VTextField
                v-model="ricerca"
                prepend-inner-icon="tabler-search"
                placeholder="Cerca notifiche..."
                density="compact"
                hide-details
                clearable
                @click:clear="ricerca = ''"
              />
            </VCol>
            <VCol
              cols="12"
              sm="3"
            >
              <VSelect
                v-model="moduloFiltro"
                :items="[{ title: 'Tutti i moduli', value: '' }, ...moduli.map(m => ({ title: m, value: m }))]"
                density="compact"
                hide-details
              />
            </VCol>
            <VCol
              cols="12"
              sm="3"
            >
              <VSwitch
                v-model="filtroNonLette"
                label="Solo non lette"
                density="compact"
                hide-details
              />
            </VCol>
          </VRow>
        </VCardText>

        <VDivider />

        <!-- Bulk actions -->
        <VCardText
          v-if="store.notifiche.length > 0"
          class="pa-2 d-flex align-center gap-2"
        >
          <VCheckbox
            :model-value="tuttiSelezionati"
            :indeterminate="alcuniSelezionati"
            density="compact"
            hide-details
            class="ms-2"
            @update:model-value="tuttiSelezionati = $event as boolean"
          />
          <span class="text-body-2 text-medium-emphasis">
            {{ selezionati.length > 0 ? `${selezionati.length} selezionate` : 'Seleziona tutto' }}
          </span>
          <VSpacer />
          <VBtn
            v-if="selezionati.length > 0"
            variant="tonal"
            color="error"
            size="small"
            prepend-icon="tabler-trash"
            @click="eliminaSelezionati"
          >
            Elimina selezionate
          </VBtn>
          <VBtn
            variant="tonal"
            color="error"
            size="small"
            prepend-icon="tabler-trash-x"
            @click="dialogResetVisible = true"
          >
            Cancella tutte
          </VBtn>
        </VCardText>

        <VDivider v-if="store.notifiche.length > 0" />

        <!-- Notification list -->
        <VList
          v-if="store.notifiche.length > 0"
          lines="two"
        >
          <template
            v-for="(notifica, index) in store.notifiche"
            :key="notifica.id"
          >
            <VDivider v-if="index > 0" />
            <VListItem
              :class="{ 'bg-grey-lighten-4': !notifica.letta }"
              class="cursor-pointer"
              @click="clickNotifica(notifica)"
            >
              <template #prepend>
                <VCheckbox
                  :model-value="selezionati.includes(notifica.id)"
                  density="compact"
                  hide-details
                  class="me-2"
                  @update:model-value="(val: any) => {
                    if (val)
                      selezionati.push(notifica.id)
                    else
                      selezionati = selezionati.filter(s => s !== notifica.id)
                  }"
                  @click.stop
                />
                <VAvatar
                  v-if="notifica.mittenteAvatar"
                  :image="notifica.mittenteAvatar"
                />
                <VAvatar
                  v-else-if="notifica.mittenteNome"
                  color="primary"
                  variant="tonal"
                >
                  {{ notifica.mittenteNome.charAt(0) }}
                </VAvatar>
                <VAvatar
                  v-else
                  :color="notifica.tipoNotifica.colore ?? 'primary'"
                  variant="tonal"
                >
                  <VIcon :icon="notifica.tipoNotifica.icona ?? 'tabler-bell'" />
                </VAvatar>
              </template>

              <VListItemTitle :class="{ 'font-weight-bold': !notifica.letta }">
                {{ notifica.titolo }}
              </VListItemTitle>
              <VListItemSubtitle>
                {{ notifica.messaggio ?? notifica.tipoNotifica.descrizione }}
              </VListItemSubtitle>

              <template #append>
                <div class="d-flex flex-column align-end gap-1">
                  <span class="text-caption text-disabled">{{ formatRelativeTime(notifica.dataCreazione) }}</span>
                  <div class="d-flex align-center gap-1">
                    <VIcon
                      v-if="!notifica.letta"
                      icon="tabler-circle-filled"
                      color="primary"
                      size="10"
                    />
                    <VBtn
                      icon="tabler-x"
                      size="x-small"
                      variant="text"
                      @click.stop="eliminaNotifica(notifica.id)"
                    />
                  </div>
                </div>
              </template>
            </VListItem>
          </template>
        </VList>

        <VCardText
          v-else
          class="text-center text-medium-emphasis py-8"
        >
          Nessuna notifica.
        </VCardText>

        <VDivider />

        <VCardText
          v-if="store.notifiche.length >= 20"
          class="text-center"
        >
          <VBtn
            variant="tonal"
            :loading="store.loading"
            @click="caricaAltro"
          >
            Carica altre
          </VBtn>
        </VCardText>
      </VCard>
    </VCol>
  </VRow>

  <!-- Reset dialog -->
  <VDialog
    v-model="dialogResetVisible"
    max-width="400"
  >
    <VCard>
      <VCardTitle>Cancella tutte le notifiche</VCardTitle>
      <VCardText>
        Sei sicuro di voler eliminare tutte le notifiche? Questa azione non può essere annullata.
      </VCardText>
      <VCardActions>
        <VSpacer />
        <VBtn
          variant="text"
          @click="dialogResetVisible = false"
        >
          Annulla
        </VBtn>
        <VBtn
          color="error"
          variant="elevated"
          @click="resetNotifiche"
        >
          Cancella tutte
        </VBtn>
      </VCardActions>
    </VCard>
  </VDialog>
</template>
