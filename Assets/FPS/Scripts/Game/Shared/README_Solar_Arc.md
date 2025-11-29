# 🌞 Guía del Arco Solar - Sistema Día/Noche

## ✅ CORRECCIÓN IMPLEMENTADA

El sol ahora describe un **arco realista** de este a oeste, similar al sol real en la Tierra.

---

## 🌍 CÓMO FUNCIONA EL ARCO SOLAR

### **Movimiento del Sol Durante el Día**

```
        ☀️ MEDIODÍA (12:00)
             (Máx. elevación)
                  |
                  |
   🌅 ESTE        |        🌇 OESTE
  (Amanecer)      |      (Atardecer)
   6:00 AM    ----+----     6:00 PM
              Horizonte
```

### **Ciclo Completo (Vista de Lado)**

```
Día (0.0 - 0.5):
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    ☀️ Mediodía
      /    \
     /      \
    /        \
   /          \
 🌅            🌇
Este ────────── Oeste
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Noche (0.5 - 1.0):
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Este ────────── Oeste
   \          /
    \        /
     \      /
      \    /
      🌙 Bajo
    horizonte
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

### **Vista Cenital (Desde Arriba)**

```
        Norte (270°)
           🌙
           |
           |
Este ──────┼────── Oeste
   🌅      |       🌇
   0°     │       180°
           |
           |
         Sur (90°)
          ☀️
```

---

## ⚙️ CONFIGURACIÓN EN EL INSPECTOR

### **LightingController → Configuración del Arco Solar**

```
🌞 Max Sun Elevation (30-90°)
   └─ Ángulo máximo del sol al mediodía
   └─ 60° = Inclinado (realista latitudes medias)
   └─ 90° = Vertical (ecuador)
   └─ 45° = Muy inclinado (latitudes altas)

🌙 Sun Depth Below Horizon (30-90°)
   └─ Qué tan bajo pasa el sol durante la noche
   └─ 60° = Profundo (transición más larga)
   └─ 30° = Superficial (amanece/anochece más rápido)

🧭 Sun Path Rotation Offset (-180 a 180°)
   └─ Ajuste fino de la dirección este-oeste
   └─ 0° = Este → Oeste estándar
   └─ 90° = Rotar el arco 90° horario
   └─ -90° = Rotar el arco 90° antihorario
```

---

## 📊 INFORMACIÓN DE DEBUG (Inspector)

Durante Play Mode, verás en tiempo real:

```
📊 Debug Info:
├─ Current Sun Elevation: 45.2°
├─ Current Sun Azimuth: 90.0°
└─ Sun Position: "☀️ Sur (Mediodía)"
```

**Posiciones Cardinals:**
- 🌅 **Este** (0°) - Amanecer
- ☀️ **Sur** (90°) - Mediodía
- 🌇 **Oeste** (180°) - Atardecer
- 🌙 **Norte** (270°) - Medianoche
- 🌅 **Este** (360°/0°) - Amanecer siguiente

---

## 🕐 POSICIONES DEL SOL POR HORA

| Hora | Tiempo Ciclo | Elevación | Azimut | Posición | Visible |
|------|--------------|-----------|--------|----------|---------|
| **00:00** | 0.00 | 0° | 0° | 🌅 Este | ✅ Horizonte |
| **03:00** | 0.125 | +30° | 45° | 🌤️ Este-Sur | ✅ Subiendo |
| **06:00** | 0.25 | +60° | 90° | ☀️ Sur | ✅ Mediodía |
| **09:00** | 0.375 | +30° | 135° | 🌤️ Sur-Oeste | ✅ Bajando |
| **12:00** | 0.5 | 0° | 180° | 🌇 Oeste | ✅ Horizonte |
| **15:00** | 0.625 | -30° | 225° | 🌑 Oeste-Norte | ❌ Bajo horizonte |
| **18:00** | 0.75 | -60° | 270° | 🌙 Norte | ❌ Medianoche |
| **21:00** | 0.875 | -30° | 315° | 🌑 Norte-Este | ❌ Bajo horizonte |
| **24:00** | 1.0 | 0° | 360°/0° | 🌅 Este | ✅ Horizonte |

---

## 🎯 AJUSTES RECOMENDADOS

### **Para Diferentes Latitudes**

**Ecuador (Trópicos):**
```
Max Sun Elevation: 90°
Sun Depth Below Horizon: 90°
Offset: 0°
Resultado: Sol casi vertical al mediodía
```

**Latitudes Medias (España, EE.UU.):**
```
Max Sun Elevation: 60°
Sun Depth Below Horizon: 60°
Offset: 0°
Resultado: Sol inclinado al mediodía (RECOMENDADO)
```

**Latitudes Altas (Escandinavia):**
```
Max Sun Elevation: 45°
Sun Depth Below Horizon: 45°
Offset: 0°
Resultado: Sol muy inclinado, nunca muy alto
```

### **Para Ajustar Dirección Este-Oeste**

Si el sol sale por el lado "incorrecto" en tu escena:

```
Sun Path Rotation Offset:
├─ +90° = Rotar todo el arco 90° horario
├─ -90° = Rotar todo el arco 90° antihorario
└─ ±180° = Invertir completamente (este ↔ oeste)
```

---

## 🧪 TESTING DEL ARCO SOLAR

### **Test 1: Ver Ciclo Completo Rápido**

```
1. TimeManager → Use Custom Duration ✅
2. Custom Cycle Duration: 60 segundos
3. TimeManagerControls → "🌅 Saltar a AMANECER"
4. Play → Observar arco completo en 1 minuto
```

### **Test 2: Verificar Mediodía**

```
1. TimeManagerControls → "☀️ Saltar a MEDIODÍA"
2. Inspector → LightingController → Debug Info
3. Verificar:
   - Sun Elevation: ~60° (o tu max configurado)
   - Sun Azimuth: ~90°
   - Sun Position: "☀️ Sur (Mediodía)"
