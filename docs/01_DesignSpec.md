# Clud Design Specification

Clud is a tool to provide pain-free deployment and infrastructure management for small projects.
It's intended to be used within Ghyston to allow devs to easily host their side projects, but could
be run in any company / on the cloud.

It was also the result of several drunken conversations at the NDC London 2020 conference. This document
is an attempt to draw together the ideas that seemed so good at the time, and turn them into something real.

## Motivation

Side projects are a great way to learn a new technology, or to show off what you can do. We want to
make it as easy as possible to have a great idea, and turn it into a working solution. 
But deploying side projects is a pain. You want to avoid having to spend any money. You don't want
to have to spend days researching various deployment technologies, as that's a great way to burn out
and not actually produce anything.

Deploying to traditional servers is a non-starter as then you need to find a server, and spend
time configuring it. You can deploy to a VM within Ghyston, but means raising a support request
to create a VM every time you think of a project. A shared company VM for all side projects could work,
but then you have issues of handling global dependencies etc on a single box.
IaaS solutions are easy to set up, but are generally not free (or have significant restrictions on 
the free tier, such as limited uptime).

Containers and Docker offer a good solution, but:
* There's a steep learning curve. If you're not doing your side project to learn Docker, it's
  not ideal having that being a big blocker on deployments.
* You still need somewhere to run those containers. That's not particularly easy within the company
  network without further work, and standard cloud solutions cost money.
* You probably have several services within an app. Web servers, DBs, Redis, etc. That means
  multiple containers, which means you need a way of orchestrating containers and handling routing
  between them. Kubernetes is the obvious solution to that, but it's very complicated to set up.

Often you want the project to be only visible to the company - it may contain sensitive
info, or not be something that you want globally accessible. Having to put in Ghyston authentication adds another
barrier to deployment, and otherwise deploying within the company network rules out cloud solutions.

These problems are solvable, but present a barrier to entry. So what if we could make it easier?
What if there was a centralised tool that could let you click one button, and have your service
automatically deployed somewhere? That tool would need to do the following:

* One click deployments to somewhere within the company network
* Understand common project models. I should be able to point it at an ASP.NET project, or a Create React
  App site, or a Spring Boot project, and have the tooling know how to produce a running piece of software
  just from the code.
* Be exensible for non-standard projects. Let me declare the steps to build and run my code. In practice,
  this can be done by accepting a Dockerfile.
* I can run standard tools, including DBs, Redis, etc. Since everything has a Docker image these days, we
  can again do this by just giving the option of running a Docker image.
* Support running multiple services together. If I need to run a web server, a DB, other persistance (e.g. Redis),
  a monitoring service (e.g. Zipkin, Seq), then the tool can run all these alongside one another, and
  ensure traffic can be routed between services.
* Let me easily run up the entire application locally. If I have to declare a way of deploying multiple
  services together, it would be useful if I can run a single command and get the same infrastructure
  running on my local machine.
* Be simple to configure. In particular, I definitely don't want to need to learn a ton about Docker / k8s
  just to deploy my app.

Enter Clud.

## Technical design

This is intentionally very overengineered, since it gives us an excuse for us to play around with some 
new techs.

Clud consists of the following:
* A CLI tool, which allows deploying an application to the Clud infrastructure. Written in C#.
* A management web application, to view and manage details for the deployed applications. Written in Blazor.
* A deployment service, responsible for actually getting applications deployed and running.
* A Kubernetes cluster, responsible for running the actual applications
* A container registry, holding Docker images

Clud uses a container based deployment model with Docker since it makes a bunch of things
easy. It uses Kubernetes to orchestrate the containers, since k8s seems pretty good and we'd
like to play with it to learn more about it.

Configuration is a via a single YAML file. Internally, Clud translates this to some form of k8s 
configuration.

### CLI
Written in C#, running on .NET Core. It'll require you to have Docker installed locally, as it'll 
build Docker images locally for the deployment.

Needs to support running on Windows and Linux.

`clud init` - Scaffolds an initial configuration file. It'd be really nice if it could do this 
automatically based off your code - e.g. detect any C# web projects, stick an entry in the config
file for that.

