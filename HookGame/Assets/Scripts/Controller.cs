using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public Boolean air_control = true;
    public float gravity = 0.0005f;
    public float h_speed = 5;
    public float h_speed_smoothening = 0.5f;
    public float h_slow_smoothening = 1;
    public float h_slow_smoothening_in_air = 0.5f;
    public float j_force = 0.001f;
    public float det_distance = 0.001f;
    public float j_decreasing;

    private float last_j_force = 0;
    private float current_h_speed, current_v_speed;
    public Boolean in_air = false;

    //private GameObject player;
    private float h_dimension, v_dimension;

    public Boolean j_request;
    private float movement_request;
    private float x_speed, y_speed;
    private float acceleration;
    private Vector3 speed;

    //Rays definition
    private RaycastHit2D landed_left, landed_right, landed_middle;
    private RaycastHit2D ground_check_left, ground_check_right, ground_check_middle;
    private RaycastHit2D foot_stuck_right, foot_stuck_left, knee_stuck_right, knee_stuck_left;
    private RaycastHit2D head_stuck_right, head_stuck_left;
    private RaycastHit2D ceiling_left, ceiling_right, ceiling_middle;
    //For walljump
    private RaycastHit2D top_right_wall, bottom_right_wall, top_left_wall, bottom_left_wall;

    // Start is called before the first frame update
    void Start()
    {
        acceleration = (float)((y_speed < 0) ? Math.Pow(Math.Abs(y_speed), 2) : 1);

        j_decreasing = j_force / 100;

        h_dimension = GetComponent<Renderer>().bounds.size.x;
        v_dimension = GetComponent<Renderer>().bounds.size.y;

        current_h_speed = 0;
        current_v_speed = 0;
    }

    private void FixedUpdate()
    {
        //Ray casting
        landed_left = Physics2D.Raycast(transform.position + new Vector3(-h_dimension / 2, -v_dimension / 2, 0), -Vector2.up, det_distance * 2);
        landed_right = Physics2D.Raycast(transform.position + new Vector3(h_dimension / 2, -v_dimension / 2, 0), -Vector2.up, det_distance * 2);
        landed_middle = Physics2D.Raycast(transform.position + new Vector3(0, -v_dimension / 2, 0), -Vector2.up, det_distance * 2);

        ground_check_left = Physics2D.Raycast(transform.position + new Vector3(-h_dimension / 2, -v_dimension / 2, 0), -Vector2.up, det_distance);
        ground_check_right = Physics2D.Raycast(transform.position + new Vector3(h_dimension / 2, -v_dimension / 2, 0), -Vector2.up, det_distance);
        ground_check_middle = Physics2D.Raycast(transform.position + new Vector3(0, -v_dimension / 2, 0), -Vector2.up, det_distance);

        foot_stuck_right = Physics2D.Raycast(transform.position + new Vector3(h_dimension / 2, -v_dimension / 2, 0), Vector2.right, det_distance);
        foot_stuck_left = Physics2D.Raycast(transform.position + new Vector3(-h_dimension / 2, -v_dimension / 2, 0), -Vector2.right, det_distance);
        knee_stuck_right = Physics2D.Raycast(transform.position + new Vector3(h_dimension / 2, -v_dimension / 4, 0), Vector2.right, det_distance);
        knee_stuck_right = Physics2D.Raycast(transform.position + new Vector3(-h_dimension / 2, -v_dimension / 4, 0), -Vector2.right, det_distance);

        head_stuck_right = Physics2D.Raycast(transform.position + new Vector3(h_dimension / 2, v_dimension / 2, 0), Vector2.right, det_distance);
        head_stuck_left = Physics2D.Raycast(transform.position + new Vector3(-h_dimension / 2, v_dimension / 2, 0), -Vector2.right, det_distance);

        ceiling_right = Physics2D.Raycast(transform.position + new Vector3(h_dimension / 2, v_dimension / 2, 0), Vector2.up, det_distance);
        ceiling_left = Physics2D.Raycast(transform.position + new Vector3(-h_dimension / 2, v_dimension / 2, 0), Vector2.up, det_distance);
        ceiling_middle = Physics2D.Raycast(transform.position + new Vector3(0, v_dimension / 2, 0), Vector2.up, det_distance);

        y_speed -= gravity * Time.fixedDeltaTime * acceleration;

        if (landed_left.collider != null || landed_middle.collider != null || landed_right.collider != null)
        {
            //Debug.Log("Landed");
            in_air = false;
            if (j_request)
            {
                current_v_speed = j_force;
            }
        }
        else
        {
            in_air = true;
        }

        if (movement_request != 0 && (!in_air || air_control))
        {
            //Debug.Log("Move request accepted");
            x_speed = movement_request * Mathf.Lerp(current_h_speed, h_speed, h_speed_smoothening) * Time.fixedDeltaTime;
        }
        else if (current_h_speed != 0 && !in_air)
        {
            //Debug.Log("Slowing down");
            x_speed = Mathf.Lerp(current_h_speed, 0, h_slow_smoothening_in_air) * Time.fixedDeltaTime;
        }
        else if (current_h_speed != 0)
        {
            //Debug.Log("Slowing down");
            x_speed = Mathf.Lerp(current_h_speed, 0, h_slow_smoothening) * Time.fixedDeltaTime;
        }

        if (last_j_force != 0)
        {
            y_speed += Mathf.Lerp(current_v_speed, 0, j_decreasing) * Time.fixedDeltaTime;
        }

        if (y_speed < 0 && (ground_check_left.collider != null || ground_check_middle.collider != null || ground_check_right.collider != null))
        {
            //Debug.Log("Grounded");
            y_speed = 0;
            //last_j_force = 0;
        }
        if (y_speed > 0 && (ceiling_left.collider != null || ceiling_middle.collider != null || ceiling_right.collider != null))
        {
            //Debug.Log("Hit the ceiling");
            y_speed = 0;
            last_j_force = 0;
        }

        if (x_speed > 0 && (knee_stuck_right.collider != null || head_stuck_right.collider != null))
        {
            //Debug.Log("Against a right wall");
            x_speed = 0;
        }
        
        if (x_speed < 0 && (knee_stuck_left.collider != null || head_stuck_left.collider != null))
        {
            //Debug.Log("Against a left wall");
            x_speed = 0;
        }

        current_h_speed = x_speed;
        current_v_speed = y_speed;

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
        }
        movement_request = Input.GetAxis("Horizontal");
    }
}
