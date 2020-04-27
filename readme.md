# Clud

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

### Install Prerequisites:
* A local Kubernetes cluster, version 1.18+. 
  * The recommended way to set this up is to use [minikube](https://minikube.sigs.k8s.io/docs/start/).
    On Windows, we use the HyperV Minikube driver. Install minikube and [Docker](https://docs.docker.com/get-docker/), 
    and run `minikube start --driver=hyperv` in an admin terminal.
  * Note that as of April 2020, we cannot use the Kubernetes built into Docker Desktop,
    since it is an older version (1.14).
  * Charles ran into an "IP not found" starting Minikube. To fix, had to manually create an External network switch
    in the Hyper-V Manager, set the created VM to use that network switch, and rerun `minikube start`.
* [Helm](https://helm.sh/docs/intro/install/)
  * You'll need to add the the Stable Helm repo: run `helm repo add stable https://kubernetes-charts.storage.googleapis.com/`
  * (Helm is a package manage for Kubernetes, allowing deploying public application definitions from the Helm repository)
* [.NET Core 3.1 SDK](https://dotnet.microsoft.com/download)

### Infrastructure setup
* Run `kubectl apply -f infrastructure/traefik.yaml`
  * [Traefik](https://docs.traefik.io/) is a reverse proxy service. All external requests to the cluster will arrive at
    Traefik, which will route the traffic to the correct Kubernetes service based on the request hostname. In Kubernetes
    terms, it's an Ingress controller)
* Setup a self-signed certificate, and add it as a Kubernetes secret:
  * `openssl req -x509 -nodes -days 3650 -newkey rsa:2048 -keyout tls.key -out tls.crt -subj "/CN=*.clud"`
  * `kubectl -n kube-system create secret tls traefik-tls-cert --key=tls.key --cert=tls.crt`
  * Trust the certificate. In Windows, double click the `tls.crt` file, click Install Certificate, choose Local Machine, choose
    "Place all certs in the following store", choose "Trusted Root Certification Authorities".
* Run `minikube ip` to get the Minikube IP.
* Add a hosts file entry for `<minikube-ip> traefik.clud`
* Check you can access http://traefik.clud and https://traefik.clud.
* QQ WIP:
  * Install Docker registry Helm chart: `helm install docker-registry stable/docker-registry`

### Running Clud
* In `src/Deployment`, `dotnet run`
* In `src/Cli`, `dotnet run -- deploy`

### Optional IDE setup & tooling
We've developed this using Jetbrains Rider. There are various plugins you can install to make your life easier. Similar
plugins may be available for other IDEs if that's what you're into.
* Install https://plugins.jetbrains.com/plugin/8277-protobuf-support

Recommended GUI for dev-ing GRPC services: https://github.com/uw-labs/bloomrpc