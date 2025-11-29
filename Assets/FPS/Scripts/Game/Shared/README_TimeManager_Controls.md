# 🎮 Guía de Controles de Testing - TimeManager

## ✅ CAMBIOS IMPLEMENTADOS

Se han añadido **controles completos en el Inspector** para facilitar el testing y debugging del sistema de día/noche.

---

## 🎯 CARACTERÍSTICAS NUEVAS

### **1️⃣ Control de Duración del Ciclo en el Inspector**

**TimeManager → Inspector → Pruebas Rápidas (Override)**

```
✅ Use Custom Duration  (checkbox)
⏱️ Custom Cycle Duration Seconds  (slider: 10-7200 seg)
⚡ Time Speed Multiplier  (slider: 0.1-100x)
```

**Cómo usar:**
1. ✅ Activar `Use Custom Duration`
2. Ajustar `Custom Cycle Duration Seconds`:
   - **30 seg** = Ciclo súper rápido (testing)
   - **120 seg (2 min)** = Ciclo rápido (desarrollo)
   - **300 seg (5 min)** = Ciclo moderado
   - **7200 seg (2h)** = Ciclo realista
3. Ajustar `Time Speed Multiplier`:
   - **0.5x** = Mitad de velocidad (cámara lenta)
   - **1x** = Velocidad normal
   - **5x** = 5 veces más rápido
   - **50x** = Ultra rápido para testing

---

### **2️⃣ Información en Tiempo Real (Inspector)**

**TimeManager → Inspector → Información en Tiempo Real**

```
📊 Current Period: "☀️ Día" o "🌙 Noche"
🕐 Time Formatted: "14:30"
⏱️ Cycle Duration Used: 120 segundos
```

**Actualización automática** en Play Mode.

---

### **3️⃣ TimeManagerControls - Component Adicional** 🆕

**Componente opcional** para controles visuales ultra-rápidos.

**Setup:**
```
1. Seleccionar GameObject con TimeManager
2. Add Component → Time Manager Controls
3. Click derecho en el componente → Ver opciones
```

---

## 🎮 CONTROLES DISPONIBLES (Context Menu)

### **⏱️ DURACIÓN DEL CICLO**

Click derecho en **TimeManagerControls** → Ver opciones:

```
⚡ Ciclo MUY RÁPIDO (30 seg)     → Testing rápido
🏃 Ciclo RÁPIDO (1 min)          → Testing normal
🚶 Ciclo MODERADO (2 min)        → Desarrollo
🐢 Ciclo NORMAL (5 min)          → Testing largo
🕐 Ciclo LENTO (10 min)          → Realista corto
🌍 Ciclo REALISTA (2 horas)      → Juego final
```

### **🕐 SALTOS DE HORA**

```
🌅 Saltar a AMANECER (6:00)
☀️ Saltar a MEDIODÍA (12:00)
🌆 Saltar a ATARDECER (18:00)
🌙 Saltar a MEDIANOCHE (00:00)
🌃 Saltar a MADRUGADA (03:00)
```

### **⚡ VELOCIDAD DEL TIEMPO**

```
⏸️ PAUSAR Tiempo
🐌 Velocidad MUY LENTA (0.25x)
🐢 Velocidad LENTA (0.5x)
▶️ Velocidad NORMAL (1x)
⏩ Velocidad RÁPIDA (2x)
⏩⏩ Velocidad MUY RÁPIDA (5x)
⚡ Velocidad ULTRA RÁPIDA (10x)
🚀 Velocidad EXTREMA (50x)
```

### **🛠️ UTILIDADES**

```
🔄 RESETEAR Ciclo (volver a mediodía)
📊 Mostrar INFORMACIÓN del Sistema
⏭️ Avanzar 1 HORA
⏭️ Avanzar 6 HORAS
⏭️ Avanzar 12 HORAS (cambiar día/noche)
```

---

## 📋 ESCENARIOS DE USO

### **Escenario 1: Testing Rápido de Transiciones Día/Noche**

```
1. TimeManager → Use Custom Duration ✅
2. Custom Cycle Duration: 30 segundos
3. Time Speed Multiplier: 1x
4. Play → Observar 1 ciclo completo en 30 seg
```

**Resultado**: Día completo en 15 seg + Noche completa en 15 seg

---

### **Escenario 2: Verificar Comportamiento de Enemigos en Noche**

```
1. Play Mode
2. TimeManagerControls → Click derecho
3. "🌙 Saltar a MEDIANOCHE"
4. Observar enemigos
```

**Resultado**: Instantáneamente es medianoche, enemigos cambian comportamiento

---

### **Escenario 3: Ver Transiciones en Cámara Lenta**

```
1. TimeManager → Use Custom Duration ✅
2. Custom Cycle Duration: 300 segundos (5 min)
3. Time Speed Multiplier: 0.25x (cámara lenta)
4. Play → Observar transiciones suaves
```

**Resultado**: Ciclo muy lento para apreciar detalles visuales

---

### **Escenario 4: Testing de Todo un Día Rápido**

```
1. TimeManagerControls → "⚡ Ciclo MUY RÁPIDO (30 seg)"
2. TimeManagerControls → "⚡ Velocidad ULTRA RÁPIDA (10x)"
3. Play
```

**Resultado**: Ciclo completo en **3 segundos** (30 seg / 10x)

---

### **Escenario 5: Verificar Eventos Horarios**

```
1. TimeManagerControls → "🌅 Saltar a AMANECER"
2. Observar eventos
3. TimeManagerControls → "⏭️ Avanzar 6 HORAS"
4. Repetir
```

