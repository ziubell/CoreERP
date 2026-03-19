---
name: nuova-pagina
description: Crea una singola pagina frontend CoreERP (lista, form, dettaglio). Usa quando l'utente chiede di aggiungere una pagina a un modulo esistente.
allowed-tools: Read, Write, Edit, Glob, Grep
argument-hint: "<percorso-pagina> [--tipo lista|form|dettaglio|wizard]"
---

# Nuova Pagina — CoreERP

Crea una singola pagina Vue seguendo le regole del subagent `vuexy`.

## Input

L'utente fornisce: **$ARGUMENTS**

- Primo argomento: percorso della pagina (es. `fatture/index`, `fatture/[id]`)
- Flag `--tipo`: tipo di pagina da generare
  - `lista` — DataTable con ricerca, paginazione, export
  - `form` — Form unificato creazione/modifica
  - `wizard` — Form a step con AppStepper (crea/modifica unificato)
  - `dettaglio` — Pagina di visualizzazione con card dettagli

## Prerequisiti

1. Leggi `.claude/agents/vuexy.md` per le regole
2. Leggi lo store e i tipi del modulo per capire i campi disponibili
3. Se il tipo non è specificato, deducilo dal nome (index → lista, [id] → dettaglio, nuovo/modifica → form)

## Pattern per Tipo

### Lista (`--tipo lista`)
- `VDataTableServer` con paginazione server-side
- Ricerca debounced, export menu, bottone "Nuovo"
- Tabella direttamente nella VCard (bordi aderenti)

### Form Unificato (`--tipo form`)
- **UNA SOLA pagina** per creazione e modifica
- `isEditMode` computed, titolo dinamico
- Pulsante "Salva" con icona floppy
- Card per sezioni logiche

### Wizard (`--tipo wizard`)
- Come form ma con `AppStepper` verticale
- Validazione per step
- Pulsanti Indietro/Avanti/Salva

### Dettaglio (`--tipo dettaglio`)
- Pattern `h6 text-h6` per label/valore
- Header con Modifica + menu Azioni
- Relazioni in VList

## Regole

Applica TUTTE le regole da `.claude/agents/vuexy.md`.
