# 🎯 GUÍA RÁPIDA DE USO - SERVICIOS DE APPLICATION

## 1️⃣ BetService - Ciclo de Vida de Apuestas

### CrearApuestaAsync
```csharp
var request = new CrearApuestaRequest(
    Game: EsportGame.Dota2,
    MontoInicialPropuesto: 1000m,
    PlatformFeePercent: 0.05m
);

var bet = await _betService.CrearApuestaAsync(
    request: request,
    liderId: userId,
    cancellationToken: ct
);

// Estado resultante: Draft
// TeamA: Inicializado con Líder = userId
// TeamB: null (aún no existe)
```

### UnirseComoRivalAsync
```csharp
var bet = await _betService.UnirseComoRivalAsync(
    betId: betId,
    liderRivalId: rivalUserId,
    cancellationToken: ct
);

// Estado resultante: Negotiating
// TeamB: Inicializado con Líder = rivalUserId
// El Líder A y Líder B ahora pueden negociar montos
```

### ProponerMontoAsync
```csharp
var request = new ProponerMontoRequest(MontoPerTeam: 1000m);

var bet = await _betService.ProponerMontoAsync(
    betId: betId,
    request: request,
    liderId: liderAId,
    cancellationToken: ct
);

// AgreedAmountPerTeam = 1000m
// TeamA.LiderAcepto = true
// TeamB.LiderAcepto = false (aún)
// Estado: Negotiating (ambos no confirmaron)
```

### ConfirmarMontoAsync
```csharp
var bet = await _betService.ConfirmarMontoAsync(
    betId: betId,
    liderId: liderBId,
    cancellationToken: ct
);

// TeamB.LiderAcepto = true
// Ambos confirman: Estado → Agreed ✅
```

### ElegirModalidadFondeoAsync
```csharp
// Líder A: Individual (él paga 100%)
var modalidadA = new ElegirModalidadRequest(FundingMode.Individual);
await _betService.ElegirModalidadFondeoAsync(betId, modalidadA, liderAId, ct);

// Líder B: Mutual (varios aportan)
var modalidadB = new ElegirModalidadRequest(FundingMode.Mutual);
await _betService.ElegirModalidadFondeoAsync(betId, modalidadB, liderBId, ct);

// Ambos eligieron: Estado → Funding ✅
// Ahora esperando aportes
```

---

## 2️⃣ FundingService - Gestión de Aportes

### ProcesarAporteAsync
```csharp
// Usuario de TeamA aporta $1000 (modalidad Individual)
var aporte = new AporteRequest(
    BetPartyId: teamAId,
    Monto: 1000m
);

var result = await _fundingService.ProcesarAporteAsync(
    betId: betId,
    request: aporte,
    userId: usuarioTeamA,
    cancellationToken: ct
);

// Resultado:
// ✅ Wallet.SaldoDisponible -= 1000
// ✅ Wallet.SaldoRetenido += 1000
// ✅ PartyMember.MontoAportado = 1000
// ✅ BetParty.MontoAcumulado = 1000
// ✅ BetParty.EstaCompleto = true
// ✅ WalletTransaction creada (BetContribution)

// Si ambos equipos llegan a 1000 → Bet.Status = Active
```

### Comportamiento por Modalidad

#### Individual (Monto Fijo)
```csharp
// VÁLIDO:
await _fundingService.ProcesarAporteAsync(betId, new(teamId, 1000m), userId, ct);

// INVÁLIDO - Exception:
await _fundingService.ProcesarAporteAsync(betId, new(teamId, 999m), userId, ct);
// DomainException: "En modalidad Individual, el aporte debe ser exactamente 1000"

// INVÁLIDO - Exception:
await _fundingService.ProcesarAporteAsync(betId, new(teamId, 1001m), userId, ct);
// ExcesoDeAporteException(1001, 1000)
```

#### Mutual (Aportes Libres, Suma Exacta)
```csharp
// Usuario 1 aporta $600 de $1000:
await _fundingService.ProcesarAporteAsync(betId, new(teamId, 600m), user1, ct);
// ✅ MontoAcumulado = 600, EstaCompleto = false

// Usuario 2 aporta $400 (exacto, suma = 1000):
await _fundingService.ProcesarAporteAsync(betId, new(teamId, 400m), user2, ct);
// ✅ MontoAcumulado = 1000, EstaCompleto = true

// Usuario 3 intenta aportar $1 (exceso):
await _fundingService.ProcesarAporteAsync(betId, new(teamId, 1m), user3, ct);
// ❌ ExcesoDeAporteException(1, 0)
// NO hay reembolsos parciales, sin excedentes
```

