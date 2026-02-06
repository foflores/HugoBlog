using System.Collections.Generic;
using System.IO;
using System.Linq;
using PersonalWebsite.Infrastructure.Components;
using Pulumi;
using Pulumi.Aws.Acm;
using Pulumi.Aws.CloudFront;
using Pulumi.Aws.CloudFront.Inputs;
using Pulumi.Aws.Iam;
using Pulumi.Aws.Iam.Inputs;
using Pulumi.Aws.Route53;
using Pulumi.Aws.Route53.Inputs;
using Pulumi.Aws.S3;

// ReSharper disable UnusedVariable

return await Deployment.RunAsync(() =>
{
    var config = new Config();

    var dnsZoneId = config.Require("hostedzone-id");

    var prefix = $"{Deployment.Instance.ProjectName}-{Deployment.Instance.StackName}";
    var hostedZoneIdV2 = config.Require("hostedzone-id-v2");
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

    // var validatedCertificate = new ValidatedCertificate(prefix, new ValidatedCertificateArgs
    // {
    //     DnsProvider = providers.DnsProvider,
    //     EnvProvider = providers.EnvProvider,
    //     PrimaryDomain = primaryDomain,
    //     SubjectAlternativeNames = subDomains,
    //     HostedZoneId = hostedZoneIdV2
    // });
    //
    // var sourceBucket = new SourceBucket(prefix, new SourceBucketArgs
    // {
    //     EnvProvider = providers.EnvProvider
    // });
    //
    // var contentDeliveryNetwork = new ContentDeliveryNetwork(prefix, new ContentDeliveryNetworkArgs
    // {
    //     EnvProvider = providers.EnvProvider,
    //     ViewerRequestFunctionFile = viewerRequestFunctionFile,
    //     ViewerResponseFunctionFile = viewerResponseFunctionFile,
    //     Bucket = sourceBucket.Bucket,
    //     Certificate = validatedCertificate.Certificate,
    //     PrimaryDomain = primaryDomain
    // });
    //
    // sourceBucket.ApplyPolicy(contentDeliveryNetwork.Distribution);
    //
    // var recordsV2 = new Records(prefix, new RecordsArgs
    // {
    //     DnsProvider = providers.DnsProvider,
    //     Distribution = contentDeliveryNetwork.Distribution,
    //     HostedZoneId = hostedZoneIdV2
    // });

    var certificate = new Certificate("personal-website-certicate", new CertificateArgs
    {
        DomainName = primaryDomain,
        SubjectAlternativeNames = subDomains,
        ValidationMethod = "DNS"
    });

    var records = certificate.DomainValidationOptions.Apply(x =>
    {
        List<Record> records = [];
        foreach (var dV in x)
        {
            if (dV.DomainName is null
                || dV.ResourceRecordName is null
                || dV.ResourceRecordType is null
                || dV.ResourceRecordValue is null)
            {
                continue;
            }

            records.Add(new Record($"{dV.DomainName}-dns-validation-record", new RecordArgs
            {
                AllowOverwrite = true,
                Name = dV.ResourceRecordName,
                Records = [ dV.ResourceRecordValue ],
                Ttl = 60,
                Type = dV.ResourceRecordType,
                ZoneId = dnsZoneId
            }, new CustomResourceOptions { Provider = providers.ManagementProvider }));
        }

        return Output.All(records.Select(y => y.Fqdn));
    });

    var certificateValidation = new CertificateValidation(
        "personal-website-certificate-validation",
        new CertificateValidationArgs
        {
            CertificateArn = certificate.Arn,
            ValidationRecordFqdns = records
        });

    var bucket = new Bucket("personal-website-bucket", new BucketArgs
    {
        BucketPrefix = "personal-website-bucket",
        ForceDestroy = true
    });

    var originAccessControl = new OriginAccessControl("personal-website-origin-access-control", new OriginAccessControlArgs
    {
        Name = "personal-website-origin-access-control",
        OriginAccessControlOriginType = "s3",
        SigningBehavior = "always",
        SigningProtocol = "sigv4"
    });

    var viewerRequestFunction = new Function("personal-website-viewer-request-function", new FunctionArgs
    {
        Code = File.ReadAllText(viewerRequestFunctionFile),
        Name = "personal-website-viewer-request",
        Runtime = "cloudfront-js-2.0"
    });

    var viewerResponseFunction = new Function("personal-website-viewer-response-function", new FunctionArgs
    {
        Code = File.ReadAllText(viewerResponseFunctionFile),
        Name = "personal-website-viewer-response",
        Runtime = "cloudfront-js-2.0"
    });

    var distribution = new Distribution("personal-website-distribution", new DistributionArgs
    {
        Aliases = [ primaryDomain ],
        CustomErrorResponses =
        [
            new DistributionCustomErrorResponseArgs
            {
                ErrorCode = 403,
                ResponseCode = 404,
                ResponsePagePath = "/index.html"
            }
        ],
        DefaultRootObject = "index.html",
        DefaultCacheBehavior = new DistributionDefaultCacheBehaviorArgs
        {
            AllowedMethods = ["GET", "HEAD"],
            CachePolicyId = "658327ea-f89d-4fab-a63d-7e88639e58f6",
            CachedMethods = ["GET", "HEAD"],
            Compress = true,
            TargetOriginId = "personal-website-bucket-origin",
            ViewerProtocolPolicy = "redirect-to-https",
            FunctionAssociations =
            [
                new DistributionDefaultCacheBehaviorFunctionAssociationArgs
                {
                    EventType = "viewer-request",
                    FunctionArn = viewerRequestFunction.Arn
                },
                new DistributionDefaultCacheBehaviorFunctionAssociationArgs
                {
                    EventType = "viewer-response",
                    FunctionArn = viewerResponseFunction.Arn
                }
            ]
        },
        Enabled = true,
        HttpVersion = "http2and3",
        Origins = new[]
        {
            new DistributionOriginArgs
            {
                DomainName = bucket.BucketRegionalDomainName,
                OriginAccessControlId = originAccessControl.Id,
                OriginId = "personal-website-bucket-origin",
            }
        },
        PriceClass = "PriceClass_100",
        Restrictions = new DistributionRestrictionsArgs
        {
            GeoRestriction = new DistributionRestrictionsGeoRestrictionArgs
            {
                Locations = [],
                RestrictionType = "none"
            }
        },
        RetainOnDelete = false,
        ViewerCertificate = new DistributionViewerCertificateArgs
        {
            AcmCertificateArn = certificate.Arn,
            SslSupportMethod = "sni-only",
            MinimumProtocolVersion = "TLSv1.2_2021"
        },
        WaitForDeployment = false,
    });

    var bucketPolicy = new BucketPolicy("personal-website-bucket-policy", new BucketPolicyArgs
    {
        Bucket = bucket.BucketName,
        Policy = GetPolicyDocument.Invoke(new GetPolicyDocumentInvokeArgs
        {
            Version = "2012-10-17",
            Statements =
            [
                new GetPolicyDocumentStatementInputArgs
                {
                    Effect = "Allow",
                    Principals =
                    [
                        new GetPolicyDocumentStatementPrincipalInputArgs
                        {
                            Identifiers = ["cloudfront.amazonaws.com"],
                            Type = "Service"
                        }
                    ],
                    Actions = ["s3:GetObject"],
                    Resources = [ bucket.Arn.Apply(x => $"{x}/*") ],
                    Conditions =
                    [
                        new GetPolicyDocumentStatementConditionInputArgs
                        {
                            Test = "StringEquals",
                            Values = distribution.Arn,
                            Variable = "AWS:SourceArn"
                        }
                    ],
                }
            ]
        }).Apply(x => x.Json)
    });

    var dnsRootRecord = new Record("dns-root-record", new RecordArgs
    {
        Name = string.Empty,
        Type = "A",
        Aliases =
        [
            new RecordAliasArgs
            {
                Name = distribution.DomainName,
                ZoneId = distribution.HostedZoneId,
                EvaluateTargetHealth = false
            }
        ],
        ZoneId = dnsZoneId
    }, new CustomResourceOptions { Provider = providers.ManagementProvider });

    var dnsWwwRecord = new Record("dns-www-record", new RecordArgs
    {
        Name = "www",
        Ttl = 300,
        Type = "CNAME",
        Records = [ distribution.DomainName ],
        ZoneId = dnsZoneId
    }, new CustomResourceOptions { Provider = providers.ManagementProvider });
});
