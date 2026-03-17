<script setup lang="ts">
import Cropper from 'cropperjs'
import 'cropperjs/dist/cropper.css'
import { useNotificheStore } from '@/stores/notifiche'

const notificheStore = useNotificheStore()
const userData = useCookie<any>('userData')
const isLoading = ref(false)
const isPhotoUploading = ref(false)

// Crop modal
const showCropDialog = ref(false)
const cropImageSrc = ref('')
const cropperRef = ref<HTMLImageElement>()
let cropperInstance: Cropper | null = null

const profileData = ref({
  nome: '',
  cognome: '',
  cellulare: '',
  email: '',
  ruolo: '',
  codiceAgente: '',
  foto: '',
  dipendente: false,
  reperibile: false,
})

const fileInput = ref<HTMLInputElement>()

const initials = computed(() => {
  const n = profileData.value.nome?.[0] ?? ''
  const c = profileData.value.cognome?.[0] ?? ''

  return (n + c).toUpperCase() || '?'
})

const fetchProfile = async () => {
  try {
    const res = await $api('/profile/me')
    profileData.value = {
      nome: res.nome ?? '',
      cognome: res.cognome ?? '',
      cellulare: res.cellulare ?? '',
      email: res.email ?? '',
      ruolo: res.ruolo ?? '',
      codiceAgente: res.codiceAgente ?? '',
      foto: res.foto ?? '',
      dipendente: res.dipendente ?? false,
      reperibile: res.reperibile ?? false,
    }
  }
  catch (err) {
    console.error('Errore caricamento profilo:', err)
  }
}

const saveProfile = async () => {
  isLoading.value = true
  try {
    const res = await $api('/profile', {
      method: 'PUT',
      body: {
        nome: profileData.value.nome,
        cognome: profileData.value.cognome,
        cellulare: profileData.value.cellulare,
      },
    })

    if (res.userData)
      userData.value = res.userData

    notificheStore.addToast('Profilo aggiornato con successo.', null, null, 'success')
  }
  catch {
    notificheStore.addToast('Errore nell\'aggiornamento del profilo.', null, null, 'error')
  }
  finally {
    isLoading.value = false
  }
}

const triggerFileInput = () => {
  fileInput.value?.click()
}

const onFileSelected = (event: Event) => {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]
  if (!file)
    return

  const reader = new FileReader()
  reader.onload = (e) => {
    cropImageSrc.value = e.target?.result as string
    showCropDialog.value = true
  }
  reader.readAsDataURL(file)

  // Reset input so same file can be selected again
  target.value = ''
}

const onCropDialogOpened = () => {
  nextTick(() => {
    if (cropperRef.value) {
      cropperInstance = new Cropper(cropperRef.value, {
        aspectRatio: 1,
        viewMode: 1,
        dragMode: 'move',
        autoCropArea: 1,
        cropBoxResizable: true,
        cropBoxMovable: true,
        guides: true,
        center: true,
        highlight: false,
        background: true,
        responsive: true,
        minCropBoxWidth: 100,
        minCropBoxHeight: 100,
      })
    }
  })
}

const closeCropDialog = () => {
  showCropDialog.value = false
  cropImageSrc.value = ''
  if (cropperInstance) {
    cropperInstance.destroy()
    cropperInstance = null
  }
}

const applyCrop = async () => {
  if (!cropperInstance)
    return

  const canvas = cropperInstance.getCroppedCanvas({
    width: 256,
    height: 256,
    imageSmoothingEnabled: true,
    imageSmoothingQuality: 'high',
  })

  canvas.toBlob(async (blob) => {
    if (!blob)
      return

    isPhotoUploading.value = true
    closeCropDialog()

    const formData = new FormData()
    formData.append('file', blob, 'avatar.png')

    try {
      const res = await $api('/profile/foto', {
        method: 'POST',
        body: formData,
      })

      profileData.value.foto = res.avatar

      if (res.userData)
        userData.value = res.userData

      notificheStore.addToast('Foto aggiornata con successo.', null, null, 'success')
    }
    catch {
      notificheStore.addToast('Errore nel caricamento della foto.', null, null, 'error')
    }
    finally {
      isPhotoUploading.value = false
    }
  }, 'image/png')
}

const deletePhoto = async () => {
  try {
    const res = await $api('/profile/foto', {
      method: 'DELETE',
    })

    profileData.value.foto = ''

    if (res.userData)
      userData.value = res.userData

    notificheStore.addToast('Foto rimossa.', null, null, 'success')
  }
  catch {
    notificheStore.addToast('Errore nella rimozione della foto.', null, null, 'error')
  }
}

onMounted(fetchProfile)
</script>

