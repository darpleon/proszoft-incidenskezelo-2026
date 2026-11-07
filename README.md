#  AI-támogatott incidenskezelő rendszer

A projekt célja egy többfelhasználós incidenskezelő rendszer megtervezése és megvalósítása, amely egy szimulált informatikai környezetből érkező technikai eseményeket fogad, feldolgoz, összekapcsol és incidensekké szervez. A rendszer támogatja az incidensek életciklusát, prioritását, jogosultságkezelését, keresését és auditálható történetét. A fejlesztés során kiemelt mérnöki feladat az ismételten vagy nem megfelelő sorrendben érkező események, az egyidejű módosítások és adatkonzisztencia, valamint a részleges meghibásodások kezelése. A rendszer része egy AI-alapú incidensasszisztens, amely 3–5, a rendszer által biztosított funkció segítségével tényleges incidens- és eseményadatokat kérdez le, ezek alapján összefoglalót, lehetséges okokat és vizsgálati javaslatokat készít strukturált formában. Az AI nem autonóm döntéshozó: működésének kontrolláltnak, jogosultságérzékenynek és visszakövethetőnek kell lennie. A projekt része a szimuláció, az automatizált tesztelés, az AI működésének kiértékelése, a műszaki dokumentáció és a rendszer bemutatása. 

---

## Build via docker compose

> `.env` must contain `MSSQL_SA_PASSWORD` and `JWT_KEY`

### Development version
```
docker compose up --build

# If already built
docker compose up
```

Ports exposed:
- Frontend: http://localhost:5173
- Backend API: http://localhost:5041
- Simulation: http://localhost:5161
- MSSQL: localhost,1433

#### Live builds
Runs the backend and simulation from your working tree with `dotnet watch`; source changes rebuild automatically.
```
    docker compose -f docker-compose.live.yml up -d
```

### Production version
```
    docker compose -f docker-compose.yml up --build
```

Ports exposed:
- Frontend: http://localhost:80
- Simulation: http://localhost:5161

(backend and db are not exposed to the host in production)
