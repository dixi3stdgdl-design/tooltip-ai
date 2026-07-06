# Lista de Verificación para Grabación de Demo de Tooltip AI

## Equipo Necesario

### Hardware
- **Computadora principal**: Windows 10/11 o macOS (preferiblemente Windows para la demo principal)
- **Monitor**: Resolución mínima 1920x1080 (Full HD), recomendado 2560x1440
- **Ratón**: Mouse con botones silenciosos (evitar clics ruidosos)
- **Teclado**: Preferiblemente mecánico con switches silenciosos
- **Micrófono**: 
  - Opción 1: Micrófono USB condensador (Blue Yeti, Audio-Technica AT2020)
  - Opción 2: Micrófono de solapa (Rode Wireless GO II)
  - Opción 3: Auriculares con micrófono integrado de buena calidad
- **Auriculares**: Para escuchar audio de referencia sin feedback
- **Iluminación**: Si se graba webcam, ring light o luz de relleno suave

### Software para Instalar
- **Sistema Operativo**: Windows 10/11 (build 19041+) o macOS 12+
- **Aplicaciones para la demo**:
  - Microsoft Excel (versión 365 o 2019+)
  - Google Chrome (última versión estable)
  - Visual Studio Code (última versión estable)
  - Tooltip AI (versión más reciente del proyecto)
- **Software de grabación**:
  - OBS Studio (recomendado) o Camtasia
  - Plugin: StreamFX para efectos de transición
- **Software de edición**:
  - Adobe Premiere Pro o DaVinci Resolve (gratuito)
  - Audacity para edición de audio
- **Utilidades**:
  - Mouse Highlighter (para resaltar cursor)
  - ScreenRuler (para medir elementos)

## Configuración Pre-Grabación

### Configuración del Sistema
1. **Resolución de pantalla**: 1920x1080 (Full HD)
2. **Escala de pantalla**: 100% (sin escalado)
3. **Frecuencia de actualización**: 60Hz mínimo
4. **Animaciones del sistema**: Desactivar (para mayor fluidez)
5. **Notificaciones**: Desactivar todas las notificaciones del sistema
6. **Fondo de escritorio**: Usar fondo de escritorio sólido oscuro o gradiente simple
7. **Iconos del escritorio**: Ocultar todos los iconos del escritorio

### Configuración de Aplicaciones
1. **Excel**:
   - Abrir libro de ejemplo con datos de presupuesto
   - Configurar vista en Zoom 100%
   - Ocultar barra de fórmulas
   - Asegurar que las celdas tengan formato claro

2. **Chrome**:
   - Abrir 3-4 pestañas de ejemplo
   - Configurar barra de marcadores visible
   - Usar tema claro para mejor contraste
   - Abrir Developer Tools (para mostrar integridad)

3. **VSCode**:
   - Abrir proyecto de ejemplo
   - Configurar tema oscuro (para contraste con tooltip)
   - Mostrar panel de terminal integrado
   - Abrir 2-3 archivos de código

### Configuración de Tooltip AI
1. Verificar que Tooltip AI esté ejecutándose
2. Configurar tema "Claro" para mejor visibilidad en video
3. Verificar que los tooltips muestren información completa
4. Ajustar opacidad al 95% para mejor legibilidad
5. Desactivar sonidos de notificación

## Configuración de OBS Studio

### Configuración de Grabación
1. **Formato**: MP4 (H.264)
2. **Resolución**: 1920x1080
3. **Tasa de cuadros**: 30 FPS (suficiente para demo)
4. **Bitrate**: 8000 kbps (calidad alta)
5. **Codificador**: x264 (CPU) o NVENC (GPU NVIDIA)

### Configuración de Audio
1. **Dispositivo de audio**: Micrófono seleccionado
2. **Tasa de muestreo**: 48 kHz
3. **Formato**: 16 bits
4. **Filtro de ruido**: Activar cancelación de ruido
5. **Compresor**: Ratio 3:1, umbral -18dB

### Escenas y Fuentes
1. **Escena Principal**: Captura de pantalla completa
2. **Escena Webcam**: Para introducción y cierre
3. **Escena Transición**: Para cambio entre aplicaciones
4. **Fuentes**:
   - Captura de pantalla (Display Capture)
   - Captura de ventana (Window Capture) para cada app
   - Audio de micrófono
   - Audio del sistema (deshabilitado)

