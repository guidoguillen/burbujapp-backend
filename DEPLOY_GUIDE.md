# 🚀 GUÍA DE DEPLOY PARA KOYEB - BurbujaApp

## 📋 RESUMEN DE TU MICROSERVICIO ÚNICO

Has optimizado exitosamente tu aplicación BurbujaApp como un **microservicio único modular**, perfecto para reducir costos mientras mantienes todas las funcionalidades.

### ✅ **LO QUE YA TIENES FUNCIONANDO:**

#### **🏗️ Arquitectura Consolidada:**
- ✅ Un solo microservicio con todos los módulos
- ✅ PostgreSQL como base de datos única
- ✅ JWT Authentication configurado
- ✅ Swagger documentación automática
- ✅ Docker optimizado para producción
- ✅ Health checks incluidos

#### **📦 Módulos Integrados:**
- 🔐 **Auth** - Registro, login, roles
- 👤 **Users** - Gestión de usuarios
- 🧽 **Services** - Servicios de lavandería (CRUD)
- 📅 **Appointments** - Sistema de citas
- 📦 **Products** - Gestión de inventario
- 💰 **Invoices** - Facturación completa

#### **🛡️ Características de Seguridad:**
- ✅ JWT Bearer tokens
- ✅ ASP.NET Identity
- ✅ Roles (Admin, Manager, Employee, Customer)
- ✅ Password hashing
- ✅ CORS configurado

---

## 🚀 DEPLOY EN KOYEB - PASO A PASO

### **Paso 1: Preparar Repositorio**
```bash
# Si no has subido a GitHub aún:
git add .
git commit -m "Microservicio único optimizado para Koyeb"
git push origin main
```

### **Paso 2: Configurar Base de Datos en Koyeb**
1. Ir a [Koyeb Dashboard](https://app.koyeb.com)
2. **Data Stores** → **Create Database**
3. **PostgreSQL** → **Free Plan**
4. Nombre: `burbuja-db`
5. **Copiar connection string** cuando esté listo

### **Paso 3: Deploy de la API**
1. **Create Web Service** en Koyeb
2. **Source**: GitHub repository
3. **Configuración Build**:
   ```
   Build command: dotnet publish BurbujApp.API/BurbujApp.API.csproj -c Release -o /app/publish
   Run command: dotnet /app/publish/BurbujApp.API.dll
   Port: 80
   Working directory: /
   ```

### **Paso 4: Variables de Entorno**
```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:80

# Reemplazar con tu connection string real de Koyeb
ConnectionStrings__DefaultConnection=Host=ep-xxx.koyeb.app;Database=koyebdb;Username=koyeb-adm;Password=xxx;SSL Mode=Require;

# JWT Settings
JwtSettings__Key=tu-super-secret-jwt-key-aqui-minimo-32-caracteres-para-seguridad-burbuja-app
JwtSettings__Issuer=BurbujaApp
JwtSettings__Audience=BurbujaApp-Client
JwtSettings__ExpiryInMinutes=60
JwtSettings__RefreshTokenExpiryInDays=7
```

### **Paso 5: Configuración del Servicio**
- **Service name**: `burbuja-app-api`
- **Region**: `fra` (Frankfurt - recomendado)
- **Instance type**: `nano` (gratuito)

---

## 📚 DOCUMENTACIÓN DE TU API

### **🔐 Autenticación**

#### **Registrar Usuario**
```bash
POST https://tu-app.koyeb.app/api/auth/register
Content-Type: application/json

{
  "email": "usuario@example.com",
  "password": "TuPassword123!",
  "firstName": "Juan",
  "lastName": "Pérez"
}
```

#### **Iniciar Sesión**
```bash
POST https://tu-app.koyeb.app/api/auth/login
Content-Type: application/json

{
  "email": "usuario@example.com",
  "password": "TuPassword123!"
}
```

### **🧽 Servicios de Lavandería**

#### **Obtener Servicios**
```bash
GET https://tu-app.koyeb.app/api/services
Authorization: Bearer {tu-jwt-token}
```

#### **Crear Servicio**
```bash
POST https://tu-app.koyeb.app/api/services
Authorization: Bearer {tu-jwt-token}
Content-Type: application/json

{
  "name": "Lavado Completo",
  "description": "Lavado, secado y planchado",
  "price": 25.50,
  "durationMinutes": 120
}
```

### **📅 Gestión de Citas**

#### **Crear Cita**
```bash
POST https://tu-app.koyeb.app/api/appointments
Authorization: Bearer {tu-jwt-token}
Content-Type: application/json

{
  "serviceId": 1,
  "appointmentDate": "2024-12-01T00:00:00Z",
  "startTime": "2024-12-01T10:00:00Z",
  "endTime": "2024-12-01T12:00:00Z",
  "notes": "Prioridad: ropa delicada"
}
```

### **📦 Productos e Inventario**

#### **Agregar Producto**
```bash
POST https://tu-app.koyeb.app/api/products
Authorization: Bearer {tu-jwt-token}
Content-Type: application/json

{
  "name": "Detergente Premium",
  "description": "Detergente concentrado para ropa delicada",
  "brand": "CleanMax",
  "category": "Detergentes",
  "price": 15.99,
  "stock": 50,
  "minimumStock": 10,
  "barCode": "1234567890123"
}
```

### **💰 Facturación**

#### **Crear Factura**
```bash
POST https://tu-app.koyeb.app/api/invoices
Authorization: Bearer {tu-jwt-token}
Content-Type: application/json

{
  "userId": "user-id-here",
  "dueDate": "2024-12-15T00:00:00Z",
  "items": [
    {
      "serviceId": 1,
      "description": "Lavado Completo",
      "quantity": 2,
      "unitPrice": 25.50
    }
  ],
  "notes": "Entrega programada para el lunes"
}
```

---

## 🔧 CONFIGURACIÓN PARA REACT NATIVE / FLUTTER

### **Base de Configuración**
```typescript
// constants/api.ts
export const API_CONFIG = {
  BASE_URL: 'https://tu-app.koyeb.app/api',
  TIMEOUT: 10000,
};

// Servicio de autenticación
class AuthService {
  async login(email: string, password: string) {
    const response = await fetch(`${API_CONFIG.BASE_URL}/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password })
    });
    
    const data = await response.json();
    
    if (response.ok) {
      // Guardar token en AsyncStorage
      await AsyncStorage.setItem('auth_token', data.token);
      return data;
    }
    
    throw new Error(data.message || 'Error de login');
  }

  async getAuthHeaders() {
    const token = await AsyncStorage.getItem('auth_token');
    return {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    };
  }
}
```

### **Servicios de API**
```typescript
// services/LaundryService.ts
class LaundryService {
  private baseUrl = API_CONFIG.BASE_URL;
  
