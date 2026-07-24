using System.Collections.Generic;
using System.Reflection;
using HugoBlog.Infrastructure.Components;
using Pulumi;
using Pulumi.Aws;
using Pulumi.Aws.Inputs;
using Pulumi.Aws.Route53;
using Config = Pulumi.Config;

// ReSharper disable UnusedVariable

return await Deployment.RunAsync(() =>
{
    var config = new Config();

    var prefix = $"{Deployment.Instance.ProjectName}-{Deployment.Instance.StackName}";
    var zoneId = config.Require("zone-id");
    var domain = config.Require("domain");
    var recordName = config.Require("record-name");
    var viewerRequestFunctionFile = config.Require("viewer-request-function-file");
    var awsAccountId = config.Require("aws-account-id");
    var awsIacRoleArn = config.Require("aws-iac-role-arn");
    var awsZoneId = config.Require("aws-zone-id");

    var providers = new Providers(prefix, new ProvidersArgs
    {
        EnvAccountId = config.Require("env-account-id"),
        DnsAccountId = config.Require("dns-account-id"),
        EnvIacRoleArn = config.Require("env-iac-role-arn"),
        DnsIacRoleArn = config.Require("dns-iac-role-arn")
    });

    var provider = new Provider($"{prefix}-provider", new ProviderArgs
    {
        AllowedAccountIds = [ awsAccountId ],
        AssumeRoles = new ProviderAssumeRoleArgs
        {
            RoleArn = awsIacRoleArn,
            SessionName = "pulumi-deploy"
        },
        Region = "us-east-1"
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

    var hugoBlogRecord = new Record($"{prefix}-record-hugoblog", new RecordArgs
    {
        Name = "hugoblog",
        Ttl = 300,
        Type = "CNAME",
        Records = [ distributions.Distribution.DomainName ],
        ZoneId = awsZoneId
    }, new CustomResourceOptions { Provider = provider });

    return new Dictionary<string, object?>
    {
        [$"{prefix}-bucket-source-arn"] = buckets.SourceBucket.Arn,
        [$"{prefix}-distribution-arn"] = distributions.Distribution.Arn,
        [$"{prefix}-version"] = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "1.0.0",
    };
});