## Proceso de Grabación Paso a Paso

### Pre-Grabación (15 minutos antes)
1. [ ] Verificar que todo el software esté instalado y actualizado
2. [ ] Ejecutar Tooltip AI y verificar funcionamiento
3. [ ] Configurar OBS Studio con los ajustes indicados
4. [ ] Hacer prueba de audio (grabar 30 segundos, escuchar)
5. [ ] Hacer prueba de video (verificar calidad y framerate)
6. [ ] Cerrar todas las aplicaciones innecesarias
7. [ ] Desactivar notificaciones del sistema
8. [ ] Verificar que el ratón funcione correctamente
9. [ ] Preparar script de teleprompter
10. [ ] Tener agua a mano (evitar sequedad de boca)

### Grabación - Escena 1: Intro (0:00 - 0:15)
1. [ ] Iniciar grabación en OBS
2. [ ] Mostrar logo de Tooltip AI con gradiente animado
3. [ ] Decir línea del script: "Tooltip AI transforma cualquier software en algo que entiendes al instante"
4. [ ] Mantener cursor visible pero no interactuar
5. [ ] Duración: 15 segundos exactos

### Grabación - Escena 2: Problema (0:15 - 0:30)
1. [ ] Mostrar pantalla con usuario confundido
2. [ ] Pasar mouse lentamente sobre diferentes elementos
3. [ ] Mostrar tooltips nativos (incompletos)
4. [ ] Decir línea: "Los tooltips nativos no explican nada. Miras un botón y solo ves 'btnSubmit'. No sabes qué hace."
5. [ ] Mantener ritmo pausado para énfasis

### Grabación - Escena 3: Solución - Excel (0:30 - 1:00)
1. [ ] Abrir Excel con libro de ejemplo
2. [ ] Pasar mouse sobre celda B2 (presupuesto Marketing: $12,500)
3. [ ] Esperar a que aparezca tooltip completo
4. [ ] Mantener mouse sobre celda 3-4 segundos
5. [ ] Decir línea: "Tooltip AI entiende que es una celda de presupuesto, te dice el formato y el atajo"
6. [ ] Pasar a otras celdas para mostrar variedad
7. [ ] Duración: 30 segundos

### Grabación - Escena 4: Solución - Chrome (1:00 - 1:15)
1. [ ] Cambiar a Chrome con pestañas abiertas
2. [ ] Pasar mouse sobre barra de direcciones
3. [ ] Mostrar tooltip: "Barra de dirección — Escribe URL o búsqueda — Ctrl+L"
4. [ ] Decir línea: "En Chrome, te dice exactamente qué puedes hacer"
5. [ ] Pasar sobre botones de navegación
6. [ ] Duración: 15 segundos

### Grabación - Escena 5: Solución - VSCode (1:15 - 1:30)
1. [ ] Cambiar a VSCode con proyecto abierto
2. [ ] Pasar mouse sobre terminal integrado
3. [ ] Mostrar tooltip: "Terminal integrada — Ejecuta comandos — Ctrl+`"
4. [ ] Decir línea: "En VSCode, te muestra los atajos que no conocías"
5. [ ] Pasar sobre barra lateral
6. [ ] Duración: 15 segundos

### Grabación - Escena 6: Velocidad (1:30 - 1:45)
1. [ ] Mostrar comparación de velocidad
2. [ ] Visual: "8.3ms vs 2,000ms"
3. [ ] Decir línea: "8 milisegundos. Más rápido que parpadeas"
4. [ ] Mostrar gráfico o animación de velocidad
5. [ ] Duración: 15 segundos

### Grabación - Escena 7: CTA (1:45 - 2:00)
1. [ ] Mostrar landing page de Tooltip AI
2. [ ] Resaltar botón de descarga
3. [ ] Decir línea: "Descarga gratis en tooltip-ai.com"
4. [ ] Mantener en pantalla 3 segundos después de hablar
5. [ ] Duración: 15 segundos

