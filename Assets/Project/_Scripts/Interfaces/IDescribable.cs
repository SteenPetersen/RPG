using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDescribable
{
    string GetTitle();
    string GetDescription(bool showSaleValue=true);
}
