using System.Collections.Generic;
using PersonalWebsite.Infrastructure.Components;
using Pulumi;

// ReSharper disable UnusedVariable

return await Deployment.RunAsync(() =>
{
    var config = new Config();

    var prefix = $"{Deployment.Instance.ProjectName}-{Deployment.Instance.StackName}";
    var zoneId = config.Require("zone-id");
    var domain = config.Require("domain");
    var subjectAlternativeNames = config.RequireObject<List<string>>("subject-alternative-names");
    var viewerRequestFunctionFile = config.Require("viewer-request-function-file");
    var viewerResponseFunctionFile = config.Require("viewer-response-function-file");

    var providers = new Providers(prefix, new ProvidersArgs
    {
        EnvAccountId = config.Require("env-account-id"),
        DnsAccountId = config.Require("dns-account-id"),
        EnvIacRoleArn = config.Require("env-iac-role-arn"),
        DnsIacRoleArn = config.Require("dns-iac-role-arn")
    });

    var certificates = new Certificates(prefix, new CertificatesArgs
    {
        DnsProvider = providers.DnsProvider,
        EnvProvider = providers.EnvProvider,
        Domain = domain,
        SubjectAlternativeNames = subjectAlternativeNames,
        ZoneId = zoneId
    });

    var buckets = new Buckets(prefix, new BucketsArgs
    {
        EnvProvider = providers.EnvProvider
    });

    var distributions = new Distributions(prefix, new DistributionsArgs
    {
        EnvProvider = providers.EnvProvider,
        ViewerRequestFunctionFile = viewerRequestFunctionFile,
        ViewerResponseFunctionFile = viewerResponseFunctionFile,
        SourceBucket = buckets.SourceBucket,
        Certificate = certificates.Certificate,
        CertificateValidation = certificates.CertificateValidation,
        Domain = domain
    });

    buckets.ApplySourceBucketPolicy(distributions.Distribution);

    var records = new Records(prefix, new RecordsArgs
    {
        DnsProvider = providers.DnsProvider,
        MainDistribution = distributions.Distribution,
        MainHostedZoneId = zoneId
    });
});
