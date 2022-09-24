import papaparse from 'https://jslib.k6.io/papaparse/5.1.1/index.js';
import http from 'k6/http';
import { sleep } from 'k6';
import { SharedArray } from 'k6/data';

//const data = open("./dados.csv");

const csvData = new SharedArray('teste', function () {
    // Load CSV file and parse it using Papa Parse
    return papaparse.parse(open('./dados.csv'), { header: false }).data;
  });

export let options = {
    insecureSkipTLSVerify: true,
    noConnectionReuse: false,
    stages: [
        // Ramp-up from 1 to 60 VUs in 60s
        { duration: "10s", target: 500 },
        { duration: "10s", target: 1000},
        { duration: "10s", target: 1500 },
        { duration: "10s", target: 2000 }
    ]
};

export default function() {
    let randomUser = csvData[Math.floor(Math.random() * csvData.length)];

    let final = JSON.stringify({
        "cpf": randomUser[0],
        "nomePessoa": randomUser[1],
        "dataNascimento": randomUser[2],
        "nomeVacina": "Pfizer",
        "dose": 1,
        "dataAgendamento": "2022-11-15",
        "dataAplicada": "2022-11-15"
    })
    //console.log(final);

    let res = http.post("http://apps.minikube.vacinacao.aplicar/api", final, {
        headers: {
          'Content-Type': 'application/json',
        },
      });
    sleep(1);
}