using Amazon.CDK;
using Amazon.CDK.AWS.Route53;
using Constructs;

// ReSharper disable UnusedVariable

namespace PersonalWebsite.Infrastructure;

public class PersonalWebsiteDnsStack : Stack
{
    public PersonalWebsiteDnsStack(Construct scope, string id, IStackProps props, IPersonalWebsiteConfig config)
        : base(scope, id, props)
    {
        HostedZoneAttributes hostedZoneAttributes = new()
        {
            HostedZoneId = config.HostedZoneId,
            ZoneName = config.HostedZoneName
        };
        var hostedZone = HostedZone.FromHostedZoneAttributes(this, "HostedZone", hostedZoneAttributes);

        ARecordProps personalWebsiteRootRecordProps = new()
        {
            Target = RecordTarget.FromIpAddresses(config.DistributionIpAddresses),
            Zone = hostedZone,
            Ttl = Duration.Minutes(5)
        };
        ARecord personalWebsiteRootRecord = new(this, "RootRecord", personalWebsiteRootRecordProps);

        CnameRecordProps personalWebsiteWwwRecordProps = new()
        {
            DomainName = config.DistributionDomainName,
            Zone = hostedZone,
            RecordName = "www",
            Ttl = Duration.Minutes(5)
        };
        CnameRecord personalWebsiteWwwRecord = new(this, "WwwRecord", personalWebsiteWwwRecordProps);
    }
}
