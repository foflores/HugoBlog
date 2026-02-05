---
title: "How to schedule Hugo deployments with GitHub Actions"
date: 2023-09-17T00:00:00-04:00
# weight: 1
# aliases: [""]
tags: ["Hugo", "GitHub Actions"]
author: "Favian Flores"
showToc: true
TocOpen: false
draft: false
hideMeta: false
comments: false
canonicalURL: "https://favianflores.com/posts/2023/how-to-schedule-hugo-deployments-with-github-actions/"
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

In this article I'm going to show you how to create a GitHub workflow which automatically deploys a hugo website on a set schedule.

## Add access keys

I host this website on AWS CloudFront. In order for GitHub to be able to deploy my website, it needs access to my AWS account.

In my [last article](/posts/2023/how-to-deploy-a-website-to-cloudfront-using-hugo) you learned how to create an IAM user with the necessary permissions for deploying a Hugo website. You will need the same setup here.

Then you can head over to your repository on GitHub and do the following:

1. Click on the Settings tab
2. Click on "Secrets and variables"
3. Click on "Actions"

Here, you need to add the "Access Key Id" and "Secret Access Key" for your IAM user as repository secrets.

## Create the workflow

Once the secrets have been added to your repository, you can create the workflow file in your repo. The workflow needs to be placed in the `.github/workflows/` directory in your repo.

1. Create a new file called `build-and-depoy.yml`
2. Paste the following template:

    ```yaml
    name: Build And Deploy
    
    on:
      workflow_dispatch:
      schedule:
        - cron: '<CRON-SCHEDULE>'
    
    jobs:
      build-and-deploy:
        runs-on: ubuntu-latest
        steps:
          - name: Checkout main
            uses: actions/checkout@v3.5.2
    
          - name: Hugo setup
            uses: peaceiris/actions-hugo@v2.6.0
            with:
              hugo-version: '0.111.3'
              extended: true
    
          - name: Build
            run: hugo
    
          - name: Deploy
            run: hugo deploy --force --maxDeletes -1 --invalidateCDN
            env:
              AWS_ACCESS_KEY_ID: ${{ secrets.<VAR-NAME> }}
              AWS_SECRET_ACCESS_KEY: ${{ secrets.<VAR-NAME> }}
    ```

3. Replace `<CRON-SCHEDULE>` with your desired schedule using [cron syntax](https://pubs.opengroup.org/onlinepubs/9699919799/utilities/crontab.html#tag_20_25_07).
4. Replace `<VAR-NAME>` with the repository variable names for the respective Access Key Id and Secret Access Key

## Commit and push the workflow

Once you've set your desired deployment schedule, you can commit the workflow to the repository and push it to GitHub. GitHub should automatically register the workflow and run it.

## Conclusion

You have now scheduled your Hugo website to be deployed regularly. Combined with the publish-date front-matter, you can schedule your blog post releases or any other website updates.

A quick note: the `workflow_dispatch` line in the template allows you to manually run the workflow from GitHub. I've found it useful at times, but can be removed if you don't need it.