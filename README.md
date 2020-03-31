# Newest Version 1.6 Updates
The latest version of the Autotask Web Services API available includes some new requirements.
These include:
1. Only resources with an API User (API-Only) security level can access the API.
2. Tracking Identifers are required to be appart of the SOAP header when making requests with version 1.6. These are assigned on the Security tab of the API-only user's
Resource Management page. These identifiers are not currently required to make API calls with version 1.5, but once a resource has been assigned a tracking identifier then it must be provided
in version 1.5 API calls as well.

## Impersonation Feature Requirements
Starting with version 1.6 you will be able to impersonate a resource when creating certain entities such as ticket notes. This functionality is supported through the SOAP header by providing
a the resource id of the resource you want to impersonate for the call.

### Example of SOAP Header With Tracking and Impersonation Tags
```xml
  <?xml version="1.0" encoding="utf-8"?>
  <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/" xmlns:x-si="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
    <soap:Header>
	  <AutotaskIntegrations xmlns="http://autotask.net/ATWS/v1_6/">
	    <IntegrationCode>[Substitute Tracking Identifier Here]</IntegrationCode>
		<ImpersonateAsResourceID>[Substitue ID of Resource to Impersonate Here]</ImpersonateAsResourceID>
	  </AutotaskIntegrations>
	</soap:Header>
	<soap:Body>
	   .
	   .
	   .
	</soap:Body>
  </soap:Envelope>
```

# Sample Code Project
This project is meant to give developers a starting point to the Autotask API with examples.  
You can use the project as a base to build your own applications/integrations. There are connection examples in [Connection Examples](https://github.com/AutotaskDevelopment/Sample-Code/tree/master/Connection%20Examples) that show how to make a successful call to the API in different programming languages.

This repository is not all inclusive. If there are examples you would like to see you can ask for them, or better yet, you can contribute to the project by adding them.

# Query Structure
Queries are in XML.
The base structure is as follows:   
```xml 
  <queryxml>  
    <entity>ENTITY NAME</entity>    
    <query>  
      ...   
    </query>   
  </queryxml>
```
Groups can be defined using the ```<condition>``` tag.   
For example:   
AccountType = 1 AND Active = true AND Country = "United States"   
Would be:   
```xml
<queryxml>
  <entity>Account</entity>
  <query>
    <field>AccountType<expression op="equals">1</expression></field>
    <field>Active<expression op="equals">true</expression></field>
    <field>Country<expression op="equals">"United States"</expression></field>
  </query>
</queryxml>
```
and AccountType = 1 AND (Active = true AND Country = "United States") would be:    
```xml
<queryxml>
  <entity>Account</entity>
  <query>
    <field>AccountType<expression op="equals">1</expression></field>
    <condition>
      <field>Active<expression op="equals">true</expression></field>
      <field>Country<expression op="equals">"United States"</expression></field>
    </condition>
  </query>
</queryxml>
```

Queries that contain OR operators must have a ```<condition>``` tag around each field.   
For example: AccountType = 1 AND Active = true AND (Country = "United States" OR Country = "United Kingdom")   
```xml
<queryxml>
  <entity>Account</entity>
  <query>
    <field>AccountType<expression op="equals">1</expression></field>
    <field>Active<expression op="equals">true</expression></field>
    <condition>
      <field>Country<expression op="equals">"United States"</expression></field>
      <condition operator="OR">
        <field>Country<expression op="equals">"United Kingdom"</expression></field>
      </condition>
    </condition>    
  </query>
</queryxml>
```

and AccountType = 1 AND Active = true AND (Country = "United States" OR Country = "United Kingdom" OR Country = "Germany")   
would be   
```xml
<queryxml>
  <entity>Account</entity>
  <query>
    <field>AccountType<expression op="equals">1</expression></field>
    <field>Active<expression op="equals">true</expression></field>
    <condition>
      <field>Country<expression op="equals">"United States"</expression></field>
      <condition operator="OR">
        <field>Country<expression op="equals">"United Kingdom"</expression></field>
      </condition>
      <condition operator="OR">
        <field>Country<expression op="equals">"Germany"</expression></field>
      </condition>
    </condition>    
  </query>
</queryxml>
```
