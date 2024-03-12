using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHasProgress
{
    event EventHandler<OnProgressChangedEventArgs> OnProgressChanged;
}
