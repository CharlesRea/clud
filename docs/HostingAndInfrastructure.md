# Hosting & Infrastructure

Clud runs on a single-node Kubernetes cluster set up on the clud-master VM. This is a Ubuntu box, set up with 100GB disk space, 16GB RAM and 8 CPUs. Charles and Alex are admins, and can log in with domain credentials.

The VM has a static IP of 10.250.8.45. DNS entry exist to point clud.ghyston.com and *.clud.ghyston.com at this IP.

See below for the steps carried out to set up the infrastructure initially.

## Deployments
To deploy a new version of the clud server:
* Obtain a kubeconfig file to allow access Kubernetes API access to the cluster, and place it at `prod-kube-config`.
  * Eg run `scp clud.ghyston.com:/home/chr/.kube/config prod-kube-config`
* Run `./build Deploy --Environment Production --SqlConnectionString "Host=clud.ghyston.com;Port=30432;Database=clud;Username=clud;Password=<password>"`

## Infrasturcture setup instructions

### Kubernetes cluster setup
Kubernetes was bootstrapped using the kubeadm tool. The steps below were based on https://kubernetes.io/docs/setup/production-environment/tools/kubeadm/install-kubeadm/

#### Install Docker
Based on https://kubernetes.io/docs/setup/production-environment/container-runtimes/

Disable swap:
* Comment out the swap line in `/etc/fstab`
* `sudo swapon -a`
* `sudo reboot`

Run the following:
```bash
cat <<EOF | sudo tee /etc/sysctl.d/k8s.conf
net.bridge.bridge-nf-call-ip6tables = 1
net.bridge.bridge-nf-call-iptables = 1
EOF
sudo sysctl --system
```

Install Docker:
```bash
## Set up the repository:
### Install packages to allow apt to use a repository over HTTPS
sudo apt-get update 
sudo apt-get install -y apt-transport-https ca-certificates curl software-properties-common gnupg2

### Add Docker’s official GPG key
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo apt-key add -

### Add Docker apt repository.
sudo add-apt-repository \
  "deb [arch=amd64] https://download.docker.com/linux/ubuntu \
  $(lsb_release -cs) \
  stable"

# TODO used eoan here

## Install Docker CE.
apt-get update && apt-get install -y \
  containerd.io=1.2.13-1 \
  docker-ce=5:19.03.8~3-0~ubuntu-$(lsb_release -cs) \
  docker-ce-cli=5:19.03.8~3-0~ubuntu-$(lsb_release -cs)

# Setup daemon.
cat > /etc/docker/daemon.json <<EOF
{
  "exec-opts": ["native.cgroupdriver=systemd"],
  "log-driver": "json-file",
  "log-opts": {
    "max-size": "100m"
  },
  "storage-driver": "overlay2"
}
EOF

mkdir -p /etc/systemd/system/docker.service.d

# Restart docker.
systemctl daemon-reload
systemctl restart docker
```

### Install kubeadm

```bash
sudo apt-get update && sudo apt-get install -y apt-transport-https curl
curl -s https://packages.cloud.google.com/apt/doc/apt-key.gpg | sudo apt-key add -
cat <<EOF | sudo tee /etc/apt/sources.list.d/kubernetes.list
deb https://apt.kubernetes.io/ kubernetes-xenial main
EOF
sudo apt-get update
sudo apt-get install -y kubelet kubeadm kubectl
sudo apt-mark hold kubelet kubeadm kubectl
```

### Install kubernetes

```bash
 sudo kubeadm init --pod-network-cidr=10.244.0.0/16
 ```

Run the command returned from kubeadm init, eg
```bash
kubeadm join 10.250.13.114:6443 --token <token> \
  --discovery-token-ca-cert-hash <hash>
```

To start using your cluster, you need to run the following as a regular user:

```bash
  mkdir -p $HOME/.kube
  sudo cp -i /etc/kubernetes/admin.conf $HOME/.kube/config
  sudo chown $(id -u):$(id -g) $HOME/.kube/config
```

You should now deploy a pod network to the cluster.
Run "kubectl apply -f [podnetwork].yaml" with one of the options listed at:
  https://kubernetes.io/docs/concepts/cluster-administration/addons/

Then you can join any number of worker nodes by running the following on each as root:

```bash
kubeadm join 10.250.8.45:6443 --token <token> \
    --discovery-token-ca-cert-hash <hash>
```

```
kubectl apply -f https://raw.githubusercontent.com/coreos/flannel/master/Documentation/kube-flannel.yml
```

```
kubectl taint nodes --all node-role.kubernetes.io/master-
```

https://kubernetes.io/docs/setup/production-environment/tools/kubeadm/create-cluster-kubeadm/


### Set up Clud

Source an SSL certificate for *.clud.ghyston.com (see Wiki)

Create a clud namespace:
* `kubectl create ns clud`
* `kubectl -n clud create secret generic clud-postgres-user --from-literal=username=clud --from-literal=password=<password>`
* `kubectl -n clud create secret tls traefik-tls-cert --key=clud.ghyston.com.key --cert=clud.ghyston.com.crt`

Install helm: 
* `sudo snap install helm --classic`
* `helm repo add stable https://kubernetes-charts.storage.googleapis.com/`
* `helm repo update`

Install OpenEBS (this is used to enable local disk storage for PVCs)
* Ensure iSCSi servers are configured: `sudo systemctl enable iscsid && sudo systemctl start iscsid`
* `helm install openebs stable/openebs --version 1.10.0 --namespace clud`

From your dev machine:
* Setup the production kubeconfig file (see above)
* Run `./build CreateRegistry --environment Production`
* Run `./build DeployCludInfrastructure --environment Production`
* Run `./build Deploy --Environment Production --SqlConnectionString "Host=clud.ghyston.com;Port=30432;Database=clud;Username=clud;Password=<password>"`

### Set up CI
TODO: This is still a WIP

Set up Drone CI (based on https://github.com/drone/charts/blob/master/charts/drone/docs/install.md): 
* Create a new Github OAuth app: https://github.com/settings/applications/new
  * For login URL, use https://drone.clud.ghyston.com/login
* Grab the client ID and client secret shown in Github
* `kubectl create secret generic drone-github-secrets -n clud --from-literal=DRONE_GITHUB_CLIENT_ID="<github-client-id>" --from-literal=DRONE_GITHUB_CLIENT_SECRET="<github-client-secret>"`
* Generate an RPC secret for Drone server to communicate with workers: `openssl rand -hex 16`
* `kubectl create secret generic drone-rpc-secrets -n clud --from-literal=DRONE_RPC_SECRET="<rpc-secret>"`
* `helm repo add drone https://charts.drone.io`
* `helm repo update`
* `helm install -n clud drone drone/drone -f infrastructure/prod/drone-server-values.yaml --version 0.1.5`
* `helm install -n clud drone-runner drone/drone-runner-kube -f infrastructure/prod/drone-runner-values.yaml --version 0.1.2`
