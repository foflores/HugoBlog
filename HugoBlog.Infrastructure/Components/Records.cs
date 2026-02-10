using Pulumi;
using Pulumi.Aws;
using Pulumi.Aws.CloudFront;
using Pulumi.Aws.Route53;
using Pulumi.Aws.Route53.Inputs;

namespace HugoBlog.Infrastructure.Components;

// ReSharper disable UnusedAutoPropertyAccessor.Global

public class RecordsArgs
{
    public required Provider DnsProvider { get; init; }
    public required Distribution MainDistribution { get; init; }
    public required string MainHostedZoneId { get; init; }
}

public class Records
{
    public Record RootRecord { get; }
    public Record WwwRecord { get; }

    public Records(string prefix, RecordsArgs args)
    {
        RootRecord = new Record($"{prefix}-record-root", new RecordArgs
        {
            Name = string.Empty,
            Type = "A",
            Aliases =
            [
                new RecordAliasArgs
                {
                    Name = args.MainDistribution.DomainName,
                    ZoneId = args.MainDistribution.HostedZoneId,
                    EvaluateTargetHealth = false
                }
            ],
            ZoneId = args.MainHostedZoneId
        }, new CustomResourceOptions { Provider = args.DnsProvider });

        WwwRecord = new Record($"{prefix}-record-www", new RecordArgs
        {
            Name = "www",
            Ttl = 300,
            Type = "CNAME",
            Records = [ args.MainDistribution.DomainName ],
            ZoneId = args.MainHostedZoneId
        }, new CustomResourceOptions { Provider = args.DnsProvider });
    }
}
