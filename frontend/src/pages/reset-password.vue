<script setup lang="ts">
import { VForm } from 'vuetify/components/VForm'
import { VNodeRenderer } from '@layouts/components/VNodeRenderer'
import { themeConfig } from '@themeConfig'

definePage({
  meta: {
    layout: 'blank',
    unauthenticatedOnly: true,
    public: true,
  },
})

const route = useRoute()
const router = useRouter()

const isPasswordVisible = ref(false)
const isConfirmPasswordVisible = ref(false)
const isLoading = ref(false)
const isResetSuccess = ref(false)
const errorMessage = ref<string>()
const refVForm = ref<VForm>()

const form = ref({
  password: '',
  confirmPassword: '',
})

const token = computed(() => route.query.token as string || '')
const email = computed(() => route.query.email as string || '')

const resetPassword = async () => {
  isLoading.value = true
  errorMessage.value = undefined

  try {
    await $api('/auth/reset-password', {
      method: 'POST',
      body: {
        email: email.value,
        token: token.value,
        password: form.value.password,
        confirmPassword: form.value.confirmPassword,
      },
      onResponseError({ response }) {
        errorMessage.value = response._data?.message || 'Si è verificato un errore. Il link potrebbe essere scaduto.'
      },
    })

    isResetSuccess.value = true
  }
  catch (err) {
    console.error(err)
  }
  finally {
    isLoading.value = false
  }
}

const onSubmit = () => {
  refVForm.value?.validate()
    .then(({ valid: isValid }) => {
      if (isValid)
        resetPassword()
    })
}

const goToLogin = () => {
  router.push({ name: 'login' })
}
</script>

<template>
  <div class="auth-wrapper d-flex align-center justify-center pa-4">
    <VCard
      class="auth-card pa-4 pt-7"
      max-width="448"
    >
      <VCardItem class="justify-center">
        <RouterLink to="/">
          <img src="@/assets/images/logos/logo_bianco.png" alt="Logo" style="height: 36px; width: auto;">
        </RouterLink>
      </VCardItem>

      <VCardText v-if="!isResetSuccess" class="pt-2">
        <h4 class="text-h4 mb-1">
          Reimposta password
        </h4>
        <p class="mb-0">
          Inserisci la nuova password per il tuo account
        </p>
      </VCardText>

      <VCardText v-if="isResetSuccess">
        <VAlert
          type="success"
          variant="tonal"
          class="mb-4"
        >
          <VAlertTitle>Password reimpostata</VAlertTitle>
          <p class="mb-0">
            La tua password è stata aggiornata con successo. Puoi ora accedere con le nuove credenziali.
          </p>
        </VAlert>

        <VBtn
          block
          class="mt-4"
          @click="goToLogin"
        >
          Vai al login
        </VBtn>
      </VCardText>

      <VCardText v-if="errorMessage && !isResetSuccess">
        <VAlert
          type="error"
          variant="tonal"
          class="mb-4"
        >
          {{ errorMessage }}
        </VAlert>
      </VCardText>

      <VCardText v-if="!isResetSuccess">
        <VForm
          ref="refVForm"
          @submit.prevent="onSubmit"
        >
          <VRow>
            <VCol cols="12">
              <AppTextField
                v-model="form.password"
                autofocus
                label="Nuova password"
                placeholder="············"
                :type="isPasswordVisible ? 'text' : 'password'"
                autocomplete="new-password"
                :rules="[requiredValidator, passwordValidator]"
                :append-inner-icon="isPasswordVisible ? 'tabler-eye-off' : 'tabler-eye'"
                @click:append-inner="isPasswordVisible = !isPasswordVisible"
              />
            </VCol>

            <VCol cols="12">
              <AppTextField
                v-model="form.confirmPassword"
                label="Conferma password"
                placeholder="············"
                :type="isConfirmPasswordVisible ? 'text' : 'password'"
                autocomplete="new-password"
                :rules="[requiredValidator, confirmedValidator(form.confirmPassword, form.password)]"
                :append-inner-icon="isConfirmPasswordVisible ? 'tabler-eye-off' : 'tabler-eye'"
                @click:append-inner="isConfirmPasswordVisible = !isConfirmPasswordVisible"
              />
            </VCol>

            <VCol cols="12">
              <VBtn
                block
                type="submit"
                :loading="isLoading"
              >
                Reimposta password
              </VBtn>
            </VCol>

            <VCol cols="12">
              <RouterLink
                class="d-flex align-center justify-center"
                :to="{ name: 'login' }"
              >
                <VIcon
                  icon="tabler-chevron-left"
                  size="20"
                  class="me-1 flip-in-rtl"
                />
                <span>Torna al login</span>
              </RouterLink>
            </VCol>
          </VRow>
        </VForm>
      </VCardText>
    </VCard>
  </div>
</template>

<style lang="scss">
@use "@core/scss/template/pages/page-auth";
</style>
