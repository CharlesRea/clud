# Clud

The solution to all your deployment concerns. Maybe.

By [Alex Potter](https://github.com/AlexJPotter) and [CharlesRea](https://github.com/CharlesRea).

## Technologies used:
* .NET
* GRPC
* Blazor
* Kubernetes

## Development setup

Very much a work in progress currently.

### Development Prerequisites:
* A local Kubernetes cluster, version 1.18+. The recommended way to set this up is to use [minikube](https://minikube.sigs.k8s.io/docs/start/). Install minikube and [Docker](https://docs.docker.com/get-docker/), and run `minikube start --driver=hyperv --extra-config=apiserver.service-node-port-range=1-65535` in an admin terminal. Note that as of April 2020, we cannot use the Kubernetes built into Docker Desktop, since it is an older version (1.14).
  * Charles ran into an "IP not found" starting Minikube. To fix, had to manually create an External network switch in the Hyper-V Manager, set the created VM to use that network switch, and rerun `minikube start`.
* [Helm](https://helm.sh/docs/intro/install/)
  * You'll need to add the the Stable Helm repo: run `helm repo add stable https://kubernetes-charts.storage.googleapis.com/`
* .NET Core 3.1 SDK

### Infrastructure setup
* Run `kubectl apply -f infrastructure/traefik.yaml`
* Run `minikube ip` to get the Minikube IP.
* Add a hostsfile entry for `192.168.178.28 traefik-ui.minikube`
* Check you can access `traefik-ui.minikube`
* QQ WIP:
  * Install Docker registry Helm chart: `helm install docker-registry stable/docker-registry`

### Running it
* In `src/Deployment`, `dotnet run`
* In `src/Cli`, `dotnet run -- deploy`

### Optional IDE setup
Assuming using Rider:
* Install https://plugins.jetbrains.com/plugin/8277-protobuf-support

Recommended GUI for deving GRPC services: https://github.com/uw-labs/bloomrpc