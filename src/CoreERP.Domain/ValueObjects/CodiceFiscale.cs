using System.Text.RegularExpressions;

namespace CoreERP.Domain.ValueObjects;

public partial record CodiceFiscale
{
    public string Valore { get; }

    public CodiceFiscale(string valore)
    {
        if (string.IsNullOrWhiteSpace(valore))
            throw new ArgumentException("Il codice fiscale non può essere vuoto.");

        valore = valore.Trim().ToUpperInvariant();

        if (!CodiceFiscaleRegex().IsMatch(valore))
            throw new ArgumentException($"Codice fiscale non valido: {valore}");

        Valore = valore;
    }

    [GeneratedRegex(@"^[A-Z]{6}[0-9]{2}[A-Z][0-9]{2}[A-Z][0-9]{3}[A-Z]$")]
    private static partial Regex CodiceFiscaleRegex();

    public override string ToString() => Valore;
}
