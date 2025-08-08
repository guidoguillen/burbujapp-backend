# 🧺 BurbujaApp Backend - Microservicio Único
## Aplicación de Lavandería con .NET 9 + PostgreSQL

Este proyecto está optimizado como un **microservicio único modular** para reducir costos de infraestructura, manteniendo la organización modular del código.

## 🏗️ Arquitectura

### Estructura del Proyecto

```
BurbujApp/
├── BurbujApp.API/                    # Aplicación principal (Web API)
│   ├── Controllers/                  # Controllers REST API
│   │   ├── AuthController.cs         # Autenticación y registro
│   │   ├── ServicesController.cs     # Gestión de servicios
│   │   ├── AppointmentsController.cs # Gestión de citas
│   │   ├── ProductsController.cs     # Inventario de productos
│   │   └── InvoicesController.cs     # Facturación
│   ├── Program.cs                    # Configuración de la aplicación
│   └── appsettings.json              # Configuraciones
├── BurbujApp.Domain/                 # Entidades de dominio
│   └── Entities/                     # Modelos de datos
├── BurbujApp.Persistence/            # Acceso a datos
│   └── Context/
│       └── BurbujAppDbContext.cs     # Contexto de Entity Framework
├── BurbujApp.Shared/                 # DTOs y objetos compartidos
│   └── DTOs/                         # Data Transfer Objects
├── BurbujApp.Application/            # Lógica de aplicación
├── BurbujApp.Infrastructure/         # Servicios de infraestructura
└── docker-compose.yml               # Configuración de Docker
```

### Módulos Funcionales

El sistema está organizado en módulos funcionales:

#### 🔐 **Módulo de Autenticación**
- Registro de usuarios
- Login con JWT
- Gestión de identidad con ASP.NET Core Identity

#### 💇‍♀️ **Módulo de Servicios**
- CRUD de servicios de belleza
- Precios y duración
- Activación/desactivación

#### 📅 **Módulo de Citas**
- Programación de citas
- Estados: Programada, Confirmada, En Progreso, Completada, Cancelada
- Relación con usuarios y servicios

#### 📦 **Módulo de Inventario**
- Gestión de productos
- Control de stock
- Movimientos de inventario
- Alertas de stock bajo

#### 💵 **Módulo de Facturación**
- Generación de facturas
- Items por servicios
- Cálculo automático de impuestos
- Estados de pago

## 🗃️ Base de Datos

### Entidades Principales

```
Users (ASP.NET Identity)
├── Id (string)
├── FirstName (string)
├── LastName (string)
├── Email (string)
├── PhoneNumber (string)
├── CreatedAt (DateTime)
└── IsActive (bool)

Services
├── Id (int)
├── Name (string)
├── Description (string)
├── Price (decimal)
├── DurationMinutes (int)
└── IsActive (bool)

Appointments
├── Id (int)
├── UserId (string) → Users
├── ServiceId (int) → Services
├── AppointmentDate (DateTime)
├── StartTime (DateTime)
├── EndTime (DateTime)
├── Status (enum)
└── Notes (string)

Products
├── Id (int)
├── Name (string)
├── Brand (string)
├── Category (string)
├── Price (decimal)
├── Stock (int)
├── MinimumStock (int)
└── BarCode (string)

Invoices
├── Id (int)
├── InvoiceNumber (string)
├── UserId (string) → Users
├── IssueDate (DateTime)
├── DueDate (DateTime)
├── SubTotal (decimal)
├── Tax (decimal)
├── Total (decimal)
└── Status (enum)

InvoiceItems
├── Id (int)
├── InvoiceId (int) → Invoices
├── ServiceId (int) → Services
├── Description (string)
├── Quantity (int)
├── UnitPrice (decimal)
└── Total (decimal)
```

## 🚀 Ejecución

### Con Docker Compose

```bash
# Compilar y ejecutar
docker-compose up --build

# Solo ejecutar (si ya está compilado)
docker-compose up
```

### Desarrollo Local

```bash
# Restaurar dependencias
dotnet restore

# Compilar
dotnet build

# Ejecutar
cd BurbujApp.API
dotnet run
```

