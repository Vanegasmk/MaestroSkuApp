# 🎨 MaestroSkuApp — Solución de Formulario de Datos Maestros

![.NET](https://img.shields.io/badge/.NET-10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET%20Core-MVC-6E2C8E?style=for-the-badge)
![SQLite](https://img.shields.io/badge/SQLite-Base%20Consolidada-0F766E?style=for-the-badge&logo=sqlite&logoColor=white)
![xUnit](https://img.shields.io/badge/Pruebas-xUnit-15803D?style=for-the-badge)
![Estado](https://img.shields.io/badge/Entrega-Lista-16A34A?style=for-the-badge)

Solución web en **ASP.NET Core MVC** con **Entity Framework Core** y **SQLite** para gestionar el ciclo de vida de solicitudes de datos maestros de SKU en dos etapas sobre un mismo `Id`, garantizando integridad de datos, reglas condicionales y normalización a **`cm`** y **`kg`**.

---

## 📦 Checklist de Entregables

| # | Requisito de Entrega | Ubicación / Evidencia en Este Repositorio | Estado |
| :- | :--- | :--- | :---: |
| **1** | **Solución funcional** | Proyecto web ASP.NET Core MVC (`MaestroSkuApp/MaestroSkuApp.csproj`) y solución (`MaestroSkuApp.slnx`) | 🟢 |
| **2** | **Base de datos consolidada** | Archivo SQLite `MaestroSkuApp/datos_maestros.db` con `SolicitudesSku` y tablas de catálogo | 🟢 |
| **3** | **Solicitudes de prueba** | Al menos **1 solicitud `Completado`** y **1 solicitud `Pendiente Etapa 2`** cargadas en SQLite | 🟢 |
| **4** | **Código fuente y pruebas** | Código de `MaestroSkuApp` y pruebas unitarias en `MaestroSkuApp.Tests` | 🟢 |
| **5** | **Arquitectura y uso de IA** | Detallado en las siguientes secciones de este `README.md` | 🟢 |

---

## 🏗️ 1. Arquitectura de la Solución

La solución está construida bajo el patrón **ASP.NET Core MVC** con **Entity Framework Core** y **SQLite**, separando la aplicación web (`MaestroSkuApp`) del proyecto de pruebas automatizadas (`MaestroSkuApp.Tests`) dentro de la solución `MaestroSkuApp.slnx`.

### Capas y Componentes Principales
* 🟣 **Modelos (`Models/`):** `SolicitudSku` centraliza los datos de **Etapa 1** (motivo, categoría del motivo, producto sustituto/no sustituto, promedio de venta mensual o campos de sustitución, material, marca, clasificación, temporada, producto planificado y jerarquía RELEX) y de **Etapa 2** (país de origen, registro sanitario y fechas, embalaje, apilado, cajas, dimensiones/pesos y trazabilidad/estado), junto con `CatalogoMotivo` y `CatalogoJerarquia` para listas y cascadas.
* 🔵 **Acceso a Datos (`Data/`):** `AppDbContext` gestiona la persistencia en SQLite (`datos_maestros.db`) durante la ejecución real y en memoria (`UseInMemoryDatabase`) para las pruebas unitarias, además de inicializar las tablas y datos de catálogo.
* 🟢 **Controladores (`Controllers/`):** `SolicitudesController` orquesta el flujo de creación en **Etapa 1**, actualización sobre el **mismo `Id`** en **Etapa 2**, consulta consolidada y endpoints para listas dependientes.
* 🟡 **Vistas (`Views/` y `wwwroot/`):** Vistas Razor que conducen el flujo en dos etapas, manejan visibilidad condicional, realizan la conversión dinámica de unidades (`cm`/`kg` e `in`/`lb` hacia `cm` y `kg` al guardar) y muestran validaciones.
* 🟢 **Pruebas Unitarias (`MaestroSkuApp.Tests/`):** Suite en **xUnit** (`SolicitudesControllerTests`) con `EF Core InMemory` para validar reglas de negocio, limpieza de campos residuales, fechas/dimensiones/pesos inválidos y endpoints.

### Flujo sobre el Mismo `Id` y Reglas Condicionales
> **Continuidad del `Id`:** Cada solicitud inicia en **Etapa 1** (`Pendiente Etapa 2`) y se completa en **Etapa 2** (`Completado`) actualizando el **mismo `Id`**, evitando duplicados y conservando la trazabilidad.

* 🔀 **Cascadas:** `Motivo` filtra `Categoría motivo`, y la jerarquía **RELEX** (`Departamento` → `Categoría` → `Subcategoría` → `Tipo`) mantiene la coherencia al cambiar selecciones.
* 🧼 **Limpieza de campos no aplicables:** Se limpian valores residuales según `Clasificación` (`Stocks` → `No planificado` sin `Temporada`), `Producto sustituto` vs. no sustituto, `RequiereRegistroSanitario` (`NO` limpia número y fechas), `Embalaje` y cantidad de cajas.
* 📐 **Normalización y validación:** Dimensiones y pesos se normalizan a **`cm`** y **`kg`**, validando valores positivos y coherencia entre fechas de emisión y vencimiento del registro sanitario.

---

## 🗄️ 2. Base de Datos: Consolidación Actual vs. Normalización Futura

* 🔵 **Decisión actual (Consolidación):** Se consolida la información de Etapa 1 y Etapa 2 en `SolicitudesSku` junto con `CatalogoMotivo` y `CatalogoJerarquia` para simplificar la consulta directa de la base consolidada y la inspección rápida de los estados de cada solicitud en SQLite.
* 🟠 **Propuesta futura (Normalización):** Para escalar en producción, conviene normalizar en tablas relacionadas (solicitudes principales, tabla de cajas/empaque sin columnas repetidas, registro sanitario y catálogos con claves foráneas e índices), manteniendo vistas consolidadas para operación y reportes.

---

## 🧪 3. Solicitudes de Prueba en la Base de Datos

En `MaestroSkuApp/datos_maestros.db` se encuentran cargados los casos requeridos:
* 🟡 **Solicitud `Pendiente Etapa 2`:** Registrada en Etapa 1, lista para continuar en Etapa 2 con su `Id` asignado.
* 🟢 **Solicitud `Completado`:** Creada en Etapa 1 y finalizada en Etapa 2 sobre el **mismo `Id`**, con sus datos consolidados y estado final.

---

## ▶️ 4. Cómo Ejecutar el Proyecto Recién Descargado

### Requisitos previos
* Tener instalado **.NET SDK (net10.0)** (puedes verificar con `dotnet --version`).
* Opcional: **Visual Studio** o **VS Code** con extensión C#.

### Paso a paso desde terminal (en la raíz del repositorio)

```bash
# 1. Clonar o descomprimir el proyecto y entrar a la carpeta raíz
cd MaestroSkuApp

# 2. Restaurar dependencias NuGet de la solución
dotnet restore

# 3. Compilar toda la solución (app web + pruebas)
dotnet build

# 4. Ejecutar la aplicación web (usa MaestroSkuApp/datos_maestros.db)
dotnet run --project MaestroSkuApp/MaestroSkuApp.csproj

# 5. Ejecutar las pruebas unitarias
dotnet test MaestroSkuApp.Tests/MaestroSkuApp.Tests.csproj
```

> **Nota al ejecutar:** Abre en tu navegador la URL local que muestre `dotnet run` en consola (por ejemplo, `https://localhost:xxxx` o `http://localhost:xxxx`). Si el archivo `datos_maestros.db` ya viene en la carpeta `MaestroSkuApp/`, conservará los datos de prueba cargados; si no existiera, la aplicación inicializará la base de datos automáticamente al arrancar.

---

## 🤖 5. Uso de Asistencia de IA / Copilot y Proceso de Validación

### Cómo se utilizó la IA
Se empleó asistencia de IA de forma iterativa como **apoyo técnico** para acelerar el diseño de pruebas unitarias con `xUnit` y `EF Core InMemory`, refinar validaciones y reglas condicionales, apoyar el comportamiento de conversión de unidades en Etapa 2 y estructurar la documentación técnica.

### Revisión, Adaptación y Validación Real
La IA se utilizó como apoyo y acelerador, **no como reemplazo del criterio ni de la verificación técnica**:
* 🟣 **Adaptación al dominio y tipos reales:** Se ajustaron nombres de propiedades, tipos de datos (`DateTime?` vs. `DateOnly`, firmas síncronas/asíncronas en `SolicitudesController`) y reglas específicas de negocio.
* 🟠 **Compilación y resolución de incidencias:** Se verificó la compilación y se resolvieron bloqueos de procesos o errores de tipos al ejecutar `dotnet build` y `dotnet test`.
* 🟢 **Pruebas automatizadas y manuales:** Se validó el comportamiento con las pruebas unitarias en verde y con pruebas manuales sobre SQLite (`datos_maestros.db`), dejando registros representativos en estado `Pendiente Etapa 2` y `Completado` para la entrega.