**Resultado**: Saltar entre eventos específicos sin esperar

---

## 🎯 CASOS DE USO ESPECÍFICOS

### **Testing de Iluminación**

```
Duración: 120 segundos (2 min)
Velocidad: 1x
Saltar a: Mediodía → Atardecer → Medianoche → Amanecer
Observar: Cambios de luz direccional y skybox
```

### **Testing de Enemigos**

```
Duración: 60 segundos (1 min)
Velocidad: 2x
Ciclo: Día completo en 30 seg
Observar: Cambio de velocidad/agresividad enemigos
```

### **Testing de UI (Reloj)**

```
Duración: 30 segundos
Velocidad: 5x
Observar: Reloj cambiando rápidamente
Verificar: Formato correcto, cambios visuales
```

### **Demo para Cliente**

```
Duración: 300 segundos (5 min)
Velocidad: 1x
Saltar a: Mediodía
Mostrar: Transición completa día → noche
```

---

## 💡 TIPS Y TRUCOS

### **1. Combinaciones Útiles**

**Testing ultra-rápido:**
```
Duración: 30 seg + Velocidad: 50x = Ciclo en 0.6 segundos
```

**Observación detallada:**
```
Duración: 300 seg + Velocidad: 0.25x = Ciclo en 20 minutos
```

**Salto rápido entre períodos:**
```
Usar "Avanzar 12 HORAS" repetidamente
```

### **2. Atajos de Teclado (con TimeSystemDebugger)**

Si tienes **TimeSystemDebugger** en escena:

```
F = Fast Forward (avance rápido)
P = Pause/Resume
R = Reset
Shift + 1 = Amanecer
Shift + 2 = Mediodía
Shift + 3 = Atardecer
Shift + 4 = Medianoche
```

### **3. Workflow Recomendado**

**Durante Desarrollo:**
```
1. Usar "Ciclo RÁPIDO (1 min)"
2. Velocidad Normal (1x)
3. Saltar a horas específicas cuando necesites
```

**Para Demos:**
```
1. Usar "Ciclo MODERADO (2 min)"
2. Velocidad Normal (1x)
3. Empezar en Mediodía
4. Dejar correr un ciclo completo
```

**Para Testing Final:**
```
1. Usar "Ciclo REALISTA (2 horas)"
2. Velocidad Normal (1x)
3. Probar gameplay real
```

---

## 🔧 API PÚBLICA NUEVA

```csharp
// TimeManager - Nuevos métodos
timeManager.SetCustomCycleDuration(120f);     // 2 minutos
timeManager.SetUseCustomDuration(true);        // Activar custom
timeManager.SetTimeSpeedMultiplier(5f);        // 5x velocidad
timeManager.GetTimeSpeedMultiplier();          // Obtener velocidad
timeManager.GetCurrentCycleDuration();         // Duración actual

// TimeManagerControls - Métodos públicos
controls.ApplyDurationPresetByName("Rápido");  // Por nombre
controls.ApplyHourPresetByName("Amanecer");    
controls.ApplySpeedPresetByName("Ultra");      
```

---

## 📊 TABLA DE REFERENCIA RÁPIDA

| Nombre | Duración | Uso Recomendado |
|--------|----------|-----------------|
| ⚡ Muy Rápido | 30 seg | Testing básico |
| 🏃 Rápido | 1 min | Desarrollo iterativo |
| 🚶 Moderado | 2 min | Testing general |
| 🐢 Normal | 5 min | Testing detallado |
| 🕐 Lento | 10 min | Near-realista |
| 🌍 Realista | 2 horas | Juego final |

| Velocidad | Multiplicador | Ejemplo |
|-----------|---------------|---------|
| 🐌 Muy Lenta | 0.25x | 30 seg → 2 min |
| 🐢 Lenta | 0.5x | 30 seg → 1 min |
| ▶️ Normal | 1x | 30 seg → 30 seg |
| ⏩ Rápida | 2x | 30 seg → 15 seg |
| ⏩⏩ Muy Rápida | 5x | 30 seg → 6 seg |
| ⚡ Ultra | 10x | 30 seg → 3 seg |
| 🚀 Extrema | 50x | 30 seg → 0.6 seg |

---

## ✅ CHECKLIST DE VERIFICACIÓN

**Antes de Testear:**
- [ ] TimeManager tiene DayNightCycle asignado
- [ ] Use Custom Duration está activado (si quieres override)
- [ ] Custom Cycle Duration está configurado
- [ ] TimeManagerControls está añadido (opcional)

**Durante Testing:**
- [ ] La hora cambia correctamente
- [ ] El período (día/noche) se muestra correcto
- [ ] La duración se respeta
- [ ] El multiplicador funciona
- [ ] Los saltos de hora funcionan

**Verificación Visual:**
- [ ] El skybox cambia de color
- [ ] La luz direccional rota
- [ ] Los enemigos cambian comportamiento
- [ ] El reloj UI se actualiza

---

## 🎉 RESULTADO FINAL

Ahora tienes **control total** del sistema de tiempo desde el Inspector:

✅ **Ajustar duración** del ciclo en tiempo real
✅ **Cambiar velocidad** del tiempo (0.1x a 100x)
✅ **Saltar a horas** específicas
✅ **Información visual** en el Inspector
✅ **Context menu** con opciones rápidas
✅ **Sin código** necesario para testing

**¡Listo para hacer pruebas eficientes del sistema día/noche! 🚀**
