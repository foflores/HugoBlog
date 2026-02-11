'use strict';

function handler(event) {
    let request = event.request;
    let host = request.headers.host.value;
    let domain = `https://www.favianflores.com${request.uri}`;

    if (host === "favianflores.com") {
        return {
            statusCode: 302,
            statusDescription: 'Found',
            headers:
                {"location": {"value": domain}}
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