<template>
  <VRow>
    <!-- Profile Photo Card -->
    <VCol cols="12">
      <VCard>
        <VCardText class="d-flex align-center gap-6 flex-wrap">
          <!-- Avatar -->
          <VAvatar
            size="100"
            :color="!profileData.foto ? 'primary' : undefined"
            :variant="!profileData.foto ? 'tonal' : undefined"
            rounded
          >
            <VImg
              v-if="profileData.foto"
              :src="profileData.foto"
            />
            <span
              v-else
              class="text-h3 font-weight-medium"
            >
              {{ initials }}
            </span>
          </VAvatar>

          <div>
            <h5 class="text-h5 mb-1">
              {{ profileData.nome }} {{ profileData.cognome }}
            </h5>
            <p class="text-body-1 text-capitalize mb-4">
              {{ profileData.ruolo }}
            </p>

            <div class="d-flex gap-4 flex-wrap">
              <VBtn
                :loading="isPhotoUploading"
                @click="triggerFileInput"
              >
                <VIcon
                  start
                  icon="tabler-upload"
                />
                Carica foto
              </VBtn>

              <VBtn
                v-if="profileData.foto"
                color="error"
                variant="tonal"
                @click="deletePhoto"
              >
                <VIcon
                  start
                  icon="tabler-trash"
                />
                Rimuovi
              </VBtn>
            </div>

            <p class="text-body-2 text-disabled mt-2">
              JPG, PNG o WebP. Max 5MB.
            </p>

            <input
              ref="fileInput"
              type="file"
              accept="image/jpeg,image/png,image/webp"
              class="d-none"
              @change="onFileSelected"
            >
          </div>
        </VCardText>
      </VCard>
    </VCol>

    <!-- Profile Form Card -->
    <VCol cols="12">
      <VCard title="Informazioni Personali">
        <VCardText>
          <VForm @submit.prevent="saveProfile">
            <VRow>
              <VCol
                cols="12"
                md="6"
              >
                <AppTextField
                  v-model="profileData.nome"
                  label="Nome"
                  placeholder="Il tuo nome"
                />
              </VCol>

              <VCol
                cols="12"
                md="6"
              >
                <AppTextField
                  v-model="profileData.cognome"
                  label="Cognome"
                  placeholder="Il tuo cognome"
                />
              </VCol>

              <VCol
                cols="12"
                md="6"
              >
                <AppTextField
                  v-model="profileData.email"
                  label="Email"
                  placeholder="nome@azienda.it"
                  type="email"
                  disabled
                />
              </VCol>

              <VCol
                cols="12"
                md="6"
              >
                <AppTextField
                  v-model="profileData.cellulare"
                  label="Cellulare"
                  placeholder="+39 333 1234567"
                />
              </VCol>

              <VCol
                cols="12"
                md="6"
              >
                <AppTextField
                  v-model="profileData.ruolo"
                  label="Ruolo"
                  disabled
                />
              </VCol>

              <VCol
                v-if="profileData.codiceAgente"
                cols="12"
                md="6"
              >
                <AppTextField
                  v-model="profileData.codiceAgente"
                  label="Codice Agente"
                  disabled
                />
              </VCol>

              <VCol cols="12">
                <div class="d-flex gap-4">
                  <VBtn
                    type="submit"
                    :loading="isLoading"
                  >
                    Salva modifiche
                  </VBtn>

                  <VBtn
                    variant="tonal"
                    color="secondary"
                    @click="fetchProfile"
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

  <!-- Crop Dialog -->
  <VDialog
    v-model="showCropDialog"
    max-width="600"
    persistent
    @after-enter="onCropDialogOpened"
  >
    <VCard>
      <VCardTitle class="d-flex align-center justify-space-between">
        <span>Ritaglia foto profilo</span>
        <VBtn
          icon
          variant="text"
          size="small"
          @click="closeCropDialog"
        >
          <VIcon icon="tabler-x" />
        </VBtn>
      </VCardTitle>

      <VCardText>
        <div class="cropper-wrapper">
          <img
            ref="cropperRef"
            :src="cropImageSrc"
            class="cropper-image"
          >
        </div>
      </VCardText>

      <VCardActions class="justify-end pa-4">
        <VBtn
          variant="tonal"
          color="secondary"
          @click="closeCropDialog"
        >
          Annulla
        </VBtn>
        <VBtn
          color="primary"
          @click="applyCrop"
        >
          <VIcon
            start
            icon="tabler-crop"
          />
          Applica
        </VBtn>
      </VCardActions>
    </VCard>
  </VDialog>

</template>

<style>
.cropper-wrapper {
  max-block-size: 400px;
  overflow: hidden;
}

.cropper-image {
  display: block;
  inline-size: 100%;
  max-inline-size: 100%;
}
</style>
