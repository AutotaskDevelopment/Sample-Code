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
