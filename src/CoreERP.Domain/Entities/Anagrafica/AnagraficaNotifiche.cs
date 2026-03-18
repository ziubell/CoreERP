using CoreERP.Domain.Attributes;

namespace CoreERP.Domain.Entities.Anagrafica;

[NotificaModulo("Anagrafica.Creata", "Nuovo contatto creato", icona: "tabler-user-plus", colore: "success")]
[NotificaModulo("Anagrafica.Modificata", "Contatto modificato", icona: "tabler-user-edit", colore: "info")]
[NotificaModulo("Anagrafica.Eliminata", "Contatto eliminato", icona: "tabler-user-minus", colore: "error")]
[NotificaModulo("Anagrafica.ContattoAggiunto", "Contatto associato all'anagrafica", icona: "tabler-user-plus", colore: "success")]
[NotificaModulo("Anagrafica.ContattoRimosso", "Contatto rimosso dall'anagrafica", icona: "tabler-user-minus", colore: "warning")]
[NotificaModulo("Contatto.Modificato", "Contatto modificato", icona: "tabler-user-edit", colore: "info")]
public static class AnagraficaNotifiche { }
