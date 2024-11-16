using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;



public class GameManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI NoteText;
    [SerializeField] private TextMeshProUGUI StringText;
    [SerializeField] private Toggle PlayToneToggle;
    [SerializeField] private Toggle ShowStringToggle;
    [SerializeField] private TMP_Dropdown StringCountDropdown;
    [SerializeField] private Slider TimeIntervalSlider;
    [SerializeField] private TextMeshProUGUI IntervalReadout;
    [SerializeField] private Button StartStopButton;
    [SerializeField] private AudioSource NoteAudio;
    
    Color myGreen = Color.green;

    private int _guitarStringCount = 1;
    private float _timeInterval = 3;
    private bool _playTone = true;
    private bool _running = false;
    private int lastNote = 0;
    
    // Tone Generation globals
    private int _sampleRate = 44100;
    private float _frequency = 440;
    private ulong _timeIndex = 0;
    
    private float _lastUpdateTime = 0;


    
    private static Dictionary<int, string> GuitarStrings = new Dictionary<int, string>()
    {
        { 0, "Low E" },
        { 1, "A" },
        { 2, "D" },
        { 3, "G" },
        { 4, "B" },
        { 5, "High E" }
    };

    private static Dictionary<int, string> Notes = new Dictionary<int, string>()
    {
        { 0, "A" },
        { 1, "A#" },
        { 2, "Bb" },
        { 3, "B" },
        { 4, "C" },
        { 5, "C#" },
        { 6, "Db" },
        { 7, "D" },
        { 8, "D#" },
        { 9, "Eb" },
        { 10, "E" },
        { 11, "F" },
        { 12, "F#" },
        { 13, "Gb" },
        { 14, "G" },
        { 15, "G#" },
        { 16, "Ab" }
    };


    
    private IEnumerator Start()
    {
        ColorUtility.TryParseHtmlString("#00C023", out myGreen);
        StartStopButton.image.color = myGreen;
        _playTone = PlayToneToggle.isOn;
        PlayToneToggle.onValueChanged.AddListener(PlayTone);
        StringText.enabled = ShowStringToggle.isOn;
        ShowStringToggle.onValueChanged.AddListener(ShowString);
        StringCountDropdown.onValueChanged.AddListener(SetStringCount);
        TimeIntervalSlider.onValueChanged.AddListener(SetTimeInterval);
        /*
        for (int s=0; s<6; s++)                                             // Play through all notes as a test run - comment out for builds
        {
            for (int n=0; n<=16; n++)
            {
                _timeInterval = 1;
                NoteText.text = Notes.GetValueOrDefault(n);
                StringText.text = GuitarStrings.GetValueOrDefault(s);
                if (_playTone) StartCoroutine(PlayNote(n));
                yield return new WaitForSeconds(1);
            }
        }
        */
        yield return null;
    }

     

    private void Update()
    {
        if (_running)
        {
            if (_lastUpdateTime + _timeInterval < Time.time)
            {
                ShowNewNote();
            }
        }
    }



    private void ShowNewNote()
    {
        _lastUpdateTime = Time.time;
        int newNote = Random.Range(0, 16);
        while (newNote == lastNote)
        {
            newNote = Random.Range(0, 16);
        }
        lastNote = newNote;
        NoteText.text = Notes.GetValueOrDefault(newNote);
        if (_playTone) StartCoroutine(PlayNote(newNote));
        StringText.text = GuitarStrings.GetValueOrDefault(Random.Range(0, _guitarStringCount));
    }
    
    
    
    private IEnumerator PlayNote(int note)
    {
        float actualNote = note;
        if (actualNote > 15) actualNote -= 1;       // Remove duplicate sharps and flats
        if (actualNote > 12) actualNote -= 1;
        if (actualNote > 8) actualNote -= 1;
        if (actualNote > 5) actualNote -= 1;
        if (actualNote > 1) actualNote -= 1;
        int stringNo = GuitarStrings.FirstOrDefault(x => x.Value == StringText.text).Key + 1;
        Debug.Log("String No: " + stringNo);
        switch (stringNo)
        {
            case 1:
                if (actualNote < 7) actualNote += 12;   // Octave up if lower than E
                actualNote -= 12;
                break;
            
            case 2:
                // A String, so no need to do anything!
                break;
            
            case 3:
                if (actualNote < 5) actualNote += 12;   // Octave up if lower than D
                break;

            case 4:
                if (actualNote < 10) actualNote += 12;   // Octave up if lower than G (?)
                break;

            case 5:
                if (actualNote < 2) actualNote += 12;   // Octave up if lower than B
                actualNote += 12;                       // Plus we're up an octave from here on
                break;
            
            case 6:
                if (actualNote < 7) actualNote += 12;   // Octave up if lower than E
                actualNote += 12;
                break;
            
            default:
                Debug.Log("Bollocks String No!");
                break;
        }
        //actualNote += stringNo * 12;
        _timeIndex = 0;
        _frequency = 110 * Mathf.Pow(2, actualNote / 12);
        Debug.Log($"Playing Note: {actualNote}, Frequency: {_frequency}");
        NoteAudio.Play();
        yield return new WaitForSeconds(_timeInterval / 2);
        NoteAudio.Stop();
    }

    

    public void ToggleStart()
    {
        _running = !_running;
        if (_running)
        {
            StartStopButton.image.color = Color.red;
            StartStopButton.GetComponentInChildren<TextMeshProUGUI>().text = "Stop";
            ShowNewNote();
        }
        else
        {
            StartStopButton.image.color = myGreen;
            StartStopButton.GetComponentInChildren<TextMeshProUGUI>().text = "Start";
            NoteAudio.Stop();
        }
    }
    
    
    
    void OnAudioFilterRead(float[] data, int channels)
    {
        for(int i = 0; i < data.Length; i+= channels)
        {
            float sin = Mathf.Sin(2 * Mathf.PI * _timeIndex * _frequency / _sampleRate);
            int c = 1;
            while (c <= channels)
            {
                data[i + c - 1] = sin;
                c++;
            }
            _timeIndex++;
        }
    }
    
    
    
    public void PlayTone(bool selected)
    {
        _playTone = selected;
        if (!selected) NoteAudio.Stop();
    }

    
    
    public void ShowString(bool selected)
    {
        StringText.enabled = selected;
    }
    
    
    
    private void SetStringCount(int dropDownUnary)
    {
        _guitarStringCount = dropDownUnary + 1; // Dropdown is 0-indexed
        //Debug.Log("New String Count Set: " + _guitarStringCount);
    }
    
    
    
    private void SetTimeInterval(float interval)
    {
        _timeInterval = Mathf.Round(interval * 10f) / 10f;      // Round to 1 decimal place
        IntervalReadout.text = $"Interval: {_timeInterval}s";
        //Debug.Log("New Interval Set: " + _timeInterval);
    }

    

    private void OnDestroy()
    {
        NoteAudio.Stop();
    }
}