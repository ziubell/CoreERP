namespace CoreERP.Domain.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class NotificaModuloAttribute : Attribute
{
    public string Codice { get; }
    public string Descrizione { get; }
    public string? Icona { get; }
    public string? Colore { get; }

    public NotificaModuloAttribute(string codice, string descrizione,
        string? icona = null, string? colore = null)
    {
        Codice = codice;
        Descrizione = descrizione;
        Icona = icona;
        Colore = colore;
    }
}
