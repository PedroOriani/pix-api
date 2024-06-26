# Pix API
<h1>Description</h1>

  <p>Pix API is an application that enables users to create their Pix keys in their bank accounts for certain banks (PSP), make payments, and perform data verification.</p>

  <h3>Commands</h3>
To properly initialize the application, execute the following commands:

```bash
dotnet restore
"Add the database information to the appsettings.json file and run the migrations"
dotnet ef migrations add <migration_name>
dotnet ef database update
dotnet build
```

<h1>Route</h1>
<h2>Keys</h2>
• <strong>Create Key:</strong> Registers a new Pix key.


    POST /keys

```bash
Body:

{
  "key": {
    "value": string,
    "type": string // CPF, Email, Phone or Random
  },
  "user": {
    "cpf": string
  },
  "account": {
    "number": string,
    "agency": string
  }
}

```

• <strong>Retrieve Key:</strong> Returns all information about a specific Pix key.

    GET /keys/:type/:value

```bash
Retrieve:
{
  "key": {
    "value": string,
    "type": string // CPF, Email, Phone or Random
  },
  "user": {
    "name": string,
    "maskedCpf": string // Only the first three and last two digits
  },
  "account": {
    "number": string,
    "agency": string,
    "bankName": string,
    "bankId": string
  }
}
```


<h2>Payments</h2>
<strong>Make Payment:</strong> Registers a new payment.

    POST /Payments

```bash
Body:
{
  "origin": {
    "user": {
      "cpf": string
    },
    "account": {
      "number": string
      "agency": string,
    }
  },
  "destiny": {
    "key": {
      "value": string,
      "type": string // CPF, Email, Phone or Random
    }
  },
  "amount": integer
  "description": string // optional
}
```


<p>For this route, the use of two additional repositories is required.</p>

  ```bash
  • Consumer: https://github.com/PedroOriani/pix-payment-consumer/settings
  • Mock: https://github.com/DiegoPinho/psp-mockl
  ```

<p>In the consumer, there is a docker-compose with the RabbitMQ configuration (the messaging software used in this project). To run it, you only need to use the command:</p>

```bash
docker compose up
  ```

<p>Mock serves as a simulator for potential adverse responses that the consumer may face during processing.</p>

<h3> All three repositories and RabbitMQ must be running at the time the request occurs. </h3>

<h2>Concilliations</h2>

It compares the informations in the database to te informations of the PSP.

    POST /Concilliations

```bash
{
	"date": "yyyy-MM-dd",
	"file": "https://psp-files/transactions-2023-04-05.json",
	"postback": "https://psp-backend/conciliation/status"
}
```

<p> This route should retrive something like that:</p>

```bash
{
	"databaseToFile": [ // tem no banco e não tem no arquivo
		{"id": number, status: string}, // ...
	],
	"fileToDatabase": [ // tem no arquivo mas não tem no banco
		{"id": number, status: string}, // ...
	],
	"differentStatus": [ // está nos dois mas com status divergentes
		{"id": number}, // ...
	]
}
```

<h1>Monitoring</h1>

<h3>Commands</h3>
To properly initialize the application monitoring, follow these steps:


```bash
- Configure the Monitoring files according to your information: IP, ports, etc.
- Open two terminals: in the first one, run the application with "dotnet run" and in the other, navigate to the "/pix/Monitoring" folder and run the command "docker compose up -d".
- Access Grafana at localhost:3000.
- Configure your dashboards according to your preferences.
```
<h1>Load Test </h1>

<h3>Commands</h3>
To properly initialize load test, follow these steps:


```bash
“there are 3 tests: createKeyTest.js, getKeyInfoTest.js and payTest.js
- Go to pasta .k6/test
- Set up the .env file according to .env.example.
- npm i
- npm run seed
- k6 run <test_name>
```
