// ============================================
// SKILLOCK - PANEL DE ADMINISTRADOR
// ============================================

// Variable global para almacenar el token JWT
let adminToken = null;

// URL base de la API
const API_BASE = 'http://localhost:5202';

// Mapeo de estados de apuestas
const BET_STATUS_MAP = {
    0: 'Draft',
    1: 'Negociando',
    2: 'Acordado',
    3: 'Fondeando',
    4: 'Activo',
    5: 'Completado',
    6: 'Cancelado',
    7: 'Disputado'
};

// Mapeo de juegos
const GAME_MAP = {
    0: 'Dota 2',
    1: 'CS2',
    2: 'Valorant'
};

// ============================================
// FUNCIONES AUXILIARES
// ============================================

/**
 * Realiza una petición HTTP autenticada
 */
async function apiCall(endpoint, options = {}) {
    const headers = {
        'Content-Type': 'application/json',
        ...options.headers
    };

    // Agregar token JWT si existe
    if (adminToken) {
        headers['Authorization'] = `Bearer ${adminToken}`;
    }

    const config = {
        ...options,
        headers
    };

    try {
        const response = await fetch(`${API_BASE}${endpoint}`, config);

        // Si es 401, volver al login
        if (response.status === 401) {
            showLoginView();
            showError('adminLoginError', 'Sesión expirada. Por favor, inicia sesión nuevamente.');
            return null;
        }

        if (!response.ok) {
            throw new Error(`HTTP Error: ${response.status}`);
        }

        return await response.json();
    } catch (error) {
        console.error('Error en petición API:', error);
        return null;
    }
}

/**
 * Muestra un mensaje de error
 */
function showError(elementId, message) {
    const element = document.getElementById(elementId);
    if (element) {
        element.textContent = message;
        element.style.display = 'block';
    }
}

/**
 * Limpia un mensaje de error
 */
function clearError(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.textContent = '';
        element.style.display = 'none';
    }
}

/**
 * Cambia de vista en el navbar
 */
function switchAdminView(viewName) {
    // Actualizar links activos
    document.querySelectorAll('[data-admin-view]').forEach(link => {
        link.classList.remove('active');
    });
    document.querySelector(`[data-admin-view="${viewName}"]`).classList.add('active');

    // Mostrar/ocultar contenido
    document.getElementById('dashboardContent').style.display = viewName === 'dashboard' ? 'block' : 'none';
    document.getElementById('betsContent').style.display = viewName === 'bets' ? 'block' : 'none';
    document.getElementById('usersContent').style.display = viewName === 'users' ? 'block' : 'none';

    // Cargar datos según la vista
    if (viewName === 'bets') {
        loadAllBets();
    } else if (viewName === 'users') {
        loadUsers();
    }
}

/**
 * Muestra la vista de login
 */
function showLoginView() {
    document.getElementById('adminLoginView').classList.add('active');
    document.getElementById('adminDashboardView').classList.remove('active');
}

/**
 * Muestra la vista del dashboard
 */
function showDashboardView() {
    document.getElementById('adminLoginView').classList.remove('active');
    document.getElementById('adminDashboardView').classList.add('active');
}

/**
 * Formatea una fecha a formato legible
 */
function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('es-ES', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
    });
}

/**
 * Obtiene los primeros 8 caracteres del ID
 */
function getBetCode(id) {
    return id.substring(0, 8).toUpperCase();
}

// ============================================
// LOGIN
// ============================================

document.getElementById('adminLoginForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    clearError('adminLoginError');

    const email = document.getElementById('adminEmail').value;
    const password = document.getElementById('adminPassword').value;

    // Llamar a POST /api/auth/acceso
    const response = await apiCall('/api/auth/acceso', {
        method: 'POST',
        body: JSON.stringify({ email, password })
    });

    if (!response || !response.token) {
        showError('adminLoginError', 'Error en el inicio de sesión. Verifica tus credenciales.');
        return;
    }

    // Guardar token en variable global
    adminToken = response.token;

    // Decodificar token JWT para verificar role
    try {
        const parts = adminToken.split('.');
        const decoded = JSON.parse(atob(parts[1]));

        // Verificar que el rol sea "Admin"
        if (decoded.role !== 'Admin') {
            showError('adminLoginError', 'Acceso denegado. Solo administradores pueden acceder.');
            adminToken = null;
            return;
        }

        // Login exitoso
        document.getElementById('adminEmail').value = '';
        document.getElementById('adminPassword').value = '';
        showDashboardView();
        loadDashboardData();
    } catch (error) {
        showError('adminLoginError', 'Error al procesar el token.');
        adminToken = null;
    }
});

// ============================================
// LOGOUT
// ============================================

document.getElementById('adminLogoutBtn').addEventListener('click', () => {
    adminToken = null;
    showLoginView();
    clearError('adminLoginError');
});

// ============================================
// NAVEGACIÓN
// ============================================

document.querySelectorAll('[data-admin-view]').forEach(link => {
    link.addEventListener('click', (e) => {
        e.preventDefault();
        const viewName = e.target.dataset.adminView;
        switchAdminView(viewName);
    });
});

// ============================================
// CARGAR DATOS DEL DASHBOARD
// ============================================

