# Instruções para minikube


### Passo 1

    1) Executar `minikube start --extra-config=kubelet.housekeeping-interval=10s`
    2) Executar `minikube tunnel`
    3) Executar `minikube dashboard`

### Passo 2 

    1) Dentro da pasta `k8s`, executar todos os yamls com o comando `kubectl apply -f arquivo.yaml`
    2) De preferência na ordem
       1) ingress
       2) mongo
       3) zookeeper
       4) kafka
       5) demais yamls

### Passo 3

    1) Para setar o autoscale do pod use `kubectl autoscale deployment nome-deployment --cpu-percent=50 --min=1 --max=10`
    2) `kubectl get hpa` para verificar as métricas de scaling definidas
    3) `kubectl top pods` para verificar os pods que foram subidos

### Passo 4

    1) Siga a instalação em `https://docs.locust.io/en/stable/installation.html` para o locust
    2) Para usar o locust, entre na pasta `basic-loading-test`
    3) execute o comando `locust -f .\nome-do-pyton.py` para o teste escolhido