### InvitarMiembroAsync
```csharp
var invitacion = new InvitarMiembroRequest(
    BetPartyId: teamId,
    UsuarioInvitadoId: nuevoMiembroId
);

var bet = await _fundingService.InvitarMiembroAsync(
    betId: betId,
    request: invitacion,
    liderInvitanteId: liderTeamId,
    cancellationToken: ct
);

// Resultado:
// ✅ PartyMember creado con:
//    - Role = Member
//    - AporteConfirmado = false
//    - MontoAportado = 0
// El miembro ahora puede aportar
```

### GetEstadoFondeoAsync
```csharp
var estado = await _fundingService.GetEstadoFondeoAsync(betId, ct);

// Resultado:
// {
//   BetId: betId,
//   StatusApuesta: Funding,
//   AgreedAmountPerTeam: 1000m,
//   TeamA: {
//     PartyId: teamAId,
//     Modalidad: Individual,
//     MontoAcumulado: 1000m,
//     MontoRestante: 0m,
//     PorcentajeCompletado: 100%,
//     EstaCompleto: true
//   },
//   TeamB: {
//     PartyId: teamBId,
//     Modalidad: Mutual,
//     MontoAcumulado: 700m,
//     MontoRestante: 300m,
//     PorcentajeCompletado: 70%,
//     EstaCompleto: false
//   }
// }
```

---

## 3️⃣ LiquidationService - Liquidación de Premios

### LiquidarApuestaAsync
```csharp
// API de juego reporta: TeamA ganó
var resultado = MatchResult.TeamAWins;

var liquidacion = await _liquidationService.LiquidarApuestaAsync(
    betId: betId,
    resultado: resultado,
    cancellationToken: ct
);

// Cálculo interno:
// PremioTotal = AgreedAmountPerTeam (1000) * 2 = 2000
// PlatformFee = 2000 * 0.05 = 100
// PremioNeto = 2000 - 100 = 1900

// Si TeamA: Individual (1 ganador con aporte de 1000):
//   Proporción = 1000 / 1000 = 100%
//   PremioRecibido = 1900 * 100% = 1900 ✅
//   Wallet.SaldoRetenido -= 1000 (su aporte original)
//   Wallet.SaldoDisponible += 1900 (premio neto)

// Si TeamB: Mutual (2 ganadores con aportes 600 + 400):
//   User1: Proporción = 600/1000 = 60%
//          PremioRecibido = 1900 * 60% = 1140 ✅
//   User2: Proporción = 400/1000 = 40%
//          PremioRecibido = 1900 * 40% = 760 ✅

// Team B (perdedor):
//   User1: Wallet.SaldoRetenido -= 600 (perdió su aporte) ❌
//   User2: Wallet.SaldoRetenido -= 400 (perdió su aporte) ❌
```

### ReembolsarApuestaAsync
```csharp
// Caso: MatchId no válido en la API
var motivo = "MatchId inválido en API de Dota2";

try {
    await _liquidationService.ReembolsarApuestaAsync(
        betId: betId,
        motivo: motivo,
        cancellationToken: ct
    );
} catch (DomainException ex) {
    // Si bet.Status == Completed → "No se puede reembolsar una apuesta ya completada"
}

// Resultado (si Status < Completed):
// Para CADA miembro que aportó:
//   Wallet.SaldoRetenido -= MontoAportado
//   Wallet.SaldoDisponible += MontoAportado  (devuelto 100%)
//   WalletTransaction creada (BetRefund)

// Estado final:
// Si motivo contiene "Disputa" → Bet.Status = Disputed
// Si no → Bet.Status = Cancelled
```

---

## 🚨 EXCEPCIONES ESPERADAS

### DomainException
```csharp
// Violación de reglas de negocio
throw new DomainException("Descripción del error");

// Ejemplos:
- "Apuesta con Id '...' no encontrada."
- "Solo se pueden hacer aportes en estado Funding. Estado actual: Draft"
- "El monto acordado no está establecido."
```

