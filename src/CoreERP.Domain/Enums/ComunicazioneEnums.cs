namespace CoreERP.Domain.Enums;

public enum TipoComunicazione
{
    Email,
    SMS,
    PushNotification,
    WhatsApp,
    Telefono
}

public enum StatoComunicazione
{
    Bozza,
    Inviata,
    Consegnata,
    Letta,
    Errore,
    Rimbalzata
}

public enum PrioritaMessaggio
{
    Bassa,
    Media,
    Alta
}

public enum AreaMessaggio
{
    Commerciale,
    Assistenza,
    Amministrazione,
    Rete,
    Generale
}
