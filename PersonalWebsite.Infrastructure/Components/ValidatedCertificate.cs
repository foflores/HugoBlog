using System.Collections.Generic;
using System.Linq;
using Pulumi;
using Pulumi.Aws;
using Pulumi.Aws.Acm;
using Pulumi.Aws.Route53;

namespace PersonalWebsite.Infrastructure.Components;

public class ValidatedCertificateArgs
{
    public required Provider DnsProvider { get; init; }
    public required Provider EnvProvider { get; init; }
    public required Input<string> PrimaryDomain { get; init; }
    public required InputList<string> SubjectAlternativeNames { get; init; }
    public required Input<string> HostedZoneId { get; init; }
}

public class ValidatedCertificate
{
    public Certificate Certificate { get; }
    public CertificateValidation Validation { get; }

    public ValidatedCertificate(string prefix, ValidatedCertificateArgs args)
    {
        Certificate = new Certificate($"{prefix}-certicate", new CertificateArgs
        {
            DomainName = args.PrimaryDomain,
            SubjectAlternativeNames = args.SubjectAlternativeNames,
            ValidationMethod = "DNS"
        }, new CustomResourceOptions { Provider = args.EnvProvider});

        var records = Certificate.DomainValidationOptions.Apply(domainValidationOptions =>
        {
            List<Record> records = [];
            foreach (var option in domainValidationOptions)
            {
                if (option.DomainName is null
                    || option.ResourceRecordName is null
                    || option.ResourceRecordType is null
                    || option.ResourceRecordValue is null)
                {
                    continue;
                }

                records.Add(new Record($"{prefix}-record-{option.DomainName}dnsvalidation", new RecordArgs
                {
                    AllowOverwrite = true,
                    Name = option.ResourceRecordName,
                    Records = [ option.ResourceRecordValue ],
                    Ttl = 60,
                    Type = option.ResourceRecordType,
                    ZoneId = args.HostedZoneId
                }, new CustomResourceOptions { Provider = args.DnsProvider }));
            }

            return Output.All(records.Select(y => y.Fqdn));
        });

        Validation = new CertificateValidation($"{prefix}-certificatevalidation", new CertificateValidationArgs
        {
            CertificateArn = Certificate.Arn,
            ValidationRecordFqdns = records
        }, new CustomResourceOptions {Provider = args.EnvProvider});
    }
}
