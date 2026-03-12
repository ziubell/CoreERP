using CoreERP.Domain.Entities.Anagrafica;
using CoreERP.Domain.Enums;
using CoreERP.Domain.Events;
using FluentAssertions;

namespace CoreERP.Domain.Tests.Entities;

public class AnagraficaTests
{
    [Fact]
    public void NomeCompleto_PersonaFisica_Should_Return_Nome_Cognome()
    {
        var anagrafica = new Anagrafica { Nome = "Mario", Cognome = "Rossi", IsPersonaGiuridica = false };
        anagrafica.NomeCompleto.Should().Be("Mario Rossi");
    }

    [Fact]
    public void NomeCompleto_PersonaGiuridica_Should_Return_RagioneSociale()
    {
        var anagrafica = new Anagrafica { RagioneSociale = "Spadhausen Srl", IsPersonaGiuridica = true };
        anagrafica.NomeCompleto.Should().Be("Spadhausen Srl");
    }

    [Fact]
    public void Should_Add_Domain_Event()
    {
        var anagrafica = new Anagrafica { Id = 1 };
        anagrafica.AddDomainEvent(new AnagraficaCreatedEvent(1));

        anagrafica.DomainEvents.Should().HaveCount(1);
        anagrafica.DomainEvents.First().Should().BeOfType<AnagraficaCreatedEvent>();
    }

    [Fact]
    public void Should_Default_To_Attivo_State()
    {
        var anagrafica = new Anagrafica();
        anagrafica.Stato.Should().Be(StatoAnagrafica.Attivo);
    }

    [Fact]
    public void Should_Initialize_Collections_Empty()
    {
        var anagrafica = new Anagrafica();
        anagrafica.Meta.Should().BeEmpty();
        anagrafica.Contatti.Should().BeEmpty();
        anagrafica.Users.Should().BeEmpty();
        anagrafica.Social.Should().BeEmpty();
    }
}
