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

Have a readthrough of the [Design Spec](./docs/01_DesignSpec.md) to understand the project motivation and infrastructure.

### Install Prerequisites:
* A local Kubernetes cluster, version 1.18+. 
  * The recommended way to set this up is to use [minikube](https://minikube.sigs.k8s.io/docs/start/).
    On Windows, we use the HyperV Minikube driver. Install minikube (`choco install minikube`) and [Docker](https://docs.docker.com/get-docker/), 
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
* Clone the repository
* Run `kubectl apply -f infrastructure/traefik.yaml`
  * [Traefik](https://docs.traefik.io/) is a reverse proxy service. All external requests to the cluster will arrive at
    Traefik, which will route the traffic to the correct Kubernetes service based on the request hostname. In Kubernetes
    terms, it's an Ingress controller)
* Setup Clud's postgres database:
    * Run `kubectl apply -f infrastructure/postgres.yaml`
* Setup a self-signed certificate, and add it as a Kubernetes secret (you might need to run the following commands in Git Bash or equivalent)
  * `openssl req -x509 -nodes -days 3650 -newkey rsa:2048 -keyout tls.key -out tls.crt -subj "/CN=*.clud"`
  * `kubectl -n kube-system create secret tls traefik-tls-cert --key=tls.key --cert=tls.crt`
  * Trust the certificate. In Windows, double click the `tls.crt` file, click Install Certificate, choose Local Machine, choose
    "Place all certs in the following store", choose "Trusted Root Certification Authorities".
* Add hosts file entries:
  * In an admin terminal run `minikube ip` to get the Minikube IP
  * Add a hosts file entry for `<minikube-ip> clud traefik.clud postgres.clud`
  * Check you can access http://traefik.clud,  https://traefik.clud (you may get a security warning as modern browsers don't like self-signed certificates - just click through)
* Setup the Docker registry, using Minikube's built in registry addon:
  * `minikube addons enable registry`
<!-- TODO Investigate if there's a nicer way to do this -->
* Unfortunately, on Windows, our local Docker engine cannot directly talk to Minikube (as they're in different HyperV
  containers). So we need to expose the registry to our local network with `kubectl port-forward`, and then create a
  Docker image to proxy the network call within the Docker engine back to our host network. (Instructions taken from the 
  [Minikube docs](https://minikube.sigs.k8s.io/docs/handbook/registry/))
  * Run `kubectl get pod -n kube-system` to get the registry pod (should be called `registry-XXXX`)
  * Run the following (and keep it running): `kubectl port-forward --namespace kube-system <registry-pod-name> 5002:5000`
  * Run the following (and keep it running): `docker run --rm -it --network=host alpine ash -c "apk add socat && socat TCP-LISTEN:5002,reuseaddr,fork TCP:host.docker.internal:5002"`
  * Create a hostsfile entry: `127.0.0.1 registry.clud`
  * To test this worked succesfully:
    * `docker pull zerokoll/helloworld`
    * `docker tag zerokoll/helloworld  registry.clud:5002/zorokoll/helloworld`
    * `docker push registry.clud:5002/zorokoll/helloworld`

### Setup the database
* Run `./Build.ps1 RebuildDatabase`

### Running Clud
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
