using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public Boolean air_control = true;
    public float gravity = 1;
    public float max_falling_speed = 10;
    public float h_speed = 5;
    public float h_speed_smoothening = 0.5f;
    public float h_slow_smoothening = 1;
    public float h_slow_smoothening_in_air = 0.5f;
    public float j_force = 80;
    public float det_distance = 0.001f;
    public float j_decreasing;
    public float strong_j_decreasing;

    private float current_h_speed;
    public float current_v_speed;
    //public Boolean in_air = false;

    //private GameObject player;
    private float h_dimension, v_dimension;

    public Boolean j_request;
    private Boolean was_jumping;
    private float movement_request;
    private float x_speed, y_speed;
    private Vector3 speed;

    //Rays definition
    private RaycastHit2D landed_left, landed_right, landed_middle;
    private RaycastHit2D ground_check_left, ground_check_right, ground_check_middle;
    private RaycastHit2D foot_stuck_right, foot_stuck_left, knee_stuck_right, knee_stuck_left;
    private RaycastHit2D head_stuck_right, head_stuck_left;
    private RaycastHit2D ceiling_left, ceiling_right, ceiling_middle;
    //For walljump
    private RaycastHit2D top_right_wall, bottom_right_wall, top_left_wall, bottom_left_wall;

    //Booleans
    public Boolean landed;
    public Boolean grounded;
    public Boolean wall_stuck_right, wall_stuck_left;
    public Boolean head_stuck;

    // Start is called before the first frame update
    void Start()
    {
        was_jumping = false;
        j_decreasing = j_force / 200;
        strong_j_decreasing = j_decreasing * 4;

        h_dimension = GetComponent<Renderer>().bounds.size.x;
        v_dimension = GetComponent<Renderer>().bounds.size.y;

        current_h_speed = 0;
        current_v_speed = 0;
    }

    private void FixedUpdate()
    {
        ////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////On lance les rayons//////////////////////////////
        // \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/ //
        landed_left = Physics2D.Raycast(transform.position + new Vector3(-h_dimension / 2, -v_dimension / 2, 0), -Vector2.up, det_distance * 2);
        landed_right = Physics2D.Raycast(transform.position + new Vector3(h_dimension / 2, -v_dimension / 2, 0), -Vector2.up, det_distance * 2);
        landed_middle = Physics2D.Raycast(transform.position + new Vector3(0, -v_dimension / 2, 0), -Vector2.up, det_distance * 2);

        ground_check_left = Physics2D.Raycast(transform.position + new Vector3(-h_dimension / 2, -v_dimension / 2, 0), -Vector2.up, det_distance);
        ground_check_right = Physics2D.Raycast(transform.position + new Vector3(h_dimension / 2, -v_dimension / 2, 0), -Vector2.up, det_distance);
        ground_check_middle = Physics2D.Raycast(transform.position + new Vector3(0, -v_dimension / 2, 0), -Vector2.up, det_distance);

        foot_stuck_right = Physics2D.Raycast(transform.position + new Vector3(h_dimension / 2, -v_dimension / 2, 0), Vector2.right, det_distance);
        foot_stuck_left = Physics2D.Raycast(transform.position + new Vector3(-h_dimension / 2, -v_dimension / 2, 0), -Vector2.right, det_distance);
        knee_stuck_right = Physics2D.Raycast(transform.position + new Vector3(h_dimension / 2, -v_dimension / 4, 0), Vector2.right, det_distance);
        knee_stuck_left = Physics2D.Raycast(transform.position + new Vector3(-h_dimension / 2, -v_dimension / 4, 0), -Vector2.right, det_distance);

        head_stuck_right = Physics2D.Raycast(transform.position + new Vector3(h_dimension / 2, v_dimension / 2, 0), Vector2.right, det_distance);
        head_stuck_left = Physics2D.Raycast(transform.position + new Vector3(-h_dimension / 2, v_dimension / 2, 0), -Vector2.right, det_distance);

        ceiling_right = Physics2D.Raycast(transform.position + new Vector3(h_dimension / 2, v_dimension / 2, 0), Vector2.up, det_distance);
        ceiling_left = Physics2D.Raycast(transform.position + new Vector3(-h_dimension / 2, v_dimension / 2, 0), Vector2.up, det_distance);
        ceiling_middle = Physics2D.Raycast(transform.position + new Vector3(0, v_dimension / 2, 0), Vector2.up, det_distance);
        // /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\ //
        ///////////////////////////////On lance les rayons//////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////Analyse des collisions//////////////////////////////
        // \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/ //
        landed = false;
        grounded = false;
        head_stuck = false;
        wall_stuck_right = false;
        wall_stuck_left = false;

        if (landed_left.collider != null || landed_middle.collider != null || landed_right.collider != null)
        {
            landed = true;
        }

        if (ground_check_left.collider != null || ground_check_middle.collider != null || ground_check_right.collider != null)
        {
            grounded = true;
        }

        if (ceiling_left.collider != null || ceiling_middle.collider != null || ceiling_right.collider != null)
        {
            head_stuck = true;
        }

        if (knee_stuck_right.collider != null || head_stuck_right.collider != null)
        {
            wall_stuck_right = true;
        }

        if (knee_stuck_left.collider != null || head_stuck_left.collider != null)
        {
            wall_stuck_left = true;
        }
        // /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\ //
        ////////////////////////////Analyse des collisions//////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////////
        //////////////////////Définition de la vitesse globale//////////////////////////
        // \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/ //

        /////////////////////////////Vitesse verticale//////////////////////////////////
        if (!was_jumping && j_request && landed)
        {
            //On est au sol et on demande de sauter
            //La force de saut est donc au maximum
            y_speed = j_force;
            //On enregistre le fait qu'on est en train de sauter
            was_jumping = true;
        }
        else if (current_v_speed > (j_force / 10))
        {
            if (was_jumping)
            {
                //Si le saut n'a pas été déclenché mais qu'on veut encore sauter en maintenant espace
                //On monte encore mais de moins en moins vite jusqu'à y_speed = 0
                //y_speed = current_v_speed - j_decreasing;
                y_speed = Mathf.Lerp(current_v_speed, 0, j_decreasing);
            }
            else
            {
                //On ne veut plus sauter, on va faire ralentir le personnage rapidement vers une vitesse nulle
                y_speed = Mathf.Lerp(current_v_speed, 0, strong_j_decreasing);
            }
        }
        else
        {
            //On maintient une vitesse de chute maximale pour éviter des déplacements insensés, et des bugs
            //y_speed = Math.Max(current_v_speed - gravity, -max_falling_speed);
            y_speed = -Mathf.Lerp(Math.Abs(current_v_speed), max_falling_speed, gravity);
        }
        //On enregistre toujours la dernière vitesse horizontale
        current_v_speed = y_speed;

        if (((y_speed < 0) && grounded) || ((y_speed > 0) && head_stuck))
        {
            y_speed = 0;
        }

        y_speed *= Time.fixedDeltaTime;

        //////////////////////////////Vitesse horizontale///////////////////////////////
        if (movement_request != 0 && (landed || air_control))
        {
            //Move request accepted
            x_speed = movement_request * Mathf.Lerp(current_h_speed, h_speed, h_speed_smoothening) * Time.fixedDeltaTime;
        }
        else if (current_h_speed != 0 && landed)
        {
            //Slowing down on the ground
            x_speed = Mathf.Lerp(current_h_speed, 0, h_slow_smoothening_in_air) * Time.fixedDeltaTime;
        }
        else if (current_h_speed != 0)
        {
            //Slowing down in air
            x_speed = Mathf.Lerp(current_h_speed, 0, h_slow_smoothening) * Time.fixedDeltaTime;
        }
        if ((x_speed < 0 && wall_stuck_left) || (x_speed > 0 && wall_stuck_right))
        {
            x_speed = 0;
        }

        // /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\ //
        //////////////////////Définition de la vitesse globale//////////////////////////
        ////////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////////
        ////////////////Définition de l'orientation du personnage///////////////////////
        // \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/  \/ //
        if (x_speed > 0)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else if (x_speed < 0)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
        //Si la vitesse est égale à 0, on ne touche à rien pour éviter de "reset" la rotation pour rien
        // /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\  /\ //
        ////////////////Définition de l'orientation du personnage///////////////////////
        ///////////////////////////////////////////////////////////////////////////////////

        //On enregistre la vitesse courante pour l'utiliser les prochaines frames
        current_h_speed = x_speed;

        //On définit enfin la vitesse actuelle de notre personnage, qu'on additionne à la position actuelle
        speed = new Vector3(x_speed, y_speed, 0);
        transform.position += speed;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Jump requested");
            j_request = true;
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            j_request = false;
            was_jumping = false;
        }
        movement_request = Input.GetAxis("Horizontal");
    }
}