```

### **Test 3: Verificar Medianoche**

```
1. TimeManagerControls → "🌙 Saltar a MEDIANOCHE"
2. Inspector → Debug Info
3. Verificar:
   - Sun Elevation: ~-60° (negativo = bajo horizonte)
   - Sun Azimuth: ~270°
   - Sun Position: "🌙 Norte (Medianoche) (Bajo horizonte)"
```

### **Test 4: Ver Amanecer/Atardecer**

```
1. Saltar a 06:00 (Amanecer)
   - Elevación: 0°
   - Posición: Este
   - Sol en horizonte

2. Saltar a 18:00 (Atardecer)
   - Elevación: 0°
   - Posición: Oeste
   - Sol en horizonte
```

---

## 🎨 VISUALIZACIÓN DEL ARCO

### **Ciclo Completo en Gráfica**

```
Elevación (°)
   90° │
       │
   60° │     ☀️ (Max)
       │    /  \
   30° │   /    \
       │  /      \
    0° ├─🌅────────🌇─────────────
       │          │ \        /
  -30° │          │  \      /
       │          │   \    /
  -60° │          │    🌙 (Min)
       │          │
       └──────────┴─────────────────
          0.0    0.5         1.0
          Amanecer  Atardecer  Amanecer
```

---

## 💡 TIPS AVANZADOS

### **1. Crear "Estaciones" del Año**

```csharp
// Verano: Sol más alto
maxSunElevation = 75f;

// Invierno: Sol más bajo
maxSunElevation = 45f;
```

### **2. Días Largos/Cortos**

```csharp
// DayNightCycle ScriptableObject
dayPercentage = 0.7f;    // Día largo (70%)
nightPercentage = 0.3f;  // Noche corta (30%)
```

### **3. Amaneceres/Atardeceres Más Lentos**

Puedes modificar la curva del arco usando `Mathf.SmoothStep` en lugar de `Mathf.Sin`:

```csharp
// En UpdateDirectionalLight():
// En lugar de:
rotationX = Mathf.Sin(dayProgress * Mathf.PI) * maxSunElevation;

// Usar:
rotationX = Mathf.SmoothStep(0, maxSunElevation, dayProgress);
```

---

## 🔧 TROUBLESHOOTING

### **Problema: El sol se mueve al revés**

**Solución:**
```
Sun Path Rotation Offset: +180°
```

### **Problema: El sol sale por el norte en lugar del este**

**Solución:**
```
Sun Path Rotation Offset: -90° o +90°
(Ajustar según tu escena)
```

### **Problema: El sol está demasiado bajo al mediodía**

**Solución:**
```
Max Sun Elevation: Aumentar a 70-90°
```

### **Problema: Las noches son demasiado brillantes**

**Solución:**
```
DayNightCycle → Night Light Intensity: 0.1-0.2
```

---

## 📐 MATEMÁTICA DEL ARCO

### **Fórmula de Elevación (Día)**

```
dayProgress = (cycleTime * 2) cuando cycleTime < 0.5
elevación = sin(dayProgress × π) × maxElevation

Ejemplo:
- cycleTime = 0.0 → elevación = 0° (horizonte este)
- cycleTime = 0.25 → elevación = 60° (mediodía)
- cycleTime = 0.5 → elevación = 0° (horizonte oeste)
```

### **Fórmula de Azimut**

```
azimut = cycleTime × 360° + offset

Ejemplo:
- cycleTime = 0.0 → azimut = 0° (este)
- cycleTime = 0.25 → azimut = 90° (sur)
- cycleTime = 0.5 → azimut = 180° (oeste)
- cycleTime = 0.75 → azimut = 270° (norte)
```

---

## ✅ RESULTADO FINAL

**Comportamiento del Sol:**

✅ Sale por el **ESTE** (horizonte)
✅ Sube en **ARCO** hasta el **SUR** (mediodía)
✅ Baja hacia el **OESTE** (horizonte)
✅ Pasa **POR DEBAJO** durante la noche
✅ Regresa al **ESTE** para el siguiente amanecer

**Completamente realista y configurable desde el Inspector!** 🌞

---

## 🎯 CHECKLIST DE VERIFICACIÓN

- [ ] El sol sale por el este (o dirección configurada)
- [ ] El sol alcanza su máxima altura al mediodía
- [ ] El sol se pone por el oeste
- [ ] Durante la noche, la luz apunta hacia abajo (sol bajo horizonte)
- [ ] El arco se completa en un ciclo
- [ ] Los ángulos en Debug Info son correctos
- [ ] La iluminación es realista

**¡Arco solar realista implementado! 🚀**
