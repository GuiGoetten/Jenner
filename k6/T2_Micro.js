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
    duration: "1800s", target: 10000
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

    //Microsservices
    let res = http.post("http://apps.minikube.vacinacao.aplicar:30910/api", final, {
        headers: {
          'Content-Type': 'application/json',
        },
      });
    sleep(10);
}