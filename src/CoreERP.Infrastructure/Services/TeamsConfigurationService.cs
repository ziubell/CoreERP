using CoreERP.Application.Interfaces;
using CoreERP.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace CoreERP.Infrastructure.Services;

public class TeamsConfigurationService : ITeamsConfigurationService
{
    private readonly TeamsOptions _options;

    public TeamsConfigurationService(IOptions<TeamsOptions> options)
    {
        _options = options.Value;
    }

    public bool IsEnabled =>
        _options.Enabled && !string.IsNullOrWhiteSpace(_options.TeamsAppId);
}
