using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface MATKPointerResponder
{
    void GeometryOnFocus(GameObject geometry);

    void GeometryOnLostFocus(GameObject geometry);
}
