/* =====================================================================
   Actualización de PRECIO y STOCK del catálogo — mercado costarricense
   Base: DATABASE_PYE (Azure SQL)   Tabla: dbo.tbProductos

   Solo toca las columnas Precio y Stock. No modifica nombre,
   descripción, imágenes, categoría, proveedor ni estado.

   CÓMO USARLO
     1) Dejá @Aplicar = 0 y ejecutalo: simula, muestra el resultado y
        revierte todo (no cambia nada).
     2) Revisá las tablas de verificación del final.
     3) Cambiá a @Aplicar = 1 y volvé a ejecutarlo para confirmar.

   La clasificación se hace por el NOMBRE del producto, no por
   CategoriaID: las categorías de la base (Piedra Natural, Piedra
   Decorativa, Mármol…) mezclan productos de rangos de precio muy
   distintos (por ejemplo grutas y cal conviven en "Piedra Decorativa").

   Las comparaciones usan COLLATE Latin1_General_CI_AI para que no
   dependan de acentos ni mayúsculas ("volcánica" = "volcanica").
   ===================================================================== */

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Aplicar BIT = 0;   -- 0 = simulación (revierte)   1 = aplicar de verdad

BEGIN TRANSACTION;

/* ---------------------------------------------------------------
   1) Respaldo de los valores actuales.
      Queda como tabla permanente para poder revertir incluso
      después de confirmar los cambios.
   --------------------------------------------------------------- */
IF OBJECT_ID('dbo.RespaldoPreciosStock') IS NULL
BEGIN
    SELECT
        ProductoID,
        Nombre,
        Precio AS PrecioAnterior,
        Stock  AS StockAnterior,
        SYSDATETIME() AS FechaRespaldo
    INTO dbo.RespaldoPreciosStock
    FROM dbo.tbProductos;
END
ELSE
BEGIN
    /* Ya existe un respaldo de una corrida anterior: se conserva.
       Sobrescribirlo guardaria precios ya modificados y se perderian
       los valores originales. Si de verdad querés rehacerlo:
           DROP TABLE dbo.RespaldoPreciosStock;  */
    PRINT 'Respaldo previo encontrado: se conserva el original.';
END


/* ---------------------------------------------------------------
   2) Clasificación de cada producto en su grupo de precio
   --------------------------------------------------------------- */
IF OBJECT_ID('tempdb..#Clasificacion') IS NOT NULL DROP TABLE #Clasificacion;

