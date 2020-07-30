#!/usr/bin/python3

# Replace 'USERNAME', 'PASSWORD', and 'TRACKING_IDENTIFIER' with your
# Autotask resource credentials and replace [ZONE] with your instance's zone.
# For example: webservices3.autotask.net
# See the API documentation for information about which zone you are in.

import requests
from requests.auth import HTTPBasicAuth
from zeep import Client
from zeep.transports import Transport

USERNAME = "USERNAME"
PASSWORD = "PASSWORD"
TRACKING_IDENTIFIER = "TRACKING_IDENTIFIER"

# to use version 1.6 update to
# https://webservices[ZONE].autotask.net/atservices/1.6/atws.wsdl
WSDL_URL = "https://webservices[ZONE].autotask.net/atservices/1.5/atws.wsdl"

api_session = requests.Session()
api_session.auth = HTTPBasicAuth(USERNAME, PASSWORD)

api_client = Client(
    WSDL_URL,
    transport=Transport(
        session=api_session))

# Create tracking header that will be re-used across requests
# Version 1.6 requires a value for the IntegrationCode tag.
integrations_header = {'AutotaskIntegrations': {
    'IntegrationCode': TRACKING_IDENTIFIER,
    'PartnerID': None,
    'ImpersonateAsResourceID': None}}
api_client.set_default_soapheaders(integrations_header)

# API endpoints will now be exposed as functions under api_client.service
# They can be listed with dir(api_client.service)

usage_info = api_client.service.getThresholdAndUsageInfo()
print(usage_info)
