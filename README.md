# DaAPI - DHCP and API

DaAPI is modern, cross-platform, web-based DHCPv6 (DHCPv4 is in the roadmap) server designed with the need of ISP in mind. 
It enabled you to assign IPv6 addresses and networks to your customers dynamically. Unlike a lot of other solutions, there is no need (and possibility) to write a config file - and learning all the possible syntax.
The integrated HTTP REST API lets you integrate DaAPI in your existing workflow seamlessly.

## Motivation
I was working on a technical lead/consulting job for small ISPs - ISPs in the classical sense of offering access to the public internet. Most devices from various vendors have excellent support for DHCPv4. They are offering server capabilities with a lot of configurational freedom. However,  compared to that the DHCPv6 capabilities are fundamental and limited. This situation created a need for a dedicated piece of software. After a bit of research and a lot of testing, none of the existing solutions seemed like a viable solution.

Sometimes, other solutions were not cross-platform. The config file could become very messy if you need to administrate thousands of customers. They had limited support for prefix delegation. Overall, it seems that this project hasn't the needs of small ISPs in mind, where extensibility and integration (Syslog, SNMP, etc.) is an essential factor to consider.

Another - and this point is fundamental - aspect of other existing solutions is that they are not easy to use and understand. Of course, out there, companies are having a huge customer base and hence can afford specialist of each aspect of a network: Engineering knowing all the bits about every protocol. Yet, there are smaller ones too, and they need a solution they can easily understand and delivering results quickly.
Long story short: DaAPI is my approach to solving theses explained issues. 

## Status
Currently, DaAPI is in its very, very early stage. I like to call this stage proof of concept. One ISP is running a field test with DaAPI to validate the ideas.   
In this stage, DaAPI has a lack of maturity, and **use it only in the context of experimental nature**.  However, I invite you to give DaAPI - even in its early stage, with a lot of limitation -  a try. Conclude for yourself if it solves one of your problems, too. Share your thoughts and help to make DaAPI more useful for others.  

## Key features
+ Modern HTTP based DHCPv6 server
+ A responsive web frontend for configuration
+ Cross-platform (Windows, Linux, macOS) support
+ OpenId based authentication flow
+ a unique and flexible scope management system
+ integrated notification engine to interact with other systems
+ localized in English and German (feel free to point out any spelling or grammar mistakes, or add a new language) 

## Not working yet
+ Receiving IPv6 Multicast (DaAPI will only accept unicast)
+ DHCPv4

## Upcoming features
The next release is titled "Making DaAPI useful."  Besides, the ongoing focus on stability, the goal is to leverage the possibility and uniqueness of DaAPI by developing the ecosystem around it. Client applications will be able to use the API endpoints. DaAPI will send logs to Syslog servers. Moreover, to improve the log system radically is a high priority task, too. As well as authenticate users via RADIUS. The notification engine will have more triggers, conditions and more generic actors like executing scripts and webhooks.

From the infrastructure point of view, the goal is to make DaAPI easier to integrate into existing systems by various steps. You can choose between different databases technologies, DaAPI will be available as a docker container, and, the repo will include binaries files for a more painless installation process.  

## Install and get started
For now, the only way to get DaAPI is to compile it by yourself. After compilation, you can copy the result to any machine or just run it on your compiling machine.

### Compiling machine is also running machine

