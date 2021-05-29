minikube addons enable metrics-server ## подключаем метрикс сервер, без него не будет работать

kubectl create namespace monitoring # создаем отдельный неймспейс для мониторинга, что бы не мешал основному

helm repo add prometheus-community https://prometheus-community.github.io/helm-charts # добавляем репозиторий с помощью хелм

helm install --namespace monitoring prometheus prometheus-community/kube-prometheus-stack
# инсталируем прометеус, он нужен для того что бы если шо создавать метрики кастомные и для того что бы хранить все данные по метрикам
# они потом парсятся как то в графану и там можно видеть свои кастомные чарты

# меняем контекcт
kubectl config set-context --current --namespace=monitoring

# добавляем ингресс графаны
kubectl apply -f Kuber/grafana/grafana-ingress.yaml

# порт форвардим под графаны на порт localhost:3000 что бы увидеть веб морду
kubectl port-forward --namespace monitoring service/prometheus-grafana 3000:80

# получаем пароль для графаны, логин admin
kubectl get secret --namespace monitoring prometheus-grafana -o jsonpath="{.data.admin-password}" | base64 --decode ; echo
