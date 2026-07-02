# ️ ARQUITECTURA GENERAL - Skillock GamerBet

## Estructura del Proyecto

```
Skillock-ProyectoFinal/
├── Skillock-ProyectoFinal.sln
├── Skillock.Domain/                  ✅ IMPLEMENTADO
│   ├── BaseEntity.cs
│   ├── Bet.cs
│   ├── BetParty.cs
│   ├── PartyMember.cs
│   ├── User.cs
│   ├── Wallet.cs
│   ├── WalletTransaction.cs
│   ├── BetEnums.cs                   (BetStatus, FundingMode, EsportGame, MatchResult, etc.)
│   ├── DomainExceptions.cs           (DomainException, ExcesoDeAporteException, etc.)
│   └── Common/
├── Skillock.Application/             ✅ PARCIALMENTE IMPLEMENTADO
│   ├── Common/
│   │   └── ApplicationResult.cs      (Wrapper de resultado genérico)
│   ├── Interfaces/                   ✅ COMPLETO
│   │   ├── IBetService.cs
│   │   ├── IFundingService.cs
│   │   ├── ILiquidationService.cs
│   │   ├── IEsportApiClient.cs
│   │   ├── IBetRepository.cs
│   │   ├── IWalletRepository.cs
│   │   ├── IRepository.cs
│   │   └── IUnitOfWork.cs
│   ├── DTOs/
│   │   ├── Requests/
│   │   │   └── BetRequests.cs        ✅ COMPLETO
│   │   └── Responses/
│   │       └── BetResponses.cs       ✅ COMPLETO
│   └── Services/                     ✅ COMPLETO (3 servicios)
│       ├── BetService.cs
│       ├── FundingService.cs
│       └── LiquidationService.cs
├── Skillock.Infrastructure/          ❌ POR HACER
│   ├── Persistence/
│   │   ├── SkilockDbContext.cs
│   │   ├── Repositories/
│   │   │   ├── BetRepository.cs
│   │   │   ├── WalletRepository.cs
│   │   │   └── UnitOfWork.cs
│   │   └── Migrations/
│   ├── ExternalApis/
│   │   ├── Dota2ApiClient.cs
│   │   ├── CS2ApiClient.cs
│   │   └── ValorantApiClient.cs
│   └── BackgroundServices/
│       ├── MatchMonitoringService.cs
│       └── MatchTimeoutHandler.cs
├── Skillock-ProyectoFinal/           ❌ POR HACER
│   ├── Program.cs                    (Configuración de DI)
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs
│   ├── Controllers/
│   │   ├── BetsController.cs
│   │   └── WalletsController.cs
│   ├── appsettings.json
│   └── appsettings.Development.json
```

---

##  CAPAS Y RESPONSABILIDADES

### 1. **Domain Layer** (Skillock.Domain) ✅
**Responsabilidad:** Lógica de negocio pura, independiente de frameworks

**Contenido:**
- Entidades: `Bet`, `BetParty`, `PartyMember`, `User`, `Wallet`, `WalletTransaction`
- Enums: `BetStatus`, `FundingMode`, `EsportGame`, `MatchResult`, `TransactionType`, `PartyRole`
- Excepciones de dominio: `DomainException`, `ExcesoDeAporteException`, `SaldoInsuficienteException`
- Métodos de dominio en entidades (invariantes de negocio)

**Ejemplo - Método de Dominio:**
```csharp
public class Wallet {
    public void ReservarFondos(decimal monto) {
        if (SaldoDisponible < monto)
            throw new SaldoInsuficienteException(SaldoDisponible, monto);
        
        SaldoDisponible -= monto;
        SaldoRetenido += monto;
    }
}
```

---

### 2. **Application Layer** (Skillock.Application) ✅
**Responsabilidad:** Orquestar casos de uso, implementar lógica de aplicación

**Partes:**

