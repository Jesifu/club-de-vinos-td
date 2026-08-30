# Sistema de Gestión de Usuarios — TP Ingeniería de Software

Aplicación de escritorio en C# / WinForms con arquitectura en capas, sistema de permisos basado en roles, soporte multiidioma y verificación de integridad de base de datos mediante dígitos verificadores.

---

## Tabla de contenidos

1. [Requisitos](#requisitos)
2. [Configuración inicial](#configuración-inicial)
3. [Estructura del proyecto](#estructura-del-proyecto)
4. [Arquitectura por capas](#arquitectura-por-capas)
5. [Modelo de datos](#modelo-de-datos)
6. [Patrones de diseño](#patrones-de-diseño)
7. [Sistema de seguridad](#sistema-de-seguridad)
8. [Sistema de integridad (DVH/DVV)](#sistema-de-integridad-dvhdvv)
9. [Control de cambios (historial)](#control-de-cambios-historial)
10. [Soporte multiidioma](#soporte-multiidioma)
11. [Convenciones de la base de datos](#convenciones-de-la-base-de-datos)
12. [Agregar funcionalidad nueva](#agregar-funcionalidad-nueva)

---

## Requisitos

- Visual Studio 2022 (Community o superior)
- .NET Framework 4.x
- SQL Server (instancia local con autenticación de Windows)
- Base de datos: `BDCAPAS`

---

## Configuración inicial

### 1. Base de datos

Crear la base de datos manualmente desde SQL Server Management Studio o con:

```sql
CREATE DATABASE BDCAPAS;
```

Luego ejecutar el script completo:

```powershell
sqlcmd -S . -d BDCAPAS -E -i "DAL\script.sql"
```

El script es mayormente idempotente. Los bloques `CREATE TABLE` / `CREATE PROCEDURE` del inicio fallan si los objetos ya existen (inofensivo). Los bloques posteriores usan `IF OBJECT_ID ... DROP` antes de recrear, por lo que son completamente idempotentes.

**Credencial de administrador por defecto:** usuario `admin`, contraseña definida al correr el script.

### 2. Compilar

```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" TP_IS.sln /p:Configuration=Debug /v:minimal
```

### 3. Ejecutar

El ejecutable de entrada es `CAPAS\bin\Debug\CAPAS.exe`.

### Reiniciar integridad para pruebas

Si se modificó la BD directamente y se quiere forzar que la app reinicialice los dígitos verificadores en el próximo arranque:

```sql
DELETE FROM DIGITO_VERIFICADOR_VERTICAL WHERE TABLA = 'USUARIO';
```

---

## Estructura del proyecto

```
TP_IS.sln
├── BE/                     Entidades (Plain Objects, sin lógica)
├── DAL/                    Acceso a datos (stored procedures exclusivamente)
│   └── script.sql          Script completo de BD (tablas + SPs + datos iniciales)
├── BLL/                    Lógica de negocio
├── SeguridadYServicios/    Servicios transversales (sesión, idioma, integridad, hash)
├── CAPAS/                  UI — Windows Forms
│   └── Program.cs          Punto de entrada
├── DER.puml                Diagrama entidad-relación (PlantUML)
├── DiagramaClases.puml     Diagrama de clases (PlantUML)
├── DiagramaComponentes.puml  Diagrama de componentes (PlantUML)
└── DIAGRAMAS/              Diagramas de secuencia por caso de uso (PlantUML)
```

Para regenerar los diagramas PNG (requiere Python e internet):

```powershell
python generar_png.py DER.puml DER.png
python generar_png.py DiagramaClases.puml DiagramaClases.png
python generar_png.py DiagramaComponentes.puml DiagramaComponentes.png
python generar_png.py DIAGRAMAS\DiagramaSecuencia_CU01_IniciarSesion.puml DIAGRAMAS\DiagramaSecuencia_CU01_IniciarSesion.png
```

Para regenerar en lote todos los diagramas de secuencia (con reintentos):

```powershell
python generar_pngs_lote.py
```

---

## Arquitectura por capas

```
CAPAS (UI / WinForms)
  ├── BLL
  │     ├── DAL
  │     │     └── BE
  │     └── BE
  └── SeguridadYServicios
              └── BE
```

Cada capa solo depende de las capas inferiores. **CAPAS** referencia BLL y SeguridadYServicios. **SeguridadYServicios** no referencia BLL ni DAL — recibe los datos que necesita desde CAPAS.

### BE — Entidades

Clases de datos sin lógica. Las principales:

| Clase | Descripción |
|---|---|
| `USUARIO` | Usuario del sistema. Incluye `DVH int` para integridad. |
| `BITACORA` | Registro de eventos de sesión. |
| `IDIOMA`, `CONTROL_IDIOMA` | Soporte multiidioma. |
| `NodoPermiso` | Clase abstracta base del árbol de roles/permisos. |
| `Rol` | Nodo rama (tipo `PERFIL` en BD). |
| `Permiso` | Nodo hoja (tipo `PERMISO` en BD). |
| `UsuarioHistorial` | Snapshot de estado de un usuario en un momento dado. |
| `LoginResultado` | Enum: `Exito`, `CredencialesInvalidas`, `UsuarioBloqueado`. |

### DAL — Acceso a datos

Todo el acceso a BD pasa por la clase interna `Acceso`, que:
- Usa **exclusivamente stored procedures** (nunca SQL inline).
- Requiere `SqlParameter` para todos los parámetros (prevención de SQL injection).
- `Acceso.Leer()` devuelve `DataTable`. `Acceso.Escribir()` devuelve filas afectadas.
- `CrearParametro` tiene overloads solo para `string` e `int`. Para `bool` pasar `valor ? 1 : 0`. Para `int?` construir el `SqlParameter` manualmente con `DBNull.Value`.

### BLL — Lógica de negocio

Orquesta operaciones combinando DAL y SeguridadYServicios. No hace llamadas directas a la BD. Instancia los DAL directamente (`new DAL.UsuarioDAL()`).

### SeguridadYServicios — Servicios transversales

No referencia BLL ni DAL. Sus componentes:

| Componente | Responsabilidad |
|---|---|
| `SessionManager` | Singleton. Guarda el usuario autenticado y su lista de permisos. |
| `IdiomaManager` | Singleton/Subject Observer. Distribuye traducciones a los formularios. |
| `IObservadorIdioma` | Interfaz Observer. Todo form que soporte multiidioma la implementa. |
| `Hasher` | SHA-256 unidireccional para contraseñas. |
| `ValidadorContrasena` | Reglas: mínimo 6 caracteres, una mayúscula, un dígito. |
| `CalculadorDVH` | Cálculo de dígitos verificadores horizontal y vertical. |

---

## Modelo de datos

### Tablas principales

| Tabla | Descripción |
|---|---|
| `USUARIO` | Usuarios del sistema. Columna `DVH` para integridad. |
| `NODO_PERMISO` | Árbol de roles y permisos. `TIPO='PERFIL'` = rol, `TIPO='PERMISO'` = permiso catálogo. |
| `ROL_PERMISO` | Relación N:M entre roles y permisos. |
| `USUARIO_PERFIL` | Relación N:M entre usuarios y roles. |
| `BITACORA` | Auditoría de eventos (login, logout, acciones). |
| `IDIOMA` | Idiomas disponibles. |
| `CONTROL_IDIOMA` | Traducciones: (control, idioma) → texto. |
| `USUARIO_HISTORIAL` | Snapshots de estado de usuarios (append-only, sin UPDATE ni DELETE). |
| `DIGITO_VERIFICADOR_VERTICAL` | DVV por tabla y columna. |

> `PERSONA` existe en la BD pero es un **remanente legacy** — no se usa.

### Permisos del catálogo

Los permisos son estáticos (no se crean ni eliminan desde la UI):

- `Administrar usuarios`
- `Gestión de roles`
- `Gestión de idiomas`
- `Ver bitácora`
- `Cambiar contraseña`

---

## Patrones de diseño

### Composite — Árbol de roles y permisos

`NodoPermiso` es el componente abstracto.  
`Rol` (archivo `PerfilPermiso.cs`) es el nodo rama, almacenado como `TIPO='PERFIL'`.  
`Permiso` es la hoja, almacenada como `TIPO='PERMISO'`.

El árbol se construye en memoria en `PerfilBLL.ObtenerArbol()`:
1. `PerfilDAL.ListarRoles()` trae todos los roles y arma la jerarquía de roles por `PADRE_ID`.
2. `PerfilDAL.ListarRolPermisos()` trae los vínculos `ROL_PERMISO`; cada `Permiso` lleva el `ROL_ID` en su campo `PadreId` como valor carrier, que `ObtenerArbol()` usa para adjuntar cada permiso al rol correcto.

`NodoPermiso.ToString()` devuelve `Nombre` (lo usa el `CheckedListBox` en `frmPerfiles`).

Diagrama de clases dedicado (con comentarios explicativos): `DIAGRAMAS/DiagramaClases_Composite.puml` / `.png`.

### Observer — Multiidioma

`IdiomaManager` es el Subject.  
Los formularios son los Observers; implementan `IObservadorIdioma.ActualizarIdioma()`.

Flujo al cambiar idioma:
1. El usuario selecciona un idioma en el combo del status bar de cualquier form.
2. CAPAS llama `IdiomaBLL.CargarTraducciones(idiomaId)` y pasa el diccionario a `IdiomaManager.CambiarIdioma()`.
3. `IdiomaManager` llama `Notificar()` → `ActualizarIdioma()` en cada form registrado.
4. Cada form busca la traducción de cada control por clave; si no existe, usa el texto de diseño como fallback.

Cada form tiene su propio selector de idioma agregado dinámicamente por `IdiomaUIHelper.AgregarSelector(form)` (llamado al final del `Load`) porque `ShowDialog()` deshabilita el form padre.

Diagrama de clases dedicado (con comentarios explicativos): `DIAGRAMAS/DiagramaClases_Observer.puml` / `.png`.

### Singleton — SessionManager e IdiomaManager

Ambos usan double-checked locking. `SessionManager.cerrarSesion()` anula `_instance` para permitir re-autenticación.

Diagrama de clases dedicado (con comentarios explicativos): `DIAGRAMAS/DiagramaClases_Singleton.puml` / `.png`.

---

## Sistema de seguridad

### Autenticación

`LoginBLL.AutenticarUsuario()` valida credenciales, resetea intentos fallidos en caso de éxito, e incrementa el contador en caso de fallo. Al superar el límite, el usuario queda bloqueado automáticamente.

Flujo post-login en `LogIn.cs`:
1. Si la integridad falló al arrancar → según quién sea el usuario, ver sección de integridad.
2. Si la integridad es válida → `RecalcularIntegridadUsuarios()` y abrir `frmMenu`.

### Permisos

Los permisos se cargan en `SessionManager` al hacer login vía `USUARIO_PERMISOS_LISTAR` (join `USUARIO_PERFIL → ROL_PERMISO → NODO_PERMISO`).

La UI consulta `SessionManager.TienePermiso("nombre del permiso")` para mostrar u ocultar elementos. **Nunca usar `EsAdmin()` para controlar visibilidad** — ese método compara el campo `ROL` del usuario y no está alineado con el sistema de permisos.

### Contraseñas

- Almacenadas como SHA-256 (hexadecimal minúscula, 64 caracteres). `PASS` nunca se guarda en `USUARIO_HISTORIAL`.
- Reglas validadas por `ValidadorContrasena`: mínimo 6 caracteres, al menos una mayúscula, al menos un dígito.

---

## Sistema de integridad (DVH/DVV)

Protege la tabla `USUARIO` contra modificaciones externas a la aplicación. `BITACORA`, `NODO_PERMISO` y `ROL_PERMISO` **no están protegidas**.

### DVH (Dígito Verificador Horizontal)

Un valor `int` por fila, almacenado en la columna `DVH` de `USUARIO`.

**Fórmula:** `Σᵢ Σⱼ Unicode(atributo[i][j]) × (i+1) × (j+1)`

Detecta modificaciones en cualquier atributo de un registro.

### DVV (Dígito Verificador Vertical)

Un valor `int` por columna lógica, almacenado en `DIGITO_VERIFICADOR_VERTICAL (TABLA, COLUMNA, DVV)`.

**Fórmula:** `Σₖ Σⱼ Unicode(fila[k][col][j]) × (k+1) × (j+1)` donde `k` es la posición de la fila ordenada por `ID`.

Detecta inserciones, eliminaciones e intercambios de filas.

### Atributos de USUARIO (orden canónico — no cambiar)

```
ID | USUARIO | PASS | INTENTOS_FALLIDOS | BLOQUEADO("1"/"0") | ROL | PERFILES
```

`PERFILES` es un atributo virtual: IDs de perfiles del usuario en `USUARIO_PERFIL`, ordenados y separados por coma. Cambios en `USUARIO_PERFIL` externos a la app invalidan el DVH del usuario afectado.

### Flujo al arrancar (`Program.cs` + `LogIn.cs`)

1. `InicializarIntegridad()`:
   - Si no hay DVVs → `RecalcularIntegridad()` (primera ejecución o reset manual).
   - Luego `VerificarIntegridad()` → resultado en `Program.ResultadoIntegridad`.
   - **No bloquea el arranque.** Siempre abre `LogIn`.

2. Tras login exitoso:
   - Integridad **válida** → `RecalcularIntegridadUsuarios()` y abrir menú.
   - Integridad **inválida**, el admin logueado no está entre los afectados → `frmRestaurarIntegridad`.
   - Integridad **inválida**, cualquier otro caso → mensaje genérico, sesión bloqueada.

### `frmRestaurarIntegridad`

Muestra los errores y ofrece tres opciones:

| Opción | Efecto |
|---|---|
| Recalcular y continuar | Acepta el estado actual de la BD, recalcula todo y abre el menú. |
| Restaurar desde historial | Abre `frmHistorialUsuario` por cada usuario afectado para hacer rollback. Deshabilitado si no hay historial. |
| Cancelar | Cierra sesión y vuelve al login. |

Los errores se loguean en `integridad_error.log` junto al `.exe`.

### Cuándo recalcular

Después de **toda** mutación de `USUARIO` o `USUARIO_PERFIL`:

| Trigger | Quién llama |
|---|---|
| Login exitoso o fallido (integridad OK) | `LogIn.cs` |
| Admin elige Recalcular o Restaurar | `frmRestaurarIntegridad` |
| Crear, eliminar, desbloquear usuario | `UsuarioBLL.*` |
| Cambiar contraseña | `UsuarioBLL.CambiarContrasena` |
| Asignar perfiles | `UsuarioPerfilBLL.GuardarAsignaciones` |
| Rollback de historial | `UsuarioHistorialBLL.Rollback` |

> `RecalcularIntegridadUsuarios()` **nunca** se llama desde `LoginBLL`. Toda responsabilidad de recálculo vive en la capa CAPAS o en BLL post-mutación.

### Limitación conocida

La verificación solo ocurre al arrancar. Si la BD se corrompe durante una sesión activa, no se detecta hasta el próximo inicio; peor aún, si ocurre una mutación posterior, `RecalcularIntegridadUsuarios()` acepta los datos corruptos como nuevo baseline.

---

## Control de cambios (historial)

`USUARIO_HISTORIAL` es una tabla append-only. Cada fila es un snapshot completo del estado de un usuario. `PASS` nunca se almacena.

### Tipos de cambio

`ALTA` · `BAJA` · `BLOQUEO` · `DESBLOQUEO` · `CAMBIO_CLAVE` · `ASIGNACION_PERFIL` · `ROLLBACK`

### Regla de timing

`RegistrarCambio` se llama **después** del cambio, con una excepción: `BAJA` se registra **antes** de `_dal.Eliminar()` para capturar el último estado conocido mientras el registro aún existe.

### Rollback

`UsuarioHistorialBLL.Rollback()` restaura `ROL`, `BLOQUEADO`, `INTENTOS_FALLIDOS` y `PERFILES` desde el snapshot elegido. `PASS` queda intacto. El rollback se registra como nueva entrada `ROLLBACK` con `VERSION_ORIGEN` apuntando al snapshot restaurado.

**Nota:** no hay FK entre `USUARIO_HISTORIAL` y `USUARIO` — intencional, para que el historial sobreviva a la eliminación del usuario.

**Anti-recursividad:** una entrada de tipo `ROLLBACK` no puede volver a restaurarse. La validación se hace en dos capas: `frmHistorialUsuario` rechaza la selección antes de pedir confirmación, y `UsuarioHistorialBLL.Rollback()` lanza `InvalidOperationException` como respaldo si igual se invoca.

---

## Soporte multiidioma

### Registrar un nuevo control

En el `Load` del form, después de `GuardarDefaults`:

```csharp
// Barra de título
_controles[this.Name] = this;
_defaults[this.Name]  = this.Text;

// Clave con nombre único si colisiona con otro form
_controles.Remove("lblTitulo");
_defaults.Remove("lblTitulo");
_controles["lblTitulo_MiForm"] = lblTitulo;
_defaults["lblTitulo_MiForm"]  = lblTitulo.Text;
```

### Texto dinámico (TreeView, nodos generados)

No pasa por `_controles`. Usar directamente:

```csharp
IdiomaManager.getInstance().Traducir("clave") ?? "fallback"
```

### Encabezados de DataGridView

No están en la colección `Controls`. Traducirlos en un método separado `ActualizarEncabezados()`, llamado desde `ActualizarIdioma()` y después de cada asignación de `DataSource`. Claves sugeridas: `colhdr_NombreColumna`.

### Agregar traducciones a la BD

Agregar al final de `DAL/script.sql`:

```sql
EXEC CONTROL_REGISTRAR 'clave_control', '[Texto por defecto]';
EXEC TRADUCCION_GUARDAR 'clave_control', 1, 'Texto en idioma 1';
EXEC TRADUCCION_GUARDAR 'clave_control', 2, 'Texto en idioma 2';
```

---

## Convenciones de la base de datos

- **Solo stored procedures.** Nunca SQL inline desde el código.
- `Acceso.CrearParametro` solo acepta `string` e `int`. Para `bool` usar `valor ? 1 : 0`. Para `int?` construir `SqlParameter` manualmente con `DBNull.Value`.
- `EXEC` no acepta expresiones como parámetros — asignar a variable primero.
- `GO` reinicia el scope de `DECLARE`. Cada batch posterior a un `GO` debe redeclarar sus variables.
- Para eliminar una columna con DEFAULT constraint, primero eliminar el constraint dinámicamente (el nombre autogenerado varía por instancia), luego la columna.
- Nuevas tablas y SPs se agregan al final de `DAL/script.sql`.

---

## Agregar funcionalidad nueva

### Nuevo formulario con soporte de idioma

1. Implementar `SeguridadYServicios.IObservadorIdioma`.
2. Agregar campos `Dictionary<string, Control> _controles` y `Dictionary<string, string> _defaults`.
3. En `Load`:
   - `GuardarDefaults(this.Controls)`
   - Registrar título: `_controles[this.Name] = this; _defaults[this.Name] = this.Text;`
   - `IdiomaManager.getInstance().Registrar(this); ActualizarIdioma();`
   - `IdiomaUIHelper.AgregarSelector(this)` al final.
4. En `FormClosed`: `IdiomaManager.getInstance().Desregistrar(this)`.
5. `ActualizarIdioma()` itera `_controles` aplicando `Traducir(key) ?? _defaults[key]`. Si tiene DataGridViews, llamar también `ActualizarEncabezados()`.
6. Agregar los eventos `Load` y `FormClosed` en el `.Designer.cs`.
7. Agregar `.cs` y `.Designer.cs` en `CAPAS/UI.csproj`.
8. Agregar claves en `script.sql` con `EXEC CONTROL_REGISTRAR` y `EXEC TRADUCCION_GUARDAR`.

### Extender integridad a una nueva entidad

1. Agregar columna `DVH INT` a la tabla.
2. Agregar filas a `DIGITO_VERIFICADOR_VERTICAL` para la nueva tabla.
3. Definir el array canónico de atributos y un método `ExtraerAtributos*` en `IntegridadBLL`.
4. Llamar `VerificarTabla` y `RecalcularTabla` (helpers internos genéricos) siguiendo el patrón existente de USUARIO.
5. Actualizar `RecalcularIntegridad()` para incluir la nueva entidad.
