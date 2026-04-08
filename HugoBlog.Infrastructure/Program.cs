using System.Collections.Generic;
using HugoBlog.Infrastructure.Components;
using Pulumi;

// ReSharper disable UnusedVariable

return await Deployment.RunAsync(() =>
{
    var config = new Config();

    var prefix = $"{Deployment.Instance.ProjectName}-{Deployment.Instance.StackName}";
    var zoneId = config.Require("zone-id");
    var domain = config.Require("domain");
    var recordName = config.Require("record-name");
    var viewerRequestFunctionFile = config.Require("viewer-request-function-file");

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
        SubjectAlternativeNames = new InputList<string>(),
        ZoneId = zoneId
    });

    var buckets = new Buckets(prefix, new BucketsArgs
    {
        EnvProvider = providers.EnvProvider
    });

    var distributions = new Distributions(prefix, new DistributionsArgs
    {
        EnvProvider = providers.EnvProvider,
        SourceBucket = buckets.SourceBucket,
        Certificate = certificates.Certificate,
        CertificateValidation = certificates.CertificateValidation,
        Domain = domain,
        ViewerRequestFunctionFile = viewerRequestFunctionFile,
    });

    buckets.ApplySourceBucketPolicy(distributions.Distribution);

    var records = new Records(prefix, new RecordsArgs
    {
        DnsProvider = providers.DnsProvider,
        MainDistribution = distributions.Distribution,
        MainHostedZoneId = zoneId,
        RecordName = recordName,
    });

    return new Dictionary<string, object?>
    {
        [$"{prefix}-bucket-source-arn"] = buckets.SourceBucket.Arn,
        [$"{prefix}-distribution-arn"] = distributions.Distribution.Arn
    };
});
