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
* A local Kubernetes cluster, version 1.18+. The recommended way to set this up is to use [minikube](https://minikube.sigs.k8s.io/docs/start/). Install minikube and [Docker](https://docs.docker.com/get-docker/), and run `minikube start --driver=docker`. Note that as of April 2020, we cannot use the Kubernetes built into Docker Desktop, since it is an older version (1.14).
* .NET Core 3.1 SDK

### Running it
* In `src/Deployment`, `dotnet run`
* In `src/Cli`, `dotnet run -- deploy`

### Optional IDE setup
Assuming using Rider:
* Install https://plugins.jetbrains.com/plugin/8277-protobuf-support

Recommended GUI for deving GRPC services: https://github.com/uw-labs/bloomrpc