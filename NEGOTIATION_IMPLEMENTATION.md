# Implementación: Negociación de Apuestas

## ✅ Completado

Se ha implementado una **VISTA 5: Negociación de Apuesta** en el frontend con todas las funcionalidades solicitadas.

---

## 📝 Características Implementadas

### 1. **VISTA HTML** (`index.html`)
Se agregó una nueva sección con:
- **Encabezado** con código de la apuesta y estado
- **Información de la apuesta**: Juego, tamaño de equipo y estado actual
- **Panel de propuestas**:
  - Tarjeta para "Tu Equipo" mostrando monto propuesto y estado de aceptación
  - Tarjeta para "Equipo Rival" con misma información
- **Sección de Acciones** con tres botones:
  - ✓ **Aceptar Monto**: Confirma la propuesta del rival
  - 📤 **Contra-proponer Monto**: Permite ingresar un nuevo monto
  - ✕ **Cancelar Apuesta**: Cancela completamente la apuesta
- **Historial de Propuestas**: Muestra un registro de todas las propuestas realizadas
- **Botón Actualizar**: Recarga el estado actual de la negociación
- **Botón Volver**: Regresa a la vista de apuestas

### 2. **LÓGICA JAVASCRIPT** (`app.js`)

Se agregaron 7 funciones principales:

#### `showNegotiationView(betCode)`
- Muestra la vista de negociación
- Carga los datos de la apuesta

#### `loadNegotiationData(betCode)`
- Obtiene los datos de la apuesta desde la API
- Determina cuál es tu equipo y cuál es el rival
- Muestra los montos propuestos
- Actualiza el estado de aceptación de ambos equipos
- Gestiona el historial de propuestas

#### `updateProposalHistory()`
- Actualiza visualmente el historial de propuestas
- Muestra quién propuso cada monto y cuándo

#### `initNegotiationView()`
- Inicializa todos los eventos interactivos:
  - **Botón Volver**: Retorna a la lista de apuestas
  - **Aceptar Monto**: Llama al endpoint de confirmación
  - **Contra-proponer**: Valida el monto y lo envía a la API
  - **Actualizar**: Recarga el estado actual
  - **Cancelar**: Cancela la apuesta con motivo

#### Endpoints API utilizados:
```javascript
GET    /bets/{id}                              // Obtener datos de la apuesta
PUT    /bets/{id}/monto                        // Proponer nuevo monto
PUT    /bets/{id}/monto/confirmacion           // Confirmar el monto
DELETE /bets/{id}                              // Cancelar la apuesta
```

### 3. **ESTILOS CSS** (`styles.css`)

Se agregaron ~150 líneas de CSS con:
- Diseño responsivo (desktop, tablet, mobile)
- Colores temáticos (verde para tu equipo, púrpura para rival, dorado para montos)
- Animaciones suaves (slideIn, hover effects)
- Gradientes y efectos de luminosidad
- Media queries para adaptarse a diferentes tamaños de pantalla

#### Clases CSS principales:
- `.negotiation-content`: Contenedor principal
- `.negotiation-header`: Encabezado con título y código
- `.negotiation-info`: Información básica de la apuesta
- `.proposal-cards`: Tarjetas de propuestas
- `.proposal-amount`: Montos con tipografía Orbitron
- `.acceptance-status`: Estados de aceptación
- `.action-section`: Sección de acciones
- `.history-section`: Historial de propuestas

---

## 🎯 Flujo de Uso

### Cuando se crea una apuesta:
1. Usuario hace click en "+ Nueva Apuesta"
2. Completa el formulario (juego, tamaño, monto inicial)
3. Hace click en "CREAR APUESTA"
4. **Se abre automáticamente la VISTA 5 de Negociación**

### En la vista de negociación:
1. Usuario ve la propuesta inicial del rival (monto inicial)
2. Puede:
   - **Aceptar**: Confirmar el monto y pasar a siguiente estado
   - **Contra-proponer**: Ingresar otro monto (ej: $50, $75, etc.)
   - **Actualizar**: Ver cambios recientes del rival
   - **Cancelar**: Cancelar la apuesta
3. Ambos equipos ven sus propuestas y estado de aceptación
4. El historial muestra todas las propuestas realizadas

---

## 🔌 Integración con API

La vista se conecta con los siguientes endpoints:
- GET `/bets/{id}` - Obtener datos de la apuesta
- PUT `/bets/{id}/monto` - Proponer nuevo monto
- PUT `/bets/{id}/monto/confirmacion` - Confirmar monto
- DELETE `/bets/{id}` - Cancelar apuesta

---

## 💡 Datos que se muestran

```
Propuesta Actual:
├─ Tu Equipo
│  ├─ Monto: $[cantidad]
│  └─ Estado: ✓ Aceptado / ⏳ Pendiente
└─ Equipo Rival
   ├─ Monto: $[cantidad]
   └─ Estado: ✓ Aceptado / ⏳ Pendiente

Historial:
├─ Rival: $50 - 14/02/2024 10:30
├─ Tú: $60 - 14/02/2024 10:35
└─ Rival: $55 - 14/02/2024 10:40
```

---

## 📱 Responsive Design

- **Desktop**: 2 columnas para propuestas, diseño completo
- **Tablet**: Ajustes de padding y font-size
- **Mobile**: 1 columna, botones full-width

---

## ✨ Características Especiales

- ✅ Validación de montos (no permite valores ≤ 0)
- ✅ Mensajes de error/éxito automáticos
- ✅ Historial con timestamps
- ✅ Diferenciación visual entre tu equipo y rival
- ✅ Estados de aceptación claramente marcados
- ✅ Botón "Volver" para regresar a lista de apuestas
- ✅ Actualización manual del estado
- ✅ Cancelación con motivo

---

## 🎨 Paleta de Colores

- Verde (`#00ff88`): Tu equipo, confirmaciones
- Púrpura (`#6c63ff`): Equipo rival
- Dorado (`#f5a623`): Montos, propuestas
- Gris oscuro: Fondos y bordes

---

## 📂 Archivos Modificados

1. **`index.html`**: +99 líneas (nueva VISTA 5)
2. **`app.js`**: +175 líneas (lógica de negociación)
3. **`styles.css`**: +150 líneas (estilos de negociación)

**Total**: ~424 líneas de código nuevo


