---
title: "How to deploy a Hugo website to CloudFront using Hugo Deploy"
date: 2023-09-16T00:00:00-04:00
# weight: 1
# aliases: [""]
tags: ["CloudFront", "Hugo"]
author: "Favian Flores"
showToc: true
TocOpen: false
draft: false
hideMeta: false
comments: false
canonicalURL: "https://favianflores.com/posts/2023/how-to-deploy-a-website-to-cloudfront-using-hugo/"
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

In this article, I'm going to show you how to use Hugo's built-in `hugo deploy` command to deploy your site to a CloudFront distribution. You're going to learn how to set up an IAM user on AWS, authenticate the AWS CLI, and configure the Hugo CLI so that you can deploy your Hugo website using 1 command.

**Before you begin, make sure you have a CloudFront distribution with an S3 bucket as an origin. You can find a guide on setting one up [here](/posts/2023/how-to-deploy-a-static-website-using-cloudfront-route53-s3-and-certificate-manager/). You are also going to need to have the [AWS CLI](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html) and [Hugo](https://gohugo.io/installation) installed.**

*If you already have the AWS CLI installed and configured with the right access, you can skip down to the [Configure Hugo](#configure-hugo) section*

## Set up IAM user

In order for Hugo to be able to update your website, it needs access to your AWS account. For this tutorial, you are going to set up a specific user with *only the permissions necessary* for Hugo to update the site.

### Gather necessary information

In order to set the correct permissions, you are going to the ARN (Amazon Resource Name) for the S3 bucket and for the CloudFront distribution.

To get the ARN for the S3 bucket:

1. In the S3 console, click the bucket for your website
2. Click "Properties"
3. Copy the ARN under "Bucket overview"

Then, to get the CloudFront ARN:

1. In the CloudFront console, click the distribution for your website
2. Copy the ARN under "Details"

### Create a policy with the necessary permissions

Amazon uses policies in order to determine whether a user has access to specific resources. You are going to create a policy that will allow Hugo to read and delete files from your S3 bucket and allow Hugo to create an invalidation on your CloudFront distribution.

1. In the IAM console, click "Policies" on the left
2. Click "Create policy"
3. Click "JSON" in the policy editor
4. Paste the following:

   ```json
   {
      "Version": "2012-10-17",
      "Statement": 
      [
         {
            "Effect": "Allow",
            "Action": 
            [
                "s3:PutObject",
                "s3:GetObject",
                "s3:ListBucket",
                "s3:DeleteObject",
                "cloudfront:CreateInvalidation"
            ],
            "Resource": 
            [
                "<S3-ARN>/*",
                "<S3-ARN>",
                "<CLOUDFRONT-DISTRIBUTION-ARN>"
            ]
         }
      ]
   }
   ```

5. Replace `<S3-ARN>` and `<CLOUDFRONT-DISTRIBUTION-ARN>` with their respective ARNs from earlier
6. Click "Next"
7. Give your new policy a name and optional description
8. Click "Create policy"

### Create an IAM user

Now, you are going to create the user that Hugo will use to deploy your website.

*if you already have an IAM user that you want to use, you can attach the policy created earlier to that user.*

1. In the IAM Dashboard, click on "Users" on the left
2. Click "Add users"
3. Give your user a name, I'm going to call it "hugo"
4. Make sure "Provide user access to the AWS Management Console" is **not** checked
5. Click "Next"
6. For "Permission options" click "Attach policies directly"
7. Search for and select the policy you created earlier
8. Click "Next"
9. Click "Create User"

### Configure Access Key

Now that you have a user with the permissions needed to deploy your website, you need to set up an access key that the AWS CLI can use to make the changes.

1. In the IAM console, click on "Users" on the left
2. Click on the user you created earlier
3. Go to the "Security credentials" tab
4. Under "Access Keys", click "Create access key"
5. Select "Other" and click "Next"
6. (Optional) Add a description for the key
7. Click "Create access key"
8. Save the access key id and secret access key
9. Click "Done"

## Configure AWS CLI

Now that your IAM user is created and has the correct permissions, you need to configure the AWS CLI to use the access key created in the last step.

1. In your terminal, run `aws configure`
2. Paste the access key id
3. Paste the secret access key
4. (Optional) set your preferred region
5. (Optional) set your preferred output format

## Configure Hugo

Now that AWS is configured, you can go ahead and configure your hugo project.

1. Copy and paste the configuration below

   ```yaml
   deployment:
     targets:
       name: <NAME>
       URL: <S3-ARN>?region=us-east-1
       cloudFrontDistributionID: <CLOUDFRONT-DISTRIBUTION-ID>
   ```

2. Replace \<NAME\> with a name of your choice
3. Replace \<S3-ARN\> with the ARN for the bucket
4. Replace \<CLOUDFRONT-DISTRIBUTION-ID\> with the distribution id.
    - The distribution id can be found at the end of the ARN
    - Everything after 'distribution/'

## Conclusion

You should now be able to run `hugo deploy` from the command line and have your website be updated. Hugo will automatically determine which files need to be uploaded and create an invalidation in CloudFront so that visitors to your website will see the latest updates.