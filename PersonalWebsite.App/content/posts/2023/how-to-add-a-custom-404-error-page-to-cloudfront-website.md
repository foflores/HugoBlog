---
title: "How to add a custom 404 error page to a CloudFront website"
date: 2023-09-18T00:00:00-04:00
# weight: 1
# aliases: [""]
tags: ["CloudFront", "S3"]
author: "Favian Flores"
showToc: true
TocOpen: false
draft: false
hideMeta: false
comments: false
canonicalURL: "https://favianflores.com/posts/2023/how-to-add-a-custom-404-error-page-to-cloudfront-website/"
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

If you followed my previous post on [deploying a static website](/posts/2023/how-to-deploy-a-static-website-using-cloudfront-route53-s3-and-certificate-manager/) using S3, CloudFront, and Route53, you might have noticed that if you go to a page that doesn't exist, you get a page that looks like this:

![Ugly xml error page](/2023/how-to-add-a-custom-404-error-page-to-cloudfront-website-ugly-xml.webp)

This is because of the origin access control settings. AWS recommends using origin access control because it allows the S3 bucket containing your website files to restrict access to everyone except CloudFront.

This increases the overall security of the S3 bucket, and most likely, you don't really want users to bypass the distribution somehow and access your website from S3 directly.

A side effect of the increased security is that when CloudFront tries to access a file that doesn't exist from the bucket, S3 responds with an access denied error, and users see that ugly XML page.

## Add a custom error page

Fortunately, there is an easy fix for this, although it's not totally straightforward.

1. In the CloudFront console, click on the distribution you want to add the error page to
2. Head over to the "Error pages" tab
3. Click "Create custom error response"
4. In the "HTTP error code" dropdown, choose "403: Forbidden"
5. (Optional) Increase the "Error caching minimum TTL" if your error page rarely changes
6. Select "Yes" under "Customize error response"
7. Set the path to the HTML file for your 404 page
8. Choose "404: Not Found" as the HTTP response code

You should now have a custom 404 error page for your website.

## Make sure you follow step 4

If you noticed, in step 4, we are looking for a 403 error and responding to the user with a 404 error. This goes back to the origin access control settings for S3. When CloudFront looks for a file that doesn't exist on S3, it responds with a 403 error.

However, the user isn't trying to access a forbidden resource. The user is trying to access a file that doesn't exist. Following these steps, we can redirect users to the 404 page and return the correct 404 HTTP response.