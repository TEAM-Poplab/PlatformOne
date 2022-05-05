/************************************************************************************
* 
* Class Purpose: the class handles the user phrases for the splashscreen quote
*
************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using KoganeUnityLib;

public class QuoteManager : MonoBehaviour
{
    //TMP_Typewriter components for author and quotation TMP field
    [SerializeField][Tooltip("TMP_Typewriter component of the quotation object")] private TMP_Typewriter quotationTextField;
    [SerializeField] [Tooltip("TMP_Typewriter component of the author object")] private TMP_Typewriter authorTextField;
    [Tooltip("Animator component of the fading object around player")] public Animator fadeAnimator;

    [Header("Quotation field settings")]
    [Min(0.1f)]
    public float quotationSpeed = 1f;
    [Min(0)]
    public int quotationDelay = 0;
    private string quoteText = "";

    [Header("Author field settings")]
    [Min(0.1f)]
    public float authorSpeed = 1f;
    private string authorText = "";

    [Space(15)]
    [Tooltip("The list of quotation messages. Random message is selected. The string must have the following format: 'quotation ~ author'")]
    [TextArea]
    public List<string> quotes;

    private int _index; //Index of the random selected phrase
    private float timer = 0;    //General timer
    private float triggerTimer = 0; //Timer to trigger author field
    private float triggerTimer2 = 0;    //Timer to trigger animator
    private bool quoteHasDone = false;  //Check if quote field has done its animation
    private bool authorHasDone = true;  //Check if author has done its animation
    private QuoteStruct _selectedQuote;

    public int Index
    {
        get
        {
            return _index;
        }
    }

    struct QuoteStruct
    {
        public string quotation;
        public string author;
    }

    //Choose a random phrase and parse author and quote
    void Awake()
    {
        _index = UnityEngine.Random.Range(0, quotes.Count);
        string[] quotationParsed = quotes[_index].Split('~');

        _selectedQuote.quotation = quotationParsed[0];
        _selectedQuote.author = quotationParsed[1];
    }

    private void Start()
    {
        quoteText += (_selectedQuote.quotation + Environment.NewLine);
        authorText += (_selectedQuote.author + Environment.NewLine);
    }

    //Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        //1. Wait for the quote typing animation to play. It has to be the first
        if (timer >= quotationDelay && !quoteHasDone)
        {
            quotationTextField.Play(quoteText, quotationSpeed, null);
            quoteHasDone = true;
            authorHasDone = false;
            triggerTimer = quoteText.Length/quotationSpeed + timer;
        }

        //2. Only after the quote has completely appeared, the author con play its typing animation
        if (!authorHasDone && timer >= triggerTimer)
        {
            transform.parent.gameObject.GetComponent<Animator>().speed = 1;
            authorTextField.Play(authorText, authorSpeed, null);
            authorHasDone = true;
            triggerTimer2 = authorText.Length / authorSpeed + timer;
        }

        //3. After both the quote and the author typing animations have ended, the animator can resume its fade out animation
        if (quoteHasDone && authorHasDone && timer >= triggerTimer2)
        {
            transform.parent.gameObject.GetComponent<Animator>().speed = 1;
        }
    }
}
