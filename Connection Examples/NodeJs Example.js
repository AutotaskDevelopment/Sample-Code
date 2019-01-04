// Replace 'USERNAME' and 'PASSWORD' with your Autotask resource credentails and replace [ZONE] with 
// the database zone you are in. example: webservices3.autotask.net
// See the API documentation for information about which zone you are in.
var https = require("https");
var xml =
    '<?xml version="1.0" encoding="utf-8"?>' +
    "<soap:Envelope " +
    'xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/" ' +
    'xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" ' +
    'xmlns:xsd="http://www.w3.org/2001/XMLSchema">' +
    "<soap:Header>" +
    '<AutotaskIntegrations xmlns="http://autotask.net/ATWS/v1_5/">' + // To use version 1.6 change to http://autotask.net/ATWS/v1_6/
    "<PartnerID>" +
    "</PartnerID>" +
    "<IntegrationCode>" +
    // For Version 1.6 insert Tracking Identifer here
    "</IntegrationCode>" +
    "<ImpersonateAsResourceID>" +
    // For version 1.6 only insert id of a resource you want to impersonate
    "</ImpersonateAsResourceID>" +
    "</AutotaskIntegrations>" +
    "</soap:Header>" +
    "<soap:Body>" +
    "<getThresholdAndUsageInfo xmlns='http://autotask.net/ATWS/v1_5/'></getThresholdAndUsageInfo>" + // To use version 1.6 change to http://autotask.net/ATWS/v1_6/
    "</soap:Body>" +
    "</soap:Envelope>";
var username = "USERNAME";
var password = "PASSWORD";

var options = {
    host: "webservices[ZONE].autotask.net",
    port: 443,
    method: "POST",
    path: "/atservices/1.5/atws.asmx", // To use version 1.6 change to /atservices/1.6/atws.asmx
    // authentication headers
    headers: {
        'Content-Type': "text/xml; charset=utf-8",
        'Content-Length': Buffer.byteLength(xml),
        'Authorization': "Basic " + new Buffer(username + ":" + password).toString("base64"),
        'SOAPAction': "http://autotask.net/ATWS/v1_5/getThresholdAndUsageInfo", // To use version 1.6 change to http://autotask.net/ATWS/v1_6/getThresholdAndUsageInfo
        'Accept': "application/json"
    }
};
//The call
request = https.request(options, function (res) {
    console.log("statusCode:", res.statusCode);

    res.on("data", (d) => {
        process.stdout.write(d);
    });
});

request.on("error", (e) => {
    console.error(e);
});
request.end(xml);
