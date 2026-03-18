namespace CoreERP.Application.Validators;

public static class FiscalCodeValidator
{
    /// <summary>
    /// Valida una Partita IVA italiana (11 cifre, algoritmo Luhn modificato).
    /// Accetta anche P.IVA europee con prefisso paese (es. "IT01234567890").
    /// </summary>
    public static bool IsValidPartitaIva(string? partitaIva)
    {
        if (string.IsNullOrWhiteSpace(partitaIva))
            return true; // campo opzionale

        var piva = partitaIva.Trim().ToUpperInvariant();

        // Rimuovi prefisso paese se presente (es. "IT")
        if (piva.Length > 2 && char.IsLetter(piva[0]) && char.IsLetter(piva[1]))
            piva = piva[2..];

        if (piva.Length != 11 || !piva.All(char.IsDigit))
            return false;

        // Algoritmo Luhn modificato per P.IVA italiana
        int sum = 0;
        for (int i = 0; i < 11; i++)
        {
            int digit = piva[i] - '0';
            if (i % 2 == 0)
            {
                sum += digit;
            }
            else
            {
                int doubled = digit * 2;
                sum += doubled > 9 ? doubled - 9 : doubled;
            }
        }

        return sum % 10 == 0;
    }

    /// <summary>
    /// Valida un Codice Fiscale italiano.
    /// Supporta sia il formato a 16 caratteri (persone fisiche) sia quello a 11 cifre (persone giuridiche).
    /// </summary>
    public static bool IsValidCodiceFiscale(string? codiceFiscale)
    {
        if (string.IsNullOrWhiteSpace(codiceFiscale))
            return true; // campo opzionale

        var cf = codiceFiscale.Trim().ToUpperInvariant();

        // CF numerico (11 cifre) = stesso formato della P.IVA (persone giuridiche)
        if (cf.Length == 11 && cf.All(char.IsDigit))
            return IsValidPartitaIva(cf);

        // CF alfanumerico (16 caratteri) = persone fisiche
        if (cf.Length != 16)
            return false;

        // Verifica formato: 6 lettere + 2 cifre + 1 lettera + 2 cifre + 1 lettera + 3 cifre + 1 lettera
        if (!char.IsLetter(cf[0]) || !char.IsLetter(cf[1]) || !char.IsLetter(cf[2]) ||
            !char.IsLetter(cf[3]) || !char.IsLetter(cf[4]) || !char.IsLetter(cf[5]) ||
            !char.IsDigit(cf[6]) || !char.IsDigit(cf[7]) ||
            !char.IsLetter(cf[8]) ||
            !char.IsDigit(cf[9]) || !char.IsDigit(cf[10]) ||
            !char.IsLetter(cf[11]) ||
            !char.IsDigit(cf[12]) || !char.IsDigit(cf[13]) || !char.IsDigit(cf[14]) ||
            !char.IsLetter(cf[15]))
            return false;

        // Calcolo carattere di controllo (posizione 15)
        var oddMap = new Dictionary<char, int>
        {
            ['0'] = 1, ['1'] = 0, ['2'] = 5, ['3'] = 7, ['4'] = 9,
            ['5'] = 13, ['6'] = 15, ['7'] = 17, ['8'] = 19, ['9'] = 21,
            ['A'] = 1, ['B'] = 0, ['C'] = 5, ['D'] = 7, ['E'] = 9,
            ['F'] = 13, ['G'] = 15, ['H'] = 17, ['I'] = 19, ['J'] = 21,
            ['K'] = 2, ['L'] = 4, ['M'] = 18, ['N'] = 20, ['O'] = 11,
            ['P'] = 3, ['Q'] = 6, ['R'] = 8, ['S'] = 12, ['T'] = 14,
            ['U'] = 16, ['V'] = 10, ['W'] = 22, ['X'] = 25, ['Y'] = 24, ['Z'] = 23
        };

        var evenMap = new Dictionary<char, int>
        {
            ['0'] = 0, ['1'] = 1, ['2'] = 2, ['3'] = 3, ['4'] = 4,
            ['5'] = 5, ['6'] = 6, ['7'] = 7, ['8'] = 8, ['9'] = 9,
            ['A'] = 0, ['B'] = 1, ['C'] = 2, ['D'] = 3, ['E'] = 4,
            ['F'] = 5, ['G'] = 6, ['H'] = 7, ['I'] = 8, ['J'] = 9,
            ['K'] = 10, ['L'] = 11, ['M'] = 12, ['N'] = 13, ['O'] = 14,
            ['P'] = 15, ['Q'] = 16, ['R'] = 17, ['S'] = 18, ['T'] = 19,
            ['U'] = 20, ['V'] = 21, ['W'] = 22, ['X'] = 23, ['Y'] = 24, ['Z'] = 25
        };

        int sum = 0;
        for (int i = 0; i < 15; i++)
        {
            sum += i % 2 == 0 ? oddMap[cf[i]] : evenMap[cf[i]];
        }

        char expectedCheck = (char)('A' + sum % 26);
        return cf[15] == expectedCheck;
    }
}
