import time
from locust import HttpUser, task, between


class WebsiteUser(HttpUser):
    wait_time = between(1, 5)
	
    @task
    def on_start(self):
        self.client.post("/api", json={
    "cpf": "07619933980",
    "nomePessoa": "Lucas",
    "dataNascimento": "1993-07-25T00:37:22.841Z",
    "nomeVacina": "Pfizer",
    "dose": 1,
    "dataAgendamento": "2022-06-15",
    "dataAplicada": "2022-06-15"
    })
        self.client.post("/api", json={
    "cpf": "06086088900",
    "nomePessoa": "Guilherme",
    "dataNascimento": "1991-03-12T00:37:22.841Z",
    "nomeVacina": "Jensen",
    "dose": 1,
    "dataAgendamento": "2022-06-15",
    "dataAplicada": "2022-06-15"
    })