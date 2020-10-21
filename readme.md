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
  * On Windows, we use the HyperV Minikube driver. Install minikube (`choco install minikube`) and [Docker](https://docs.docker.com/get-docker/), 
    and run `minikube start --driver=hyperv --insecure-registry "10.0.0.0/24"` in an admin terminal.
* [Helm](https://helm.sh/docs/intro/install/)
  * (Helm is a package manage for Kubernetes, allowing deploying public application definitions from the Helm repository)
* [.NET Core 3.1 SDK](https://dotnet.microsoft.com/download)

### Infrastructure setup
* Clone the repository
* Setup a self-signed certificate, and add it as a Kubernetes secret (On Windows, you might need to run the following commands in Git Bash or equivalent)
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
* Add hosts file entries:
  * In an admin terminal run `minikube ip` to get the Minikube IP
  * Add a hosts file entry for `<minikube-ip> clud.local clud.clud.local traefik.clud.local registry.clud.local` (in Windows, the hosts file is at `C:\Windows\System32\drivers\etc\hosts`)
  * Check you can access http://traefik.clud.local,  https://traefik.clud.local
* Unfortunately, on Windows, our local Docker engine cannot directly talk to Minikube (as they're in different HyperV
  containers, and so different networks). So we need to set up a proxy to allow them to communicate.
  * Run (and keep running) `./build RegistryProxy`
  * To test this worked succesfully:
    * `docker pull zerokoll/helloworld`
    * `docker tag zerokoll/helloworld  localhost:5002/zorokoll/helloworld`
    * `docker push localhost:5002/zorokoll/helloworld`

### Setup the database
* Run `./Build.ps1 RebuildDatabase`

### Running the Clud server
* In `src/Web`, run `yarn watch` to compile the CSS
* In `src/Api`, `dotnet watch run` to run the API and Blazor server
* Open https://localhost:5001/. You should see the Clud frontend

### Test a deployment
* In `src/Cli`, `dotnet run -- deploy`

### Optional IDE setup & tooling
We've developed this using Jetbrains Rider. There are various plugins you can install to make your life easier. Similar
plugins may be available for other IDEs if that's what you're into.
* Install https://plugins.jetbrains.com/plugin/8277-protobuf-support

Recommended GUI for dev-ing GRPC services: https://github.com/uw-labs/bloomrpc


### Debugging
`minikube dashboard`
