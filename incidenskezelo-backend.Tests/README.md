# incidenskezelo-backend.Tests

Unit tesztek az `incidenskezelo-backend` projekthez (MSTest, EF Core InMemory).
A futtatáshoz nem kell adatbázis.

## Futtatás

```bash
dotnet test incidenskezelo-backend/incidenskezelo-backend.slnx
```

Riderben és Visual Studióban a tesztek a megoldás (`incidenskezelo-backend.slnx`)
megnyitása után a Tests / Test Explorer ablakban jelennek meg.

A tesztek minden pull requestnél automatikusan lefutnak (GitHub Actions).
