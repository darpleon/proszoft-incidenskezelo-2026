#  AI-támogatott incidenskezelő rendszer

A projekt célja egy többfelhasználós incidenskezelő rendszer megtervezése és megvalósítása, amely egy szimulált informatikai környezetből érkező technikai eseményeket fogad, feldolgoz, összekapcsol és incidensekké szervez. A rendszer támogatja az incidensek életciklusát, prioritását, jogosultságkezelését, keresését és auditálható történetét. A fejlesztés során kiemelt mérnöki feladat az ismételten vagy nem megfelelő sorrendben érkező események, az egyidejű módosítások és adatkonzisztencia, valamint a részleges meghibásodások kezelése. A rendszer része egy AI-alapú incidensasszisztens, amely 3–5, a rendszer által biztosított funkció segítségével tényleges incidens- és eseményadatokat kérdez le, ezek alapján összefoglalót, lehetséges okokat és vizsgálati javaslatokat készít strukturált formában. Az AI nem autonóm döntéshozó: működésének kontrolláltnak, jogosultságérzékenynek és visszakövethetőnek kell lennie. A projekt része a szimuláció, az automatizált tesztelés, az AI működésének kiértékelése, a műszaki dokumentáció és a rendszer bemutatása. 

---
## Struktúra
```
./incidenskezelo-frontend
./incidenskezelo-backend
./szimulacio
    /frontend
    /backend
```