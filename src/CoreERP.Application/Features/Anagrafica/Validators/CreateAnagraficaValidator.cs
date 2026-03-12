using CoreERP.Application.Features.Anagrafica.Commands;
using FluentValidation;

namespace CoreERP.Application.Features.Anagrafica.Validators;

public sealed class CreateAnagraficaValidator : AbstractValidator<CreateAnagraficaCommand>
{
    public CreateAnagraficaValidator()
    {
        When(x => x.IsPersonaGiuridica, () =>
        {
            RuleFor(x => x.RagioneSociale)
                .NotEmpty().WithMessage("La ragione sociale è obbligatoria per le persone giuridiche.");

            RuleFor(x => x.PartitaIva)
                .NotEmpty().WithMessage("La partita IVA è obbligatoria per le persone giuridiche.")
                .Matches(@"^[0-9]{11}$").WithMessage("La partita IVA deve essere di 11 cifre.");
        });

        When(x => !x.IsPersonaGiuridica, () =>
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Il nome è obbligatorio.");
            RuleFor(x => x.Cognome)
                .NotEmpty().WithMessage("Il cognome è obbligatorio.");
        });

        RuleFor(x => x.CodiceFiscale)
            .Matches(@"^[A-Za-z]{6}[0-9]{2}[A-Za-z][0-9]{2}[A-Za-z][0-9]{3}[A-Za-z]$")
            .When(x => !string.IsNullOrWhiteSpace(x.CodiceFiscale))
            .WithMessage("Codice fiscale non valido.");

        RuleFor(x => x.Cap)
            .Matches(@"^[0-9]{5}$")
            .When(x => !string.IsNullOrWhiteSpace(x.Cap))
            .WithMessage("Il CAP deve essere di 5 cifre.");

        RuleFor(x => x.Provincia)
            .Length(2)
            .When(x => !string.IsNullOrWhiteSpace(x.Provincia))
            .WithMessage("La provincia deve essere di 2 caratteri.");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("Email non valida.");
    }
}
