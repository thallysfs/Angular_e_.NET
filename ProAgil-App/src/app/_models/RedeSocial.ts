export interface RedeSocial {
  Id: number;
  Nome: string;
  URL: string;
  // o '?' após o tipo informa que o atributo pode ser nulo
  EventoId?: number;
  PalestranteId?: number;
}
