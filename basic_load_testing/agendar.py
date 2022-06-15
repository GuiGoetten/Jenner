import time
from locust import HttpUser, task, between


class WebsiteUser(HttpUser):
    wait_time = between(1, 5)

    @task
    def on_agendar(self):
       self.client.post("/api", json={
  "cpf": "07619933980",
  "nomePessoa": "Teste",
  "dataNascimento": "2022-06-15",
  "nomeVacina": "Aztrazeneca",
  "dose": 1,
  "dataAgendamento": "2022-06-15"
})

