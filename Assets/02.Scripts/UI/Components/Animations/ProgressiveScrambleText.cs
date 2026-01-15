using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class ProgressiveScrambleText
{
    [Header("UI References")]
    [SerializeField] private TMP_Text _text;

    [Header("Settings")]
    [SerializeField] private float _charDuration = 0.15f;
    [SerializeField] private int _scrambleIterations = 5;
    [SerializeField] private string _scrambleChars = "0123456789";

    [Header("Format")]
    [SerializeField] private string _numberFormat = "N0";
    [SerializeField] private string _timeFormat = "00";
    [SerializeField] private char _timeSeparator = ':';
    [Tooltip("Characters that should not be scrambled (e.g., '.:')")]
    [SerializeField] private string _skipChars = ".:";

    public event Action OnComplete;

    private Coroutine _currentCoroutine;
    private MonoBehaviour _coroutineRunner;
    private readonly StringBuilder _stringBuilder = new StringBuilder(32);
    private char[] _charBuffer = new char[32];

    public void Init(MonoBehaviour runner)
    {
        _coroutineRunner = runner;
    }

    public void Play(int targetValue)
    {
        PlayInternal(targetValue.ToString(_numberFormat));
    }

    public void Play(float targetValue)
    {
        PlayInternal(targetValue.ToString(_numberFormat));
    }

    public void Play(string targetValue)
    {
        PlayInternal(targetValue);
    }

    public void PlayTime(float totalSeconds)
    {
        int minutes = Mathf.FloorToInt(totalSeconds / 60f);
        int seconds = Mathf.FloorToInt(totalSeconds % 60f);

        _stringBuilder.Clear();
        _stringBuilder.Append(minutes.ToString(_timeFormat));
        _stringBuilder.Append(_timeSeparator);
        _stringBuilder.Append(seconds.ToString(_timeFormat));

        PlayInternal(_stringBuilder.ToString());
    }

    public void PlayRaw(string targetValue)
    {
        PlayInternal(targetValue);
    }

    private void PlayInternal(string targetValue)
    {
        if (_coroutineRunner == null)
        {
            Debug.LogError("[ProgressiveScrambleText] CoroutineRunner is not set. Call Init() first.");
            return;
        }

        Stop();
        _currentCoroutine = _coroutineRunner.StartCoroutine(ProgressiveScrambleCoroutine(targetValue));
    }

    public void Stop()
    {
        if (_currentCoroutine != null && _coroutineRunner != null)
        {
            _coroutineRunner.StopCoroutine(_currentCoroutine);
            _currentCoroutine = null;
        }
    }

    public void SetTextImmediate(int value)
    {
        _text.text = value.ToString(_numberFormat);
    }

    public void SetTextImmediate(float value)
    {
        _text.text = value.ToString(_numberFormat);
    }

    public void SetTextImmediate(string value)
    {
        _text.text = value;
    }

    public void SetTimeImmediate(float totalSeconds)
    {
        int minutes = Mathf.FloorToInt(totalSeconds / 60f);
        int seconds = Mathf.FloorToInt(totalSeconds % 60f);

        _stringBuilder.Clear();
        _stringBuilder.Append(minutes.ToString(_timeFormat));
        _stringBuilder.Append(_timeSeparator);
        _stringBuilder.Append(seconds.ToString(_timeFormat));

        _text.text = _stringBuilder.ToString();
    }

    public void Clear()
    {
        _text.text = string.Empty;
    }

    public void SetActive(bool active)
    {
        _text.gameObject.SetActive(active);
    }

    private IEnumerator ProgressiveScrambleCoroutine(string target)
    {
        int length = target.Length;
        if (_charBuffer.Length < length)
        {
            _charBuffer = new char[length];
        }

        var currentLength = 0;
        var waitTime = new WaitForSeconds(_charDuration / _scrambleIterations);

        for (var i = 0; i < length; i++)
        {
            char targetChar = target[i];

            if (_skipChars.IndexOf(targetChar) >= 0)
            {
                _charBuffer[currentLength++] = targetChar;
                _text.SetCharArray(_charBuffer, 0, currentLength);
                continue;
            }

            for (var j = 0; j < _scrambleIterations; j++)
            {
                char randomChar = _scrambleChars[Random.Range(0, _scrambleChars.Length)];
                _charBuffer[currentLength] = randomChar;
                _text.SetCharArray(_charBuffer, 0, currentLength + 1);
                yield return waitTime;
            }

            _charBuffer[currentLength++] = targetChar;
            _text.SetCharArray(_charBuffer, 0, currentLength);
        }

        _currentCoroutine = null;
        OnComplete?.Invoke();
    }
}