using System.Text.RegularExpressions;

namespace CoreERP.Domain.ValueObjects;

public partial record PartitaIva
{
    public string Valore { get; }

    public PartitaIva(string valore)
    {
        if (string.IsNullOrWhiteSpace(valore))
            throw new ArgumentException("La partita IVA non può essere vuota.");

        valore = valore.Trim();

        if (!PartitaIvaRegex().IsMatch(valore))
            throw new ArgumentException($"Partita IVA non valida: {valore}");

        Valore = valore;
    }

    [GeneratedRegex(@"^[0-9]{11}$")]
    private static partial Regex PartitaIvaRegex();

    public override string ToString() => Valore;
}
