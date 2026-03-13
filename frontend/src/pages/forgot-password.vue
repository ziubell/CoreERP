<script setup lang="ts">
import { VForm } from 'vuetify/components/VForm'
import { VNodeRenderer } from '@layouts/components/VNodeRenderer'
import { themeConfig } from '@themeConfig'

definePage({
  meta: {
    layout: 'blank',
    unauthenticatedOnly: true,
  },
})

const email = ref('')
const isLoading = ref(false)
const isEmailSent = ref(false)
const errorMessage = ref<string>()
const refVForm = ref<VForm>()

const sendResetLink = async () => {
  isLoading.value = true
  errorMessage.value = undefined

  try {
    await $api('/auth/forgot-password', {
      method: 'POST',
      body: { email: email.value },
      onResponseError({ response }) {
        errorMessage.value = response._data?.message || 'Si è verificato un errore. Riprova.'
      },
    })

    isEmailSent.value = true
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
        sendResetLink()
    })
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
          <div class="d-flex align-center gap-x-3">
            <VNodeRenderer :nodes="themeConfig.app.logo" />
            <h1 class="auth-title">
              {{ themeConfig.app.title }}
            </h1>
          </div>
        </RouterLink>
      </VCardItem>

      <VCardText v-if="!isEmailSent" class="pt-2">
        <h4 class="text-h4 mb-1">
          Password dimenticata?
        </h4>
        <p class="mb-0">
          Inserisci la tua email e ti invieremo le istruzioni per reimpostare la password
        </p>
      </VCardText>

      <VCardText v-if="isEmailSent">
        <VAlert
          type="success"
          variant="tonal"
          class="mb-4"
        >
          <VAlertTitle>Email inviata</VAlertTitle>
          <p class="mb-0">
            Se l'indirizzo <strong>{{ email }}</strong> è associato a un account, riceverai un'email con le istruzioni per reimpostare la password.
          </p>
        </VAlert>
      </VCardText>

      <VCardText v-if="!isEmailSent">
        <VForm
          ref="refVForm"
          @submit.prevent="onSubmit"
        >
          <VRow>
            <VCol cols="12">
              <AppTextField
                v-model="email"
                autofocus
                label="Email"
                type="email"
                placeholder="nome@azienda.it"
                :rules="[requiredValidator, emailValidator]"
                :error-messages="errorMessage"
              />
            </VCol>

            <VCol cols="12">
              <VBtn
                block
                type="submit"
                :loading="isLoading"
              >
                Invia link di recupero
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

      <VCardText v-if="isEmailSent">
        <VRow>
          <VCol cols="12">
            <VBtn
              block
              variant="outlined"
              @click="isEmailSent = false"
            >
              Invia di nuovo
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
      </VCardText>
    </VCard>
  </div>
</template>

<style lang="scss">
@use "@core/scss/template/pages/page-auth";
</style>