DaAPI is a .NET Core application. For building the application, you need the .NET Core 3.1 SDK. You can find detailed steps [here](https://dotnet.microsoft.com/download/dotnet-core/3.1).

For frontend assets to compile, you need node and npm. You can find detailed steps [here](https://www.npmjs.com/get-npm).
A Git client needs to be installed too. [Here](https://git-scm.com/book/en/v2/Getting-Started-Installing-Git/), you can find the official documentation and how to accomplish it. 

``` cmd
git clone https://github.com/just-the-benno/DaAPI.git
cd ../daapi/src/DaAPI.Host
dotnet run  --urls "https://localhost:5001;http://localhost:5002"
``` 

> Because DaAPI is using well-known ports, under Linux, the user running the process needs to have the rights to open such ports. 

### Compiling machine is different from running machine
#### Compiling machine
DaAPI is a .NET Core application. For building the application, you need the .NET Core 3.1 SDK. You can find detailed steps [here](https://dotnet.microsoft.com/download/dotnet-core/3.1).

For frontend assets to compile, you need node and npm. You can find detailed steps [here](https://www.npmjs.com/get-npm).
A Git client needs to be installed too. [Here](https://git-scm.com/book/en/v2/Getting-Started-Installing-Git/), you can find the official documentation and how to accomplish it. 

``` cmd
git clone https://github.com/just-the-benno/DaAPI.git
dotnet publish ./daapi/src/DaAPI.Host/DaAPI.Host.csproj -c release -o ./publish 
```

The result is a folder *publish* with all needed files to run DaAPI. Copy his folder onto the running machine. 

> If you want to avoid installing the runtime on the running machine you can include the runtime in the publish process as well. You need to add the -r parameter with an [Runtime Identifer](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog) like -r linux-arm

#### Running machine
 If you want to run a compiled binary, the .NET Core Runtime is enough.  [Here](https://dotnet.microsoft.com/download/dotnet-core/3.1), you can find the official documentation. 
 
Copy the files from the step before into to a folder and run  
 
``` cmd
dotnet .\DaAPI.Host.dll --urls "https://localhost:5001;http://localhost:5002"
 ```
 
 > Because DaAPI is using well-known ports, under Linux, the user running the process needs to have the rights to do so. Windows will prompt a dialog, asking to create firewall rules. 

### First steps
The examples above assume you are running and managing DaAPI on the same machines. Hence, using localhost is the right choice. If you want to have different ports or different URLs, this page in the wiki describes what else needs to change accordingly. 

DaAPI will occupy, despite the DHCPv6 ports, two ports: 5001 and 5002. The web application uses port 5001, and https is mandatory. DaAPi will use a self-signed certificate. The second port is technically a workaround for certificate issues. The second port 5002 is only used by DaAPI itself, to communicate with the authentication service, living "inside" of DaAPI. The wiki page "OpenId Interaction" elaborate about this issue in more detail.  

You can open a browser and access DaAPI via https://localhost:5001 (or any other URL, you have chosen). The first step is to create a local user. (This part is not essential if you are using an external OpenId system.)

![DaAPI setting up the first local user](https://raw.githubusercontent.com/just-the-benno/DaAPI/master/documentation/daapi-first-steps-initilize.png)

After you create the user, the page refreshs, and you need to use the credentials to login into the system. After a successful login, you are redirected to an empty dashboard.  

![DaAPI empty Dashboard after initilizing](https://raw.githubusercontent.com/just-the-benno/DaAPI/master/documentation/daapi-first-steps-empty-dashboard.png)

### Assign interfaces to DaAPI

![DaAPI Binding an interface](https://raw.githubusercontent.com/just-the-benno/DaAPI/master/documentation/daapi-first-steps-bind-interface.png)

By default, DaAPI is not listening to any interface, unless explicitly to said so.  You can choose the interface point in the menu and select the interfaces used by DaAPI. If you have multiple interfaces, a name can be an excellent hint to identify it. Besides, the name will be displayed in logs and analysis later. 

### Creating your first Scope
DaAPI has a unique concept of  "**Scopes**" and "**Hierarchy of Scopes**". Scopes can be created in a hierarchy, allowing child scopes to inheritance properties from parent Scopes. The concept is explained in details in the documentation, and you should be familiar with the ideas to unleash the potential of DaAPI.

![DaAPI Create first scope](https://raw.githubusercontent.com/just-the-benno/DaAPI/master/documentation/daapi-first-steps-create-first-scope.png)

For now, we focus on creating the first Scope. Choose *Scope* in the menu, and after that, the »Create« button. Give your Scope a name and move on the next card with the start and end address. You can choose any start and end range. You can even select the same address for both values if you want to assign always the same address. Feel free to type in any valid IPv6 address you like. You can see there is a lot more configuration possible, which is explained in the documentation. As a reminder, you can use the infoboxes. You don't need to change anything for now.

![DaAPI Scope miminum required properties](https://raw.githubusercontent.com/just-the-benno/DaAPI/master/documentation/daapi-first-steps-first-scope-minimun-required-propertie.png)

The section »Properties« is for all the additional options like DNS server.  To create a Scope, you don't need to have any of these options. 

![DaAPI Scope properties](https://raw.githubusercontent.com/just-the-benno/DaAPI/master/documentation/daapi-first-steps-scope-properties.png)

The section »Resolver« is a speciality of DaAPI. In a nutshell, a resolver is like a matchmaker, finding a match between an incoming packet and a scope. A very generic match can be a relay agent address. But there are also very picky ones.  Please have a look at the documentation to find out more about each of them. 

![DaAPI Scope resolvers](https://raw.githubusercontent.com/just-the-benno/DaAPI/master/documentation/daapi-first-steps-scope-resolver.png)

There is also an example of how you can quickly write your own.  You can also create an issue and describe what type of resolvers you need and what kind of packets should be a match.

![DaAPI relay agent address resolver](https://raw.githubusercontent.com/just-the-benno/DaAPI/master/documentation/daapi-first-steps-scope-relay-agent-address-resolver.png)

After you entered a valid IPv6 address for your relay agent, you can create your first Scope. All you need now is to have at least one client requesting an IPv6 address and observe either the Dashboard or the Scope Details. (Unfortunately, there is no auto-update capability yet, so you have to reload the page, to see updated data).

The documentation has an article about how to create a complex scope hierarchy and a corresponding resolver structure. The real power of DaAPI. :) 

## More Information

This page is an overview. Check out the documentation for articles and in-depth cover of certain aspects of DaAPI. 

## Support

If you have any issues using DaAPI, create an issue. Usually, I'll reply within a couple of hours. 

## Contribute

Every contribution is highly and honestly appreciated. I'd love to see a vibrant community around this project, and there is so much that can be done!
You can use the issue tracker for any concerns, corrections or feedback. 

### Technical Contributors
Everyone using DaAPI is, in my view, a technical contributor. Point to errors or misunderstanding by filling out an issue. Request new features, giving feedback, explaining your use case, all of this helps to shape this product to become more and more useful. 

### Documentation and language Magicians
I'm still on the learning curve to master English. There is no doubt that the localization and documentation can be significantly improved from a perspective of correctness and clarity.  Or, you want to add another language.  

### Developers
For developers, there is a section in the documentation explaining the architectures and thoughts behind it. It should speed up the understanding process. As a reference to understand the "business logic", [RFC8415](https://tools.ietf.org/html/rfc8415) is an excellent starting point.

## License

DaAPI can be used under the MIT license.

## Thanks

I want to say thanks to all projects authors and contributors that I used to build DaAPI. All these libraries make life so much easier.  Thanks, I deeply appreciate your work and endurance. 

(The list is not completed nor a ranking) 
+ [.NET Core](https://github.com/dotnet/core)
+ [Humanizer](https://github.com/Humanizr/Humanizer)
+ [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
+ [NetCoreServer](https://github.com/chronoxor/NetCoreServer)
+ [MediatR](https://github.com/jbogard/MediatR)
+ [IdentityServer4](https://github.com/IdentityServer/IdentityServer4)
+ [Serilog](https://github.com/serilog/serilog)
+ [Node and NPM](https://nodejs.org/en/)
+ [grunt](https://github.com/gruntjs/grunt)
+ [BlazorDateRangePicker](https://github.com/jdtcn/BlazorDateRangePicker)
+ [Blazored.Modal](https://github.com/Blazored/Modal)
+ [Blazored.Toast](https://github.com/Blazored/Toast)
+ [BlazorStrap](https://github.com/chanan/BlazorStrap)
+ [ChartJs](https://github.com/chartjs/Chart.js)
+ [ChartJs.Blazor](https://github.com/mariusmuntean/ChartJs.Blazor)
+ [Sve-Blazor-DataTable](https://github.com/SveNord/Sve-Blazor-DataTable)
+ [Moq](https://github.com/moq/moq4)
+ [xUnit](https://github.com/xunit/xunit)
+ [adminLTE](https://github.com/ColorlibHQ/AdminLTE)
+ [Fontawesome](https://github.com/FortAwesome/Font-Awesome)
+ [jquery](https://github.com/jquery/jquery)
+ [bootstrap](https://github.com/twbs/bootstrap)
+ [icheck-bootstrap](https://github.com/bantikyan/icheck-bootstrap)