`clud deploy` - Initiates a deployment based on your config file. Builds Docker images for your
projects and uploads them to the container registry. Then talks to the deployment service to kick
off the deployment.

`clud local` - Runs your application locally. This relies on you having k8s set up locally.
(QQ this command needs a better name). Should expose a way to skip running one component - e.g.
`--skip web`. This lets you run every component except your main project, which you may want to run
more explicitly through your IDE / standard development tools, for a better development experience
(e.g. through `dotnet watch run`).

`clud eject <serviceName>` - Takes a service definition pointing at a project file, and ejects the
configuration to produce a customisable Dockerfile. This allows making manual edits to the
deployment process assumed by Clud.

### Service configuration
The CLI will look for a `clud.yml` file sitting in the project root, with the schema:

```yml
name: applicationName
services: # Rather than a map, we could just have an array and add name property.
  web: # Services are exposed to each other as DNS names - i.e. this service is accessible from other services using the hostname 'web'
    project: ./src/Web/Web.csproj # Clud knows how to turn this into a Docker image.
    replicas: 2 # optional, defaults to 1
    ports:
      - port: 80 # Expose port 80
      - port: 8080 # Publically expose port 80, mapping internally to port 8081
        targetPort: 8081
    env: # Map of environment variables to supply to the image
      ASPNETCORE__ENVIRONMENT: Production
  api:
    dockerfile: ./src/Api/dockerfile # Custom dockerfile support
    ports:
      - port: 5000
  db:
    dockerImage: postgresql # Use a prebuilt Docker image, from the Docker registry
    ports:
      - port: 5432
    expose: false # Expose an Ingress endpoint for public access. Defaults to true
entryPoint: web # The main entry point for the app, accessible externally at applicationName.clud
```

### Management web application
A web app, allowing general management of applications. A Blazor web site - I think
we can probably get away without having much of a backend here, it'll just talk directly
to the deployment service.

Functionality includes:
* View list of running applications
* View an individual application. View uptime, deployed time, etc.
* Restart an application
* View application logs, collected from stdout. With support for filtering by service,
  using correlation IDs for distributed tracing maybe?
* Delete an application

### Deployment service
The core service. Responsible for handling deployment requests, talking to Kubernetes, etc.

ASP.NET Core web service. Exposes GRPC endpoints, which the CLI and management web service talk
to. Has a DB for actually storing application details.

### Kubernetes cluster
A Kubernetes cluster, set up with:
* [Traefik](https://docs.traefik.io/) - An ingress service. This will handle routing external
  requests from outside the cluster to the correct service. Each deployed service in an app will
  be accessible at `service.appName.clud`. An entry service can be defined, so that it is accessible
  at `appName.clud`.
* A Docker registry - storing the Docker images.

Internally, a Clud application maps to Kubernetes primitives as follows:
* A separate namespace is set up for each application
* For each Clud service, a Kubernetes deployment is set up, running a pod. A Kubernetes service points
  at the pod, and optionally an ingress rule exposes the service externally.

## Things we need to figure out
* Volume mapping, so that DB images can have data persisted between deployments
* App level configuration - injection of environment variables
* App secret management
* Environment management - how can we vary configuration values between environments? Do we even
  need to?

## Future things it might be nice to do
* CLI support to scaffold additional components - e.g. `clud add postgres` to automatically add
  a Postgres entry to your config file. We'd probably support a list of hardcoded tools, so that
  we know how to set up the config file. Things we'd want to consider include PostgreSQL, SQL Server,
  Redis, Zipkin, Seq, RabbitMQ.
* Support hosting Clud on non-Ghyston infrastructure. Need to allowing configuring the CLI on where
  it should look for the Clud service.
* Rolling deployments? In the super far future, if only because it would be cool.

## Things Clud won't do
* Support building the Docker images on the Clud server, meaning you don't need Docker running 
  locally. I think given that Docker is so commonly used in development these days, is's fairly
  safe to assume that it exists on dev machines / CI servers, so this seems unnecessary.
