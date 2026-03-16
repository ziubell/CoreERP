<script setup lang="ts">
import type { PreferenzaNotificaApi, TipoNotificaApi } from '@/types/notifiche'
import { useNotificheStore } from '@/stores/notifiche'
import { $api } from '@/utils/api'

const store = useNotificheStore()
const snackbar = ref({ show: false, message: '', color: 'success' })
const loading = ref(true)
const saving = ref(false)

// Check if user has Microsoft linked (from profile/cookie)
const hasMicrosoft = ref(false)

interface PreferenzaRow {
  tipoNotificaId: number
  codice: string
  modulo: string
  descrizione: string
  email: boolean
  browser: boolean
  teams: boolean
}

const preferenzeRows = ref<PreferenzaRow[]>([])

// Group by module
const moduliRaggruppati = computed(() => {
  const grouped: Record<string, PreferenzaRow[]> = {}
  for (const row of preferenzeRows.value) {
    if (!grouped[row.modulo])
      grouped[row.modulo] = []
    grouped[row.modulo].push(row)
  }
  return grouped
})

onMounted(async () => {
  try {
    // Check if user has Microsoft account linked
    try {
      const profile = await $api<{ microsoftLinked: boolean }>('/profile/me')
      hasMicrosoft.value = profile.microsoftLinked
    }
    catch {
      hasMicrosoft.value = false
    }

    await Promise.all([
      store.fetchTipiNotifica(),
      store.fetchPreferenze(),
    ])

    // Build rows from notification types + user preferences
    preferenzeRows.value = store.tipiNotifica.map(tipo => {
      const pref = store.preferenze.find(p => p.tipoNotificaId === tipo.id)
      return {
        tipoNotificaId: tipo.id,
        codice: tipo.codice,
        modulo: tipo.modulo,
        descrizione: tipo.descrizione,
        email: pref?.email ?? true,
        browser: pref?.browser ?? true,
        teams: pref?.teams ?? false,
      }
    })
  }
  finally {
    loading.value = false
  }
})

const saveNotifications = async () => {
  saving.value = true
  try {
    const prefs: PreferenzaNotificaApi[] = preferenzeRows.value.map(row => ({
      tipoNotificaId: row.tipoNotificaId,
      email: row.email,
      browser: row.browser,
      teams: row.teams,
    }))
    await store.salvaPreferenze(prefs)
    showSnackbar('Preferenze notifiche salvate.', 'success')
  }
  catch {
    showSnackbar('Errore nel salvataggio delle preferenze.', 'error')
  }
  finally {
    saving.value = false
  }
}

const showSnackbar = (message: string, color: string) => {
  snackbar.value = { show: true, message, color }
}
</script>

<template>
  <VRow>
    <VCol cols="12">
      <VCard title="Notifiche">
        <VCardText>
          <div
            v-if="loading"
            class="d-flex justify-center pa-4"
          >
            <VProgressCircular indeterminate />
          </div>

          <template v-else-if="preferenzeRows.length > 0">
            <template
              v-for="(rows, modulo) in moduliRaggruppati"
              :key="modulo"
            >
              <h6 class="text-h6 mb-3 mt-4">
                {{ modulo }}
              </h6>

              <VTable class="text-no-wrap mb-4">
                <thead>
                  <tr>
                    <th scope="col">
                      Tipo
                    </th>
                    <th
                      scope="col"
                      class="text-center"
                    >
                      Email
                    </th>
                    <th
                      scope="col"
                      class="text-center"
                    >
                      Browser
                    </th>
                    <th
                      v-if="hasMicrosoft"
                      scope="col"
                      class="text-center"
                    >
                      Teams
                    </th>
                  </tr>
                </thead>
                <tbody>
                  <tr
                    v-for="row in rows"
                    :key="row.tipoNotificaId"
                  >
                    <td>{{ row.descrizione }}</td>
                    <td class="text-center">
                      <VCheckbox
                        v-model="row.email"
                        density="compact"
                        hide-details
                        class="d-inline-flex"
                      />
                    </td>
                    <td class="text-center">
                      <VCheckbox
                        v-model="row.browser"
                        density="compact"
                        hide-details
                        class="d-inline-flex"
                      />
                    </td>
                    <td
                      v-if="hasMicrosoft"
                      class="text-center"
                    >
                      <VCheckbox
                        v-model="row.teams"
                        density="compact"
                        hide-details
                        class="d-inline-flex"
                      />
                    </td>
                  </tr>
                </tbody>
              </VTable>
            </template>
          </template>

          <p
            v-else
            class="text-medium-emphasis pa-4"
          >
            Nessun tipo di notifica disponibile.
          </p>
        </VCardText>

        <VDivider />

        <VCardText>
          <div class="d-flex gap-4">
            <VBtn
              :loading="saving"
              @click="saveNotifications"
            >
              Salva modifiche
            </VBtn>
          </div>
        </VCardText>
      </VCard>
    </VCol>
  </VRow>

  <!-- Snackbar -->
  <VSnackbar
    v-model="snackbar.show"
    :color="snackbar.color"
    location="top end"
    :timeout="3000"
  >
    {{ snackbar.message }}
  </VSnackbar>
</template>
