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

const isPasswordVisible = ref(false)
const isLoading = ref(false)
const isMicrosoftLoading = ref(false)

const route = useRoute()
const router = useRouter()
const ability = useAbility()

const errors = ref<Record<string, string | undefined>>({
  email: undefined,
  password: undefined,
})

const refVForm = ref<VForm>()

const credentials = ref({
  email: '',
  password: '',
})

const rememberMe = ref(false)

const handleLoginSuccess = async (res: any) => {
  const { accessToken, userData, userAbilityRules } = res

  useCookie('userAbilityRules').value = userAbilityRules
  ability.update(userAbilityRules)

  useCookie('userData').value = userData
  useCookie('accessToken').value = accessToken

  await nextTick(() => {
    router.replace(route.query.to ? String(route.query.to) : '/')
  })
}

const login = async () => {
  isLoading.value = true
  errors.value = { email: undefined, password: undefined }

  try {
    const res = await $api('/auth/login', {
      method: 'POST',
      body: {
        email: credentials.value.email,
        password: credentials.value.password,
      },
      onResponseError({ response }) {
        console.error('Login response error:', response.status, response._data)
        if (response._data?.errors) {
          errors.value = response._data.errors
        }
        else if (response._data?.message) {
          errors.value = { email: response._data.message }
        }
      },
    })

    await handleLoginSuccess(res)
  }
  catch (err: any) {
    console.error('Login error:', err?.data || err?.message || err)
  }
  finally {
    isLoading.value = false
  }
}

const loginWithMicrosoft = async () => {
  isMicrosoftLoading.value = true

  try {
    // Redirect to backend Microsoft OAuth endpoint
    window.location.href = `${import.meta.env.VITE_API_BASE_URL || '/api'}/auth/microsoft-login?returnUrl=${encodeURIComponent(route.query.to ? String(route.query.to) : '/')}`
  }
  catch (err) {
    console.error(err)
    isMicrosoftLoading.value = false
  }
}

const onSubmit = () => {
  refVForm.value?.validate()
    .then(({ valid: isValid }) => {
      if (isValid)
        login()
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
          <img src="@/assets/images/logos/logo_bianco.png" alt="Logo" style="height: 36px; width: auto;">
        </RouterLink>
      </VCardItem>

      <VCardText class="pt-2">
        <h4 class="text-h4 mb-1">
          Benvenuto
        </h4>
        <p class="mb-0">
          Accedi al tuo account per continuare
        </p>
      </VCardText>

      <VCardText>
        <VForm
          ref="refVForm"
          @submit.prevent="onSubmit"
        >
          <VRow>
            <VCol cols="12">
              <AppTextField
                v-model="credentials.email"
                label="Email"
                placeholder="nome@azienda.it"
                type="email"
                autofocus
                :rules="[requiredValidator, emailValidator]"
                :error-messages="errors.email"
              />
            </VCol>

            <VCol cols="12">
              <AppTextField
                v-model="credentials.password"
                label="Password"
                placeholder="············"
                :rules="[requiredValidator]"
                :type="isPasswordVisible ? 'text' : 'password'"
                autocomplete="current-password"
                :error-messages="errors.password"
                :append-inner-icon="isPasswordVisible ? 'tabler-eye-off' : 'tabler-eye'"
                @click:append-inner="isPasswordVisible = !isPasswordVisible"
              />

              <div class="d-flex align-center flex-wrap justify-space-between my-6">
                <VCheckbox
                  v-model="rememberMe"
                  label="Ricordami"
                />
                <RouterLink
                  class="text-primary ms-2 mb-1"
                  :to="{ name: 'forgot-password' }"
                >
                  Password dimenticata?
                </RouterLink>
              </div>

              <VBtn
                block
                type="submit"
                :loading="isLoading"
              >
                Accedi
              </VBtn>
            </VCol>

            <VCol
              cols="12"
              class="d-flex align-center"
            >
              <VDivider />
              <span class="mx-4 text-high-emphasis">oppure</span>
              <VDivider />
            </VCol>

            <VCol cols="12">
              <VBtn
                block
                variant="outlined"
                color="secondary"
                :loading="isMicrosoftLoading"
                @click="loginWithMicrosoft"
              >
                <VIcon
                  start
                  icon="tabler-brand-windows"
                />
                Accedi con Microsoft 365
              </VBtn>
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
