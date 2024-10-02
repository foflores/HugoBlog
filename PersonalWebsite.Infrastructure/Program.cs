using Amazon.CDK;
using PersonalWebsite.Infrastructure;

// ReSharper disable UnusedVariable

App app = new();
IPersonalWebsiteConfig config = new PersonalWebsiteProductionConfig();

Environment dnsEnvironment = new()
{
    Account = config.AwsDnsAccountId,
    Region = config.AwsDnsRegionId
};

DefaultStackSynthesizerProps dnsStackSynthesizerProps = new()
{
    Qualifier = config.DnsQualifierId
};
DefaultStackSynthesizer synthesizer = new(dnsStackSynthesizerProps);

StackProps personalWebsiteDnsStackProps = new()
{
    Description = "Contains dns settings for personal website app.",
    Env = dnsEnvironment,
    Synthesizer = synthesizer
};

PersonalWebsiteDnsStack personalWebsiteDnsStack = new(app, config.StackName, personalWebsiteDnsStackProps, config);

app.Synth();
