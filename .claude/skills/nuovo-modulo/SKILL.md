---
name: nuovo-modulo
description: Scaffolding completo di un nuovo modulo frontend CoreERP. Usa quando l'utente chiede di creare un nuovo modulo, una nuova sezione, o un nuovo CRUD (es. "crea il modulo fatture", "aggiungi la gestione prodotti").
allowed-tools: Read, Write, Edit, Glob, Grep, Bash, Agent
argument-hint: "<NomeModulo> [--wizard] [--senza-export]"
---

# Nuovo Modulo Frontend — CoreERP

Crea lo scaffolding completo per un nuovo modulo frontend seguendo **tutte le regole** del subagent `vuexy`.

## Prerequisiti

Prima di generare codice:
1. Leggi `.claude/agents/vuexy.md` per le regole di stile aggiornate
2. Leggi un modulo esistente simile (es. `src/pages/anagrafiche/`, `src/stores/anagrafiche.ts`, `src/types/anagrafica.ts`) per replicare i pattern
3. Chiedi all'utente la struttura dei campi se non specificata

## Input

L'utente fornisce: **$ARGUMENTS**

Primo argomento = nome del modulo (es. `Fatture`, `Prodotti`, `Ordini`)
Flag opzionali:
- `--wizard` → form a step con AppStepper (default: form semplice)
- `--senza-export` → ometti il menu export dalla lista

## File da Generare

Genera i file nell'ordine seguente:

### 1. Tipi — `src/types/<nomeModulo>.ts`
- Interface `<NomeModulo>Api` (risposta backend completa)
- Interface `<NomeModulo>ListItemApi` (sottoinsieme per la lista)
- Interface `Create<NomeModulo>Request`
- Interface `Update<NomeModulo>Request` (o unica `SaveRequest` se identica)
- Costanti `STATO_LABELS` come `Record<number, string>` se servono

### 2. Store — `src/stores/<nomeModulo>.ts`
- Pinia Composition API con `defineStore`
- HTTP tramite `$api` da `@/utils/api`
- State: `items`, `totalCount`, `current`, `loading`
- Metodi: `fetchList`, `fetchById`, `create`, `update`, `remove`
- Lookups se necessari con `fetchLookups()` → `Promise.all()`

### 3. Pagina Lista — `src/pages/<nome-modulo>/index.vue`
- `VDataTableServer` con paginazione server-side
- Ricerca debounced 300ms
- Menu export (salvo `--senza-export`)
- Bottone "Nuovo" che punta al form
- Template no-data con icona
- Tabella direttamente nella `VCard` (regola bordi aderenti)

### 4. Pagina Form Unificata — `src/pages/<nome-modulo>/[id].vue` o routing appropriato
- **UNA SOLA PAGINA** per creazione e modifica
- `isEditMode` computed basata sulla presenza dell'ID nel route param
- In creazione: form vuoto
- In modifica: carica dati con `store.fetchById()`
- Header con titolo dinamico: `isEditMode ? 'Modifica X' : 'Nuovo X'`
- Pulsante submit "Salva" con `prepend-icon="tabler-device-floppy"`
- Se `--wizard`: usa AppStepper con validazione per step
- Se form semplice: card singola con VRow/VCol

### 5. Pagina Dettaglio — `src/pages/<nome-modulo>/[id].vue` (se separata dal form)
- Card dettagli con pattern `<h6 class="text-h6">Label: <span class="text-body-1 d-inline-block">Valore</span></h6>`
- Header con pulsanti "Modifica" + menu "Azioni" (regola #8)
- Contatti/relazioni in VList se presenti

### 6. Navigazione — `src/navigation/vertical/<nomeModulo>.ts`
- Nodo padre con icona Tabler
- Nodi figli SENZA icona (regola circle automatico)
- Registra nel file `src/navigation/vertical/index.ts`

## Regole da Rispettare

- Leggi e applica TUTTE le regole in `.claude/agents/vuexy.md`
- Pulsanti: rounded (globale), submit "Salva" primary, elimina error pieno, secondari tonal
- Toast: `useNotificheStore().addToast()`
- Form elements: solo wrapper `App*`
- CSS: solo classi Vuetify, mai CSS custom
- Variabili in italiano
- Nessuna icona nei titoli delle card
- Dialog con `VCardText d-flex justify-end gap-4` (mai VCardActions)

## Routing

Struttura file-based routing (unplugin-vue-router):
```
src/pages/<nome-modulo>/
├── index.vue           → /nome-modulo (lista)
├── nuovo.vue           → /nome-modulo/nuovo (form creazione)
├── modifica-[id].vue   → /nome-modulo/modifica-123 (form modifica)
└── [id].vue            → /nome-modulo/123 (dettaglio)
```

**IMPORTANTE:** `nuovo.vue` e `modifica-[id].vue` devono essere lo STESSO componente. Crea il componente form come componente in `src/components/<nomeModulo>/` e importalo in entrambe le pagine route, oppure usa un unico file route con logica condizionale.

## Dopo la Generazione

1. Verifica che il progetto compili senza errori
2. Controlla che la navigazione sia registrata
3. Assicurati che le rotte funzionino
