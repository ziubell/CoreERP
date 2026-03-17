namespace CoreERP.Application.Interfaces;

public interface ITeamsConfigurationService
{
    /// <summary>
    /// Indica se il modulo Teams è attivo (app registrata e configurata).
    /// </summary>
    bool IsEnabled { get; }
}
