# 🔧 Guía de Solución: Errores de Skybox

## ❌ Error Común

```
Material 'Default-Skybox' with Shader 'Skybox/Procedural' doesn't have a color property '_SkyColor'
```

## ✅ Solución Aplicada

El **LightingController** ha sido actualizado para ser compatible con **múltiples tipos de shaders de skybox**.

---

## 🌌 Tipos de Skybox Soportados

### 1️⃣ **Skybox/Procedural** (Default Unity) ✅ RECOMENDADO
- **Ventajas**: No requiere texturas, procedural, ligero
- **Propiedades modificadas**:
  - `_SkyTint` - Color del cielo
  - `_Exposure` - Exposición (brillo)
  - `_AtmosphereThickness` - Grosor atmosférico

### 2️⃣ **Skybox/6 Sided** ✅ SOPORTADO
- **Ventajas**: Control individual de cada cara del cubo
- **Propiedades modificadas**:
  - `_Tint` - Tinte general
  - `_Exposure` - Exposición

### 3️⃣ **Skybox/Cubemap** ✅ SOPORTADO
- **Ventajas**: Usa texturas cubemap profesionales
- **Propiedades modificadas**:
  - `_Tint` - Tinte general
  - `_Exposure` - Exposición

### 4️⃣ **Shader Personalizado** ✅ SOPORTADO
- Debe tener propiedad `_SkyColor`
- Compatible si implementa las propiedades estándar

---

## 🚀 Configuración Rápida

### Opción A: Usar Skybox Actual (Procedural)

Tu skybox actual **YA ES COMPATIBLE**. El error se ha corregido en el código.

**No necesitas hacer nada**, el sistema funcionará automáticamente.

### Opción B: Crear Skybox Automáticamente

1. **Crear GameObject vacío**: `SkyboxManager`
2. **Add Component**: `SkyboxHelper`
3. **Configurar**:
   - ✅ `Log Info On Start` = true
   - ✅ `Auto Create Skybox` = true (si no tienes skybox)
   - Elegir `Preferred Shader Type` = Procedural
4. **Play** → Se creará automáticamente

### Opción C: Configurar Manualmente

```csharp
// En Window > Rendering > Lighting > Environment
Skybox Material = [Crear nuevo material con Skybox/Procedural]
```

**Pasos detallados**:
1. Project → Create → Material → Nombre: "DayNightSkybox"
2. Inspector → Shader → Skybox → Procedural
3. Lighting Settings → Environment → Skybox Material → Asignar "DayNightSkybox"

---

## 🔍 Diagnóstico

### Verificar Compatibilidad

**Método 1: Usar SkyboxHelper**
```csharp
// Añadir SkyboxHelper a cualquier GameObject
// Click derecho en componente → Log Skybox Info
```

**Método 2: Código directo**
```csharp
LightingController lighting = FindObjectOfType<LightingController>();
Debug.Log(lighting.GetSkyboxInfo());
bool compatible = lighting.IsSkyboxCompatible();
```

### Interpretar Resultados

✅ **Compatible**: Muestra propiedades detectadas
⚠️ **No compatible**: Sugiere shader alternativo

---

## 📋 Tabla de Compatibilidad

| Shader | _SkyTint | _Tint | _SkyColor | Compatible |
|--------|----------|-------|-----------|------------|
| Skybox/Procedural | ✅ | ❌ | ❌ | ✅ **Recomendado** |
| Skybox/6 Sided | ❌ | ✅ | ❌ | ✅ Soportado |
| Skybox/Cubemap | ❌ | ✅ | ❌ | ✅ Soportado |
| Custom (con _SkyColor) | ❌ | ❌ | ✅ | ✅ Soportado |
| Otro shader | ❌ | ❌ | ❌ | ⚠️ Requiere modificación |

---

## 🛠️ Funcionalidad del LightingController Actualizado

