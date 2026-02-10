using System.Collections.Generic;
using System.Linq;
using Pulumi;
using Pulumi.Aws;
using Pulumi.Aws.Acm;
using Pulumi.Aws.Route53;

namespace HugoBlog.Infrastructure.Components;

public class CertificatesArgs
{
    public required Provider DnsProvider { get; init; }
    public required Provider EnvProvider { get; init; }
    public required Input<string> Domain { get; init; }
    public required InputList<string> SubjectAlternativeNames { get; init; }
    public required Input<string> ZoneId { get; init; }
}

public class Certificates
{
    public Certificate Certificate { get; }
    public CertificateValidation CertificateValidation { get; }

    public Certificates(string prefix, CertificatesArgs args)
    {
        var count = 1;
        Certificate = new Certificate($"{prefix}-certicate", new CertificateArgs
        {
            DomainName = args.Domain,
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

                records.Add(new Record($"{prefix}-record-dnsval-{count:00}", new RecordArgs
                {
                    AllowOverwrite = true,
                    Name = option.ResourceRecordName,
                    Records = [ option.ResourceRecordValue ],
                    Ttl = 60,
                    Type = option.ResourceRecordType,
                    ZoneId = args.ZoneId
                }, new CustomResourceOptions { Provider = args.DnsProvider }));
                count++;
            }

            return Output.All(records.Select(y => y.Fqdn));
        });

        CertificateValidation = new CertificateValidation($"{prefix}-certval", new CertificateValidationArgs
        {
            CertificateArn = Certificate.Arn,
            ValidationRecordFqdns = records
        }, new CustomResourceOptions {Provider = args.EnvProvider});
    }
}