;WITH Base AS (
    SELECT
        p.ProductoID,
        p.Nombre,
        n = LOWER(p.Nombre) COLLATE Latin1_General_CI_AI
    FROM dbo.tbProductos p
),
Clasificado AS (
    SELECT
        ProductoID,
        Nombre,
        n,
        Grupo = CASE
            /* El producto de prueba queda fuera (decisión acordada) */
            WHEN n LIKE '%prueba%'                                   THEN 'EXCLUIDO'

            /* Piezas artesanales — se evalúan primero porque son las
               más específicas y algunas contienen palabras genéricas */
            WHEN n LIKE '%mollejon%afilar%'
              OR n LIKE '%mollejones para afilar%'                   THEN 'AFILAR'
            WHEN n LIKE '%lavatorio%' OR n LIKE '%pila%'             THEN 'LAVATORIO'
            WHEN n LIKE '%hoguera%'                                  THEN 'HOGUERA'
            WHEN n LIKE '%gruta%'                                    THEN 'GRUTA'
            WHEN n LIKE '%chimenea%'                                 THEN 'CHIMENEA'
            WHEN n LIKE '%tina de piedra%' OR n LIKE '%tinas de piedra%'
              OR n LIKE 'tina%'                                      THEN 'TINA'
            WHEN n LIKE '%figura%'                                   THEN 'FIGURA'
            WHEN n LIKE '%huella%'                                   THEN 'HUELLAS'

            /* Cal: se aísla para que no la capture "caliza" */
            WHEN n = 'cal' OR n LIKE 'cal %' OR n LIKE '% cal'
              OR n LIKE '% cal %' OR n LIKE 'saco de cal%'           THEN 'CAL'

            WHEN n LIKE '%piedra de rio%' OR n LIKE '%piedra rio%'   THEN 'RIO'

            /* Decorativas comunes por saco (~20 kg).
               La caliza blanca decorativa entra acá por decisión del cliente,
               antes de que la capture la regla de enchapes. */
            WHEN n LIKE '%caliza blanca%'                            THEN 'COMUN'
            WHEN n LIKE '%cuarta%' OR n LIKE '%pomez%'               THEN 'COMUN'

            /* Decorativas finas por saco (10 kg) */
            WHEN n LIKE '%marmol%'   OR n LIKE '%cuarzo%'
              OR n LIKE '%ojo de tigre%' OR n LIKE '%ojo tigre%' OR n LIKE '%sangre%'
              OR n LIKE '%capas%'     OR n LIKE '%maple%'
              OR n LIKE '%dragon%'    OR n LIKE '%piel de elefante%' THEN 'FINA'

            /* Enchapes por m² */
            WHEN n LIKE '%mollejon%'  OR n LIKE '%sanchez%'
              OR n LIKE '%lajon%'     OR n LIKE '%laja%'
              OR n LIKE '%caliza%'    OR n LIKE '%travertino%'
              OR n LIKE '%coralina%'  OR n LIKE '%volcanica%'        THEN 'ENCHAPE'

            ELSE 'SIN_CLASIFICAR'
        END
    FROM Base
)
SELECT
    c.ProductoID,
    c.Nombre,
    c.Grupo,

    /* ---------- PRECIO ----------
       Cada grupo parte de un precio base por material y suma ajustes
       por formato y acabado. El desfase por ProductoID evita que
       productos equivalentes queden con el precio idéntico. */
    Precio = CAST(
        CASE c.Grupo

            /* Enchapes por m²: ₡9.000 – ₡18.000
               irregular/semirrústico abajo; canteado/pulido/fachaleta arriba */
            WHEN 'ENCHAPE' THEN
                  CASE WHEN c.n LIKE '%travertino%'                        THEN 13500
                       WHEN c.n LIKE '%coralina%'                          THEN 12500
                       WHEN c.n LIKE '%volcanica%'                         THEN 11500
                       WHEN c.n LIKE '%lajon%'                             THEN 11500
                       WHEN c.n LIKE '%caliza%'                            THEN 11000
                       WHEN c.n LIKE '%sanchez%'                           THEN 10500
                       WHEN c.n LIKE '%laja%'                              THEN 10000
                       WHEN c.n LIKE '%mollejon%' AND c.n LIKE '%veteado%' THEN 10000
                       WHEN c.n LIKE '%mollejon%'                          THEN  9500
                       ELSE 10500 END
                + CASE WHEN c.n LIKE '%fachaleta%'                         THEN 2500
                       WHEN c.n LIKE '%cantead%'                           THEN 1500
                       WHEN c.n LIKE '%tirilla%' OR c.n LIKE '%piezas%'
                         OR c.n LIKE '%una sola medida%'
                         OR c.n LIKE '%una misma medida%'
                         OR c.n LIKE '%pisos y senderos%'                  THEN 1000
                       ELSE 0 END
                + CASE WHEN c.n LIKE '%pulid%'                             THEN 2000
                       WHEN c.n LIKE '%rustic%' OR c.n LIKE '%mustic%'            THEN 0
                       ELSE 500 END

            /* Decorativas finas por saco 10 kg: ₡9.000 – ₡13.500 */
            WHEN 'FINA' THEN
                  CASE WHEN c.n LIKE '%cuarzo lechoso%'      THEN 12000
                       WHEN c.n LIKE '%cuarzo%'              THEN 11500
                       WHEN c.n LIKE '%ojo de tigre%'
                         OR c.n LIKE '%ojo tigre%'    THEN 11500
                       WHEN c.n LIKE '%sangre%'              THEN 12000
                       WHEN c.n LIKE '%dragon%'              THEN 11000
                       WHEN c.n LIKE '%piel de elefante%'    THEN 10500
                       WHEN c.n LIKE '%maple%'               THEN 10000
                       WHEN c.n LIKE '%capas%'               THEN  9500
                       ELSE 10500 END
                + CASE WHEN c.n LIKE '%brillante%' THEN 1500 ELSE 0 END
                + CASE WHEN c.n LIKE '%2 a 4%'     THEN  500
                       WHEN c.n LIKE '%4 a 6%'     THEN  250
                       ELSE 0 END
                /* matiz por color, para que no se repitan entre sí */
                + CASE WHEN c.n LIKE '%verde%'   THEN 800
                       WHEN c.n LIKE '%rosa%'    THEN 800
                       WHEN c.n LIKE '%roja%'    THEN 600
                       WHEN c.n LIKE '%negra%'   THEN 400
                       WHEN c.n LIKE '%amarilla%' THEN 200
                       WHEN c.n LIKE '%mixta%'   THEN -300
                       ELSE 0 END
                + ((c.ProductoID * 7) % 4) * 100

            /* Decorativas comunes por saco 20 kg: ₡3.000 – ₡4.500 */
            WHEN 'COMUN' THEN
                  CASE WHEN c.n LIKE '%caliza blanca%'            THEN 4400
                       WHEN c.n LIKE '%negra%'                    THEN 4300
                       WHEN c.n LIKE '%blanca%'                   THEN 4200
                       WHEN c.n LIKE '%roja%'                     THEN 4000
                       WHEN c.n LIKE '%amarilla%'                 THEN 3900
                       WHEN c.n LIKE '%mixta%'                    THEN 3500
                       WHEN c.n LIKE '%pomez%'                    THEN 3200
                       ELSE 3800 END

            /* Mollejones para afilar, por unidad: ₡3.500 – ₡6.000 */
            WHEN 'AFILAR' THEN
                  CASE WHEN c.n LIKE '%pulid%' THEN 5500 ELSE 3800 END

            /* Piedra de río por saco: ₡3.000 – ₡5.500 */
            WHEN 'RIO'       THEN 4200 + ((c.ProductoID * 11) % 6) * 200

            /* Huellas por unidad: ₡2.500 – ₡5.000 */
            WHEN 'HUELLAS'   THEN 3200 + ((c.ProductoID * 13) % 6) * 250

            /* Cal por saco: ₡2.000 – ₡3.500 */
            WHEN 'CAL'       THEN 2800

            /* Piezas artesanales, por unidad */
            WHEN 'LAVATORIO' THEN  62000 + ((c.ProductoID * 13) % 7) *  8000   /*  62.000 – 110.000 */
            WHEN 'HOGUERA'   THEN  78000 + ((c.ProductoID * 13) % 7) * 11000   /*  78.000 – 144.000 */
            WHEN 'GRUTA'     THEN 105000 + ((c.ProductoID * 13) % 7) * 23000   /* 105.000 – 243.000 */
            WHEN 'CHIMENEA'  THEN 180000 + ((c.ProductoID * 13) % 7) * 35000   /* 180.000 – 390.000 */
            WHEN 'TINA'      THEN 240000 + ((c.ProductoID * 13) % 7) * 42000   /* 240.000 – 492.000 */
            WHEN 'FIGURA'    THEN  32000 + ((c.ProductoID * 13) % 7) *  9000   /*  32.000 –  86.000 */

            ELSE NULL
        END AS DECIMAL(18,2)),

    /* ---------- STOCK ----------
       Granel: 40–300 | Intermedio: 10–40 | Artesanal: 1–8
       El desfase por ProductoID da variación sin ser aleatorio. */
    Stock = CASE
        WHEN c.Grupo IN ('ENCHAPE','FINA','COMUN','RIO','CAL')
            THEN 40 + ((c.ProductoID * 37) % 261)
        WHEN c.Grupo IN ('AFILAR','HUELLAS')
            THEN 10 + ((c.ProductoID * 13) %  31)
        WHEN c.Grupo IN ('LAVATORIO','HOGUERA','GRUTA','CHIMENEA','TINA','FIGURA')
            THEN  1 + ((c.ProductoID *  7) %   8)
        ELSE NULL
    END

