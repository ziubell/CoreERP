namespace CoreERP.Domain.Enums;

public enum TipoAnagrafica
{
    Cliente,
    Potenziale,
    Web,
    Adesione,
    Fornitore
}

public enum StatoAnagrafica
{
    Attivo,
    Sospeso,
    Bloccato,
    Annullato
}

public enum MetodoPagamento
{
    Bonifico,
    RID,
    Bollettino,
    CartaDiCredito,
    Contanti
}

public enum PeriodicitaPagamento
{
    Mensile,
    Bimestrale,
    Trimestrale,
    Semestrale,
    Annuale
}
