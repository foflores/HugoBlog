namespace PersonalWebsite.Infrastructure;

public interface IPersonalWebsiteConfig
{
    string HostedZoneId { get; }
    string HostedZoneName { get; }
    string AwsAppAccountId { get; }
    string AwsAppRegionId { get; }
    string AwsDnsAccountId { get; }
    string AwsDnsRegionId { get; }
    string DistributionDomainName { get; }
    string DnsQualifierId { get; }
    string[] DistributionIpAddresses { get; }
    string StackName { get; }
}

public class PersonalWebsiteDevelopmentConfig : IPersonalWebsiteConfig
{
    public string HostedZoneId => "Z064174917OLIOD8DL3OY";
    public string HostedZoneName => "favianflores.net";
    public string AwsAppAccountId => "412433735452";
    public string AwsAppRegionId => "us-east-1";
    public string AwsDnsAccountId => "888359517863";
    public string AwsDnsRegionId => "us-east-1";
    public string DistributionDomainName => "d237x8jjkh1uhm.cloudfront.net";
    public string DnsQualifierId => "dev";
    public string[] DistributionIpAddresses =>
        ["108.138.128.73", "108.138.128.63", "108.138.128.60", "108.138.128.117"];
    public string StackName => "PersonalWebsiteDevelopmentDns";
}

public class PersonalWebsiteProductionConfig : IPersonalWebsiteConfig
{
    public string HostedZoneId => "Z0926047XIN35SIX6DXS";
    public string HostedZoneName => "favianflores.com";
    public string AwsAppAccountId => "633067888675";
    public string AwsAppRegionId => "us-east-1";
    public string AwsDnsAccountId => "888359517863";
    public string AwsDnsRegionId => "us-east-1";
    public string DistributionDomainName => "d2e9hvgoj0x6j1.cloudfront.net";
    public string DnsQualifierId => "prod";
    public string[] DistributionIpAddresses =>
        ["3.168.122.115", "3.168.122.71", "3.168.122.126", "3.168.122.92"];
    public string StackName => "PersonalWebsiteProductionDns";
}
