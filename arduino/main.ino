int lecturas[][5] = {
  {0,0,0,0,0},
  {0,0,0,0,0},
  {0,0,0,0,0},
  {0,0,0,0,0},
  {0,0,0,0,0},
  {0,0,0,0,0}
};

int lecturas_pasadas[] = {
  0,
  0,
  0,
  0,
  0,
  0
};

void setup() {
  Serial.begin(9600);
}

void loop() {
  for (int sensor = 0; sensor <= 5; sensor++)
  {
    readSensor(sensor);
    delay(25);
  }
}

void readSensor(int sensor_pin) {
  int lectura = analogRead(sensor_pin);
  int distancia = lectura / 2;
  if (distancia > 22)
    distancia = 0;
  if (distancia > 20)
    distancia = 20;

  for (int i = 1; i < 5; i++)
    lecturas[sensor_pin][i] = lecturas[sensor_pin][i-1];

  lecturas[sensor_pin][0] = distancia;

  int promedio = 0;
  
  for (int i = 0; i < 5; i++)
    promedio += lecturas[sensor_pin][i];

  promedio = map(promedio/5, 0, 20, 0, 6);

  if (promedio > 0 && promedio != lecturas_pasadas[sensor_pin])
  {
    lecturas_pasadas[sensor_pin] = promedio;
    int enviar = (6 * promedio) - (5 - sensor_pin);
    Serial.write(enviar);
  }
}
