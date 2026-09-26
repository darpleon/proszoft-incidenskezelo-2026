export type ServiceKind = "Database" | "Api";

export interface ISimulationService {
  serviceId: number;
  code: string;
  name: string;
  kind: ServiceKind;
  isActive: boolean;
}
