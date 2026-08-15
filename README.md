# AParCar

## Sistema Web para la Gestión y Reserva de Parqueos Municipales

AParCar es una plataforma web desarrollada para la **gestión, consulta y reserva de espacios de parqueo municipales**. El sistema centraliza las operaciones relacionadas con los parqueos y proporciona diferentes funcionalidades según el tipo de usuario.

La plataforma permite consultar la disponibilidad de espacios, gestionar vehículos, realizar reservas, procesar pagos, administrar zonas y espacios de parqueo, gestionar tarifas y generar información para el análisis administrativo.

El sistema está desarrollado utilizando **ASP.NET Core MVC**, **Entity Framework Core** y **PostgreSQL**, incorporando además ASP.NET Identity para la autenticación y autorización de usuarios.

---

## Características principales

### Gestión de usuarios

* Registro de usuarios.
* Inicio y cierre de sesión.
* Autenticación mediante ASP.NET Identity.
* Autorización basada en roles.
* Gestión de perfiles.
* Recuperación y restablecimiento de contraseñas.
* Autenticación de dos factores.
* Gestión de información personal.
* Registro de actividades y accesos.

### Gestión de parqueos

* Administración de zonas de parqueo.
* Administración de espacios individuales.
* Consulta del estado de los espacios.
* Control de ocupación.
* Gestión de tarifas.
* Administración de tickets.

### Reservas

* Consulta de espacios disponibles.
* Selección de espacios.
* Creación y confirmación de reservas.
* Asociación de vehículos con clientes.
* Consulta del historial de reservas.
* Gestión del ciclo de vida de las reservas.

### Gestión de vehículos

* Registro de vehículos.
* Edición de información.
* Asociación de vehículos con clientes.
* Consulta del historial.
* Administración de diferentes tipos de vehículos.

### Pagos

* Integración con PayPal.
* Procesamiento de pagos asociados a reservas.
* Confirmación de pagos.
* Gestión de pagos en efectivo.
* Registro del estado de las transacciones.
* Pantallas de confirmación y cancelación.

### Correos electrónicos

El sistema incorpora servicios de correo electrónico mediante SMTP.

Entre las funcionalidades contempladas se encuentran:

* Confirmación de correo electrónico.
* Confirmación de cambio de correo.
* Recuperación de contraseña.
* Restablecimiento de contraseña.
* Notificaciones de pago.
* Correos de bienvenida.

### Reportes y estadísticas

El sistema incorpora diferentes reportes para apoyar la administración de los parqueos:

* Ocupación.
* Ingresos.
* Clientes con y sin vehículos.
* Relación entre clientes y vehículos.
* Historial de clientes y vehículos.
* Vehículos asociados a clientes.

---

# Roles del sistema

AParCar utiliza un sistema de roles para controlar el acceso a las funcionalidades.

| Rol               | Descripción                                                                                          |
| ----------------- | ---------------------------------------------------------------------------------------------------- |
| **Administrador** | Administra usuarios, zonas, espacios, tarifas, configuraciones, reportes y estadísticas del sistema. |
| **Operador**      | Gestiona las operaciones relacionadas con los parqueos y supervisa el estado de los espacios.        |
| **Cliente**       | Consulta disponibilidad, administra sus vehículos, realiza reservas y gestiona sus pagos.            |

Las funcionalidades disponibles dependen de los permisos asignados al usuario.

---

# Arquitectura

AParCar utiliza una arquitectura basada en **ASP.NET Core MVC**, manteniendo una separación de responsabilidades entre la presentación, los controladores, los modelos, los servicios y la persistencia de datos.

```text
                         AParCarWeb
                              │
              ┌───────────────┼───────────────┐
              │               │               │
         Controllers        Models           Views
              │               │               │
              └───────────────┼───────────────┘
                              │
                           Services
                              │
                              ▼
                             Data
                              │
                              ▼
                         PostgreSQL

        Areas/Identity → Autenticación y autorización
        Templates      → Correos electrónicos
        wwwroot        → Recursos del frontend
```

### Principios utilizados

* Separación de responsabilidades.
* Arquitectura MVC.
* Inyección de dependencias.
* Control de acceso basado en roles.
* Persistencia mediante Entity Framework Core.
* Migraciones para la administración de la base de datos.
* Servicios independientes para funcionalidades específicas.
* Protección de información sensible mediante variables de entorno.

