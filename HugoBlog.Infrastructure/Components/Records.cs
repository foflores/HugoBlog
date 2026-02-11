using Pulumi;
using Pulumi.Aws;
using Pulumi.Aws.CloudFront;
using Pulumi.Aws.Route53;

namespace HugoBlog.Infrastructure.Components;

// ReSharper disable UnusedAutoPropertyAccessor.Global

public class RecordsArgs
{
    public required Provider DnsProvider { get; init; }
    public required Distribution MainDistribution { get; init; }
    public required string MainHostedZoneId { get; init; }
    public required string RecordName { get; init; }
}

public class Records
{
    public Record Record { get; }

    public Records(string prefix, RecordsArgs args)
    {
        Record = new Record($"{prefix}-record", new RecordArgs
        {
            Name = args.RecordName,
            Ttl = 300,
            Type = "CNAME",
            Records = [ args.MainDistribution.DomainName ],
            ZoneId = args.MainHostedZoneId
        }, new CustomResourceOptions { Provider = args.DnsProvider });
    }
}
