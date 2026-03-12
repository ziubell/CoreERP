namespace CoreERP.Domain.Enums;

public enum TipoImpianto
{
    Residenziale,
    Commerciale,
    Industriale,
    Infrastruttura
}

public enum StatoImpianto
{
    Attivo,
    InProgetto,
    Dismesso,
    Sospeso
}

public enum TipoCopertura
{
    FTTH,
    FTTC,
    FWA,
    Misto,
    NonCoperto
}

public enum TipoApparato
{
    Armadio,
    Edificio,
    Pozzetto,
    Box,
    Cavo,
    Fibra,
    Splitter,
    Giunto,
    Switch
}

public enum StatoApparato
{
    Attivo,
    InManutenzione,
    Dismesso,
    InProgetto
}