---

# Tecnologías utilizadas

| Tecnología                    | Uso                                 |
| ----------------------------- | ----------------------------------- |
| **C# / .NET**                 | Lenguaje y plataforma principal     |
| **ASP.NET Core MVC**          | Desarrollo de la aplicación web     |
| **ASP.NET Identity**          | Autenticación y autorización        |
| **Entity Framework Core**     | Acceso y persistencia de datos      |
| **Npgsql**                    | Conexión entre .NET y PostgreSQL    |
| **PostgreSQL**                | Sistema gestor de base de datos     |
| **Razor Pages / Razor Views** | Interfaz y renderizado de páginas   |
| **HTML / CSS**                | Estructura y estilos de la interfaz |
| **JavaScript**                | Interactividad del frontend         |
| **Bootstrap**                 | Diseño y componentes de interfaz    |
| **jQuery**                    | Funcionalidades del frontend        |
| **PayPal**                    | Procesamiento de pagos              |
| **SMTP**                      | Envío de correos electrónicos       |
| **QuestPDF**                  | Generación de documentos PDF        |
| **Docker**                    | Contenerización de la aplicación    |
| **Git / GitHub**              | Control de versiones                |
| **Microsoft Azure**           | Infraestructura y despliegue        |

---

# Estructura del proyecto

AParCar está organizado siguiendo una arquitectura basada en ASP.NET MVC, separando controladores, modelos, vistas, servicios, persistencia, autenticación y recursos estáticos.

```text
AParCarWeb/
├── Areas/
│   └── Identity/          # Autenticación y gestión de cuentas
├── Controllers/            # Controladores MVC
├── Data/                   # Contexto y acceso a datos
├── Extensions/             # Métodos de extensión
├── Helpers/                # Clases auxiliares
├── Migrations/             # Migraciones de Entity Framework Core
├── Models/                 # Entidades y ViewModels
├── Services/               # Servicios de aplicación
├── Templates/              # Plantillas de correo
├── Views/                  # Vistas Razor
├── wwwroot/                # CSS, JavaScript, imágenes y librerías
├── Properties/             # Configuración de ejecución
├── appsettings.json        # Configuración de la aplicación
├── Dockerfile              # Configuración de Docker
├── Program.cs              # Configuración e inicio de la aplicación
├── AParCarWeb.csproj       # Configuración del proyecto .NET
└── README.md               # Documentación del proyecto
```

### Principales componentes

| Carpeta           | Descripción                                                                                               |
| ----------------- | --------------------------------------------------------------------------------------------------------- |
| `Areas/Identity/` | Autenticación, registro, gestión de cuentas, recuperación de contraseñas y autenticación de dos factores. |
| `Controllers/`    | Controladores responsables de procesar las solicitudes HTTP y coordinar las operaciones del sistema.      |
| `Data/`           | Contexto de Entity Framework Core y configuración relacionada con la persistencia.                        |
| `Extensions/`     | Métodos de extensión utilizados por la aplicación.                                                        |
| `Helpers/`        | Funciones y clases auxiliares reutilizables.                                                              |
| `Models/`         | Entidades del dominio y ViewModels.                                                                       |
| `Migrations/`     | Migraciones utilizadas para administrar la estructura de la base de datos.                                |
| `Services/`       | Servicios para correo, notificaciones, configuración, PayPal y renderización de vistas.                   |
| `Templates/`      | Plantillas Razor utilizadas para generar correos electrónicos.                                            |
| `Views/`          | Vistas Razor correspondientes a los diferentes módulos del sistema.                                       |
| `wwwroot/`        | Recursos estáticos como CSS, JavaScript, imágenes y librerías frontend.                                   |
| `Properties/`     | Configuración relacionada con la ejecución y despliegue.                                                  |

---

# Base de datos

AParCar utiliza **PostgreSQL** como sistema gestor de base de datos y **Entity Framework Core** como tecnología de acceso y persistencia.

El proyecto utiliza migraciones de Entity Framework Core para controlar la evolución del esquema de la base de datos.

### Principales entidades

