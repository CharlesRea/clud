# clud

The solution to all your deployment concerns. Maybe.

By [Alex Potter](https://github.com/AlexJPotter) and [CharlesRea](https://github.com/CharlesRea).

## Technologies used:
* .NET
* GRPC
* Blazor
* Kubernetes

## Development environment setup

Very much a work in progress currently. These instructions are written assuming a Windows machine - it should
be possible to install on Linux, but some tweaks may be needed.

Have a readthrough of the [Design Spec](./docs/01_DesignSpec.md) to understand the project motivation and infrastructure.

### Install Prerequisites:
* [Docker Desktop](https://www.docker.com/get-started)
* A local Kubernetes cluster, version 1.18+. 
  * The recommended way to set this up is to use [minikube](https://minikube.sigs.k8s.io/docs/start/).
  * On Windows, we use the HyperV Minikube driver. Install minikube (`choco install minikube`)
    and run `minikube start --driver=hyperv` in an admin terminal.
* [Helm](https://helm.sh/docs/intro/install/)
  * (Helm is a package manage for Kubernetes, allowing deploying public application definitions from the Helm repository)
* [.NET Core 5.0 SDK](https://dotnet.microsoft.com/download)
* [Node 14+](https://nodejs.org/en/)
* [Yarn v1](https://classic.yarnpkg.com/lang/en/)

### Infrastructure setup
* Setup a self-signed certificate (used for HTTPS), and add it as a Kubernetes secret (On Windows, you might need to run the following commands in Git Bash or equivalent)
  * Run `infrastructure/dev/create-self-signed-cert.sh`
  * Trust the certificate. In Windows, double click the `infrastructure/dev/certs/tls.crt` file, click Install Certificate, choose Local Machine, choose
    "Place all certs in the following store", choose "Trusted Root Certification Authorities".
* Run `./build CreateLocalInfrastructure`. This will set up:
  * [Traefik](https://docs.traefik.io/), a reverse proxy service. All external requests to the cluster will arrive at
    Traefik, which will route the traffic to the correct Kubernetes service based on the request hostname. (In Kubernetes
    terms, it's an Ingress controller.
  * Postgres DB, accessible on your local machine (once you've set up hosts entries as below) at
    `Host=clud.local;Port=30432;Database=clud;Username=clud;Password=supersecret`
  * A Docker registry
* It may take a few minutes for the resources to be created while k8s downloads the docker images - run `kubectl get pod -n clud` to check when these resources have been created.
* clud services are exposed at subdomains of `clud.local`. You'll need to add hosts file entries to point this at your kubenernetes cluster:
  * In an admin terminal, run `minikube ip` to get the Minikube cluster IP
  * Add a hosts file entry for `<minikube-ip> clud.local clud.clud.local traefik.clud.local registry.clud.local` (in Windows, the hosts file is at `C:\Windows\System32\drivers\etc\hosts`)
  * Check you can access http://traefik.clud.local and  https://traefik.clud.local.
* On Windows, our local Docker engine cannot directly talk to Minikube (as they're in different HyperV
  containers, and so different networks). So we need to set up a proxy to allow them to communicate.
  * Run (and keep running) `./build RegistryProxy`
  * To test this worked succesfully:
    * `docker pull zerokoll/helloworld`
    * `docker tag zerokoll/helloworld  localhost:5002/zorokoll/helloworld`
    * `docker push localhost:5002/zorokoll/helloworld`

### Setup the database
* Run `./Build.ps1 RebuildDatabase`. This will recreate the database from scratch, wiping any existing schema & data.

### Running the Clud server
* In `src/Web`, run `yarn watch` to compile the CSS
* In `src/Api`, `dotnet watch run` to run the API and Blazor server
* Open https://localhost:5001/. You should see the Clud frontend

### Deploy through the CLI
To run the CLI, in `src/Cli` run `dotnet run -- <arguments>`. 
* e.g, to run a deployment, run `dotnet run -- deploy <path-to-cludfile>`. Sample deployments can be found in the `samples` directory.

### Optional IDE setup & tooling
We've developed this using Jetbrains Rider. There are various plugins you can install to make your life easier. Similar
plugins may be available for other IDEs if that's what you're into.
* Install https://plugins.jetbrains.com/plugin/8277-protobuf-support

Recommended GUI for dev-ing GRPC services: https://github.com/uw-labs/bloomrpc


### Debugging
`minikube dashboard`
