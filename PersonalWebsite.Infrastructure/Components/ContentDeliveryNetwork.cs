using System.IO;
using Pulumi;
using Pulumi.Aws;
using Pulumi.Aws.Acm;
using Pulumi.Aws.CloudFront;
using Pulumi.Aws.CloudFront.Inputs;
using Pulumi.Aws.S3;

namespace PersonalWebsite.Infrastructure.Components;

public class ContentDeliveryNetworkArgs
{
    public required Bucket Bucket { get; init; }
    public required Certificate Certificate { get; init; }
    public required CertificateValidation CertificateValidation { get; init; }
    public required Provider EnvProvider { get; init; }
    public required string PrimaryDomain { get; init; }
    public required string ViewerRequestFunctionFile { get; init; }
    public required string ViewerResponseFunctionFile { get; init; }
}

public class ContentDeliveryNetwork
{
    public Distribution Distribution { get; }
    public OriginAccessControl OriginAccessControl { get; }
    public Function ViewerRequestFunction { get; }
    public Function ViewerResponseFunction { get; }

    public ContentDeliveryNetwork(string prefix, ContentDeliveryNetworkArgs args)
    {
        OriginAccessControl = new OriginAccessControl($"{prefix}-originaccesscontrol", new OriginAccessControlArgs
        {
            Name = $"{prefix}-originaccesscontrol",
            OriginAccessControlOriginType = "s3",
            SigningBehavior = "always",
            SigningProtocol = "sigv4"
        }, new CustomResourceOptions { Provider = args.EnvProvider });

        ViewerRequestFunction = new Function($"{prefix}-function-viewerrequest", new FunctionArgs
        {
            Code = File.ReadAllText(args.ViewerRequestFunctionFile),
            Name = $"{prefix}-function-viewerrequest",
            Runtime = "cloudfront-js-2.0"
        }, new CustomResourceOptions { Provider = args.EnvProvider });

        ViewerResponseFunction = new Function($"{prefix}-function-viewerresponse", new FunctionArgs
        {
            Code = File.ReadAllText(args.ViewerResponseFunctionFile),
            Name = $"{prefix}-function-viewerresponse",
            Runtime = "cloudfront-js-2.0"
        }, new CustomResourceOptions { Provider = args.EnvProvider });

        Distribution = new Distribution($"{prefix}-distribution", new DistributionArgs
        {
            Aliases = [ args.PrimaryDomain ],
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
                TargetOriginId = $"{prefix}-bucket-origin",
                ViewerProtocolPolicy = "redirect-to-https",
                FunctionAssociations =
                [
                    new DistributionDefaultCacheBehaviorFunctionAssociationArgs
                    {
                        EventType = "viewer-request",
                        FunctionArn = ViewerRequestFunction.Arn
                    },
                    new DistributionDefaultCacheBehaviorFunctionAssociationArgs
                    {
                        EventType = "viewer-response",
                        FunctionArn = ViewerResponseFunction.Arn
                    }
                ]
            },
            Enabled = true,
            HttpVersion = "http2and3",
            Origins = new[]
            {
                new DistributionOriginArgs
                {
                    DomainName = args.Bucket.BucketRegionalDomainName,
                    OriginAccessControlId = OriginAccessControl.Id,
                    OriginId = $"{prefix}-bucket-origin",
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
                AcmCertificateArn = args.Certificate.Arn,
                SslSupportMethod = "sni-only",
                MinimumProtocolVersion = "TLSv1.2_2021"
            },
            WaitForDeployment = false,
        }, new CustomResourceOptions { Provider = args.EnvProvider, DependsOn = [ args.CertificateValidation ]});
    }
}
