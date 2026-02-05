using Pulumi;
using Pulumi.Aws;
using Pulumi.Aws.Inputs;

namespace PersonalWebsite.Infrastructure.Components;

public class ProvidersArgs
{
    //public required string DevelopmentAccountId { get; init; }
    //public required string ProductionAccountId { get; init; }
    public required string DnsAccountId { get; init; }
    public required string ManagementAccountId { get; init; }
    //public required string ProductionIacRoleArn { get; init; }
    //public required string DevelopmentIacRoleArn { get; init; }
    public required string DnsIacRoleArn { get; init; }
    public required string ManagementIacRoleArn { get; init; }
}

public class Providers
{
    public ProviderResource DnsProvider { get; }
    //public ProviderResource EnvironmentProvider { get; }

    public Providers(ProvidersArgs args)
    {
        DnsProvider = new Provider("dns-provider", new ProviderArgs
        {
            AllowedAccountIds = [args.ManagementAccountId],
            AssumeRole = new ProviderAssumeRoleArgs
            {
                RoleArn = args.ManagementIacRoleArn,
                SessionName = "pulumi-personalwebsite-deploy"
            }
        });
    }
}
