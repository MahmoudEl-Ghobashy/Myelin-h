using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Scripting.Python;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public string[,] Timings = new string[12, 2];       //Array to store timing for the reach and grab events
    public Transform[] Apples;                          //Array of 6 apples transforms 

    public Transform m_target;                          //Target object which the arm will follow and apply Inverse kinematics to it 
    public Transform m_center;                          //Center object to return to
    public AudioSource m_audioSource;                   //Audio source to play beeb sound
    public Animator m_handAnimator;                     //Hand animator to control grab and release animations

    public float TimeToReach = 10f;                     //Time taken to reach target "10 seconds for apple, 5 seconds for center"

    Transform newPoint;                                 //Point to follow

    int[] numberOfVisits;                               //Number of visits for every apple
    int applesVisited = 0;                              //Number of visits for all apples
    float distance = 0;                                 

    bool canInvokeMethods = false;                      //Boolean to stop update method from executing when not neccessary

    private void Start()
    {
        numberOfVisits = new int[Apples.Length];                                    //Initialize and set all elements in array to 2 "only 2 visits for each apple"
        for (int i = 0; i < numberOfVisits.Length; i++)
            numberOfVisits[i] = 2;

        distance = Vector3.Distance(m_center.position, m_target.position);          //Calculate distance to next target "center" 
        ResetTarget();                                                              //Go to center
    }

    void SelectRandom()                                                             //Select random apple from the apples array based on the numberOfVisits array
    {
        int random = UnityEngine.Random.Range(0, Apples.Length);                    //choose random number between 0 and 6

        if (numberOfVisits[random] == 0)                                            //Apple already visited twice, repeat with different value    
        {
            SelectRandom();
        }
        else
        {
            applesVisited++;                                                                
            distance = Vector3.Distance(m_center.position, Apples[random].position);    //Calculate distance to next target "apple" 
            Timings[applesVisited - 1, 0] = DateTime.Now.ToLongTimeString();            //Add new reach timing
            TimeToReach = 10f;                                                          //Set lerp time to 10 seconds
            numberOfVisits[random] = numberOfVisits[random] - 1;                        //reduce number of visits for this apple by 1
            m_audioSource.Play();                                                       //Play beeb sound
            newPoint = Apples[random];                                                  //Set new target so the update method will start working
        }
    }

    private void Update()
    {

        if (m_target.position != newPoint.position)                                 //While target didn't reach the apple or center position keep moving towards it with a hight rate till reached exactly in time
        {
            m_target.position = Vector3.MoveTowards(m_target.position, newPoint.position, (distance / TimeToReach) * Time.deltaTime);
            canInvokeMethods = true;
        }
        else if (newPoint != m_center && canInvokeMethods)                          //Target reached and target is apple, start grab animation and set next point to center but wait for 5 seconds first
        {
            m_handAnimator.SetTrigger("Grab");
            Timings[applesVisited - 1, 1] = DateTime.Now.ToLongTimeString();        //Log grab timing to array in the 2nd column
            canInvokeMethods = false;
            Invoke("ResetTarget", 5f);                                              //Wait 5 seconds and return to center 
        }
        else if (newPoint == m_center && applesVisited != 12 && canInvokeMethods)   //Target reached and target is center, next point to new random apple but wait for 5 seconds first
        {
            canInvokeMethods = false;
            Invoke("SelectRandom", 5f);                                             //Wait 5 seconds and select new apple
        }
    }

    void ResetTarget()                                                              //Return to center            
    {
        m_handAnimator.SetTrigger("Grab");                                          //Start release animation
        TimeToReach = 5;                                                            //Set lerp time to 5 seconds
        newPoint = m_center;                                                        //Set new target to follow in update method
        if (applesVisited == 12)                                                    //If all apples are visited end the task and start the python script
        {
            Invoke("EndTask", 10f);
        }
    }

    void EndTask()
    {
        string PathToPythonScript = Application.dataPath + "\\Scripts\\PythonScript.py";        //Path to python script
        PythonRunner.RunFile(PathToPythonScript);                                               //Execute the script


        //Python script as follows
        /*
            import UnityEngine as ue                                                            //Import Unity engine to use it's features
            import numpy as np                                                                  //Import numpy to use the numpy array and save the array to file

            gameManager = ue.GameObject.Find("GameManager")                                     //Find game manager object
            ManagerComponent = gameManager.GetComponent("GameManager")                          //Find GameManager component
            ue.Debug.Log(ManagerComponent.Timings[0,0])                                         //Debug for testing

            data = np.array(range(24),dtype=object).reshape(12,2)                               //Create 2D numpy array (12,2)
            for x in range(12):                                                                 //Loop to fill numpy array with data from Timings 2D array in GameManager
	            data[x,0] = ManagerComponent.Timings[x,0]
	            data[x,1] = ManagerComponent.Timings[x,1]
            ue.Debug.Log(data[0,0])                                                             //Debug for testing

            np.save('Data.npy' , data)                                                          //Save numpy to "project folder/Data.npy" 
         */
    }
}