  async getServices() {
    const headers = await AuthService.getAuthHeaders();
    const response = await fetch(`${this.baseUrl}/services`, { headers });
    return response.json();
  }
  
  async createAppointment(appointmentData: any) {
    const headers = await AuthService.getAuthHeaders();
    const response = await fetch(`${this.baseUrl}/appointments`, {
      method: 'POST',
      headers,
      body: JSON.stringify(appointmentData)
    });
    return response.json();
  }
  
  async getInvoices() {
    const headers = await AuthService.getAuthHeaders();
    const response = await fetch(`${this.baseUrl}/invoices`, { headers });
    return response.json();
  }
}
```

---

## 💰 ANÁLISIS DE COSTOS

### **Koyeb Free Tier (Tu Configuración Actual)**
- ✅ **Web Service**: Gratis (512MB RAM, 2GB storage)
- ✅ **PostgreSQL**: Gratis (100MB storage)
- ✅ **Bandwidth**: Gratis (100GB/mes)
- ✅ **SSL Certificates**: Gratis
- ✅ **Custom Domain**: Gratis

### **Total Mensual: $0 USD** 🎉

### **Cuando Necesites Escalar:**
- **Nano Pro**: $7/mes (1GB RAM, 3GB storage)
- **Small**: $15/mes (2GB RAM, 10GB storage)
- **Medium**: $30/mes (4GB RAM, 20GB storage)

---

## 📊 MONITOREO Y SALUD

### **Health Checks**
Tu aplicación incluye endpoints de salud automáticos:
```bash
GET https://tu-app.koyeb.app/health
GET https://tu-app.koyeb.app/health/ready
GET https://tu-app.koyeb.app/health/live
```

### **Logs y Debugging**
- Logs disponibles en Koyeb Dashboard
- Structured logging con Serilog
- Error tracking automático

---

## 🎯 PRÓXIMOS PASOS

### **1. Despliegue Inmediato**
- [ ] Subir código a GitHub
- [ ] Crear cuenta en Koyeb
- [ ] Configurar PostgreSQL
- [ ] Deploy de la API
- [ ] Probar endpoints

### **2. Optimizaciones Futuras**
- [ ] Implementar Redis Cache
- [ ] Agregar health checks personalizados
- [ ] Configurar CI/CD automático
- [ ] Implementar rate limiting
- [ ] Agregar monitoring avanzado

### **3. Características Adicionales**
- [ ] Push notifications
- [ ] Sistema de pagos (Stripe/PayPal)
- [ ] Geolocalización para delivery
- [ ] Chat en tiempo real
- [ ] Reportes y analytics

---

## 🏆 ¡FELICIDADES!

Has creado exitosamente un **microservicio único modular** que:

✅ **Reduce costos** - Un solo servicio en lugar de 7 microservicios
✅ **Mantiene modularidad** - Código organizado por funcionalidades
✅ **Escala fácilmente** - Preparado para dividir en el futuro
✅ **Es gratuito** - Totalmente gratis en Koyeb Free Tier
✅ **Está listo para producción** - Docker, PostgreSQL, JWT, Swagger

**Tu app de lavandería está lista para conquistar el mundo! 🚀🧺**

---

## 📞 SOPORTE Y RECURSOS

- **Swagger UI**: `https://tu-app.koyeb.app/swagger`
- **Koyeb Docs**: [docs.koyeb.com](https://docs.koyeb.com)
- **PostgreSQL Docs**: [postgresql.org/docs](https://postgresql.org/docs)
- **JWT Debugger**: [jwt.io](https://jwt.io)

¿Listo para hacer el deploy? 🚀
