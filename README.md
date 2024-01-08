# Delivery_AA3

1.1 --> MyOctopusController line 211
1.2 --> MovingTarget line 65 (GameStuff / Blue Team / Blue Target)
1.3 --> IK_Scorpion line 85 (GameStuff / Scorpion)
1.4 --> IK_Scorpion line 67 (GameStuff / Scorpion) and MovingBall line 36 (GameStuff / Ball)


2.1 --> CustomRigidBody (GameStuff / Ball) 
2.2 --> Custom Rigidbody line 264, la formula seria F = 1/2pCLA*(WxV) que la hemos simplificado a F = WxV (GameStuff / Ball)
2.3 --> Custom Rigidbody line 208, en nuestro caso solo hemos dado posibilidad a rotaciones positivas (GameStuff / Ball)
2.4 --> Custom Rigidbody line 96 (GameStuff / Ball)
2.5 --> Custom Rigidbody line 166 (GameStuff / Ball)


//Hemos dividido el escorpion en 3 partes de modo que cada parte es mas influenciada por sus patas y hemos anadido una fuerza extra 
que tiende a unir el cuerpo, por lo que la variacion de posicion o rotacion de una parte sigue afectando a las otras

3.1 --> BodyManager line 130 (GameStuff / Scorpion / Body / BodyManager)
3.2 --> (Map / Obstacles)
3.3 --> BodyManager line 110 (GameStuff / Scorpion / Body / BodyManager)
3.4 --> BodyManager line 172 (GameStuff / Scorpion / Body / BodyManager)
3.5 --> BodyManager line 178 (GameStuff / Scorpion / Body / BodyManager)
3.6 --> IK_Scorpion line 95 (GameStuff / Scorpion)


4.1 --> MyScorpionController line 79
4.2 --> MyScorpionController line 56
4.3 --> MyScorpionController line 70


5.1 --> MyOctopusController line 165
5.2 --> MyOctopusController line 115 and 129
5.3 --> MyOctopusController line 135, no rotamos en el eje que permite que el tentaculo haga twist y de la espalda a la camara.