```csharp
UpdateSkybox() ahora detecta automáticamente:

1. Skybox/Procedural
   └─> Modifica: _SkyTint, _Exposure, _AtmosphereThickness

2. Skybox/6 Sided
   └─> Modifica: _Tint, _Exposure

3. Skybox/Cubemap
   └─> Modifica: _Tint, _Exposure

4. Custom (_SkyColor)
   └─> Modifica: _SkyColor

5. Desconocido
   └─> Log warning, solo actualiza luz ambiente
```

---

## 🎨 Configuraciones Recomendadas

### Para Skybox/Procedural

**Configuración DÍA**:
```csharp
_SkyTint = Color(0.5, 0.7, 1.0)      // Azul cielo
_Exposure = 1.3                       // Brillante
_AtmosphereThickness = 1.0            // Atmósfera normal
```

**Configuración NOCHE**:
```csharp
_SkyTint = Color(0.05, 0.05, 0.15)   // Azul oscuro
_Exposure = 0.8                       // Oscuro
_AtmosphereThickness = 0.5            // Atmósfera reducida
```

### Para Skybox/6 Sided

**Configuración DÍA**:
```csharp
_Tint = Color(0.47, 0.76, 1.0)       // Azul cielo
_Exposure = 1.0                       // Normal
```

**Configuración NOCHE**:
```csharp
_Tint = Color(0.05, 0.05, 0.15)      // Azul oscuro
_Exposure = 0.5                       // Oscuro
```

---

## 🐛 Solución de Problemas

### Problema: "Doesn't have property '_SkyColor'"

**Causa**: Skybox/Procedural usa `_SkyTint`, no `_SkyColor`
**Solución**: ✅ **Ya corregido en el código**

### Problema: Skybox no cambia de color

**Diagnóstico**:
```csharp
1. Verificar que LightingController tenga referencia al skybox
2. Verificar que DayNightConfig esté asignado
3. Usar SkyboxHelper para validar compatibilidad
```

**Solución**:
- Inspector → LightingController → Skybox Material → Asignar material

### Problema: Warning "shader no compatible"

**Solución**: Cambiar shader a Skybox/Procedural
```
Material → Inspector → Shader → Skybox → Procedural
```

---

## 📝 Métodos de Contexto (Click Derecho)

En el componente **SkyboxHelper**, click derecho:
- **Log Skybox Info** → Muestra información detallada
- **Validate Compatibility** → Verifica compatibilidad
- **Create Procedural Skybox** → Crea skybox procedural
- **Create 6 Sided Skybox** → Crea skybox de 6 caras

---

## ✅ Checklist de Verificación

- [ ] LightingController tiene DayNightConfig asignado
- [ ] LightingController tiene Skybox Material asignado
- [ ] LightingController tiene Directional Light asignado
- [ ] Material de skybox usa shader compatible
- [ ] SkyboxHelper muestra "✅ Compatible"
- [ ] No hay errores en consola al iniciar
- [ ] Skybox cambia de color con el tiempo

---

## 🎯 Próximos Pasos

1. **Verificar**: Play mode → No debe haber errores
2. **Testear**: TimeSystemDebugger → Tecla F (fast forward)
3. **Observar**: El skybox debe cambiar de azul claro a azul oscuro
4. **Ajustar**: DayNightConfig → Modificar colores a gusto

---

## 📞 Referencia Rápida de Código

```csharp
// Obtener información del skybox
LightingController lc = FindObjectOfType<LightingController>();
Debug.Log(lc.GetSkyboxInfo());

// Verificar compatibilidad
bool compatible = lc.IsSkyboxCompatible();

// Forzar actualización
lc.ForceLightingUpdate();

// Crear skybox automáticamente
SkyboxHelper helper = gameObject.AddComponent<SkyboxHelper>();
helper.CreateCompatibleSkybox();
```

---

**Estado: ✅ PROBLEMA RESUELTO**

El sistema ahora es compatible con todos los shaders de skybox estándar de Unity.
