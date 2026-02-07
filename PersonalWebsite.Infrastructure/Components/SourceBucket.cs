using Pulumi;
using Pulumi.Aws;
using Pulumi.Aws.CloudFront;
using Pulumi.Aws.Iam;
using Pulumi.Aws.Iam.Inputs;
using Pulumi.Aws.S3;

namespace PersonalWebsite.Infrastructure.Components;

public class SourceBucketArgs
{
    public required Provider EnvProvider { get; init; }
}

public class SourceBucket
{
    private readonly string _prefix;
    private readonly SourceBucketArgs _args;

    public Bucket Bucket { get; }
    public BucketPolicy? BucketPolicy { get; private set; }
    public SourceBucket(string prefix, SourceBucketArgs args)
    {
        _prefix = prefix;
        _args = args;
        Bucket = new Bucket($"{prefix}-bucket-source", new BucketArgs
        {
            BucketName = $"{prefix}-bucket-source",
            ForceDestroy = true
        }, new CustomResourceOptions { Provider = args.EnvProvider });
    }

    public void ApplyPolicy(Distribution distribution)
    {
        BucketPolicy = new BucketPolicy($"{_prefix}-bucketpolicy-source", new BucketPolicyArgs
        {
            Bucket = Bucket.BucketName,
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
                        Resources = [ Bucket.Arn.Apply(x => $"{x}/*") ],
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
            }, new InvokeOptions{ Provider = _args.EnvProvider}).Apply(x => x.Json)
        }, new CustomResourceOptions { Provider = _args.EnvProvider });
    }
}
