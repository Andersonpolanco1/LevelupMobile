namespace LevelUp.Mobile.Core.Enums
{
    public enum ExerciseType
    {
        /// <summary>
        /// Ejercicios de fuerza con peso externo (barra, mancuernas, máquinas, kettlebell).
        /// El peso corporal NO se incluye en el cálculo.
        /// <br/><br/>
        /// <b>Ejemplos:</b> Press banca, Sentadilla, Peso muerto, Curl con mancuernas.
        /// <br/><br/>
        /// <b>Campos requeridos:</b> Weight, Reps.
        /// <br/>
        /// <b>Campos opcionales:</b> RPE, RIR, Tempo, RestSeconds, Notes.
        /// <br/><br/>
        /// <b>Métricas disponibles:</b> Volumen (Weight × Reps), 1RM estimado, PR de carga,
        /// progresión de peso, volumen total por sesión.
        /// </summary>
        WeightedStrength = 1,

        /// <summary>
        /// Ejercicios de fuerza donde la resistencia principal es el peso corporal del usuario.
        /// Opcionalmente puede añadirse peso extra (lastre).
        /// <br/><br/>
        /// <b>Ejemplos:</b> Pull-ups, Dips, Push-ups, Chin-ups, Pistol squat.
        /// <br/><br/>
        /// <b>Campos requeridos:</b> Reps.
        /// <br/>
        /// <b>Campos opcionales:</b> ExtraWeight (lastre), RPE, RIR, RestSeconds, Notes.
        /// <br/><br/>
        /// <b>Métricas disponibles:</b> Máximo de reps, progresión hacia lastre,
        /// volumen estimado (si hay peso extra + peso corporal snapshot).
        /// </summary>
        BodyweightStrength = 2,

        /// <summary>
        /// Ejercicios basados en repeticiones donde el peso no es la métrica relevante.
        /// No se registra carga externa ni peso corporal.
        /// <br/><br/>
        /// <b>Ejemplos:</b> Crunches, Jump squats, Mountain climbers, Lunges sin peso.
        /// <br/><br/>
        /// <b>Campos requeridos:</b> Reps.
        /// <br/>
        /// <b>Campos opcionales:</b> Tempo, RestSeconds, Notes.
        /// <br/><br/>
        /// <b>Métricas disponibles:</b> Total de repeticiones, progresión semanal,
        /// resistencia muscular.
        /// </summary>
        RepetitionOnly = 3,

        /// <summary>
        /// Ejercicios estáticos donde la métrica principal es el tiempo bajo tensión.
        /// No hay movimiento, distancia ni carga externa.
        /// <br/><br/>
        /// <b>Ejemplos:</b> Plancha, Wall sit, Dead hang, L-sit.
        /// <br/><br/>
        /// <b>Campos requeridos:</b> DurationSeconds.
        /// <br/>
        /// <b>Campos opcionales:</b> RPE, RestSeconds, Notes.
        /// <br/><br/>
        /// <b>Métricas disponibles:</b> Tiempo total bajo tensión, progresión de duración,
        /// consistencia.
        /// </summary>
        Isometric = 4,

        /// <summary>
        /// Ejercicios cardiovasculares basados en tiempo y/o distancia.
        /// Puede incluir calorías e intensidad percibida.
        /// <br/><br/>
        /// <b>Ejemplos:</b> Correr, Ciclismo, Remo, Caminadora, Battle ropes.
        /// <br/><br/>
        /// <b>Campos requeridos:</b> DurationSeconds o DistanceMeters (al menos uno).
        /// <br/>
        /// <b>Campos opcionales:</b> DistanceMeters, Calories, IntensityLevel, RestSeconds, Notes.
        /// <br/><br/>
        /// <b>Métricas disponibles:</b> Tiempo total, ritmo (pace), distancia acumulada,
        /// calorías quemadas, progresión cardiovascular.
        /// </summary>
        Cardio = 5,

        /// <summary>
        /// Ejercicios de movilidad, flexibilidad y estiramientos.
        /// Puede registrarse por lado del cuerpo para detectar desbalances.
        /// <br/><br/>
        /// <b>Ejemplos:</b> Estiramiento femoral, Movilidad de hombro, Hip flexor stretch.
        /// <br/><br/>
        /// <b>Campos requeridos:</b> DurationSeconds.
        /// <br/>
        /// <b>Campos opcionales:</b> Side (Left/Right/Both), IntensityPerceived, Notes.
        /// <br/><br/>
        /// <b>Métricas disponibles:</b> Tiempo total de movilidad, consistencia semanal,
        /// balance izquierda/derecha.
        /// </summary>
        Mobility = 6
    }
}