INTO #Clasificacion
FROM Clasificado c;


/* ---------------------------------------------------------------
   3) Ajuste final: garantiza que ningún precio se salga del rango
      pedido, aunque los ajustes se hayan acumulado.
   --------------------------------------------------------------- */
UPDATE #Clasificacion
SET Precio = CASE Grupo
        WHEN 'ENCHAPE'   THEN CASE WHEN Precio <   9000 THEN   9000 WHEN Precio <  18000 THEN Precio ELSE  18000 END
        WHEN 'FINA'      THEN CASE WHEN Precio <   9000 THEN   9000 WHEN Precio <  13500 THEN Precio ELSE  13500 END
        WHEN 'COMUN'     THEN CASE WHEN Precio <   3000 THEN   3000 WHEN Precio <   4500 THEN Precio ELSE   4500 END
        WHEN 'AFILAR'    THEN CASE WHEN Precio <   3500 THEN   3500 WHEN Precio <   6000 THEN Precio ELSE   6000 END
        WHEN 'RIO'       THEN CASE WHEN Precio <   3000 THEN   3000 WHEN Precio <   5500 THEN Precio ELSE   5500 END
        WHEN 'HUELLAS'   THEN CASE WHEN Precio <   2500 THEN   2500 WHEN Precio <   5000 THEN Precio ELSE   5000 END
        WHEN 'CAL'       THEN CASE WHEN Precio <   2000 THEN   2000 WHEN Precio <   3500 THEN Precio ELSE   3500 END
        WHEN 'LAVATORIO' THEN CASE WHEN Precio <  45000 THEN  45000 WHEN Precio < 120000 THEN Precio ELSE 120000 END
        WHEN 'HOGUERA'   THEN CASE WHEN Precio <  60000 THEN  60000 WHEN Precio < 150000 THEN Precio ELSE 150000 END
        WHEN 'GRUTA'     THEN CASE WHEN Precio <  80000 THEN  80000 WHEN Precio < 250000 THEN Precio ELSE 250000 END
        WHEN 'CHIMENEA'  THEN CASE WHEN Precio < 150000 THEN 150000 WHEN Precio < 400000 THEN Precio ELSE 400000 END
        WHEN 'TINA'      THEN CASE WHEN Precio < 200000 THEN 200000 WHEN Precio < 500000 THEN Precio ELSE 500000 END
        WHEN 'FIGURA'    THEN CASE WHEN Precio <  25000 THEN  25000 WHEN Precio <  90000 THEN Precio ELSE  90000 END
        ELSE Precio
    END