async function loadDashboardData() {
    // Cargar apuestas completadas
    const bets = await apiCall('/api/bets');

    if (!bets) {
        console.error('No se pudieron cargar las apuestas');
        return;
    }

    // Filtrar solo apuestas completadas (status == 5)
    const completedBets = bets.filter(bet => bet.status === 5);

    // Calcular estadísticas
    const totalCompletedBets = completedBets.length;
    const totalBetAmount = completedBets.reduce((sum, bet) => {
        // Monto total = agreedAmountPerTeam * 2 (ambos equipos)
        return sum + (bet.agreedAmountPerTeam * 2);
    }, 0);
    const totalCommission = totalBetAmount * 0.07; // 7% de comisión

    // Actualizar cards
    document.getElementById('totalCompletedBets').textContent = totalCompletedBets;
    document.getElementById('totalBetAmount').textContent = `$${totalBetAmount.toFixed(2)}`;
    document.getElementById('totalCommission').textContent = `$${totalCommission.toFixed(2)}`;

    // Cargar tabla de apuestas completadas en dashboard
    loadDashboardBetsTable(completedBets);
}

/**
 * Carga la tabla de apuestas completadas en el dashboard
 */
function loadDashboardBetsTable(completedBets) {
    const tbody = document.querySelector('#adminBetsTable tbody');
    tbody.innerHTML = '';

    if (completedBets.length === 0) {
        tbody.innerHTML = '<tr><td colspan="5" style="text-align: center; color: var(--text-secondary);">No hay apuestas completadas</td></tr>';
        return;
    }

    // Ordenar por fecha descendente (más recientes primero)
    completedBets.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));

    completedBets.forEach(bet => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td>${getBetCode(bet.id)}</td>
            <td>${GAME_MAP[bet.game] || 'Desconocido'}</td>
            <td>$${bet.agreedAmountPerTeam.toFixed(2)}</td>
            <td>$${(bet.agreedAmountPerTeam * 2).toFixed(2)}</td>
            <td>${formatDate(bet.createdAt)}</td>
        `;
        tbody.appendChild(row);
    });
}

// ============================================
// CARGAR TODAS LAS APUESTAS
// ============================================

async function loadAllBets() {
    const bets = await apiCall('/api/bets');

    if (!bets) {
        console.error('No se pudieron cargar las apuestas');
        return;
    }

    // Filtrar solo apuestas completadas
    const completedBets = bets.filter(bet => bet.status === 5);

    const tbody = document.querySelector('#adminAllBetsTable tbody');
    tbody.innerHTML = '';

    if (completedBets.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" style="text-align: center; color: var(--text-secondary);">No hay apuestas completadas</td></tr>';
        return;
    }

    // Ordenar por fecha descendente
    completedBets.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));

    completedBets.forEach(bet => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td>${getBetCode(bet.id)}</td>
            <td>${GAME_MAP[bet.game] || 'Desconocido'}</td>
            <td>$${bet.agreedAmountPerTeam.toFixed(2)}</td>
            <td>$${(bet.agreedAmountPerTeam * 2).toFixed(2)}</td>
            <td><span class="status-badge" style="background: #4caf50; color: white; padding: 4px 8px; border-radius: 4px; font-size: 12px;">Completado</span></td>
            <td>${formatDate(bet.createdAt)}</td>
        `;
        tbody.appendChild(row);
    });
}

// ============================================
// CARGAR USUARIOS
// ============================================

async function loadUsers() {
    const users = await apiCall('/api/users');

    const tbody = document.querySelector('#adminUsersTable tbody');
    const usersMessageDiv = document.getElementById('usersMessage');
    tbody.innerHTML = '';

    if (!users) {
        // Endpoint no disponible
        usersMessageDiv.style.display = 'block';
        tbody.innerHTML = `
            <tr>
                <td>Admin User</td>
                <td>admin@skillock.com</td>
                <td><span style="background: #f5a623; color: #1a1a2e; padding: 4px 8px; border-radius: 4px; font-size: 12px;">Admin</span></td>
                <td>2024-01-15 10:30</td>
            </tr>
            <tr>
                <td>Juan Pérez</td>
                <td>juan.perez@example.com</td>
                <td><span style="background: #667eea; color: white; padding: 4px 8px; border-radius: 4px; font-size: 12px;">User</span></td>
                <td>2024-02-20 14:45</td>
            </tr>
            <tr>
                <td>María García</td>
                <td>maria.garcia@example.com</td>
                <td><span style="background: #667eea; color: white; padding: 4px 8px; border-radius: 4px; font-size: 12px;">User</span></td>
                <td>2024-03-10 09:15</td>
            </tr>
        `;
        return;
    }

    usersMessageDiv.style.display = 'none';

    if (users.length === 0) {
        tbody.innerHTML = '<tr><td colspan="4" style="text-align: center; color: var(--text-secondary);">No hay usuarios registrados</td></tr>';
        return;
    }

    users.forEach(user => {
        const row = document.createElement('tr');
        const roleDisplay = user.role === 'Admin' 
            ? '<span style="background: #f5a623; color: #1a1a2e; padding: 4px 8px; border-radius: 4px; font-size: 12px;">Admin</span>'
            : '<span style="background: #667eea; color: white; padding: 4px 8px; border-radius: 4px; font-size: 12px;">User</span>';
        
        row.innerHTML = `
            <td>${user.username}</td>
            <td>${user.email}</td>
            <td>${roleDisplay}</td>
            <td>${formatDate(user.createdAt)}</td>
        `;
        tbody.appendChild(row);
    });
}