## 🔗 API Endpoints

### Autenticación
- `POST /api/auth/register` - Registro de usuario
- `POST /api/auth/login` - Login de usuario

### Servicios
- `GET /api/services` - Listar servicios activos
- `GET /api/services/{id}` - Obtener servicio específico
- `POST /api/services` - Crear servicio
- `PUT /api/services/{id}` - Actualizar servicio
- `DELETE /api/services/{id}` - Desactivar servicio

### Citas
- `GET /api/appointments` - Listar todas las citas
- `GET /api/appointments/my-appointments` - Mis citas
- `GET /api/appointments/{id}` - Obtener cita específica
- `POST /api/appointments` - Crear cita
- `PUT /api/appointments/{id}` - Actualizar cita
- `DELETE /api/appointments/{id}` - Cancelar cita

### Inventario
- `GET /api/products` - Listar productos
- `GET /api/products/{id}` - Obtener producto específico
- `GET /api/products/low-stock` - Productos con stock bajo
- `POST /api/products` - Crear producto
- `PUT /api/products/{id}` - Actualizar producto
- `POST /api/products/{id}/inventory-movement` - Registrar movimiento
- `GET /api/products/{id}/movements` - Historial de movimientos

### Facturación
- `GET /api/invoices` - Listar facturas
- `GET /api/invoices/{id}` - Obtener factura específica
- `GET /api/invoices/by-user/{userId}` - Facturas por usuario
- `GET /api/invoices/pending` - Facturas pendientes
- `POST /api/invoices` - Crear factura
- `PUT /api/invoices/{id}/status` - Actualizar estado

## 🔧 Configuración

### Variables de Entorno

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=burbujapp;Username=postgres;Password=rootpassword;"
  },
  "Jwt": {
    "Key": "YourSecretKeyHere",
    "Issuer": "BurbujApp",
    "Audience": "BurbujApp",
    "ExpiryInDays": 7
  }
}
```

### Docker Services

- **burbujapp-api**: Aplicación principal (Puerto 5000)
- **postgres-burbujapp**: Base de datos PostgreSQL (Puerto 5432)

## 🔄 Migración desde Microservicios

Este proyecto fue originalmente diseñado como microservicios y ha sido consolidado en un monolito modular por las siguientes razones:

### Ventajas del Monolito Modular
- ✅ **Reducción de costos** - Una sola base de datos y servidor
- ✅ **Simplicidad operacional** - Menos servicios que gestionar
- ✅ **Desarrollo más rápido** - Sin complejidad de comunicación entre servicios
- ✅ **Transacciones ACID** - Consistencia garantizada
- ✅ **Debugging simplificado** - Todo en un solo proceso

### Mantenimiento de la Modularidad
- 📁 Separación clara de responsabilidades por carpetas
- 🎯 Controllers específicos por dominio
- 📄 DTOs separados por módulo
- 🏗️ Arquitectura preparada para futura división en microservicios

## 🛡️ Seguridad

- **JWT Authentication** - Tokens seguros para API
- **Authorization** - Protección de endpoints
- **Identity Framework** - Gestión robusta de usuarios
- **HTTPS** - Comunicación segura
- **CORS** - Configuración de dominios permitidos

## 📈 Próximos Pasos

1. **Implementar AutoMapper** - Para mapeo automático de DTOs
2. **Agregar Logging** - Con Serilog o similar
3. **Unit Testing** - Pruebas unitarias con xUnit
4. **Integration Testing** - Pruebas de integración
5. **API Documentation** - Swagger/OpenAPI completo
6. **Caching** - Redis o memoria para optimización
7. **Background Jobs** - Para notificaciones y reportes
8. **File Upload** - Para imágenes de productos/servicios

## 💡 Notas de Desarrollo

- Usa **Entity Framework Core** con **PostgreSQL**
- Implementa **Clean Architecture** principles
- Preparado para **containerización** con Docker
- Configurado para **desarrollo local** y **producción**
- Base sólida para **escalabilidad futura**
