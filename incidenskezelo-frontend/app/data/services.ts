import type { ISimulationService } from "~/interfaces/ISimulationService";

export const services: ISimulationService[] = [
  { serviceId: 1, code: "postgres-main", name: "Main database", kind: "Database", isActive: true },
  { serviceId: 2, code: "orders-api", name: "Orders API", kind: "Api", isActive: true },
  { serviceId: 3, code: "batch-worker", name: "Batch worker", kind: "Api", isActive: true },
  { serviceId: 4, code: "web-portal", name: "Web portal", kind: "Api", isActive: true },
  { serviceId: 5, code: "file-ingest", name: "File ingest", kind: "Api", isActive: true },
  { serviceId: 6, code: "payment-gw", name: "Payment gateway", kind: "Api", isActive: true },
];