#### A. Interfaces (Contratos)
```csharp
// Servicios de aplicación
IBetService              // Ciclo de vida de apuestas
IFundingService         // Gestión de aportes
ILiquidationService     // Distribución de premios

// Acceso a datos (implementados en Infrastructure)
IRepository<T>          // CRUD genérico
IBetRepository          // Queries específicas de Bet
IWalletRepository       // Queries específicas de Wallet
IUnitOfWork             // Agregador de repositorios + transacciones

// Clientes externos
IEsportApiClient        // Abstracción de APIs de juegos
IEsportApiClientFactory // Factory para resolver cliente por juego
```

#### B. DTOs (Contrato HTTP)
```csharp
// Requests
CrearApuestaRequest         // POST /bets
ProponerMontoRequest
AporteRequest              // POST /bets/{id}/aportes
InvitarMiembroRequest

// Responses
BetResponse                // GET /bets/{id}
BetPartyResponse
PartyMemberResponse
AporteResponse
FondeoEstadoResponse
LiquidacionResponse
```

#### C. **Servicios** ✅

**BetService**
- Responsable de: Ciclo de vida de apuestas (Draft → Active)
- Métodos: CrearApuesta, UnirseComoRival, ProponerMonto, ConfirmarMonto, ElegirModalidad, CancelarApuesta, GetBet, GetHistorial

**FundingService**
- Responsable de: Fase de fondeo y activación
- Métodos: ProcesarAporte, InvitarMiembro, GetEstadoFondeo

**LiquidationService**
- Responsable de: Distribución de premios y reembolsos
- Métodos: LiquidarApuesta, ReembolsarApuesta

---

### 3. **Infrastructure Layer** (Skillock.Infrastructure) ❌ POR HACER
**Responsabilidad:** Implementaciones técnicas (BD, APIs externas, etc.)

**Componentes Necesarios:**

#### A. Persistence (EF Core)
```csharp
// Program.cs
builder.Services.AddDbContext<SkilockDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// DbContext
public class SkilockDbContext : DbContext {
    public DbSet<Bet> Bets { get; set; }
    public DbSet<BetParty> BetParties { get; set; }
    public DbSet<PartyMember> PartyMembers { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<WalletTransaction> Transactions { get; set; }
}

// Repositorios
public class BetRepository : Repository<Bet>, IBetRepository {
    // Implementación de métodos especializados
}

public class WalletRepository : Repository<Wallet>, IWalletRepository {
    // GetByUserIdWithLockAsync() con FOR UPDATE
}

// UnitOfWork
public class UnitOfWork : IUnitOfWork {
    public IBetRepository Bets { get; }
    public IWalletRepository Wallets { get; }
    
    public async Task BeginTransactionAsync(CancellationToken cancellationToken) { }
    public async Task CommitTransactionAsync(CancellationToken cancellationToken) { }
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken) { }
}
```

#### B. External Clients (API Esports)
```csharp
// IEsportApiClient implementaciones
public class Dota2ApiClient : IEsportApiClient {
    public EsportGame JuegoSoportado => EsportGame.Dota2;
    public async Task<bool> ValidarMatchIdAsync(string matchId, CancellationToken ct) { }
    public async Task<MatchResultResponse?> ConsultarResultadoAsync(string matchId, CancellationToken ct) { }
}

public class CS2ApiClient : IEsportApiClient { }
public class ValorantApiClient : IEsportApiClient { }

// Factory
public class EsportApiClientFactory : IEsportApiClientFactory {
    public IEsportApiClient GetClient(EsportGame game) => game switch {
        EsportGame.Dota2 => new Dota2ApiClient(...),
        EsportGame.CS2 => new CS2ApiClient(...),
        _ => throw new ArgumentException()
    };
}
```

#### C. Background Services
```csharp
// Host.Services.AddHostedService<MatchMonitoringService>();

public class MatchMonitoringService : BackgroundService {
    // Cada 5 minutos:
    // 1. Obtener apuestas en estado Active
    // 2. Consultar API de juegos por MatchId
    // 3. Si hay resultado → Llamar ILiquidationService.LiquidarApuestaAsync()
}

public class MatchTimeoutHandler : BackgroundService {
    // Cada hora:
    // 1. Obtener apuestas en Funding > X horas
    // 2. Llamar ILiquidationService.ReembolsarApuestaAsync()
}
```

