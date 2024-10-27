'use strict';
function handler(event) {
    let request = event.request;
    let response = event.response;
    let headers = response.headers;

    if (request.uri.match(/^.*(\.ttf|\.webp|\.js|\.ico|\.png)$/)) {
        headers["cache-control"] = {
            value: "public, max-age=31536000, immutable",
        };
    } else {
        headers["cache-control"] = {
            value: "no-cache",
        };
    }

    return response;
}
