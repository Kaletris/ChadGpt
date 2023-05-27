# Messenger

## Feladat [2-3 mondat]

Olyan alkalmazás amelyen threadekre lehet üzeneteket küldeni és reagálni rájuk.

## A kisháziban elérhető funkciók [adatmódosítással járó is legyen benne]
- Felhasználói fiók létrehozása és belépés.
- Threadek megnyitása, olvasása, üzenetek beküldése.
- Üzenetek szerkesztése
- Üzenetekre reagálás

## Adatbázis entitások [min. 3 db.]
- Felhasználók
- Threadek
- Üzenetek
- Reakciók

## Alkalmazott alaptechnológiák
- adatelérés: Entity Framework Core v7
- kommunikáció, szerveroldal: ASP.NET Core v7
- kliensoldal: Android

## Továbbfejlesztési tervek [opcionális, a pontrendszerből érdemes válogatni. Célja, hogy KHF bemutatáskor a felmerülő kérdéseket megbeszélhessük]
- Szerver oldali autentikáció. Saját token provider készítése, használata esetén nem jár pont.<br>
Token alapú, ASP.NET Core Identity + Duende Server/IdentityServer5<br>
egyéb kliens esetén [12]

- Publikálás docker konténerbe és futtatás konténerből [7]

- SignalR Core alkalmazása valós idejű, szerver felől érkező push jellegű kommunikációra [7]
- teljes szerveroldal hosztolása külső szolgáltatónál [5-7]
Azure (ingyenes App Services - WebApp szolgáltatás) 7

- az API funkciók egy részének elérhetővé tétele gRPC HTTP/2 vagy gRPC-Web hívásokon keresztül. Szemléltetés példahívásokkal kliensből vagy gRPC teszteszközből (pl. bloomrpc) Azure App Service-szel, IIS-sel, böngészős klienssel korlátozottan kompatibilis! [7]
- hosztolás Azure-ban
- OpenAPI leíró (swagger) alapú dokumentáció
- nem nullozható referencia típusok (NRT) kényszerítése a nullable context bekapcsolásával minden szerveroldali projektre és minden nullable context sértés figyelmeztetés hibaként kezelése. Nullable context kikapcsolása projekten belül csak indokolt esetekben.