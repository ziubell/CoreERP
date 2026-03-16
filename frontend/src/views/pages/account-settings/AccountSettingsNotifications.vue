<script setup lang="ts">
const snackbar = ref({ show: false, message: '', color: 'success' })

const notifications = ref([
  {
    type: 'Nuovi aggiornamenti',
    email: true,
    browser: true,
    app: true,
  },
  {
    type: 'Attivita\u0300 account',
    email: true,
    browser: false,
    app: false,
  },
  {
    type: 'Nuovo accesso da browser',
    email: true,
    browser: true,
    app: true,
  },
  {
    type: 'Nuovo dispositivo collegato',
    email: true,
    browser: false,
    app: true,
  },
])

const notificationTiming = ref('online')

const saveNotifications = () => {
  showSnackbar('Preferenze notifiche salvate.', 'success')
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
          <VTable class="text-no-wrap">
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
                  scope="col"
                  class="text-center"
                >
                  App
                </th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="(item, index) in notifications"
                :key="index"
              >
                <td>{{ item.type }}</td>
                <td class="text-center">
                  <VCheckbox
                    v-model="item.email"
                    density="compact"
                    hide-details
                    class="d-inline-flex"
                  />
                </td>
                <td class="text-center">
                  <VCheckbox
                    v-model="item.browser"
                    density="compact"
                    hide-details
                    class="d-inline-flex"
                  />
                </td>
                <td class="text-center">
                  <VCheckbox
                    v-model="item.app"
                    density="compact"
                    hide-details
                    class="d-inline-flex"
                  />
                </td>
              </tr>
            </tbody>
          </VTable>
        </VCardText>

        <VDivider />

        <VCardText>
          <h6 class="text-h6 mb-4">
            Quando inviare le notifiche?
          </h6>

          <VRadioGroup
            v-model="notificationTiming"
            inline
          >
            <VRadio
              label="Solo quando sono online"
              value="online"
            />
            <VRadio
              label="In qualsiasi momento"
              value="anytime"
            />
          </VRadioGroup>
        </VCardText>

        <VCardText>
          <div class="d-flex gap-4">
            <VBtn @click="saveNotifications">
              Salva modifiche
            </VBtn>
            <VBtn
              variant="tonal"
              color="secondary"
            >
              Annulla
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
