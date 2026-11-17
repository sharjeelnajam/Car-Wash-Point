// Talks to the helper program running on THIS PC (the shop PC), not to the web server.
// The helper controls the scanner and relays the website's calls to the cash machine.
(function () {
    const BASE = "http://127.0.0.1:5555";
    const HELPER_MISSING = { ok: false, helperMissing: true, error: "The scanner helper is not running on this PC." };

    async function call(path, method) {
        try {
            const options = { method: method };
            if (method === "POST") {
                options.headers = { "Content-Type": "application/json" };
                options.body = "{}";
            }
            const response = await fetch(BASE + path, options);
            return await response.json();
        } catch (e) {
            return HELPER_MISSING;
        }
    }

    window.scannerBridge = {
        status: function () { return call("/status", "GET"); },
        open: function () { return call("/open", "POST"); },
        close: function () { return call("/close", "POST"); },
        scan: function () { return call("/scan", "POST"); }
    };

    // One call to the cash machine, sent through the helper.
    window.cashBridge = {
        request: async function (method, path, body) {
            try {
                const response = await fetch(BASE + "/cash", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({ method: method, path: path, body: body })
                });
                return await response.json();
            } catch (e) {
                return { status: 0, helperMissing: true, error: HELPER_MISSING.error };
            }
        }
    };
})();