WHERE Precio IS NOT NULL;


/* ---------------------------------------------------------------
   4) Aplicación del cambio
      Solo se tocan Precio y Stock, y solo de productos clasificados.
   --------------------------------------------------------------- */
UPDATE p
SET p.Precio = c.Precio,
    p.Stock  = c.Stock
FROM dbo.tbProductos p
INNER JOIN #Clasificacion c ON c.ProductoID = p.ProductoID
WHERE c.Grupo NOT IN ('SIN_CLASIFICAR','EXCLUIDO')
  AND c.Precio IS NOT NULL
  AND c.Stock  IS NOT NULL;

DECLARE @Actualizados INT = @@ROWCOUNT;


/* =================================================================
   VERIFICACIÓN
   ================================================================= */

/* 4.1 — Productos que ninguna regla clasificó: hay que revisarlos a mano */
SELECT 'SIN CLASIFICAR — revisar' AS Aviso, ProductoID, Nombre, Grupo
FROM #Clasificacion
WHERE Grupo IN ('SIN_CLASIFICAR','EXCLUIDO')
ORDER BY Grupo, Nombre;

/* 4.2 — Resumen por grupo */
SELECT
    c.Grupo,
    Productos   = COUNT(*),
    PrecioMin   = MIN(p.Precio),
    PrecioMax   = MAX(p.Precio),
    PreciosDistintos = COUNT(DISTINCT p.Precio),
    StockMin    = MIN(p.Stock),
    StockMax    = MAX(p.Stock)
FROM #Clasificacion c
INNER JOIN dbo.tbProductos p ON p.ProductoID = c.ProductoID
WHERE c.Grupo NOT IN ('SIN_CLASIFICAR','EXCLUIDO')
GROUP BY c.Grupo
ORDER BY MIN(p.Precio);

/* 4.3 — Detalle completo con el antes y el después */
SELECT
    c.Grupo,
    p.ProductoID,
    p.Nombre,
    PrecioAntes = r.PrecioAnterior,
    PrecioNuevo = p.Precio,
    StockAntes  = r.StockAnterior,
    StockNuevo  = p.Stock
FROM #Clasificacion c
INNER JOIN dbo.tbProductos p        ON p.ProductoID = c.ProductoID
LEFT  JOIN dbo.RespaldoPreciosStock r ON r.ProductoID = c.ProductoID
WHERE c.Grupo NOT IN ('SIN_CLASIFICAR','EXCLUIDO')
ORDER BY c.Grupo, p.Nombre;

SELECT
    Productos_actualizados = @Actualizados,
    Modo = CASE WHEN @Aplicar = 1 THEN 'APLICADO (COMMIT)' ELSE 'SIMULACION (ROLLBACK) — no se guardo nada' END;


/* ---------------------------------------------------------------
   5) Confirmar o revertir
   --------------------------------------------------------------- */
IF @Aplicar = 1
    COMMIT TRANSACTION;
ELSE
    ROLLBACK TRANSACTION;
GO

/* =====================================================================
   PARA DESHACER después de haber confirmado (@Aplicar = 1):

   BEGIN TRANSACTION;
       UPDATE p
       SET p.Precio = r.PrecioAnterior,
           p.Stock  = r.StockAnterior
       FROM dbo.tbProductos p
       INNER JOIN dbo.RespaldoPreciosStock r ON r.ProductoID = p.ProductoID;
   COMMIT TRANSACTION;
   ===================================================================== */
