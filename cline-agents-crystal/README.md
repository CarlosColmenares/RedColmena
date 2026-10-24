# Agentes de Cline para mantener reportes Crystal Reports (migración VB6 → .NET)

Plantillas listas para copiar a tu proyecto .NET en VS Code con Cline.

## Estructura

```
cline-agents-crystal/
├── rules/                          → copiar a .clinerules/ de tu proyecto
│   ├── 01-contexto-proyecto.md        (contexto global: qué puede y no puede hacer un agente)
│   └── 02-manejo-reportes.md          (reglas específicas para tocar .rpt y su código)
├── workflows/                      → copiar a .cline/workflows/ de tu proyecto
│   ├── inventario-reportes.md         (Agente Auditor — /inventario-reportes.md)
│   ├── migrar-odbc-a-sql.md           (Agente Migrador — /migrar-odbc-a-sql.md)
│   ├── agregar-campo-reporte.md       (Agente Modificador — /agregar-campo-reporte.md)
│   └── verificar-reportes.md          (Agente QA — /verificar-reportes.md)
└── tools/                          → compilar en tu solución (Windows + runtime CR v13)
    ├── CrystalReportHelper.cs         (helper central de conexión, sustituye el DSN ODBC)
    ├── VerifyAllReports.cs            (consola: carga + VerifyDatabase de todos los .rpt)
    └── AddFieldToReport.cs            (consola avanzada: inserta campos en el .rpt vía RAS in-proc)
```

## Cómo instalarlos en tu proyecto

1. Abre tu solución .NET en VS Code con Cline instalado.
2. Copia `rules/*.md` a la carpeta `.clinerules/` (raíz del repo). Cline las aplicará en toda conversación.
3. Copia `workflows/*.md` a `.cline/workflows/`. Se ejecutan escribiendo `/nombre-del-workflow.md` en el chat de Cline.
4. Integra `tools/CrystalReportHelper.cs` en tu proyecto (ajusta el namespace y la lectura de configuración).
5. (Opcional) Crea un proyecto de consola `Herramientas.Reportes` con `VerifyAllReports.cs` y `AddFieldToReport.cs`,
   referenciando los ensamblados del SDK de Crystal (CrystalDecisions.*).

## Los 4 "agentes" y cuándo usarlos

| Agente (workflow) | Qué hace | Cuándo |
|---|---|---|
| **Auditor** (`/inventario-reportes.md`) | Mapea todos los .rpt, quién los usa, qué DSN/ODBC traen, qué SPs usan | Una vez al inicio y tras cada migración grande |
| **Migrador** (`/migrar-odbc-a-sql.md`) | Elimina `DSN=`/ODBC del código y centraliza la conexión vía `CrystalReportHelper` | La deuda principal de tu migración VB6 |
| **Modificador** (`/agregar-campo-reporte.md`) | Agrega campos: actualiza el SP, corre VerifyDatabase e inserta el campo (manual asistida o automática) | Cada vez que pidan un campo nuevo en un reporte |
| **QA** (`/verificar-reportes.md`) | Carga y verifica TODOS los .rpt y reporta rotos por categoría | Antes de cada release |

## Lo que un agente NO puede hacer (sé realista con esto)

- **Los `.rpt` son binarios**: Cline no los edita directamente. El diseño visual complejo (subreportes,
  gráficos, secciones condicionales) sigue requiriendo el Crystal Reports Designer.
- La **opción B** (inserción automática de campos con `AddFieldToReport.cs`) funciona para casos
  sencillos (agregar una columna al detalle con su encabezado), pero solo corre en **Windows** con el
  runtime **Crystal Reports for Visual Studio v13** instalado.
- El runtime CR tiene límite de ~75 motores de impresión concurrentes: por eso las reglas obligan a
  disponer los `ReportDocument`.

## Flujo recomendado de adopción

1. Semana 1: Agente Auditor → inventario completo (`docs/reportes-inventario.md`).
2. Semana 2-3: Agente Migrador → todos los reportes dejan de depender del DSN de máquina.
3. Continuo: Agente Modificador para campos nuevos + Agente QA antes de cada release.

## Alternativa estratégica (evaluación futura)

Crystal Reports en .NET sigue soportado, pero si el volumen de cambios sobre reportes es alto,
evalúa migrar gradualmente a RDLC/SSRS, DevExpress o Telerik Reporting. Los agentes anteriores
te sirven igualmente para documentar todo antes de una migración así.
