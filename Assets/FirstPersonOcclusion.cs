using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FirstPersonOcclusion : MonoBehaviour
{
    [Header("FMOD Event")]
    [SerializeField] EventReference SelectAudio;
    private EventInstance Audio;
    private EventDescription AudioDes;
    private StudioListener Listener;
    private PLAYBACK_STATE pb;

    [Header("Occlusion Options")]
    [SerializeField]
    [Range(0f, 10f)]
    private float SoundOcclusionWidening = 1f;
    [SerializeField]
    [Range(0f, 10f)]
    private float PlayerOcclusionWidening = 1f;
    [SerializeField]
    private LayerMask OcclusionLayer;

    private bool AudioIsVirtual;
    private float MaxDistance;
    private float ListenerDistance;
    private float lineCastHitCount = 0f;
    private Color colour;

    private void Start()
    {
        Audio = RuntimeManager.CreateInstance(SelectAudio);
        RuntimeManager.AttachInstanceToGameObject(Audio, GetComponent<Transform>(), GetComponent<Rigidbody>());
        Audio.start();
        Audio.release();

        AudioDes = RuntimeManager.GetEventDescription(SelectAudio);
        AudioDes.getMinMaxDistance(out float MinDistance, out  MaxDistance);
        Listener = FindObjectOfType<StudioListener>();
    }
    
    private void FixedUpdate()
    {
        Audio.isVirtual(out AudioIsVirtual);
        Audio.getPlaybackState(out pb);
        ListenerDistance = Vector3.Distance(transform.position, Listener.transform.position);
        Debug.Log("AudioIsVirtual"+AudioIsVirtual);
        Debug.Log("pb"+pb);
        Debug.Log("ListenerDistance"+ListenerDistance);
        Debug.Log("MaxDistance"+MaxDistance);

        if (!AudioIsVirtual && pb == PLAYBACK_STATE.PLAYING && ListenerDistance <= MaxDistance)
            OccludeBetween(transform.position, Listener.transform.position);

        lineCastHitCount = 0f;
        
    }

    private void OccludeBetween(Vector3 sound, Vector3 listener)
    {
        Vector3 SoundLeft = CalculatePoint(sound, listener, SoundOcclusionWidening, true);
        Vector3 SoundRight = CalculatePoint(sound, listener, SoundOcclusionWidening, false);

        Vector3 SoundAbove = new Vector3(sound.x, sound.y + SoundOcclusionWidening, sound.z);
        Vector3 SoundBelow = new Vector3(sound.x, sound.y - SoundOcclusionWidening, sound.z);

        Vector3 ListenerLeft = CalculatePoint(listener, sound, PlayerOcclusionWidening, true);
        Vector3 ListenerRight = CalculatePoint(listener, sound, PlayerOcclusionWidening, false);

        Vector3 ListenerAbove = new Vector3(listener.x, listener.y + PlayerOcclusionWidening * 0.5f, listener.z);
        Vector3 ListenerBelow = new Vector3(listener.x, listener.y - PlayerOcclusionWidening * 0.5f, listener.z);
        

        CastLine(SoundLeft, ListenerLeft);
        CastLine(SoundLeft, listener);
        CastLine(SoundLeft, ListenerRight);

        CastLine(sound, ListenerLeft);
        CastLine(sound, listener);
        CastLine(sound, ListenerRight);

        CastLine(SoundRight, ListenerLeft);
        CastLine(SoundRight, listener);
        CastLine(SoundRight, ListenerRight);
        
        CastLine(SoundAbove, ListenerAbove);
        CastLine(SoundBelow, ListenerBelow);

        if (PlayerOcclusionWidening == 0f || SoundOcclusionWidening == 0f)
        {
            colour = Color.blue;
        }
        else
        {
            colour = Color.green;
        }

        SetParameter();
    }

    private Vector3 CalculatePoint(Vector3 a, Vector3 b, float m, bool posOrneg)
    {
        float x;
        float z;
        float n = Vector3.Distance(new Vector3(a.x, 0f, a.z), new Vector3(b.x, 0f, b.z));
        float mn = (m / n);
        if (posOrneg)
        {
            x = a.x + (mn * (a.z - b.z));
            z = a.z - (mn * (a.x - b.x));
        }
        else
        {
            x = a.x - (mn * (a.z - b.z));
            z = a.z + (mn * (a.x - b.x));
        }
        return new Vector3(x, a.y, z);
    }

    private void CastLine(Vector3 Start, Vector3 End)
    {
        RaycastHit hit;
        Physics.Linecast(Start, End, out hit, OcclusionLayer);

        if (hit.collider)
        {
            lineCastHitCount++;
            Debug.DrawLine(Start, End, Color.red);
            
        }
        else
            Debug.DrawLine(Start, End, colour);
            
    }

    private void SetParameter()
    {
        Audio.setParameterByName("Occlusion", lineCastHitCount / 11);
    }
}