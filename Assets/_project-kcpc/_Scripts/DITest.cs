using UnityEngine;
using VContainer;
using VContainer.Unity;

public class DITest : MonoBehaviour
{
    [Inject]
    private void Construct(StaticData staticData)
    {
        Debug.Log(staticData.SampleMessage);
    }
}