---

### 4. **WebAPI/Presentation Layer** (Skillock-ProyectoFinal) ❌ POR HACER
**Responsabilidad:** Exponer APIs REST, autenticación, manejo de excepción

**Componentes:**

#### A. Program.cs (DI Configuration)
```csharp
// Servicios
builder.Services.AddScoped<IBetService, BetService>();
builder.Services.AddScoped<IFundingService, FundingService>();
builder.Services.AddScoped<ILiquidationService, LiquidationService>();

// Repositorios y UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IBetRepository, BetRepository>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();

// APIs externas
builder.Services.AddScoped<IEsportApiClientFactory, EsportApiClientFactory>();

// BackgroundServices
builder.Services.AddHostedService<MatchMonitoringService>();
builder.Services.AddHostedService<MatchTimeoutHandler>();

// DbContext
builder.Services.AddDbContext<SkilockDbContext>(...);

// Autenticación JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(...);

// Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

#### B. Middleware (Exception Handling)
```csharp
public class ExceptionHandlingMiddleware {
    public async Task InvokeAsync(HttpContext context) {
        try {
            await _next(context);
        } catch (DomainException ex) {
            context.Response.StatusCode = 422;  // Unprocessable Entity
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        } catch (Exception ex) {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { error = "Error interno del servidor" });
        }
    }
}
```

#### C. Controllers
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BetsController {
    private readonly IBetService _betService;
    private readonly ICurrentUserService _currentUser;
    
    [HttpPost]
    public async Task<ActionResult<BetResponse>> CrearApuesta(
        [FromBody] CrearApuestaRequest request,
        CancellationToken ct)
    {
        var userId = _currentUser.UserId;  // Del JWT token
        var result = await _betService.CrearApuestaAsync(request, userId, ct);
        return Ok(result);
    }
    
    [HttpGet("{betId}")]
    public async Task<ActionResult<BetResponse>> GetBet(
        [FromRoute] Guid betId,
        CancellationToken ct)
    {
        var result = await _betService.GetBetAsync(betId, ct);
        return result != null ? Ok(result) : NotFound();
    }
}
```

---

##  FLUJO DE UNA SOLICITUD HTTP

```
1. Cliente HTTP
   │
   ├─→ POST /api/bets (CrearApuestaRequest)
   │
2. Controller
   │
   ├─→ Obtiene UserId del JWT token
   ├─→ Valida autorización
   ├─→ Llama IBetService.CrearApuestaAsync()
   │
3. BetService (Application)
   │
   ├─→ Validaciones de negocio
   ├─→ Carga entidades vía IUnitOfWork.Bets
   ├─→ Ejecuta métodos de dominio (Bet.EsTransicionValida, etc.)
   ├─→ Persiste cambios: await _unitOfWork.SaveChangesAsync()
   ├─→ Mapea Bet → BetResponse (DTO)
   │
4. Repository (Infrastructure)
   │
   ├─→ Implementa IRepository<Bet>
   ├─→ Ejecuta query: context.Bets.Add(bet)
   │
5. EF Core
   │
   ├─→ Genera SQL INSERT
   ├─→ Ejecuta en SQL Server
   │
6. Respuesta
   │
   ├─→ BetResponse { Id, Status: Draft, TeamA, ... }
   │
7. Middleware
   │
   ├─→ Si DomainException → 422 Unprocessable Entity
   ├─→ Si Exception → 500 Internal Server Error
   └─→ Si Ok → 200 Success
```

---

##  Flujo de Transacción Crítica (ProcesarAporteAsync)

