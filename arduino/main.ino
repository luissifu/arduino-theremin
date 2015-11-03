#include "pitches.h"

const int SENSOR = 1; //analogico
const int BOCINA = 2; //digital
const int DURACION_TONO = 20;

const int TONOS_SENSOR[] = {
  NOTE_B0,
  NOTE_B1,
  NOTE_B2,
  NOTE_B3,
  NOTE_B4,
  NOTE_B5,
  NOTE_B6,
  NOTE_B7
};

int lecturas[] = {
  0, 0, 0, 0, 0
};

void setup() {
  Serial.begin(9600);
}

void loop() {
  int lectura = analogRead(SENSOR);
  int distancia = lectura / 2;

  if (distancia > 20)
    distancia = 20;

  for (int i = 1; i < 5; i++)
    lecturas[i] = lecturas[i-1];

  lecturas[0] = distancia;

  int promedio = 0;
  for (int i = 0; i < 5; i++)
    promedio += lecturas[i];

  promedio = map(promedio/5, 0, 20, 0, 9);
  Serial.println(promedio);

  if (promedio > 0)
  {
    tone(BOCINA, TONOS_SENSOR[promedio-1], DURACION_TONO);
  }
}