### Post-Grabación
1. [ ] Detener grabación en OBS
2. [ ] Guardar archivo con nombre: `tooltip-ai-demo-raw-YYYYMMDD.mp4`
3. [ ] Verificar calidad del audio grabado
4. [ ] Verificar calidad del video grabado
5. [ ] Anotar cualquier error para re-grabar si es necesario

## Notas de Post-Producción

### Edición de Video
1. **Corte inicial**: Recortar inicio y fin de grabación
2. **Transiciones**: Usar transiciones suaves (dissolve, fade) entre escenas
3. **Zoom**: Aplicar zoom suave en momentos clave (tooltips apareciendo)
4. **Velocidad**: Acelerar partes lentas (movimiento de mouse entre apps)
5. **Texto superpuesto**: Añadir labels para applications (Excel, Chrome, VSCode)
6. **Gráficos**: Añadir gráfico de comparación de velocidad
7. **Logo**: Añadir logo de Tooltip AI en esquina inferior derecha

### Edición de Audio
1. **Ruido de fondo**: Reducir ruido ambiental
2. **Volumen**: Normalizar a -3dB
3. **Compresión**: Aplicar compresión suave (ratio 2:1)
4. **Ecualización**: Realzar frecuencias medias (2-4kHz) para claridad
5. **Música**: Añadir música de fondo tech/upbeat (volumen bajo, -20dB)
6. **Efectos**: Añadir efectos de sonido sutiles para aparición de tooltips

### Exportación Final
1. **Formato**: MP4 (H.264)
2. **Resolución**: 1920x1080
3. **Tasa de cuadros**: 30 FPS
4. **Bitrate**: 10,000 kbps
5. **Subtítulos**: Incluir subtítulos en español e inglés
6. **Thumbnail**: Crear thumbnail atractivo con tooltip de ejemplo

### Distribución
1. **YouTube**: Subir con título "Tooltip AI - Tooltips Inteligentes para Tu Software"
2. **Twitter/X**: Crear clip de 30 segundos de la parte más impactante
3. **LinkedIn**: Versión vertical (9:16) para stories
4. **GitHub**: Añadir al README.md
5. **Landing Page**: Insertar en sección de demo

## Checklist Final de Calidad

### Audio
- [ ] Voz clara y sin distorsión
- [ ] Sin ruidos de fondo molestos
- [ ] Volumen consistente durante toda la demo
- [ ] Música de fondo no distrae del mensaje

### Video
- [ ] Resolución nítida (1080p mínimo)
- [ ] Colores vibrantes y correctos
- [ ] Texto legible en tooltips
- [ ] Transiciones suaves entre escenas
- [ ] Cursor visible pero no distractivo

### Contenido
- [ ] Mensaje claro en cada escena
- [ ] Tiempo correcto (2 minutos exactos)
- [ ] Todas las features mostradas
- [ ] Call-to-action efectivo
- [ ] Sin errores o tropiezos

### Técnico
- [ ] Archivo final en formato correcto
- [ ] Subtítulos sincronizados
- [ ] Thumbnail atractivo
- [ ] Descripción completa para YouTube
- [ ] Tags relevantes añadidos

## Notas Adicionales

### Consejos para el Presentador
1. **Practica**: Graba 2-3 veces antes de la grabación final
2. **Ritmo**: Habla a velocidad natural, no muy rápido
3. **Énfasis**: Pausa ligeramente antes de información clave
4. **Visual**: Mantén el cursor moviéndose suavemente, nunca estático
5. **Error**: Si te equivocas, para y repite la frase completa

### Troubleshooting Común
1. **Audio bajo**: Verificar nivel del micrófono en OBS
2. **Video pixelado**: Aumentar bitrate en configuración de OBS
3. **Mouse lag**: Cerrar aplicaciones en segundo plano
4. **Tooltips no aparecen**: Verificar que Tooltip AI esté ejecutándose
5. **Ruido de fondo**: Usar filtro de cancelación de ruido en OBS

### Respaldo
1. **Grabar 2 versiones**: Una completa, otra con tomas alternativas
2. **Guardar archivos RAW**: No eliminar hasta verificar calidad final
3. **Backup en nube**: Subir a Google Drive o OneDrive
4. **Documentar errores**: Anotar problemas para futuras grabaciones