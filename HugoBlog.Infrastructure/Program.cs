using System.Collections.Generic;
using System.Reflection;
using HugoBlog.Infrastructure.Components;
using Pulumi;
using Pulumi.Aws;
using Pulumi.Aws.Inputs;
using Config = Pulumi.Config;

// ReSharper disable UnusedVariable

return await Deployment.RunAsync(() =>
{
    var config = new Config();

    var prefix = $"{Deployment.Instance.ProjectName}-{Deployment.Instance.StackName}";
    var domain = config.Require("domain");
    var viewerRequestFunctionFile = config.Require("viewer-request-function-file");

    var providers = new Providers(prefix, new ProvidersArgs
    {
        EnvAccountId = config.Require("env-account-id"),
        DnsAccountId = config.Require("dns-account-id"),
        EnvIacRoleArn = config.Require("env-iac-role-arn"),
        DnsIacRoleArn = config.Require("dns-iac-role-arn")
    });

    var awsAccountId = config.Require("aws-account-id");
    var awsIacRoleArn = config.Require("aws-iac-role-arn");
    var awsZoneId = config.Require("aws-zone-id");

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

    return new Dictionary<string, object?>
    {
        [$"{prefix}-version"] = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "1.0.0",
    };
});
