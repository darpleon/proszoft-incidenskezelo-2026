import type { ISimulationService } from "~/interfaces/ISimulationService";

export const services: ISimulationService[] = [
  { serviceId: 1, code: "postgres-main", name: "Fő adatbázis", kind: "Database", isActive: true },
  { serviceId: 2, code: "orders-api", name: "Rendelési API", kind: "Api", isActive: true },
  { serviceId: 3, code: "batch-worker", name: "Háttérfeldolgozó", kind: "Api", isActive: true },
  { serviceId: 4, code: "web-portal", name: "Webportál", kind: "Api", isActive: true },
  { serviceId: 5, code: "file-ingest", name: "Fájlfeldolgozó", kind: "Api", isActive: true },
  { serviceId: 6, code: "payment-gw", name: "Fizetési átjáró", kind: "Api", isActive: true },
];
