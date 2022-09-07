import time
from locust import HttpUser, task, between
from locust_plugins.csvreader import CSVReader

ssn_reader = CSVReader("dados.csv")

class WebsiteUser(HttpUser):
    wait_time = between(1, 5)
	
    @task
    def on_start(self):
        customer = next(ssn_reader)
        self.client.post("/api", json={
        "cpf": customer[0],
        "nomePessoa": customer[1],
        "dataNascimento": customer[2],
        "nomeVacina": "Pfizer",
        "dose": 1,
        "dataAgendamento": "2022-12-15",
        "dataAplicada": "2022-12-15"
        })