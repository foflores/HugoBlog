using Pulumi;
using Pulumi.Aws;
using Pulumi.Aws.Inputs;

namespace PersonalWebsite.Infrastructure.Components;

public class ProvidersArgs
{
    public required string EnvAccountId { get; init; }
    public required string DnsAccountId { get; init; }
    public required string EnvIacRoleArn { get; init; }
    public required string DnsIacRoleArn { get; init; }
}

public class Providers
{
    public Provider DnsProvider { get; }
    public Provider EnvProvider { get; }

    public Providers(string prefix, ProvidersArgs args)
    {
        DnsProvider = new Provider($"{prefix}-provider-dns", new ProviderArgs
        {
            AllowedAccountIds = [args.DnsAccountId],
            AssumeRoles = new ProviderAssumeRoleArgs
            {
                RoleArn = args.DnsIacRoleArn,
                SessionName = $"pulumi-{prefix}-deploy"
            },
            Region = "us-east-1"
        });

        EnvProvider = new Provider($"{prefix}-provider-env", new ProviderArgs
        {
            AllowedAccountIds = [args.EnvAccountId],
            AssumeRoles = new ProviderAssumeRoleArgs
            {
                RoleArn = args.EnvIacRoleArn,
                SessionName = $"pulumi-{prefix}-deploy"
            },
            Region = "us-east-1"
        });
    }
}
