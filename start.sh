#!bin/bash

minikube start --alsologtostderr --v=1
minikube addons enable ingress
eval $(minikube docker-env)

docker build -t service2:latest services_cs_projects/Service2/.
docker build -t service1:latest services_cs_projects/Service1/.

kubectl apply -f kafka_setup/
kubectl apply -f service_and_deployment/

