# dmr-webservice
DMR (Dansk Motor Register) Webservice (Scraper)

Scraperen henter data fra Motorregistret og pakker dem ind i et typestærkt retur-objekt.

## Installation
Installeres på IIS webserver eller lign server med .NET 8, som kan hoste et ASP.NET website.

Der medfølger også en eksempel dockerfile til at bygge et docker image.

## Brug
```API
/api/{nummerplade} giver oplysninger om køretøj
```

# Fork
Dette er en fork af [https://github.com/Montago/dmr-webservice](https://github.com/Montago/dmr-webservice) & [https://github.com/alex191a/dmr-webapi](https://github.com/alex191a/dmr-webapi). Opgraderet til .NET 8 og med inkluderet forsikrings informationer.
