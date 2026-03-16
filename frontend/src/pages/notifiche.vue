<script setup lang="ts">
import { useNotificheStore } from '@/stores/notifiche'

const store = useNotificheStore()
const router = useRouter()

const filtroNonLette = ref(false)
const pagina = ref(1)

onMounted(() => {
  store.fetchNotifiche(false, 1)
})

watch(filtroNonLette, () => {
  pagina.value = 1
  store.fetchNotifiche(filtroNonLette.value, 1)
})

const caricaAltro = () => {
  pagina.value++
  store.fetchNotifiche(filtroNonLette.value, pagina.value)
}

const clickNotifica = (notifica: typeof store.notifiche[0]) => {
  if (!notifica.letta)
    store.segnaComeLetta(notifica.id)

  if (notifica.link)
    router.push(notifica.link)
}

const eliminaNotifica = (id: number) => {
  store.elimina(id)
}

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
        <VCardTitle class="d-flex align-center justify-space-between pa-4">
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

        <VDivider />

        <VCardText class="pa-2">
          <VSwitch
            v-model="filtroNonLette"
            label="Solo non lette"
            density="compact"
            hide-details
            class="ms-2"
          />
        </VCardText>

        <VDivider />

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
</template>
