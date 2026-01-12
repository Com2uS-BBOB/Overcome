using System;

public interface ISequentialUI
{
    event Action OnShowComplete;
    void Show();
    void Hide();
}