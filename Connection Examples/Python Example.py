# Replace 'USERNAME' and 'PASSWORD' with your Autotask resource credentials and replace [ZONE] with 
# the database zone you are in. example: webservices3.autotask.net
# See the API documentation for information about which zone you are in.
from requests import Request, Session
import base64
import sys

username = "USERNAME"
password = "PASSWORD"
url = "https://webservices[ZONE].autotask.net/atservices/1.5/atws.asmx"
body = """<?xml version='1.0' encoding='utf-8'?>
<soap:Envelope xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'>
    <soap:Header>
        <AutotaskIntegrations xmlns='http://autotask.net/ATWS/v1_5/'>
            <PartnerID></PartnerID>
            <IntegrationCode></IntegrationCode>
        </AutotaskIntegrations>
    </soap:Header>
    <soap:Body>
        <getThresholdAndUsageInfo xmlns='http://autotask.net/ATWS/v1_5/'></getThresholdAndUsageInfo>
    </soap:Body>
</soap:Envelope>"""
headers = {
        'Content-Type': "text/xml; charset=utf-8",
        'Content-Length': str(sys.getsizeof(body)),
        'Authorization': "Basic " + bytes.decode(base64.b64encode(bytes(username + ":" + password, "utf-8"))),
        'SOAPAction': "http://autotask.net/ATWS/v1_5/getThresholdAndUsageInfo",
        'Accept': "application/json"
    }
print(headers)
s = Session()
req = Request('POST', url, data=body, headers=headers)
prepped = req.prepare()
response = resp = s.send(prepped)
print(response.status_code)
print(response.content)
