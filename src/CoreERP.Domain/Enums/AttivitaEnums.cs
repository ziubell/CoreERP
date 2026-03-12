namespace CoreERP.Domain.Enums;

public enum StatoAttivita
{
    Aperta,
    Assegnata,
    InCorso,
    InPausa,
    InAttesa,
    Chiusa,
    Annullata
}

public enum TipoAttivita
{
    Assistenza,
    Provisioning,
    Disdetta,
    Risoluzione,
    Manutenzione,
    Sopralluogo,
    Commerciale,
    Amministrativo
}

public enum PrioritaAttivita
{
    Bassa,
    Media,
    Alta,
    Urgente
}
