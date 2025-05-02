# TaskManagerAPI - Test Tecnico

## Obiettivo

Implementa una Minimal API in .NET 8 che consenta di gestire task multi-tenant, salvando i dati in un file JSON locale, con particolare attenzione alla sicurezza dei dati.

## Funzionalità richieste

- POST /tasks
- GET /tasks (con filtraggio tramite header `X-Tenant-ID`
- Salvataggio dei dati in file JSON

## Funzionalità facoltative

- PUT /tasks/{id}
- Test unitari

## Extra

- Per ogni task crea un record di dati nel nostro applicativo. 

Valorizza i seguenti campi (fieldName):

	1. TASK_ID (String 255)
	2. TASK_DESCRIPTION (String 255)
	3. CREATION DATE (Date)

Al seguente link trovi lo swagger: https://services.paloalto.swiss:10443/api2/swagger/index.html

## Istruzioni

- Puoi modificare la struttura del progetto come preferisci
- Usa solo file system (niente database)
- Inserisci le tue risposte nel file `README.md` alla fine

## Domande finali

1. Hai riscontrato difficoltà? Dove?
   
Durante la scrittura dei test, ho notato che a volte uno passava e la volta successiva falliva.
Dopo aver dedicato un po’ di tempo al debugging, ho scoperto che i test venivano eseguiti in parallelo,
causando problemi di race condition e concorrenza nell’accesso ai dati. A parte questo, non ho riscontrato
particolari difficoltà.

2. Hai fatto assunzioni? Se sì, quali?

Sì, ho interpretato la richiesta GET come un'operazione per recuperare l’intera lista di task,
e non una singola task specificata tramite ID.

3. Come miglioreresti il codice se fosse un progetto reale?
   
- Attualmente i test verificano solo i casi di successo più comuni. Aggiungerei test per i casi limite e per le situazioni in cui le richieste dovrebbero fallire, ad esempio quando viene fornito un tenantId non valido;

- Al momento, i test usano il POST, che manda direttamente il record all'applicativo. In un contesto reale, sarebbe preferibile utilizzare un mock del servizio per evitare chiamate reali ad ogni esecuzione dei test;

- I valori come passwordWS e cabinetId sono hardcoded nel codice. In un ambiente di produzione, dovrebbero essere gestiti tramite variabili d’ambiente. 

4. Hai usato strumenti di supporto (AI, StackOverflow, ecc)? Se sì, come?

Sì, ho utilizzato ChatGPT come strumento di supporto, soprattutto nelle fasi iniziali, per farmi un’idea generale su come strutturare il codice e per chiarire alcuni dubbi su sintassi e logica di .NET, che non conoscevo bene. L’ho trovato utile come guida per partire, ma ho poi adattato il codice alle specifiche del progetto man mano che approfondivo la comprensione del contesto. Per un paio di problemi specifici, ho anche consultato documentazione ufficiale e StackOverflow.