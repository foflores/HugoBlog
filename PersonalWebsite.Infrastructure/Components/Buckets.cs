using Pulumi;
using Pulumi.Aws;
using Pulumi.Aws.CloudFront;
using Pulumi.Aws.Iam;
using Pulumi.Aws.Iam.Inputs;
using Pulumi.Aws.S3;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PersonalWebsite.Infrastructure.Components;

public class BucketsArgs
{
    public required Provider EnvProvider { get; init; }
}

public class Buckets
{
    private readonly string _prefix;
    private readonly BucketsArgs _args;

    public Bucket SourceBucket { get; }
    public BucketPolicy? SourceBucketPolicy { get; private set; }
    public Buckets(string prefix, BucketsArgs args)
    {
        _prefix = prefix;
        _args = args;

        SourceBucket = new Bucket($"{prefix}-bucket-source", new BucketArgs
        {
            BucketName = $"{prefix}-bucket-source",
            ForceDestroy = true
        }, new CustomResourceOptions { Provider = args.EnvProvider });
    }

    public void ApplySourceBucketPolicy(Distribution mainDistribution)
    {
        SourceBucketPolicy = new BucketPolicy($"{_prefix}-bucketpolicy-source", new BucketPolicyArgs
        {
            Bucket = SourceBucket.BucketName,
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
                        Resources = [ SourceBucket.Arn.Apply(x => $"{x}/*") ],
                        Conditions =
                        [
                            new GetPolicyDocumentStatementConditionInputArgs
                            {
                                Test = "StringEquals",
                                Values = mainDistribution.Arn,
                                Variable = "AWS:SourceArn"
                            }
                        ],
                    }
                ]
            }, new InvokeOptions{ Provider = _args.EnvProvider}).Apply(x => x.Json)
        }, new CustomResourceOptions { Provider = _args.EnvProvider });
    }
}