```
Usuario A aporta $1000 a Bet

1. FundingService.ProcesarAporteAsync()
   │
   ├─→ Validaciones iniciales
   │
   ├─→ await _unitOfWork.BeginTransactionAsync()
   │   ↓
   │   SQL: BEGIN TRANSACTION;
   │
   ├─→ Obtener Wallet CON LOCK pesimista
   │   ├─→ await _unitOfWork.Wallets.GetByUserIdWithLockAsync(userId)
   │   │
   │   SQL: SELECT * FROM Wallets WHERE UserId = 'A' FOR UPDATE;
   │   (Otros procesos esperan aquí)
   │
   ├─→ Validar SaldoDisponible >= 1000
   │
   ├─→ Ejecutar métodos de dominio
   │   ├─→ wallet.ReservarFondos(1000)
   │   ├─→ miembro.MontoAportado = 1000
   │   ├─→ betParty.MontoAcumulado += 1000
   │   └─→ Detectar activación si ambos equipos completos
   │
   ├─→ Persistir cambios
   │   ├─→ await _unitOfWork.SaveChangesAsync()
   │   │
   │   SQL: UPDATE Wallets SET SaldoDisponible = ..., SaldoRetenido = ...;
   │   SQL: UPDATE PartyMembers SET MontoAportado = ...;
   │   SQL: UPDATE BetParties SET MontoAcumulado = ...;
   │   SQL: INSERT INTO WalletTransactions VALUES (...);
   │
   ├─→ await _unitOfWork.CommitTransactionAsync()
   │   │
   │   SQL: COMMIT;
   │   (Lock liberado, otros procesos pueden proceder)
   │
   └─→ Retornar AporteResponse
```

---

##  Próximos Pasos

### Fase 1: Infrastructure
- [ ] Crear SkilockDbContext
- [ ] Configurar Fluent API (keys, relaciones, índices)
- [ ] Implementar Repositories
- [ ] Crear Migrations
- [ ] Implementar UnitOfWork

### Fase 2: External APIs
- [ ] Implementar Dota2ApiClient
- [ ] Implementar CS2ApiClient
- [ ] Implementar ValorantApiClient
- [ ] Crear Factory

### Fase 3: Background Services
- [ ] MatchMonitoringService (polling cada 5 min)
- [ ] MatchTimeoutHandler (reembolsos automáticos)

### Fase 4: WebAPI
- [ ] Configurar Program.cs (DI)
- [ ] Crear Controllers (Bets, Wallets, Users)
- [ ] Implementar Middleware (Exception Handling)
- [ ] Configurar Autenticación JWT

### Fase 5: Testing
- [ ] Unit Tests para Services
- [ ] Integration Tests para Repositories
- [ ] Load Testing concurrencia

---

##  Diagrama de Entidades (ER)

```
User (1)
  ├─ id: Guid
  ├─ username: string
  ├─ email: string
  ├─ passwordHash: string
  └─ avatarUrl: string?

    ↓ (1:1)

Wallet (1)
  ├─ id: Guid
  ├─ userId: Guid
  ├─ saldoDisponible: decimal
  ├─ saldoRetenido: decimal
  └─ Transactions: WalletTransaction[]

    ↓ (1:N)

WalletTransaction
  ├─ id: Guid
  ├─ walletId: Guid
  ├─ type: TransactionType
  ├─ amount: decimal
  ├─ balanceAfter: decimal
  ├─ betId: Guid?
  └─ description: string?

Bet (1)
  ├─ id: Guid
  ├─ game: EsportGame
  ├─ status: BetStatus
  ├─ agreedAmountPerTeam: decimal?
  ├─ platformFeePercent: decimal
  ├─ matchId: string?
  ├─ matchResult: MatchResult
  └─ Parties: BetParty[]  (exactamente 2)

    ↓ (1:N)

BetParty
  ├─ id: Guid
  ├─ betId: Guid
  ├─ isTeamA: bool
  ├─ fundingMode: FundingMode
  ├─ montoAcumulado: decimal
  ├─ estaCompleto: bool
  ├─ liderAcepto: bool
  └─ Members: PartyMember[]

    ↓ (1:N)

PartyMember
  ├─ id: Guid
  ├─ betPartyId: Guid
  ├─ userId: Guid           ←─→ User
  ├─ role: PartyRole
  ├─ montoAportado: decimal
  ├─ aporteConfirmado: bool
  └─ fechaAporte: DateTime?
```

---

**Estado del Proyecto: ✅ 30% Completo (Domain + Application Layer)**

**Siguiente Meta: Implementar Infrastructure Layer con EF Core + SQL Server**
