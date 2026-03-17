namespace CoreERP.Infrastructure.Configuration;

public class TeamsOptions
{
    public const string SectionName = "Teams";

    /// <summary>
    /// Abilita/disabilita il modulo notifiche Teams.
    /// Richiede la registrazione di una Teams App in Azure.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// L'ID dell'app Teams dal manifest (usato per la verifica di configurazione).
    /// </summary>
    public string TeamsAppId { get; set; } = string.Empty;

    /// <summary>
    /// L'ID dell'app nel catalogo org-wide di Teams (visibile in Teams Admin Center).
    /// Necessario per installare l'app automaticamente e per i deep link nelle notifiche.
    /// </summary>
    public string TeamsCatalogAppId { get; set; } = string.Empty;
}
