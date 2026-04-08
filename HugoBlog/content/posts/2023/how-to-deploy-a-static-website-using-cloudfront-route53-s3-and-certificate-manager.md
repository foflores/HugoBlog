---
title: "How to deploy a static website using AWS S3, CloudFront, and Route53"
date: 2023-09-15T00:00:00-04:00
# weight: 1
# aliases: [""]
tags: ["S3", "CloudFront", "Route53"]
author: "Favian Flores"
showToc: true
TocOpen: false
draft: false
hideMeta: false
comments: false
canonicalURL: "https://favianflores.com/posts/2023/how-to-deploy-a-static-website-using-cloudfront-route53-s3-and-certificate-manager/"
disableHLJS: false
disableShare: false
hideSummary: false
searchHidden: false
ShowReadingTime: true
ShowBreadCrumbs: true
ShowPostNavLinks: true
ShowWordCount: false
ShowRssButtonInSectionTermList: false
UseHugoToc: true
cover:
    image: "" # image path/url
    alt: "" # alt text
    caption: "" # display caption under cover
    relative: false # when using page bundles set this to true
    hidden: true # only hide on current single page
---

In this article, you will learn how to store a static website on S3, use CloudFront as a content delivery network to distribute your website globally, and finally, use Route53 to access your website from a custom domain of your choice.

**Before you start, you should have the files for your static website and a domain name with a hosted zone on Route53.**

Ultimately, you will have a website that uses HTTPS, is accessible from a custom domain, and is distributed globally, giving your users faster loading times.

## Set up S3

Amazon's S3 service uses buckets to organize files, similar to how folders work on a computer. So the first step is to create a bucket to upload your files.

1. In the S3 Console, click "Create bucket"
2. Give your bucket a unique name and choose the region that works best for you
3. Leave the other settings as is and click "Create bucket"

Now you can upload your files to the bucket.

1. Click on the bucket you just made
2. Click "Upload"
3. Add your files

## Create a public certificate

Now that your website is on S3, the next step is to create a public certificate to use HTTPS with your domain.

1. In the Certificate Manager console, click "Request a certificate"
2. Make sure "Request a public certificate" is selected and click "Next"
3. Under fully qualified domain names, add \<DOMAIN-NAME\> and \*.\<DOMAIN-NAME\>
    - For example, if your domain were example.com, then you would add example.com and \*.example.com
4. Leave the rest as is and click "Request"

You should now be able to see the certificate listed. The next step is to validate it by verifying that you are the domain owner. Fortunately, since we're using Route53, AWS provides a simple verification method.

1. Open the certificate and click "Create records in Route53"
2. Click "Create records" again to confirm
3. Wait until the certificate's status changes to "Issued"

## Set up CloudFront

Now that you have a valid certificate, you need to create a CloudFront distribution to distribute your website.

1. In the CloudFront Console, click "Create distribution"
2. Choose the bucket you just created as the origin domain
3. Give the distribution a name
4. For Origin access, choose "Origin access control settings"
5. Create a control setting with the default options
6. Head to the Viewer section and choose "Redirect HTTP to HTTPS"
7. (Optional) Enable AWS WAF
    - WAF provides some extra [security features](https://aws.amazon.com/waf/features/)
    - Pricing for WAF can be found [here](https://aws.amazon.com/waf/pricing/)
8. (Optional) Choose the price class that best works for you
    - By default, CloudFront distributes your website to all their locations
    - You can choose to distribute only to certain areas to [reduce costs](https://aws.amazon.com/cloudfront/pricing/)
9. Under Alternate domain name, add your domain
10. Under Custom SSL certificate, choose the certificate you made earlier
11. Make sure the latest recommended version of TLS is selected as the security policy
12. Under Default root object, type in the name of the html file the distribution should serve when visiting your domain.
    - This is usually index.html
13. Click "Create distribution"

CloudFront should now distribute your website, which takes around 15 minutes to complete. In the meantime, you can move on to configure the proper permissions for your S3 bucket so that CloudFront can read from it.

## Configure S3 permissions

After clicking "Create distribution", there should be a notification at the top of your screen labeled "**The S3 bucket policy needs to be updated**"

1. In the notification, click "Copy policy"
2. Click the link to your s3 bucket
3. Under the Bucket policy section, click "Edit"
4. Paste the policy in the text field
5. Click "Save changes"

## Configure Route53

Now your website is up and should be available at the distribution domain name CloudFront gave. The last step is to connect your domain to the CloudFront distribution.

1. In the Route53 console, click on "Hosted zones" on the left
2. Click on the hosted zone for your website
3. Click "Create record"
4. (Optional) Under record name, choose the subdomain you want to use
   - Leave it blank to use your domain
5. Make sure the record type is an A record
6. Turn on "Alias"
7. Select "Alias to CloudFront distribution" from the dropdown
8. Select your distribution
9. Click "Create record"

It should take a minute or two to update, and then, if everything was done right, you should be able to visit your website using your domain name.

## Conclusion

You now have a website that uses HTTPS, is accessible from a custom domain, and is distributed globally, giving your users faster loading times.

There is some follow-up reading to learn how to update your website, enable client-side caching, or add a custom error page. These are outside the scope of this article since it varies a lot based on what tools you are using to build your website. However, if you are interested in fine-tuning your website, you can find plenty of great articles online covering these topics.