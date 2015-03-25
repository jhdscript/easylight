// Interaction entre Arduino et DotNet
// (Ce code fonctionne avec la classe DotNet "Arduino" )
// (Ce code s'inspire de plusieurs projets libres)
// www.zem.fr

int pt = 0;  			        			//Pointeur courant
boolean cfound = false;		        		//Fin de commande trouvée
byte currentCmd[200];                     	//Buffer de commande
long previousMillis = 0;                	//Interval de verification du watchdog
long interval = 1000;                   	//Interval d'émission entre 2 watchdogs

#define SOC 40								//Byte de debut de commande
#define EOC 41								//Byte de fin de commande

void setup() {
	Serial.flush();
	Serial.begin(9600);
	delay(500);
	Serial.print("(S)");					//On envoie une commande pour notifier que l'arduino est démarré
}

void loop() {
	
	//STEP1: Lecture du Buffer
	int sa;
	byte bt;
	sa = Serial.available();	        	//On recupere les données du port serie
	if (sa > 0) {			        		//On ecrit le buffer dans une variable jusqu'à ce que l'on rencontre le caractère de fin de commande
		for (int i=0; i < sa; i++){
			bt = Serial.read();
			currentCmd[pt] = bt;
			pt++;
			if (bt == EOC) {
				cfound = true;
				break;						//Le caractere de fin a été trouvé, on pourra proceder à l'execution de la commande
			}
		} 
	}

	//STEP2: Execution d'une commande
	if (cfound) {
		if (int(stringIn[0]) == SOC) {		//On verifie que le premier caractère est le caractere de debut de commande
			RunCommand();
		}
		ResetCurrentCmd();
		cfound = false;
	}

	//STEP3: Divers traitements
	//Vous pouvez ajouter des traitements ici
	
	//STEP4: Déclenchement du WatchDog
	if (millis() - previousMillis > interval) {
		previousMillis = millis();   
		Serial.print("(W)");       
	}
}

//Fonction qui reset la commande courante
void ResetCurrentCmd(void) {
  for (int i=0; i<=200; i++) {
    currentCmd[i] = 0;
    pt = 0;
  }
}

//Fonction qui distribue les differentes commandes
void RunCommand(void) {
	char c = stringIn[1];                	//Type de commande
 
	switch (c) {
		case 'D':							//Mode Dynamic
			break;
			
		case 'F':							//Mode Static
			break;

		break;
	}
}







