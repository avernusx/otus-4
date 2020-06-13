helm install nginx stable/nginx-ingress -f ingress.yaml --atomic

helm install prom stable/prometheus-operator -f prometheus.yaml --atomic --namespace=monitoring

grafana: admin / prom-operator
kubectl port-forward --namespace=monitoring service/prom-grafana 9000:80 
kubectl port-forward --namespace=monitoring service/prom-prometheus-operator-prometheus 9090
kubectl apply -f grafana.yaml --namespace=monitoring