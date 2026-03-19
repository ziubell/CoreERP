<script setup lang="ts">
import { useNotificheStore } from '@/stores/notifiche'

const notificheStore = useNotificheStore()
const isCurrentPasswordVisible = ref(false)
const isNewPasswordVisible = ref(false)
const isConfirmPasswordVisible = ref(false)
const isLoading = ref(false)

const passwordForm = ref({
  currentPassword: '',
  newPassword: '',
  confirmPassword: '',
})

// Microsoft account state
const microsoftStatus = ref({
  isLinked: false,
  microsoftEmail: null as string | null,
  dataCollegamento: null as string | null,
  hasPassword: true,
})
const isMicrosoftLoading = ref(false)
const showUnlinkDialog = ref(false)

const route = useRoute()

const loadMicrosoftStatus = async () => {
  try {
    const res = await $api('/profile/microsoft-status')
    microsoftStatus.value = {
      isLinked: res.isLinked,
      microsoftEmail: res.microsoftEmail,
      dataCollegamento: res.dataCollegamento,
      hasPassword: res.hasPassword,
    }
  }
  catch {
    // Silently fail - Microsoft section will show as not linked
  }
}

onMounted(async () => {
  await loadMicrosoftStatus()

  // Handle redirect from Microsoft link callback
  const msResult = route.query.microsoft as string
  if (msResult) {
    switch (msResult) {
      case 'linked':
        showSnackbar('Account Microsoft collegato con successo.', 'success')
        await loadMicrosoftStatus()
        break
      case 'link_failed':
        showSnackbar('Errore durante il collegamento dell\'account Microsoft.', 'error')
        break
      case 'already_linked_other':
        showSnackbar('Questo account Microsoft è già collegato a un altro utente.', 'error')
        break
      case 'auth_required':
        showSnackbar('Devi essere autenticato per collegare un account Microsoft.', 'error')
        break
    }
  }
})

const changePassword = async () => {
  if (passwordForm.value.newPassword !== passwordForm.value.confirmPassword) {
    showSnackbar('Le password non corrispondono.', 'error')

    return
  }

  isLoading.value = true
  try {
    await $api('/profile/password', {
      method: 'PUT',
      body: {
        currentPassword: passwordForm.value.currentPassword,
        newPassword: passwordForm.value.newPassword,
        confirmPassword: passwordForm.value.confirmPassword,
      },
    })

    showSnackbar('Password aggiornata con successo.', 'success')
    resetForm()
  }
  catch {
    showSnackbar('Password attuale non corretta o nuova password non valida.', 'error')
  }
  finally {
    isLoading.value = false
  }
}

const resetForm = () => {
  passwordForm.value = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: '',
  }
}

const linkMicrosoft = () => {
  isMicrosoftLoading.value = true
  window.location.href = `${import.meta.env.VITE_API_BASE_URL || '/api'}/profile/microsoft-link`
}

const unlinkMicrosoft = async () => {
  showUnlinkDialog.value = false
  isMicrosoftLoading.value = true

  try {
    await $api('/profile/microsoft-link', {
      method: 'DELETE',
    })

    showSnackbar('Account Microsoft scollegato con successo.', 'success')
    await loadMicrosoftStatus()
  }
  catch (err: any) {
    const message = err?.data?.message || 'Errore durante lo scollegamento dell\'account Microsoft.'
    showSnackbar(message, 'error')
  }
  finally {
    isMicrosoftLoading.value = false
  }
}

const formatDate = (dateStr: string | null) => {
  if (!dateStr) return ''

  return new Date(dateStr).toLocaleDateString('it-IT', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  })
}

const showSnackbar = (message: string, color: string) => {
  notificheStore.addToast(message, null, null, color)
}
</script>

