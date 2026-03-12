using CoreERP.Application.Features.Anagrafica.Commands;
using CoreERP.Application.Features.Anagrafica.Validators;
using CoreERP.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace CoreERP.Application.Tests.Validators;

public class CreateAnagraficaValidatorTests
{
    private readonly CreateAnagraficaValidator _validator = new();

    [Fact]
    public void PersonaFisica_Without_Nome_Should_Fail()
    {
        var command = new CreateAnagraficaCommand { IsPersonaGiuridica = false, Cognome = "Rossi" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Nome);
    }

    [Fact]
    public void PersonaGiuridica_Without_RagioneSociale_Should_Fail()
    {
        var command = new CreateAnagraficaCommand { IsPersonaGiuridica = true };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.RagioneSociale);
    }

    [Fact]
    public void Valid_PersonaFisica_Should_Pass()
    {
        var command = new CreateAnagraficaCommand
        {
            IsPersonaGiuridica = false,
            Nome = "Mario",
            Cognome = "Rossi",
            Tipo = TipoAnagrafica.Cliente,
            Email = "mario@test.com",
            Cap = "00100",
            Provincia = "RM"
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Invalid_Cap_Should_Fail()
    {
        var command = new CreateAnagraficaCommand
        {
            IsPersonaGiuridica = false,
            Nome = "Mario",
            Cognome = "Rossi",
            Cap = "ABC"
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Cap);
    }

    [Fact]
    public void Invalid_PartitaIva_Should_Fail()
    {
        var command = new CreateAnagraficaCommand
        {
            IsPersonaGiuridica = true,
            RagioneSociale = "Test Srl",
            PartitaIva = "123"
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PartitaIva);
    }
}
