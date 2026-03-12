namespace CoreERP.Domain.Enums;

public enum StatoAdesione
{
    Bozza,
    Inviata,
    Firmata,
    Completata,
    Annullata
}

public enum StatoPreventivo
{
    Bozza,
    Inviato,
    Accettato,
    Rifiutato,
    Scaduto
}

public enum StatoContratto
{
    Attivo,
    Sospeso,
    Scaduto,
    Disdetto,
    Annullato
}

public enum TipoDocumento
{
    Offerta,
    DDT,
    Fattura,
    NotaDiCredito,
    Preventivo
}

public enum StatoDocumento
{
    Bozza,
    Emesso,
    Inviato,
    Pagato,
    Annullato
}
