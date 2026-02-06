using System.Collections.Generic;
using PersonalWebsite.Infrastructure.Components;
using Pulumi;

// ReSharper disable UnusedVariable

return await Deployment.RunAsync(() =>
{
    var config = new Config();

    var prefix = $"{Deployment.Instance.ProjectName}-{Deployment.Instance.StackName}";
    var hostedZoneId = config.Require("hostedzone-id");
    var primaryDomain = config.Require("primary-domain");
    var subDomains = config.RequireObject<List<string>>("sub-domains");
    var viewerRequestFunctionFile = config.Require("viewer-request-function-file");
    var viewerResponseFunctionFile = config.Require("viewer-response-function-file");

    var providers = new Providers(prefix, new ProvidersArgs
    {
        EnvAccountId = config.Require("env-account-id"),
        DnsAccountId = config.Require("dns-account-id"),
        ManagementAccountId = config.Require("management-account-id"),
        EnvIacRoleArn = config.Require("env-iac-role-arn"),
        DnsIacRoleArn = config.Require("dns-iac-role-arn"),
        ManagementIacRoleArn = config.Require("management-iac-role-arn")
    });

    var validatedCertificate = new ValidatedCertificate(prefix, new ValidatedCertificateArgs
    {
        DnsProvider = providers.DnsProvider,
        EnvProvider = providers.EnvProvider,
        PrimaryDomain = primaryDomain,
        SubjectAlternativeNames = subDomains,
        HostedZoneId = hostedZoneId
    });

    var sourceBucket = new SourceBucket(prefix, new SourceBucketArgs
    {
        EnvProvider = providers.EnvProvider
    });

    var contentDeliveryNetwork = new ContentDeliveryNetwork(prefix, new ContentDeliveryNetworkArgs
    {
        EnvProvider = providers.EnvProvider,
        ViewerRequestFunctionFile = viewerRequestFunctionFile,
        ViewerResponseFunctionFile = viewerResponseFunctionFile,
        Bucket = sourceBucket.Bucket,
        Certificate = validatedCertificate.Certificate,
        PrimaryDomain = primaryDomain
    });

    sourceBucket.ApplyPolicy(contentDeliveryNetwork.Distribution);

    var recordsV2 = new Records(prefix, new RecordsArgs
    {
        DnsProvider = providers.DnsProvider,
        Distribution = contentDeliveryNetwork.Distribution,
        HostedZoneId = hostedZoneId
    });
});