<template>
  <VRow>
    <!-- Change Password Card -->
    <VCol cols="12">
      <VCard title="Cambia Password">
        <VCardText>
          <VAlert
            color="warning"
            variant="tonal"
            class="mb-6"
          >
            <VAlertTitle class="mb-1">
              Requisiti password
            </VAlertTitle>
            <span>Minimo 6 caratteri, almeno una lettera maiuscola, una minuscola e un numero.</span>
          </VAlert>

          <VForm @submit.prevent="changePassword">
            <VRow>
              <VCol
                cols="12"
                md="6"
              >
                <AppTextField
                  v-model="passwordForm.currentPassword"
                  label="Password attuale"
                  placeholder="Inserisci la password attuale"
                  :type="isCurrentPasswordVisible ? 'text' : 'password'"
                  autocomplete="current-password"
                  :append-inner-icon="isCurrentPasswordVisible ? 'tabler-eye-off' : 'tabler-eye'"
                  :rules="[requiredValidator]"
                  @click:append-inner="isCurrentPasswordVisible = !isCurrentPasswordVisible"
                />
              </VCol>
            </VRow>

            <VRow>
              <VCol
                cols="12"
                md="6"
              >
                <AppTextField
                  v-model="passwordForm.newPassword"
                  label="Nuova password"
                  placeholder="Inserisci la nuova password"
                  :type="isNewPasswordVisible ? 'text' : 'password'"
                  autocomplete="new-password"
                  :append-inner-icon="isNewPasswordVisible ? 'tabler-eye-off' : 'tabler-eye'"
                  :rules="[requiredValidator]"
                  @click:append-inner="isNewPasswordVisible = !isNewPasswordVisible"
                />
              </VCol>

              <VCol
                cols="12"
                md="6"
              >
                <AppTextField
                  v-model="passwordForm.confirmPassword"
                  label="Conferma nuova password"
                  placeholder="Conferma la nuova password"
                  :type="isConfirmPasswordVisible ? 'text' : 'password'"
                  autocomplete="new-password"
                  :append-inner-icon="isConfirmPasswordVisible ? 'tabler-eye-off' : 'tabler-eye'"
                  :rules="[requiredValidator, confirmedValidator(passwordForm.confirmPassword, passwordForm.newPassword)]"
                  @click:append-inner="isConfirmPasswordVisible = !isConfirmPasswordVisible"
                />
              </VCol>

              <VCol cols="12">
                <div class="d-flex gap-4">
                  <VBtn
                    type="submit"
                    color="primary"
                    prepend-icon="tabler-device-floppy"
                    :loading="isLoading"
                  >
                    Salva
                  </VBtn>

                </div>
              </VCol>
            </VRow>
          </VForm>
        </VCardText>
      </VCard>
    </VCol>

    <!-- Microsoft 365 Card -->
    <VCol cols="12">
      <VCard title="Account Microsoft 365">
        <VCardText>
          <!-- Linked state -->
          <div v-if="microsoftStatus.isLinked">
            <div class="d-flex align-center gap-3 mb-4">
              <VIcon
                icon="tabler-brand-windows"
                color="primary"
                size="32"
              />
              <div>
                <div class="d-flex align-center gap-2">
                  <VChip
                    color="success"
                    size="small"
                    variant="tonal"
                  >
                    Collegato
                  </VChip>
                </div>
                <div class="text-body-1 mt-1">
                  {{ microsoftStatus.microsoftEmail }}
                </div>
                <div
                  v-if="microsoftStatus.dataCollegamento"
                  class="text-body-2 text-medium-emphasis"
                >
                  Collegato il {{ formatDate(microsoftStatus.dataCollegamento) }}
                </div>
              </div>
            </div>

            <VAlert
              v-if="!microsoftStatus.hasPassword"
              color="warning"
              variant="tonal"
              class="mb-4"
            >
              <span>Devi prima impostare una password prima di poter scollegare l'account Microsoft.</span>
            </VAlert>

            <VBtn
              variant="outlined"
              color="error"
              :loading="isMicrosoftLoading"
              :disabled="!microsoftStatus.hasPassword"
              @click="showUnlinkDialog = true"
            >
              <VIcon
                start
                icon="tabler-unlink"
              />
              Scollega account
            </VBtn>
          </div>

          <!-- Not linked state -->
          <div v-else>
            <div class="d-flex align-center gap-3 mb-4">
              <VIcon
                icon="tabler-brand-windows"
                color="secondary"
                size="32"
              />
              <div>
                <VChip
                  color="secondary"
                  size="small"
                  variant="tonal"
                >
                  Non collegato
                </VChip>
                <div class="text-body-2 text-medium-emphasis mt-1">
                  Collega il tuo account Microsoft per accedere con un click.
                </div>
              </div>
            </div>

            <VBtn
              variant="outlined"
              color="primary"
              :loading="isMicrosoftLoading"
              @click="linkMicrosoft"
            >
              <VIcon
                start
                icon="tabler-brand-windows"
              />
              Collega account Microsoft
            </VBtn>
          </div>
        </VCardText>
      </VCard>
    </VCol>
  </VRow>

  <!-- Unlink confirmation dialog -->
  <VDialog
    v-model="showUnlinkDialog"
    max-width="450"
  >
    <VCard>
      <VCardTitle class="text-h5 pa-6">
        Scollega account Microsoft
      </VCardTitle>
      <VCardText>
        Sei sicuro di voler scollegare l'account Microsoft <strong>{{ microsoftStatus.microsoftEmail }}</strong>?
        <br><br>
        Non potrai più accedere tramite Microsoft 365 finché non lo collegherai di nuovo.
      </VCardText>
      <VCardText class="d-flex justify-end gap-4">
        <VBtn variant="tonal" color="secondary" @click="showUnlinkDialog = false">
          Annulla
        </VBtn>
        <VBtn color="error" @click="unlinkMicrosoft">
          Scollega
        </VBtn>
      </VCardText>
    </VCard>
  </VDialog>

</template>