```text
Usuario
   │
   ├── Cliente
   │      │
   │      └── ClienteVehiculo
   │                  │
   │                  └── Vehiculo
   │
   └── BitacoraAcceso

Zona
   │
   └── Espacio
          │
          └── Ticket
                 │
                 └── Pago
                        │
                        └── Tarifa

ConfiguracionSistema
Notificacion
ReporteGenerado
```

Entre las relaciones principales se encuentran:

* Un cliente puede tener varios vehículos.
* Una zona puede contener múltiples espacios.
* Los espacios están relacionados con los tickets generados.
* Los tickets se relacionan con los pagos.
* Las tarifas pueden utilizarse para determinar los importes correspondientes.
* Los usuarios pueden estar asociados con diferentes roles y operaciones dentro del sistema.

---

# Requisitos

Para ejecutar el proyecto localmente se requiere:

* [.NET](https://dotnet.microsoft.com/)
* [PostgreSQL](https://www.postgresql.org/)
* Git
* Visual Studio, Visual Studio Code u otro IDE compatible con .NET
* Cuenta de PayPal Sandbox para realizar pruebas de pago.
* Servidor SMTP para las funcionalidades de correo.

---

# Instalación

## 1. Clonar el repositorio

```bash
git clone <URL_DEL_REPOSITORIO>
cd AParCarWeb
```

## 2. Configurar PostgreSQL

Crear una base de datos PostgreSQL para el proyecto.

Ejemplo:

```text
Base de datos: aparcar
Puerto: 5432
```

La conexión se configura mediante una variable de entorno.

---

# Configuración

AParCar utiliza variables de entorno para almacenar información de configuración y credenciales sensibles.

Crear un archivo `.env` en la raíz del proyecto para el entorno local.

Ejemplo:

```env
DATABASE_URL="Host=localhost;Port=5432;Username=postgres;Password=TU_PASSWORD;Database=aparcar;Pooling=true;SSL Mode=Require;Trust Server Certificate=true"

PAYPAL_CLIENT_ID=TU_CLIENT_ID
PAYPAL_SECRET=TU_SECRET

EMAIL_SMTP_SERVER=TU_SERVIDOR_SMTP
EMAIL_PORT=TU_PUERTO
EMAIL_FROM=TU_CORREO
EMAIL_USERNAME=TU_USUARIO
EMAIL_PASSWORD=TU_PASSWORD
```

Los valores deben reemplazarse con las credenciales correspondientes al entorno de desarrollo.

## Configuración de PayPal

Para realizar pruebas de pago se recomienda utilizar **PayPal Sandbox**.

```env
PAYPAL_CLIENT_ID=TU_CLIENT_ID
PAYPAL_SECRET=TU_SECRET
```

## Configuración de correo

El sistema utiliza SMTP para enviar correos electrónicos.

```env
EMAIL_SMTP_SERVER=
EMAIL_PORT=
EMAIL_FROM=
EMAIL_USERNAME=
EMAIL_PASSWORD=
```

La configuración depende del proveedor de correo utilizado.

---

# Seguridad

El archivo `.env` contiene información sensible y **no debe subirse al repositorio**.

El proyecto debe incluir las siguientes reglas en `.gitignore`:

```gitignore
.env
.env.*
!.env.example
```

Nunca deben publicarse:

* Contraseñas de PostgreSQL.
* Secretos de PayPal.
* Credenciales SMTP.
* API Keys.
* Tokens.
* Credenciales de producción.

Para facilitar la configuración de nuevos entornos se puede proporcionar un archivo `.env.example` sin valores sensibles.

---

# Ejecución

Una vez configurado PostgreSQL y las variables de entorno, restaurar las dependencias:

```bash
dotnet restore
```

Compilar el proyecto:

```bash
dotnet build
```

Ejecutar la aplicación:

```bash
dotnet run
```

También es posible ejecutar el proyecto directamente desde Visual Studio utilizando el perfil de ejecución configurado.

---

# Migraciones de base de datos

El proyecto utiliza Entity Framework Core para administrar las migraciones.

Para crear una nueva migración:

```bash
dotnet ef migrations add NombreDeLaMigracion
```

Para aplicar las migraciones:

```bash
dotnet ef database update
```

Antes de ejecutar estos comandos, verificar que la conexión a PostgreSQL esté correctamente configurada.

---

# Funcionalidades por rol

## Cliente

El cliente puede:

1. Registrarse.
2. Iniciar sesión.
3. Gestionar su cuenta.
4. Registrar vehículos.
5. Consultar espacios disponibles.
6. Seleccionar un espacio.
7. Realizar reservas.
8. Procesar pagos.
9. Consultar historial.
10. Consultar información relacionada con sus vehículos y reservas.

## Administrador

El administrador puede:

1. Gestionar usuarios.
2. Gestionar zonas.
3. Gestionar espacios.
4. Administrar tarifas.
5. Consultar reportes.
6. Consultar estadísticas.
7. Administrar configuraciones del sistema.
8. Consultar información relacionada con las operaciones del parqueo.

## Operador

El operador puede:

1. Iniciar sesión.
2. Consultar el estado de los espacios.
3. Gestionar operaciones relacionadas con los parqueos.
4. Consultar información necesaria para la operación diaria.
5. Ejecutar acciones según los permisos asignados.

---

# Reportes

AParCar incorpora diferentes vistas orientadas a la consulta de información administrativa.

Entre ellas:

* Reporte de ingresos.
* Reporte de ocupación.
* Clientes sin vehículo.
* Clientes y vehículos.
* Historial de clientes y vehículos.
* Vehículos asociados a clientes.

Estos reportes permiten utilizar la información generada por el sistema para facilitar el seguimiento de las operaciones.

---

# Correos electrónicos

El sistema cuenta con plantillas Razor ubicadas en:

```text
Templates/
└── Email/
    ├── ConfirmEmail.cshtml
    ├── ConfirmEmailChange.cshtml
    ├── PaymentSuccess.cshtml
    ├── ResetPassword.cshtml
    └── WelcomeUser.cshtml
```

Estas plantillas se utilizan junto con los servicios de correo para generar mensajes relacionados con las principales operaciones del sistema.

---

# Despliegue

El proyecto incluye un `Dockerfile` para facilitar la preparación de la aplicación para entornos contenerizados.

La infraestructura contempla **Microsoft Azure** como plataforma de despliegue.

El proceso de publicación debe realizarse después de completar las etapas de:

* Desarrollo.
* Pruebas.
* Configuración del entorno.
* Validación de seguridad.
* Configuración de variables de entorno.
* Preparación de la base de datos.

Las credenciales y configuraciones específicas de producción deben mantenerse fuera del código fuente.

---

# Requisitos de calidad

El proyecto considera los siguientes aspectos:

* Seguridad.
* Rendimiento.
* Disponibilidad.
* Usabilidad.
* Escalabilidad.
* Mantenibilidad.
* Integridad de los datos.
* Separación de responsabilidades.

Entre los objetivos definidos para el sistema se encuentran:

* Tiempo de respuesta del mapa de disponibilidad inferior a 3 segundos bajo una carga definida.
* Comunicación mediante HTTPS/TLS en producción.
* Contraseñas almacenadas mediante mecanismos seguros de hashing.
* Interfaz responsive.
* Disponibilidad objetivo del 99.5%.
* Capacidad de crecimiento horizontal.
* Código organizado siguiendo convenciones de C# y .NET.

---

# Documentación del proyecto

La documentación asociada al proyecto contempla:

* Especificación de requisitos.
* Diseño de arquitectura.
* Diseño de base de datos.
* Diccionario de datos.
* Scripts de PostgreSQL.
* Diagramas de casos de uso.
* Diseño funcional.
* Diseño técnico.
* Manual de usuario.
* Flujos de procesos.
* Matriz de permisos CRUD.
* Pruebas.
* Documentación de incidencias.

---

# Estado del proyecto

**En desarrollo**

AParCar continúa evolucionando mediante la implementación, integración, pruebas y mejora de sus diferentes módulos.

---

# Equipo de desarrollo

Proyecto desarrollado por estudiantes de la **Universidad Internacional San Isidro Labrador**:

* Manuel Antonio Jiménez Víctor
* Roberth Steff Gamboa Rojas
* Henry Josué Mora Chacón
* **Luis Ángel Hernández Monge**
* Lex Rojas Monge
* Ashley Sardí Fonseca
* Jimena Corrales Rodríguez

Proyecto correspondiente al curso:

**Administración de Proyectos de Sistemas (ISB-31)**

---

# Licencia

Este proyecto fue desarrollado con fines académicos.

Para una distribución comercial o productiva, se deberá definir la licencia correspondiente.
