<script setup lang="ts">
const isCurrentPasswordVisible = ref(false)
const isNewPasswordVisible = ref(false)
const isConfirmPasswordVisible = ref(false)
const isLoading = ref(false)
const snackbar = ref({ show: false, message: '', color: 'success' })

const passwordForm = ref({
  currentPassword: '',
  newPassword: '',
  confirmPassword: '',
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

const showSnackbar = (message: string, color: string) => {
  snackbar.value = { show: true, message, color }
}
</script>

<template>
  <VRow>
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
                    :loading="isLoading"
                  >
                    Aggiorna password
                  </VBtn>

                  <VBtn
                    variant="tonal"
                    color="secondary"
                    @click="resetForm"
                  >
                    Annulla
                  </VBtn>
                </div>
              </VCol>
            </VRow>
          </VForm>
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