### ExcesoDeAporteException
```csharp
throw new ExcesoDeAporteException(montoIntentado: 100, montoRestante: 50);
// Mensaje: "El aporte de $100 excede el monto restante de $50. 
//           El sistema no admite excedentes ni reembolsos parciales."
```

### SaldoInsuficienteException
```csharp
throw new SaldoInsuficienteException(disponible: 500, requerido: 1000);
// Mensaje: "Saldo disponible insuficiente. Disponible: $500, requerido: $1000."
```

---

## 📊 TRANSACCIONES Y BLOQUEOS

### Operaciones ACID Garantizadas
```csharp
// Estas operaciones siempre son transaccionales:
- ProcesarAporteAsync()     // Modifica Wallet + BetParty + WalletTransaction
- CancelarApuestaAsync()    // Reembolsa N wallets
- LiquidarApuestaAsync()    // Distribuye a ganadores + consume perdedores
- ReembolsarApuestaAsync()  // Reembolsa N wallets

// Rollback automático si falla:
await _unitOfWork.BeginTransactionAsync(ct);
try {
    // Operaciones...
    await _unitOfWork.SaveChangesAsync(ct);
    await _unitOfWork.CommitTransactionAsync(ct);
} catch {
    await _unitOfWork.RollbackTransactionAsync(ct);
    throw;
}
```

### Bloqueo Pesimista
```csharp
// Obtiene wallet con lock FOR UPDATE en la BD:
var wallet = await _unitOfWork.Wallets.GetByUserIdWithLockAsync(userId, ct);

// Otros hilos/procesos NO pueden modificar esta wallet
// hasta que se complete la transacción
// → Previene double-spending

// Lock es liberado automáticamente con CommitTransactionAsync()
```

---

## 🎲 FLUJO COMPLETO EJEMPLO

```csharp
// 1. Crear apuesta
var bet = await betService.CrearApuestaAsync(
    new(EsportGame.CS2, 500m, 0.05m),
    userId: alicia,
    ct
);
// Estado: Draft, TeamA Líder = Alicia

// 2. Rival se une
await betService.UnirseComoRivalAsync(
    betId: bet.Id,
    liderRivalId: bob,
    ct
);
// Estado: Negotiating, TeamB Líder = Bob

// 3. Negociación de monto
await betService.ProponerMontoAsync(
    betId: bet.Id,
    new(MontoPerTeam: 500m),
    liderId: alicia,
    ct
);
await betService.ConfirmarMontoAsync(
    betId: bet.Id,
    liderId: bob,
    ct
);
// Estado: Agreed, AgreedAmountPerTeam = 500m

// 4. Elegir modalidades
await betService.ElegirModalidadFondeoAsync(
    betId: bet.Id,
    new(FundingMode.Individual),
    liderId: alicia,
    ct
);
await betService.ElegirModalidadFondeoAsync(
    betId: bet.Id,
    new(FundingMode.Mutual),
    liderId: bob,
    ct
);
// Estado: Funding, esperando aportes

// 5. Aportes
await fundingService.ProcesarAporteAsync(
    betId: bet.Id,
    new(teamIdA, 500m),
    userId: alicia,
    ct
);
// TeamA completo, esperando TeamB

await fundingService.InvitarMiembroAsync(
    betId: bet.Id,
    new(teamIdB, charlie),
    liderInvitanteId: bob,
    ct
);

await fundingService.ProcesarAporteAsync(
    betId: bet.Id,
    new(teamIdB, 300m),
    userId: bob,
    ct
);

await fundingService.ProcesarAporteAsync(
    betId: bet.Id,
    new(teamIdB, 200m),
    userId: charlie,
    ct
);
// TeamB completo: Estado → Active ✅

// 6. Liquidación (por BackgroundService)
await liquidationService.LiquidarApuestaAsync(
    betId: bet.Id,
    resultado: MatchResult.TeamBWins,
    ct
);
// Estado: Completed
// Alicia: Pierde 500
// Bob: Gana proporción (300/500) * 1900 = 1140
// Charlie: Gana proporción (200/500) * 1900 = 760
```

---

**🚀 Listo para implementar Infrastructure Layer y Controladores REST.**

