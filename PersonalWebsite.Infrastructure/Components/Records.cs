using Pulumi;
using Pulumi.Aws;
using Pulumi.Aws.CloudFront;
using Pulumi.Aws.Route53;
using Pulumi.Aws.Route53.Inputs;

namespace PersonalWebsite.Infrastructure.Components;

// ReSharper disable UnusedAutoPropertyAccessor.Global

public class RecordsArgs
{
    public required Provider DnsProvider { get; init; }
    public required Distribution Distribution { get; init; }
    public required string HostedZoneId { get; init; }
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
                    Name = args.Distribution.DomainName,
                    ZoneId = args.Distribution.HostedZoneId,
                    EvaluateTargetHealth = false
                }
            ],
            ZoneId = args.HostedZoneId
        }, new CustomResourceOptions { Provider = args.DnsProvider });

        WwwRecord = new Record($"{prefix}-record-www", new RecordArgs
        {
            Name = "www",
            Ttl = 300,
            Type = "CNAME",
            Records = [ args.Distribution.DomainName ],
            ZoneId = args.HostedZoneId
        }, new CustomResourceOptions { Provider = args.DnsProvider });
    }
}
