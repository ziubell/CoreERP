---
name: nuovo-dialog
description: Crea un componente dialog/modal per CoreERP. Usa quando l'utente chiede di creare un dialog, un modal, una finestra popup per inserimento o conferma dati.
allowed-tools: Read, Write, Edit, Glob, Grep
argument-hint: "<NomeDialog> [--conferma]"
---

# Nuovo Dialog — CoreERP

Crea un componente dialog Vue seguendo le regole del subagent `vuexy`.

## Input

L'utente fornisce: **$ARGUMENTS**

- Primo argomento: nome del dialog (es. `FatturaDialog`, `ConfermaEliminaDialog`)
- Flag `--conferma`: genera un dialog di conferma semplice (senza form)

## Prerequisiti

1. Leggi `.claude/agents/vuexy.md` per le regole
2. Il dialog va creato in `src/components/` organizzato per feature o in `src/components/dialogs/`

## Pattern: Dialog Form

```vue
<script setup lang="ts">
interface Props {
  isDialogVisible: boolean
  datiIniziali?: TipoForm
}
interface Emit {
  (e: 'update:isDialogVisible', value: boolean): void
  (e: 'submit', value: TipoForm): void
}

const props = withDefaults(defineProps<Props>(), {
  datiIniziali: () => ({ /* defaults */ }),
})
const emit = defineEmits<Emit>()

const form = ref<TipoForm>(structuredClone(toRaw(props.datiIniziali)))

const resetForm = () => {
  emit('update:isDialogVisible', false)
  form.value = structuredClone(toRaw(props.datiIniziali))
}

const onFormSubmit = () => {
  emit('update:isDialogVisible', false)
  emit('submit', form.value)
}
</script>

<template>
  <VDialog
    :width="$vuetify.display.smAndDown ? 'auto' : 900"
    :model-value="props.isDialogVisible"
    @update:model-value="val => $emit('update:isDialogVisible', val)"
  >
    <DialogCloseBtn @click="$emit('update:isDialogVisible', false)" />
    <VCard class="pa-sm-10 pa-2">
      <VCardText>
        <h4 class="text-h4 text-center mb-2">Titolo</h4>
        <p class="text-body-1 text-center mb-6">Sottotitolo</p>
        <VForm @submit.prevent="onFormSubmit">
          <VRow>
            <!-- Campi con AppTextField / AppSelect -->
          </VRow>
        </VForm>
      </VCardText>
      <VCardText class="d-flex justify-end gap-4">
        <VBtn variant="tonal" color="secondary" @click="resetForm">Annulla</VBtn>
        <VBtn color="primary" prepend-icon="tabler-device-floppy" type="submit">Salva</VBtn>
      </VCardText>
    </VCard>
  </VDialog>
</template>
```

## Pattern: Dialog Conferma (`--conferma`)

```vue
<script setup lang="ts">
interface Props {
  isDialogVisible: boolean
  titolo?: string
  messaggio?: string
}
interface Emit {
  (e: 'update:isDialogVisible', value: boolean): void
  (e: 'confirm'): void
}

const props = withDefaults(defineProps<Props>(), {
  titolo: 'Conferma',
  messaggio: 'Sei sicuro?',
})
const emit = defineEmits<Emit>()
</script>

<template>
  <VDialog
    max-width="400"
    :model-value="props.isDialogVisible"
    @update:model-value="val => $emit('update:isDialogVisible', val)"
    persistent
  >
    <VCard :title="props.titolo">
      <VCardText>{{ props.messaggio }}</VCardText>
      <VCardText class="d-flex justify-end gap-4">
        <VBtn variant="tonal" color="secondary" @click="$emit('update:isDialogVisible', false)">
          Annulla
        </VBtn>
        <VBtn color="error" prepend-icon="tabler-trash" @click="$emit('confirm')">
          Elimina
        </VBtn>
      </VCardText>
    </VCard>
  </VDialog>
</template>
```

## Regole

- MAI usare `VCardActions` — usare `VCardText class="d-flex justify-end gap-4"`
- Pulsanti a dimensione normale (non slim)
- Submit: "Salva" con `prepend-icon="tabler-device-floppy"`, colore pieno primary
- Elimina: colore pieno error con `prepend-icon="tabler-trash"`
- Annulla: `variant="tonal" color="secondary"`
- Clonazione dati: `structuredClone(toRaw(props.datiIniziali))`
- Form elements: solo wrapper `App*`
