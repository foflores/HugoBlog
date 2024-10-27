'use strict';

function handler(event) {
    let request = event.request;
    let host = request.headers["host"];
    let primaryDomain = `https://favianflores.com${request.uri}`;

    if (host === "www.favianflores.com") {
        return {
            statusCode: 302,
            statusDescription: 'Found',
            headers:
                {"location": {"value": primaryDomain}}
        };
    }

    if (request.uri !== "/" && (request.uri.endsWith("/") || request.uri.lastIndexOf(".") < request.uri.lastIndexOf("/"))) {
        if (request.uri.endsWith("/")) {
            request.uri = request.uri.concat("index.html");
        } else {
            request.uri = request.uri.concat("/index.html");
        }
    }

    return request;
}
